using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ApiGestionTurnosMedicos.Middlewares;
using BusinessLogic.AppLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Models.CustomModels;

namespace ApiGestionTurnosMedicos.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;

        public AuthController(IConfiguration config, IAuthService authService)
        {
            _config = config;
            _authService = authService;
        }

        /// <summary>  
        /// Genera un token JWT administrativo basado en la API Key.
        /// </summary>  
        [HttpPost("token")]
        [ApiKey]
        public IActionResult GetToken()
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiresInMinutes"]!)),
                SigningCredentials = credentials,
                // Agregamos un claim de sistema para identificar tokens generados por API Key
                Subject = new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("type", "ApiKeyToken") })
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new { token = tokenHandler.WriteToken(token) });
        }

        /// <summary>
        /// Login de usuario: valida credenciales y retorna JWT con datos de usuario.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Datos de login inválidos", details = ModelState });

            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success)
                return Unauthorized(new { message = result.ErrorMessage });

            return Ok(new
            {
                token = result.Token,
                nombre = result.NombreUsuario,
                rol = result.Rol
            });
        }

        /// <summary>
        /// Obtiene el perfil del usuario autenticado mediante el token.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            // Extraemos el userId del Claim configurado en AuthService
            var userIdStr = User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Token inválido o no contiene identificador de usuario." });

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            return Ok(new
            {
                id = user.Id,
                nombre = user.Username,
                rol = user.Rol,
                isActive = user.IsActive
            });
        }

        /// <summary>
        /// Cambia la contraseña del usuario actual.
        /// </summary>
        [HttpPost("new-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromForm] string actualPassword, [FromForm] string nuevoPassword)
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Token inválido" });

            if (string.IsNullOrWhiteSpace(actualPassword) || string.IsNullOrWhiteSpace(nuevoPassword))
                return BadRequest(new { message = "Debe completar ambos campos de contraseña." });

            var result = await _authService.ChangePasswordAsync(userId, actualPassword, nuevoPassword);

            if (!result.Success)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = "Contraseña actualizada correctamente.", cambiada = 1 });
        }

        /// <summary>
        /// Resetea la contraseña y la envía por email al nombre de usuario provisto.
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
                return BadRequest(new { message = "El nombre de usuario es requerido." });

            var result = await _authService.SendNewPasswordAsync(dto.Username);

            if (!result.Success)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = "Email enviado con éxito", enviado = 1 });
        }
    }

    // DTO auxiliar para evitar problemas de parseo en ForgotPassword
    public class ForgotPasswordDto { public string Username { get; set; } = string.Empty; }
}