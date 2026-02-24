using BusinessLogic.AppLogic;
using DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using ApiGestionTurnosMedicos.CustomModels;
using ApiGestionTurnosMedicos.Validations;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using DataAccess.Context;

namespace ApiGestionTurnosMedicos.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class MedicoController : ControllerBase
    {
        private readonly ILogger<MedicoController> _logger;

        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        public MedicoController(GestionTurnosContext context, ILogger<MedicoController> logger)
        {
            _context = context;
            _logger = logger;
        }
        #endregion

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<List<Medico>> Get()
        {
            MedicoLogic dLogic = new(_context);
            return await dLogic.DoctorList();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Medico")]
        public async Task<Medico> Get(int id)
        {
            MedicoLogic dLogic = new(_context);
            return await dLogic.GetDoctorForId(id);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Post([FromBody] MedicoCustom oDoctor)
        {
            MedicoLogic dLogic = new(_context);
            ValidationsMethodPost validations = new(_context);

            // CORRECCIÓN: Se agrega 'await' porque el método ahora es asíncrono
            ValidationsMethodPost validationResult = await validations.ValidationsMethodPostDoctor(oDoctor);

            if (validationResult.IsValid == false) return BadRequest(new { validationResult.ErrorMessage });

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

            var horarios = oDoctor.Horarios.Select(h => new HorarioMedico
            {
                DiaSemana = h.DiaSemana,
                HorarioAtencionInicio = h.HorarioAtencionInicio,
                HorarioAtencionFin = h.HorarioAtencionFin
            }).ToList();

            await dLogic.CreateDoctor(medico, horarios);
            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Medico")]
        public async Task<IActionResult> Put(int id, [FromBody] MedicoCustom oDoctor)
        {
            ValidationsMethodPut validations = new ValidationsMethodPut(_context);

            // CORRECCIÓN: Se agrega 'await' para obtener el resultado de la validación
            ValidationsMethodPut validationResult = await validations.ValidationsMethodPutDoctor(oDoctor);

            if (validationResult.IsValid == false) return BadRequest(new { validationResult.ErrorMessage });

            MedicoLogic dLogic = new(_context);
            await dLogic.UpdateDoctor(id, oDoctor);

            _logger.LogInformation("El usuario {Usuario} modificó el Médico {id} a las {FechaHora}",
                User?.Identity?.Name ?? "Anónimo", id, DateTime.UtcNow);

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task Delete(int id)
        {
            MedicoLogic dLogic = new(_context);
            await dLogic.DeleteDoctor(id);
        }

        [HttpGet("list-for-specialty/{id}")]
        [Authorize]
        public async Task<IActionResult> GetListDoctorForSpecialty(int id)
        {
            try
            {
                MedicoLogic dLogic = new(_context);
                var doctors = await dLogic.FindDoctorForSpecialty(id);

                if (doctors == null || !doctors.Any())
                {
                    return NotFound($"No se encuentran médicos con la ID {id}");
                }

                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error recuperando médicos por especialidad: {ex.Message}");
            }
        }

        [HttpGet("get-all-doctors")]
        [Authorize(Roles = "Admin,Medico")]
        public async Task<List<MedicoCustom>> ReturnAllDoctorsWithOurSpecialty()
        {
            MedicoLogic dLogic = new(_context);
            return await dLogic.ReturnAllDoctorsWithOurSpecialty();
        }

        [HttpGet("get-qty")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<int>> GetDoctorsQtyi()
        {
            try
            {
                MedicoLogic pLogic = new(_context);
                return await pLogic.GetDoctorsQty();
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }

        [HttpGet("get-schedule/{id}")]
        public async Task<IActionResult> GetScheduleForDoctor(int id)
        {
            try
            {
                MedicoLogic dLogic = new(_context);
                var schedule = await dLogic.GetScheduleForDoctor(id);
                if (schedule == null || !schedule.Any())
                {
                    return NotFound($"No se encontraron horarios para el médico con ID {id}");
                }
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error recuperando horarios del médico: {ex.Message}");
            }
        }

        [HttpGet("get-full-schedule/{id}")]
        public async Task<IActionResult> GetFullScheduleForDoctor(int id)
        {
            try
            {
                MedicoLogic dLogic = new(_context);
                var fechasOcupadas = await dLogic.GetFullScheduleForDoctor(id);

                var fechasOcupadasStr = fechasOcupadas?
                    .OrderBy(f => f)
                    .Select(f => f.ToString("yyyy-MM-dd"))
                    .ToList() ?? new List<string>();

                return Ok(fechasOcupadasStr);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al calcular fechas ocupadas", detail = ex.Message });
            }
        }
    }
}