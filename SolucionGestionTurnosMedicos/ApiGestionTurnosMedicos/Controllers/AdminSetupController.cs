// Controllers/AdminSetupController.cs
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

        [HttpPost("crear-admin")]
        public async Task<IActionResult> CrearAdmin([FromBody] AdminRegisterDto dto)
        {
            // Permitir solo en entorno Development
            if (!_env.IsDevelopment())
                return Unauthorized("Este endpoint solo está disponible en entorno de desarrollo.");

            var (success, message) = await _adminSetupService.CrearAdminAsync(dto);

            return success ? Ok(message) : BadRequest(message);
        }
    }
}
