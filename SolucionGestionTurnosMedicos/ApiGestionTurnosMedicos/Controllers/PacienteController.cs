using ApiGestionTurnosMedicos.Validations;
using BusinessLogic.AppLogic;
using DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.CustomModels;
using Models.DTOs;
using System.Security.Claims;
using DataAccess.Context;

namespace ApiGestionTurnosMedicos.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class PacienteController : ControllerBase
    {
        private readonly PacienteLogic _pacienteLogic;
        private readonly ILogger<PacienteController> _logger;
        private readonly GestionTurnosContext _context;

        public PacienteController(PacienteLogic pacienteLogic, ILogger<PacienteController> logger, GestionTurnosContext context)
        {
            _pacienteLogic = pacienteLogic;
            _logger = logger;
            _context = context;
        }

        #region MÉTODOS PARA ADMIN

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        public async Task<ActionResult<List<Paciente>>> GetAllPatients()
        {
            try
            {
                return Ok(await _pacienteLogic.PatientsListAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo todos los pacientes");
                return BadRequest(new { message = ex.Message });
            }
        }

        // CORRECCIÓN 1: Agregamos :int para que este método SOLO acepte números.
        // Si el front manda "get-dni", .NET ignorará este método y buscará el siguiente.
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Paciente>> GetPatientById(int id)
        {
            try
            {
                var patient = await _pacienteLogic.GetPatientForIdAsync(id);
                if (patient == null) return NotFound(new { message = "Paciente no encontrado" });
                return Ok(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo paciente por ID");
                return BadRequest(new { message = ex.Message });
            }
        }

        // CORRECCIÓN 2: Agregamos explícitamente el método para el DNI que pide tu Frontend.
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("get-dni")]
        public async Task<ActionResult<Paciente>> GetPatientByDni([FromQuery] string dni)
        {
            try
            {
                if (string.IsNullOrEmpty(dni)) return BadRequest(new { message = "El DNI es requerido" });

                var patient = await _pacienteLogic.GetPatientForDNIAsync(dni);
                if (patient == null) return NotFound(new { message = "No existe paciente con ese DNI" });

                return Ok(patient);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDto dto)
        {
            var oPatient = new Paciente
            {
                Apellido = dto.Apellido,
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                Email = dto.Email,
                FechaNacimiento = dto.FechaNacimiento,
                Dni = dto.Dni
            };

            var validations = new ValidationsMethodPost(_context);
            var validationResult = await validations.ValidationsMethodPostPatient(oPatient);

            if (!validationResult.IsValid)
            {
                return BadRequest(new { message = validationResult.ErrorMessage });
            }

            try
            {
                await _pacienteLogic.CreatePatientWithUserAsync(oPatient, dto.Password);
                return Ok(new { message = "Paciente creado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando paciente");
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{id:int}")] // También agregamos :int aquí por seguridad
        public async Task<IActionResult> UpdatePatientAsync(int id, [FromBody] UpdatePatientDto pacienteDto)
        {
            try
            {
                await _pacienteLogic.UpdatePatientAsync(id, pacienteDto);
                return Ok(new { message = "Paciente actualizado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando paciente");
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id:int}")] // Y aquí
        public async Task<IActionResult> DeletePatient(int id)
        {
            try
            {
                await _pacienteLogic.DeletePatientAsync(id);
                return Ok(new { message = "Paciente eliminado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando paciente");
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("get-qty")]
        public async Task<ActionResult<int>> GetPatientsCount()
        {
            try
            {
                return Ok(await _pacienteLogic.GetPatientsQtyAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cantidad de pacientes");
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region MÉTODOS PARA PACIENTE (SELF)

        [Authorize(Roles = UserRoles.Paciente)]
        [HttpGet("my-profile")]
        public async Task<ActionResult<Paciente>> GetMyProfile()
        {
            try
            {
                var dni = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(dni))
                    return Unauthorized(new { message = "No se pudo identificar al usuario." });

                var paciente = await _pacienteLogic.GetPatientForDNIAsync(dni);
                return Ok(paciente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo perfil del paciente");
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion
    }
}