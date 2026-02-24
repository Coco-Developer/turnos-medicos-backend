
using BusinessLogic.AppLogic;
using DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.CustomModels;
using Models.DTOs;
using System.Security.Claims;

namespace ApiGestionTurnosMedicos.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class PacienteController : ControllerBase
    {
        private readonly PacienteLogic _pacienteLogic;
        private readonly ILogger<PacienteController> _logger;

        public PacienteController(
            PacienteLogic pacienteLogic,
            ILogger<PacienteController> logger)
        {
            _pacienteLogic = pacienteLogic;
            _logger = logger;
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
                _logger.LogError(ex, "Error obteniendo pacientes");
                return StatusCode(500, new { message = "Error interno al obtener pacientes." });
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Paciente>> GetPatientById(int id)
        {
            try
            {
                var patient = await _pacienteLogic.GetPatientForIdAsync(id);
                if (patient == null)
                    return NotFound(new { message = "Paciente no encontrado" });

                return Ok(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo paciente ID {Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("get-dni")]
        public async Task<ActionResult<Paciente>> GetPatientByDni([FromQuery] string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
                return BadRequest(new { message = "El DNI es requerido." });

            try
            {
                var patient = await _pacienteLogic.GetPatientForDNIAsync(dni);

                if (patient == null)
                    return NoContent();

                return Ok(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo paciente por DNI");
                return StatusCode(500, new { message = "Error al verificar DNI." });
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Datos inválidos." });

            var patient = new Paciente
            {
                Apellido = dto.Apellido,
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                Email = dto.Email,
                FechaNacimiento = dto.FechaNacimiento,
                Dni = dto.Dni
            };

            try
            {
                await _pacienteLogic.CreatePatientWithUserAsync(patient, dto.Password);

                return CreatedAtAction(
                    nameof(GetPatientById),
                    new { id = patient.Id },
                    new { message = "Paciente creado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando paciente");
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] UpdatePatientDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Datos inválidos." });

            try
            {
                await _pacienteLogic.UpdatePatientAsync(id, dto);
                return Ok(new { message = "Paciente actualizado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando paciente {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            try
            {
                await _pacienteLogic.DeletePatientAsync(id);
                return Ok(new { message = "Paciente eliminado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando paciente {Id}", id);
                return BadRequest(new { message = "No se pudo eliminar el paciente." });
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
                return StatusCode(500, new { message = ex.Message });
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
                    return Unauthorized();

                var paciente = await _pacienteLogic.GetPatientForDNIAsync(dni);

                if (paciente == null)
                    return NotFound(new { message = "Perfil no encontrado." });

                return Ok(paciente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo perfil");
                return StatusCode(500, new { message = "Error al recuperar perfil." });
            }
        }

        #endregion
    }
}