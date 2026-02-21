using ApiGestionTurnosMedicos.Validations;
using BusinessLogic.AppLogic;
using DataAccess.Context;
using DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.CustomModels;
using Models.DTOs;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApiGestionTurnosMedicos.Controllers
{
    /// <summary>
    /// Controlador API para la gestión de pacientes en el sistema de turnos médicos.
    /// </summary>
    [Authorize] // JWT obligatorio
    [Route("[controller]")]
    [ApiController]
    public class PacienteController : ControllerBase
    {
        private readonly PacienteLogic _pacienteLogic;
        private readonly GestionTurnosContext _context;
        private readonly ILogger<PacienteController> _logger;

        public PacienteController(PacienteLogic pacienteLogic, GestionTurnosContext context, ILogger<PacienteController> logger)
        {
            _pacienteLogic = pacienteLogic;
            _context = context;
            _logger = logger;
        }

        // ===========================
        // MÉTODOS PARA ADMIN
        // ===========================

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
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("{id}")]
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
                _logger.LogError(ex, "Error obteniendo paciente por ID");
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDto dto)
        {
            var validations = new ValidationsMethodPost(_context);

            var oPatient = new Paciente
            {
                Apellido = dto.Apellido,
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                Email = dto.Email,
                FechaNacimiento = dto.FechaNacimiento,
                Dni = dto.Dni
            };

            var result = validations.ValidationsMethodPostPatient(oPatient);
            if (!result.IsValid)
                return BadRequest(new { message = result.ErrorMessage });

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
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatientAsync(int id, [FromBody] UpdatePatientDto pacienteDto)
        {
            if (pacienteDto == null)
                return BadRequest(new { message = "Datos de paciente inválidos." });

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
        [HttpDelete("{id}")]
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
        [HttpGet("get-dni")]
        public async Task<ActionResult<Paciente>> GetPatientByDni([FromQuery] string dni)
        {
            try
            {
                var patient = await _pacienteLogic.GetPatientForDNIAsync(dni);
                if (patient == null)
                    return NotFound(new { message = "Paciente no encontrado" });
                return Ok(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo paciente por DNI");
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("get-qty")]
        public async Task<ActionResult<int>> GetPatientsCount()
        {
            try
            {
                var qty = await _pacienteLogic.GetPatientsQtyAsync();
                return Ok(qty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cantidad de pacientes");
                return BadRequest(new { message = ex.Message });
            }
        }

        // ===========================
        // MÉTODOS PARA PACIENTE (SELF)
        // ===========================

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
    }
}