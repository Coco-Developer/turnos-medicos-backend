using BusinessLogic.AppLogic;
using DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Models.CustomModels;
using ApiGestionTurnosMedicos.Validations;
using Microsoft.AspNetCore.Authorization;
using DataAccess.Context;

namespace ApiGestionTurnosMedicos.Controllers
{
    [Authorize]
    [Route("[controller]")] // Eliminado el "/" inicial, [controller] es suficiente
    [ApiController]
    public class MedicoController : ControllerBase
    {
        private readonly ILogger<MedicoController> _logger;
        private readonly GestionTurnosContext _context;

        public MedicoController(GestionTurnosContext context, ILogger<MedicoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<Medico>>> Get()
        {
            try
            {
                MedicoLogic dLogic = new(_context);
                return Ok(await dLogic.DoctorList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Get Doctors");
                return StatusCode(500, "Error interno al listar médicos");
            }
        }

        // REGLA: Usamos :int para evitar que rutas de texto caigan aquí
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Medico")]
        public async Task<ActionResult<Medico>> Get(int id)
        {
            try
            {
                MedicoLogic dLogic = new(_context);
                var medico = await dLogic.GetDoctorForId(id);
                if (medico == null) return NotFound(new { message = "Médico no encontrado" });
                return Ok(medico);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Post([FromBody] MedicoCustom oDoctor)
        {
            if (oDoctor == null) return BadRequest(new { message = "Cuerpo de médico inválido" });

            MedicoLogic dLogic = new(_context);
            ValidationsMethodPost validations = new(_context);

            var validationResult = await validations.ValidationsMethodPostDoctor(oDoctor);
            if (!validationResult.IsValid) return BadRequest(new { message = validationResult.ErrorMessage });

            try
            {
                Medico medico = new()
                {
                    Nombre = oDoctor.Nombre,
                    Apellido = oDoctor.Apellido,
                    EspecialidadId = oDoctor.EspecialidadId,
                    FechaAltaLaboral = oDoctor.FechaAltaLaboral,
                    Direccion = oDoctor.Direccion,
                    Dni = oDoctor.Dni,
                    Telefono = oDoctor.Telefono,
                    Matricula = oDoctor.Matricula,
                    Foto = string.IsNullOrEmpty(oDoctor.Foto) ? null : Convert.FromBase64String(oDoctor.Foto)
                };

                var horarios = oDoctor.Horarios?.Select(h => new HorarioMedico
                {
                    DiaSemana = h.DiaSemana,
                    HorarioAtencionInicio = h.HorarioAtencionInicio,
                    HorarioAtencionFin = h.HorarioAtencionFin
                }).ToList() ?? new List<HorarioMedico>();

                await dLogic.CreateDoctor(medico, horarios);
                return Ok(new { message = "Médico creado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear médico");
                return StatusCode(500, new { message = "Error al guardar en base de datos", detail = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Medico")]
        public async Task<IActionResult> Put(int id, [FromBody] MedicoCustom oDoctor)
        {
            if (oDoctor == null) return BadRequest(new { message = "Datos de actualización inválidos" });

            ValidationsMethodPut validations = new ValidationsMethodPut(_context);
            var validationResult = await validations.ValidationsMethodPutDoctor(oDoctor);

            if (!validationResult.IsValid) return BadRequest(new { message = validationResult.ErrorMessage });

            try
            {
                MedicoLogic dLogic = new(_context);
                await dLogic.UpdateDoctor(id, oDoctor);

                _logger.LogInformation("Médico {id} modificado por {Usuario}", id, User?.Identity?.Name ?? "Anónimo");
                return Ok(new { message = "Médico actualizado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                MedicoLogic dLogic = new(_context);
                await dLogic.DeleteDoctor(id);
                return Ok(new { message = "Médico eliminado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al eliminar");
            }
        }

        [HttpGet("list-for-specialty/{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetListDoctorForSpecialty(int id)
        {
            try
            {
                MedicoLogic dLogic = new(_context);
                var doctors = await dLogic.FindDoctorForSpecialty(id);
                if (doctors == null || !doctors.Any()) return NotFound($"No se encuentran médicos con la especialidad {id}");
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("get-all-doctors")]
        [Authorize(Roles = "Admin,Medico")]
        public async Task<ActionResult<List<MedicoCustom>>> ReturnAllDoctorsWithOurSpecialty()
        {
            MedicoLogic dLogic = new(_context);
            return Ok(await dLogic.ReturnAllDoctorsWithOurSpecialty());
        }

        [HttpGet("get-qty")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<int>> GetDoctorsQtyi()
        {
            try
            {
                MedicoLogic pLogic = new(_context);
                return Ok(await pLogic.GetDoctorsQty());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ex.Message });
            }
        }

        [HttpGet("get-schedule/{id:int}")]
        public async Task<IActionResult> GetScheduleForDoctor(int id)
        {
            try
            {
                MedicoLogic dLogic = new(_context);
                var schedule = await dLogic.GetScheduleForDoctor(id);
                if (schedule == null || !schedule.Any()) return NotFound("No se encontraron horarios");
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get-full-schedule/{id:int}")]
        public async Task<IActionResult> GetFullScheduleForDoctor(int id)
        {
            try
            {
                MedicoLogic dLogic = new(_context);
                var fechasOcupadas = await dLogic.GetFullScheduleForDoctor(id);
                var fechasOcupadasStr = fechasOcupadas?.OrderBy(f => f).Select(f => f.ToString("yyyy-MM-dd")).ToList() ?? new List<string>();
                return Ok(fechasOcupadasStr);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al calcular fechas ocupadas", detail = ex.Message });
            }
        }
    }
}