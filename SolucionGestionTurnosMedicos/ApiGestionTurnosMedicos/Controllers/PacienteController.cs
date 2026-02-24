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

        /// <summary>
        /// Obtiene la lista completa de pacientes.
        /// </summary>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        public async Task<ActionResult<List<Paciente>>> GetAllPatients()
        {
            try
            {
                var patients = await _pacienteLogic.PatientsListAsync();
                return Ok(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo todos los pacientes");
                return StatusCode(500, new { message = "Error interno al obtener la lista de pacientes." });
            }
        }

        /// <summary>
        /// Obtiene un paciente por su ID numérico.
        /// </summary>
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
                _logger.LogError(ex, "Error obteniendo paciente por ID: {Id}", id);
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un paciente por su DNI. 
        /// Modificado para no lanzar 500 si no existe.
        /// </summary>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("get-dni")]
        public async Task<ActionResult<Paciente>> GetPatientByDni([FromQuery] string dni)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dni))
                    return BadRequest(new { message = "El DNI es requerido" });

                var patient = await _pacienteLogic.GetPatientForDNIAsync(dni);

                // IMPORTANTE: Si no existe, devolvemos 204 (No Content) o 404.
                // Esto evita que el Frontend reciba un Error 500 genérico.
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

        /// <summary>
        /// Crea un nuevo paciente y su usuario asociado.
        /// </summary>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Datos nulos." });

            var oPatient = new Paciente
            {
                Apellido = dto.Apellido,
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                Email = dto.Email,
                FechaNacimiento = dto.FechaNacimiento,
                Dni = dto.Dni
            };

            // Validaciones de negocio manuales (DNI duplicado, Email, etc.)
            var validations = new ValidationsMethodPost(_context);
            var validationResult = await validations.ValidationsMethodPostPatient(oPatient);

            if (!validationResult.IsValid)
            {
                return BadRequest(new { message = validationResult.ErrorMessage });
            }

            try
            {
                await _pacienteLogic.CreatePatientWithUserAsync(oPatient, dto.Password);
                return CreatedAtAction(nameof(GetPatientById), new { id = oPatient.Id }, new { message = "Paciente creado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en la creación del paciente");
                // Enviamos un 400 Bad Request con el mensaje real de la excepción
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza los datos de un paciente existente.
        /// </summary>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePatientAsync(int id, [FromBody] UpdatePatientDto pacienteDto)
        {
            if (pacienteDto == null) return BadRequest(new { message = "Datos inválidos." });

            try
            {
                await _pacienteLogic.UpdatePatientAsync(id, pacienteDto);
                return Ok(new { message = "Paciente actualizado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando paciente ID: {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un paciente del sistema.
        /// </summary>
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
                _logger.LogError(ex, "Error eliminando paciente ID: {Id}", id);
                return BadRequest(new { message = "No se pudo eliminar el paciente." });
            }
        }

        /// <summary>
        /// Obtiene la cantidad total de pacientes.
        /// </summary>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("get-qty")]
        public async Task<ActionResult<int>> GetPatientsCount()
        {
            try
            {
                var count = await _pacienteLogic.GetPatientsQtyAsync();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cantidad");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        #endregion

        #region MÉTODOS PARA PACIENTE (SELF)

        /// <summary>
        /// Obtiene el perfil del paciente logueado.
        /// </summary>
        [Authorize(Roles = UserRoles.Paciente)]
        [HttpGet("my-profile")]
        public async Task<ActionResult<Paciente>> GetMyProfile()
        {
            try
            {
                var dni = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(dni)) return Unauthorized();

                var paciente = await _pacienteLogic.GetPatientForDNIAsync(dni);
                if (paciente == null) return NotFound(new { message = "Perfil no encontrado." });

                return Ok(paciente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error perfil");
                return StatusCode(500, new { message = "Error al recuperar el perfil." });
            }
        }

        #endregion
    }
}