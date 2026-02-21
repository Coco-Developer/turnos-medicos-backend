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
    /// <summary>  
    /// Controlador API para la gestión de la autorización JWT y API Key.  
    /// </summary>  
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
        /// Genera un token JWT si la clave API es válida.  
        /// </summary>  
        [HttpPost("token")]
        [ApiKey] // ahora protegido por el atributo
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
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { token = tokenString });
        }

        /// <summary>
        /// Realiza login de usuario (por Nombre y Password) y retorna un JWT si las credenciales son válidas.
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
        /// Retorna el perfil del usuario autenticado.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Token inválido o no contiene userId." });

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
        /// Cambia la contraseña del usuario por la nueva
        /// </summary>
        [HttpPost("new-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromForm] string actualPassword, [FromForm] string nuevoPassword)
        {
            var userIdStr = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Token inválido" });

            if (string.IsNullOrWhiteSpace(actualPassword) || string.IsNullOrWhiteSpace(nuevoPassword))
                return BadRequest(new { message = "La contraseña actual y la nueva no pueden estar vacías." });

            var result = await _authService.ChangePasswordAsync(userId, actualPassword, nuevoPassword);

            if (!result.Success)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = "Contraseña cambiada con éxito", cambiada = 1 });
        }

        /// <summary>
        /// Crea una nueva contraseña y envía un correo con la nueva contraseña al usuario.
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest(new { message = "El nombre de usuario no debe estar vacío." });

            var result = await _authService.SendNewPasswordAsync(username);

            if (!result.Success)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = "Email enviado con éxito", enviado = 1 });
        }
    }
}