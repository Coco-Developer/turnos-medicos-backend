using ApiGestionTurnosMedicos.Validations;
using BusinessLogic.AppLogic;
using DataAccess.Context;
using DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.CustomModels;
using System.Diagnostics;

namespace ApiGestionTurnosMedicos.Controllers
{
    [Authorize]
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

        [HttpGet]
        public async Task<ActionResult<List<VwTurno>>> Get()
        {
            var sLogic = new TurnoLogic(_context);
            return Ok(await sLogic.ShiftList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VwTurno>> Get(int id)
        {
            var sLogic = new TurnoLogic(_context);
            var turno = await sLogic.GetShiftForId(id);
            if (turno == null)
                return NotFound(new { message = "Turno no encontrado." });
            return Ok(turno);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TurnoCustom oShift)
        {
            var validations = new ValidationsMethodPost(_context);

            // CORRECCIÓN: Se agrega 'await' para resolver la Task
            var validationResult = await validations.ValidationsMethodPostShift(oShift);

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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTurno(int id, [FromBody] TurnoCustom turnoUpdate)
        {
            if (turnoUpdate == null)
                return BadRequest(new { message = "Datos de turno inválidos." });

            var validations = new ValidationsMethodPut(_context);

            // CORRECCIÓN: Se agrega 'await'
            var validationResult = await validations.ValidationsMethodPutShift(turnoUpdate);

            if (!validationResult.IsValid)
                return BadRequest(new { message = validationResult.ErrorMessage });

            try
            {
                var sLogic = new TurnoLogic(_context);

                var shift = new Turno
                {
                    Fecha = DateTime.Parse(turnoUpdate.Fecha),
                    Hora = TimeSpan.Parse(turnoUpdate.Hora),
                    MedicoId = turnoUpdate.MedicoId,
                    PacienteId = turnoUpdate.PacienteId,
                    EstadoId = turnoUpdate.EstadoId,
                    Observaciones = turnoUpdate.Observaciones
                };

                await sLogic.UpdateShift(id, shift);

                _logger.LogInformation("Usuario {Usuario} modificó el turno {id} a las {FechaHora}",
                    User?.Identity?.Name ?? "Anónimo", id, DateTime.UtcNow);

                return Ok(new { message = "Turno actualizado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar turno");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("set-turno-status/{id}")]
        public async Task<IActionResult> UpdateTurnoStatus(int id, [FromQuery] int st, [FromQuery] int? o = null)
        {
            var oStatus = new Estado { Id = st };
            int src = o ?? 0;

            var validations = new ValidationsMethodPut(_context);

            // CORRECCIÓN: Se agrega 'await'
            var validationResult = await validations.ValidationMethodPutStatus(oStatus);

            if (!validationResult.IsValid)
                return BadRequest(new { message = validationResult.ErrorMessage });

            var sLogic = new TurnoLogic(_context);
            await sLogic.UpdateShiftStatus(id, oStatus.Id, src);

            return Ok(new { message = "Estado del turno actualizado correctamente." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var sLogic = new TurnoLogic(_context);
                await sLogic.DeleteShift(id);
                return Ok(new { message = "Turno eliminado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando turno");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("get-turnos-by-patient/{idPaciente}")]
        public async Task<ActionResult<List<VwTurno>>> GetListOfShiftsByPatientVw(int idPaciente)
        {
            var sLogic = new TurnoLogic(_context);
            return Ok(await sLogic.ListOfShiftsByPatientVw(idPaciente));
        }

        [HttpGet("get-turnos-of-doctor/{idMedico}")]
        public async Task<ActionResult<List<Turno>>> GetListOfShiftsByDoctor(int idMedico)
        {
            var sLogic = new TurnoLogic(_context);
            return Ok(await sLogic.ListOfShiftsByDoctor(idMedico));
        }

        [HttpGet("get-dashboard-data")]
        public async Task<ActionResult<Dictionary<string, object>>> GetDashboardData()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var sLogic = new TurnoLogic(_context);

                var taskQtyYr = sLogic.ListOfShiftQtyCurrentYear();
                var taskQtyMo = sLogic.ListOfShiftQtyCurrentMonth();
                var taskQtyDoc = sLogic.ListOfShiftByDoctorQtyCurrentYear();

                var qtyStatesYr = sLogic.ListOfShiftStateQtyCurrentYear();

                await Task.WhenAll(taskQtyYr, taskQtyMo, taskQtyDoc);

                var result = new Dictionary<string, object>
                {
                    { "qtyTurnosYr", await taskQtyYr },
                    { "qtyTurnosMo", await taskQtyMo },
                    { "qtyTurnosXMedico", await taskQtyDoc },
                    { "qtyStatesYr", qtyStatesYr }
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