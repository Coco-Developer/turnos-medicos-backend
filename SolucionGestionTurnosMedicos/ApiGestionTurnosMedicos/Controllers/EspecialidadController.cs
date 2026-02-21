using BusinessLogic.AppLogic;
using DataAccess.Context;
using DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGestionTurnosMedicos.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class EspecialidadController : ControllerBase
    {
        private readonly EspecialidadLogic _eLogic;
        private readonly ILogger<EspecialidadController> _logger;

        public EspecialidadController(GestionTurnosContext context, ILogger<EspecialidadController> logger)
        {
            _eLogic = new EspecialidadLogic(context);
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<List<Especialidad>> Get()
        {
            try
            {
                return Ok(_eLogic.SpecialtyList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo lista de especialidades");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public ActionResult<Especialidad> Get(int id)
        {
            try
            {
                var especialidad = _eLogic.GetSpecialtyForId(id);
                if (especialidad == null)
                    return NotFound(new { message = "Especialidad no encontrada" });

                return Ok(especialidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo especialidad ID {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Post([FromBody] Especialidad oEspecialidad)
        {
            try
            {
                _eLogic.CreateSpecialty(oEspecialidad);
                _logger.LogInformation("Especialidad creada: {Nombre}", oEspecialidad.Nombre);
                return Ok(new { message = "Especialidad creada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando especialidad");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Put(int id, [FromBody] Especialidad oEspecialidad)
        {
            try
            {
                _eLogic.UpdateSpecialty(id, oEspecialidad);
                _logger.LogInformation("Especialidad ID {Id} actualizada", id);
                return Ok(new { message = "Especialidad actualizada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando especialidad ID {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            try
            {
                _eLogic.DeleteSpecialty(id);
                _logger.LogInformation("Especialidad ID {Id} eliminada", id);
                return Ok(new { message = "Especialidad eliminada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando especialidad ID {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("list-covered-specialty")]
        public ActionResult<List<Especialidad>> GetListCoveredSpecialty()
        {
            try
            {
                return Ok(_eLogic.CoveredSpecialtyList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo especialidades cubiertas");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}