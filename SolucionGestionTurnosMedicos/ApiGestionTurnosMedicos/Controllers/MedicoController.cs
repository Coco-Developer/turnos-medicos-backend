using BusinessLogic.AppLogic;
using Microsoft.AspNetCore.Mvc;
using Models.CustomModels;
using ApiGestionTurnosMedicos.Validations;
using Microsoft.AspNetCore.Authorization;

namespace ApiGestionTurnosMedicos.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class MedicoController : ControllerBase
    {
        private readonly ILogger<MedicoController> _logger;
        private readonly MedicoLogic _medicoLogic;
        private readonly ValidationsMethodPost _validationsPost;
        private readonly ValidationsMethodPut _validationsPut;

        public MedicoController(
            MedicoLogic medicoLogic,
            ValidationsMethodPost validationsPost,
            ValidationsMethodPut validationsPut,
            ILogger<MedicoController> logger)
        {
            _medicoLogic = medicoLogic;
            _validationsPost = validationsPost;
            _validationsPut = validationsPut;
            _logger = logger;
        }

        #region GET ALL

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var doctors = await _medicoLogic.DoctorList();
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando médicos");
                return StatusCode(500, "Error interno al listar médicos");
            }
        }

        #endregion

        #region GET BY ID

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Medico")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var medico = await _medicoLogic.GetDoctorForId(id);

                if (medico == null)
                    return NotFound(new { message = "Médico no encontrado" });

                return Ok(medico);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo médico {Id}", id);
                return StatusCode(500, "Error interno");
            }
        }

        #endregion

        #region POST

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Post([FromBody] MedicoCustom dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Cuerpo inválido" });

            var validation = await _validationsPost.ValidateDoctorAsync(dto);

            if (!validation.IsValid)
                return BadRequest(new { message = validation.ErrorMessage });

            try
            {
                await _medicoLogic.CreateDoctor(dto);
                return Ok(new { message = "Médico creado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando médico");
                return StatusCode(500, new
                {
                    error = ex.Message,
                    inner = ex.InnerException?.Message,
                    stack = ex.StackTrace
                });
            }
        }

        #endregion

        #region PUT

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Medico")]
        public async Task<IActionResult> Put(int id, [FromBody] MedicoCustom dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Datos inválidos" });

            var validation = await _validationsPut.ValidateDoctorAsync(dto);

            if (!validation.IsValid)
                return BadRequest(new { message = validation.ErrorMessage });

            try
            {
                await _medicoLogic.UpdateDoctor(id, dto);
                return Ok(new { message = "Médico actualizado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando médico {Id}", id);
                return StatusCode(500, "Error interno al actualizar médico");
            }
        }

        #endregion

        #region DELETE

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _medicoLogic.DeleteDoctor(id);
                return Ok(new { message = "Médico eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando médico {Id}", id);
                return StatusCode(500, "Error interno al eliminar");
            }
        }

        #endregion

        #region EXTRA ENDPOINTS

        [HttpGet("get-all-doctors")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllDoctorsCustom()
        {
            try
            {
                // Este es el método que devuelve MedicoCustom (con la especialidad en texto)
                var doctors = await _medicoLogic.ReturnAllDoctorsWithOurSpecialty();
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en get-all-doctors");
                return StatusCode(500, "Error interno");
            }
        }

        [HttpGet("list-for-specialty/{id:int}")]
        public async Task<IActionResult> GetListDoctorForSpecialty(int id)
        {
            try
            {
                var doctors = await _medicoLogic.FindDoctorForSpecialty(id);

                if (!doctors.Any())
                    return NotFound(new { message = "No se encontraron médicos para la especialidad" });

                return Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buscando médicos por especialidad");
                return StatusCode(500, "Error interno");
            }
        }

        [HttpGet("get-qty")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDoctorsQty()
        {
            try
            {
                var qty = await _medicoLogic.GetDoctorsQty();
                return Ok(qty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo cantidad");
                return StatusCode(500, "Error interno");
            }
        }

        [HttpGet("get-schedule/{id:int}")]
        public async Task<IActionResult> GetScheduleForDoctor(int id)
        {
            try
            {
                var schedule = await _medicoLogic.GetScheduleForDoctor(id);

                if (!schedule.Any())
                    return NotFound(new { message = "No se encontraron horarios" });

                return Ok(schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo horario médico");
                return StatusCode(500, "Error interno");
            }
        }

        [HttpGet("get-full-schedule/{id:int}")]
        public async Task<IActionResult> GetFullScheduleForDoctor(int id)
        {
            try
            {
                var fechas = await _medicoLogic.GetFullScheduleForDoctor(id);

                var result = fechas
                    .OrderBy(f => f)
                    .Select(f => f.ToString("yyyy-MM-dd"))
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculando agenda completa");
                return StatusCode(500, "Error interno");
            }
        }

        #endregion
    }
}