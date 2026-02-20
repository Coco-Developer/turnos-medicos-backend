using ApiGestionTurnosMedicos.Validations;
using BusinessLogic.AppLogic;
using DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.CustomModels;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Models.DTOs;

namespace ApiGestionTurnosMedicos.Controllers
{
    /// <summary>
    /// Controlador API para la gestión de pacientes en el sistema de turnos médicos
    /// </summary>
    [Authorize]
    //[Route("/[controller]")]
    [Route("[controller]")]
    [ApiController]
    public class PacienteController : ControllerBase
    {
        private readonly PacienteLogic _pacienteLogic;
        private readonly GestionTurnosContext _context;
        private readonly ILogger<PacienteController> _logger;

        // Inyectamos PacienteLogic y el contexto de la base de datos
        public PacienteController(PacienteLogic pacienteLogic, GestionTurnosContext context, ILogger<PacienteController> logger)
        {
            _pacienteLogic = pacienteLogic;
            _context = context;
            /*
             Todo el código para generar el LOG queda comentado ya que se
             solucionó el inconveniente (derivado de IIS). Todo lo de LOG queda
             como referencia por las dudas.
             Se rehabilitó para obtener métricas y depurar errores.
             */
            _logger = logger;
        }

        /// <summary>
        /// Devuelve la lista de todos los pacientes (Solo para administradores)
        /// </summary>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        public async Task<ActionResult<List<Paciente>>> GetAllPatients()
        {
            //_logger.LogInformation("Entrando al método GET /Paciente/");
            try
            {
                var patients = await _pacienteLogic.PatientsListAsync();
                return Ok(patients);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un paciente por ID (Solo para administradores)
        /// </summary>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Paciente>> GetPatientById(int id)
        {
            //_logger.LogInformation("Entrando al método GET /Paciente/{id}", id);
            try
            {
                var patient = await _pacienteLogic.GetPatientForIdAsync(id);
                return Ok(patient);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo paciente (Solo Admin)
        /// </summary>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDto dto)
        {
            //_logger.LogInformation("Entrando al método POST /Paciente/");

            var validations = new ValidationsMethodPost(_context);

            // Mapear el DTO a entidad Paciente
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
            {
                //_logger.LogWarning("Datos inválidos");
                return BadRequest(new { errorMessage = result.ErrorMessage });
            }

            try
            {
                // Pasamos el password al método que maneja la creación del paciente y el usuario
                await _pacienteLogic.CreatePatientWithUserAsync(oPatient, dto.Password);
                //_logger.LogInformation("Paciente creado correctamente");
                return Ok(new { message = "Paciente creado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando paciente");
                return BadRequest(new { message = ex.Message });
            }
        }


        /// <summary>
        /// Actualiza los datos de un paciente (Solo Admin)
        /// </summary>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatientAsync(int id, [FromBody] UpdatePatientDto pacienteDto)
        {
            //_logger.LogInformation("Entrando al método PUT /Paciente/{id}", id);
            // Validar entrada
            if (pacienteDto == null) 
            {
                //_logger.LogWarning("pacienteDto es nulo");
                return BadRequest("Datos de paciente inválidos.");
            }

            try
            {
                await _pacienteLogic.UpdatePatientAsync(id, pacienteDto);
                //_logger.LogInformation("Paciente actualizado correctamente");
                return Ok(new { message = "Paciente actualizado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando paciente");
                return BadRequest(new { message = ex.Message });
            }
        }


        /// <summary>
        /// Elimina un paciente por ID (Solo Admin)
        /// </summary>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            //_logger.LogInformation("Entrando al método DELETE /Paciente/{id}", id);
            try
            {
                await _pacienteLogic.DeletePatientAsync(id);
                //_logger.LogInformation("Registro de Paciente eliminado correctamente");
                return Ok(new { message = "Paciente eliminado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando registro de paciente");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un paciente por su número de DNI (Solo Admin)
        /// </summary>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("get-dni")]
        public async Task<ActionResult<Paciente>> GetPatientByDni([FromQuery] string dni)
        {
            try
            {
                var patient = await _pacienteLogic.GetPatientForDNIAsync(dni);
                return Ok(patient);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Devuelve la cantidad total de pacientes registrados (Solo Admin)
        /// </summary>
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
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Devuelve los datos del paciente autenticado (Rol: Paciente)
        /// </summary>
        [Authorize(Roles = UserRoles.Paciente)]
        [HttpGet("my-profile")]
        public async Task<ActionResult<Paciente>> GetMyProfile()
        {
            try
            {
                // Obtener el DNI desde el Claim
                var dni = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(dni))
                    return Unauthorized("No se pudo identificar al usuario.");

                var paciente = await _pacienteLogic.GetPatientForDNIAsync(dni);
                return Ok(paciente);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


    }
}
