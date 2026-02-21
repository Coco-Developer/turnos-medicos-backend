using BusinessLogic.AppLogic.Services;
using Microsoft.AspNetCore.Mvc;
using Models.CustomModels;

namespace ApiGestionTurnosMedicos.Controllers
{
    [ApiController]
    [Route("admin-setup")]
    public class AdminSetupController : ControllerBase
    {
        private readonly AdminSetupService _adminSetupService;
        private readonly IWebHostEnvironment _env;

        public AdminSetupController(AdminSetupService adminSetupService, IWebHostEnvironment env)
        {
            _adminSetupService = adminSetupService;
            _env = env;
        }

        /// <summary>
        /// Crea un usuario administrador.
        /// SOLO DISPONIBLE EN ENTORNO DEVELOPMENT.
        /// </summary>
        [HttpPost("crear-admin")]
        public async Task<IActionResult> CrearAdmin([FromBody] AdminRegisterDto dto)
        {
            if (!_env.IsDevelopment())
                return Unauthorized(new { message = "Este endpoint solo está disponible en entorno de desarrollo." });

            var (success, message) = await _adminSetupService.CrearAdminAsync(dto);

            return success ? Ok(new { message }) : BadRequest(new { message });
        }
    }
}