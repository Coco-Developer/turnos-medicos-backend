using ApiGestionTurnosMedicos.Validations;
using BusinessLogic.AppLogic;
using DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.CustomModels;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace ApiGestionTurnosMedicos.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class TurnoController : ControllerBase
    {
        private readonly ILogger<TurnoController> _logger;
        private readonly TurnoLogic _turnoLogic;
        private readonly ValidationsMethodPut _validationsPut;

        public TurnoController(
            TurnoLogic turnoLogic,
            ValidationsMethodPut validationsPut,
            ILogger<TurnoController> logger)
        {
            _turnoLogic = turnoLogic;
            _validationsPut = validationsPut;
            _logger = logger;
        }

        // ---------------- GET ALL ----------------

        [HttpGet]
        public async Task<ActionResult<List<VwTurno>>> Get()
        {
            return Ok(await _turnoLogic.ShiftList());
        }

        // ---------------- GET BY ID ----------------

        [HttpGet("{id}")]
        public async Task<ActionResult<VwTurno>> Get(int id)
        {
            var turno = await _turnoLogic.GetShiftForId(id);

            if (turno == null)
                return NotFound(new { message = "Turno no encontrado." });

            return Ok(turno);
        }

        // ---------------- CREATE ----------------

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TurnoCustom oShift)
        {
            if (oShift == null)
                return BadRequest(new { message = "Datos de turno inválidos." });

            try
            {
                var shift = new Turno
                {
                    MedicoId = oShift.MedicoId,
                    PacienteId = oShift.PacienteId,
                    Fecha = new TurnoCustom().ModifyDate(oShift.Fecha),
                    Hora = new TurnoCustom().ModifyHour(oShift.Hora),
                    EstadoId = oShift.EstadoId,
                    Observaciones = oShift.Observaciones
                };

                await _turnoLogic.CreateShift(shift);

                return Ok(new { message = "Turno creado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando turno");
                return BadRequest(new { message = ex.Message });
            }
        }

        // ---------------- UPDATE ----------------

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTurno(int id, [FromBody] TurnoCustom dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Datos de turno inválidos." });

            var validation = await _validationsPut.ValidateShiftAsync(dto);

            if (!validation.IsValid)
                return BadRequest(new { message = validation.ErrorMessage });

            try
            {
                var shift = new Turno
                {
                    Fecha = DateTime.Parse(dto.Fecha),
                    Hora = TimeSpan.Parse(dto.Hora),
                    MedicoId = dto.MedicoId,
                    PacienteId = dto.PacienteId,
                    EstadoId = dto.EstadoId,
                    Observaciones = dto.Observaciones
                };

                await _turnoLogic.UpdateShift(id, shift);

                return Ok(new { message = "Turno actualizado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando turno");
                return StatusCode(500, "Error interno");
            }
        }

        // ---------------- UPDATE STATUS ----------------

        [HttpPut("set-turno-status/{id:int}")]
        public async Task<IActionResult> UpdateTurnoStatus(int id, [FromQuery] int st, [FromQuery] int? o = null)
        {
            var status = new Estado { Id = st };

            var validation = await _validationsPut.ValidateStatusAsync(status);

            if (!validation.IsValid)
                return BadRequest(new { message = validation.ErrorMessage });

            try
            {
                await _turnoLogic.UpdateShiftStatus(id, st, o ?? 0);

                return Ok(new { message = "Estado actualizado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando estado");
                return StatusCode(500, "Error interno");
            }
        }

        // ---------------- DELETE ----------------

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _turnoLogic.DeleteShift(id);
                return Ok(new { message = "Turno eliminado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando turno");
                return BadRequest(new { message = ex.Message });
            }
        }

        // ---------------- LIST BY PATIENT ----------------

        [HttpGet("get-turnos-by-patient/{idPaciente}")]
        public async Task<ActionResult<List<VwTurno>>> GetListOfShiftsByPatientVw(int idPaciente)
        {
            return Ok(await _turnoLogic.ListOfShiftsByPatientVw(idPaciente));
        }

        // ---------------- LIST BY DOCTOR ----------------

        [HttpGet("get-turnos-of-doctor/{idMedico}")]
        public async Task<ActionResult<List<Turno>>> GetListOfShiftsByDoctor(int idMedico)
        {
            return Ok(await _turnoLogic.ListOfShiftsByDoctor(idMedico));
        }

        // ---------------- DASHBOARD ----------------

        [HttpGet("get-dashboard-data")]
        public async Task<ActionResult<Dictionary<string, object>>> GetDashboardData()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var taskQtyYr = _turnoLogic.ListOfShiftQtyCurrentYear();
                var taskQtyMo = _turnoLogic.ListOfShiftQtyCurrentMonth();
                var taskQtyDoc = _turnoLogic.ListOfShiftByDoctorQtyCurrentYear();
                var qtyStatesYr = _turnoLogic.ListOfShiftStateQtyCurrentYear();

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