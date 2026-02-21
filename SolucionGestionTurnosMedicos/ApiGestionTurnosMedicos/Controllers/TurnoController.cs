using ApiGestionTurnosMedicos.Validations;
using BusinessLogic.AppLogic;
using DataAccess.Context;
using DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.CustomModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ApiGestionTurnosMedicos.Controllers
{
    /// <summary>
    /// Controlador API para la gestión de turnos médicos.
    /// </summary>
    [Authorize] // JWT obligatorio
    [Route("[controller]")]
    [ApiController]
    public class TurnoController : ControllerBase
    {
        private readonly ILogger<TurnoController> _logger;
        private readonly GestionTurnosContext _context;

        public TurnoController(GestionTurnosContext context, ILogger<TurnoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la lista completa de turnos.
        /// </summary>
        [HttpGet]
        public ActionResult<List<VwTurno>> Get()
        {
            var sLogic = new TurnoLogic(_context);
            return Ok(sLogic.ShiftList());
        }

        /// <summary>
        /// Obtiene un turno específico por ID.
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<VwTurno> Get(int id)
        {
            var sLogic = new TurnoLogic(_context);
            var turno = sLogic.GetShiftForId(id);
            if (turno == null)
                return NotFound(new { message = "Turno no encontrado." });
            return Ok(turno);
        }

        /// <summary>
        /// Crea un nuevo turno médico.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TurnoCustom oShift)
        {
            var validations = new ValidationsMethodPost(_context);
            var validationResult = validations.ValidationsMethodPostShift(oShift);
            if (!validationResult.IsValid)
                return BadRequest(new { message = validationResult.ErrorMessage });

            try
            {
                var sLogic = new TurnoLogic(_context);
                var shift = new Turno
                {
                    MedicoId = oShift.MedicoId,
                    PacienteId = oShift.PacienteId,
                    Fecha = new TurnoCustom().ModifyDate(oShift.Fecha),
                    Hora = new TurnoCustom().ModifyHour(oShift.Hora),
                    EstadoId = oShift.EstadoId,
                    Observaciones = oShift.Observaciones
                };

                await sLogic.CreateShift(shift);
                return Ok(new { message = "Turno creado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando turno");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un turno existente.
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult UpdateTurno(int id, [FromBody] TurnoCustom turnoUpdate)
        {
            if (turnoUpdate == null)
                return BadRequest(new { message = "Datos de turno inválidos." });

            var turno = _context.Turnos.Find(id);
            if (turno == null)
                return NotFound(new { message = "Turno no encontrado." });

            var medicoExistente = _context.Medicos.Find(turnoUpdate.MedicoId);
            if (medicoExistente == null)
                return BadRequest(new { message = "Médico no encontrado." });

            var validations = new ValidationsMethodPut(_context);
            var validationResult = validations.ValidationsMethodPutShift(turnoUpdate);
            if (!validationResult.IsValid)
                return BadRequest(new { message = validationResult.ErrorMessage });

            if (!DateTime.TryParse(turnoUpdate.Fecha, out DateTime fecha) ||
                !TimeSpan.TryParse(turnoUpdate.Hora, out TimeSpan hora))
            {
                return BadRequest(new { message = "Formato de fecha u hora inválido." });
            }

            // Actualizar campos
            turno.Fecha = fecha;
            turno.Hora = hora;
            turno.MedicoId = turnoUpdate.MedicoId;
            turno.PacienteId = turnoUpdate.PacienteId;
            turno.EstadoId = turnoUpdate.EstadoId;
            turno.Observaciones = turnoUpdate.Observaciones;

            _context.SaveChanges();

            _logger.LogInformation("Usuario {Usuario} modificó el turno {id} a las {FechaHora}",
                User?.Identity?.Name ?? "Anónimo", id, DateTime.UtcNow);

            return Ok(new { message = "Turno actualizado correctamente." });
        }

        /// <summary>
        /// Actualiza el estado de un turno específico.
        /// </summary>
        [HttpPut("set-turno-status/{id}")]
        public IActionResult UpdateTurnoStatus(int id, [FromQuery] int st, [FromQuery] int? o = null)
        {
            var oStatus = new Estado { Id = st };
            int src = o ?? 0;

            var validations = new ValidationsMethodPut(_context);
            var validationResult = validations.ValidationMethodPutStatus(oStatus);
            if (!validationResult.IsValid)
                return BadRequest(new { message = validationResult.ErrorMessage });

            var sLogic = new TurnoLogic(_context);
            sLogic.UpdateShiftStatus(id, oStatus.Id, src);

            return Ok(new { message = "Estado del turno actualizado correctamente." });
        }

        /// <summary>
        /// Elimina un turno.
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var sLogic = new TurnoLogic(_context);
                sLogic.DeleteShift(id);
                return Ok(new { message = "Turno eliminado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando turno");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene turnos de un paciente específico.
        /// </summary>
        [HttpGet("get-turnos-by-patient/{idPaciente}")]
        public ActionResult<List<VwTurno>> GetListOfShiftsByPatientVw(int idPaciente)
        {
            var sLogic = new TurnoLogic(_context);
            return Ok(sLogic.ListOfShiftsByPatientVw(idPaciente));
        }

        /// <summary>
        /// Obtiene turnos de un doctor específico.
        /// </summary>
        [HttpGet("get-turnos-of-doctor/{idMedico}")]
        public ActionResult<List<Turno>> GetListOfShiftsByDoctor(int idMedico)
        {
            var sLogic = new TurnoLogic(_context);
            return Ok(sLogic.ListOfShiftsByDoctor(idMedico));
        }

        /// <summary>
        /// Obtiene datos para dashboard.
        /// </summary>
        [HttpGet("get-dashboard-data")]
        public ActionResult<Dictionary<string, object>> GetDashboardData()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var sLogic = new TurnoLogic(_context);
                var result = new Dictionary<string, object>
                {
                    { "qtyTurnosYr", sLogic.ListOfShiftQtyCurrentYear() },
                    { "qtyTurnosMo", sLogic.ListOfShiftQtyCurrentMonth() },
                    { "qtyTurnosXMedico", sLogic.ListOfShiftByDoctorQtyCurrentYear() },
                    { "qtyStatesYr", sLogic.ListOfShiftStateQtyCurrentYear() }
                };

                stopwatch.Stop();
                _logger.LogInformation("Dashboard generado en {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo datos del dashboard");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}