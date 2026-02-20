using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DataAccess.Data;
using DataAccess.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models.CustomModels;


namespace BusinessLogic.AppLogic.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher<Usuario> passwordHasher,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            // 1. Buscar usuario
            var user = await _userRepository.GetByNombreAsync(dto.Username);
            if (user == null || !user.IsActive)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    ErrorMessage = "Usuario no encontrado o inactivo"
                };
            }

            // 2. Verificar contraseña
            var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (verify != PasswordVerificationResult.Success)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    ErrorMessage = "Credenciales inválidas"
                };
            }

            // 3. Generar JWT
            var jwtSection = _configuration.GetSection("JwtSettings");
            var keyBytes = Encoding.UTF8.GetBytes(jwtSection["Key"]!);
            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Rol ?? UserRoles.Paciente),  // Rol con default en caso de que no exista
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())  // ID del usuario en el claim
            };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSection["ExpiresInMinutes"]!)),
                signingCredentials: creds
            );
            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

            // 4. Devolver respuesta con token y datos del usuario
            return new LoginResponseDto
            {
                Success = true,
                Token = tokenStr,
                NombreUsuario = user.Username,
                Rol = user.Rol
            };
        }

        // Implementación de GetUserByIdAsync
        public async Task<Usuario?> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        // Cambiar contraseña
        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdTrackingAsync(userId);
            if (user == null)
                return (false, "Usuario no encontrado");

            var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
            if (verify != PasswordVerificationResult.Success)
                return (false, "La contraseña actual no es correcta");

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);

            
            var changes = await _userRepository.SaveChangesAsync();

            if (changes == false)
                return (false, "No se guardaron cambios en la base");

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> SendNewPasswordAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username); // Nuevo método en repo
            if (user == null)
                return (false, "Usuario no encontrado");

            // Generar nueva contraseña aleatoria
            var newPassword = GenerateRandomPassword(); // <- Esta función no existe aún
            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            // Guardar cambios
            var changes = await _userRepository.SaveChangesAsync();
            if (!changes)
                return (false, "No se guardaron cambios en la base");

            // Enviar email con la nueva contraseña
            var emailService = new EmailService(new SmtpEmailSender());
            emailService.SendPasswordResetEmail(user.Email, newPassword);

            return (true, null);
        }

        private string GenerateRandomPassword()
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var randomBytes = new byte[8]; // 8 caracteres
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            var chars = new char[8];
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = validChars[randomBytes[i] % validChars.Length];
            }

            return new string(chars);
        }

    }
}
