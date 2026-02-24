using ApiGestionTurnosMedicos.Validations;
using BusinessLogic.AppLogic;
using DataAccess.Context;
using DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.CustomModels;

namespace ApiGestionTurnosMedicos.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class EspecialidadController : ControllerBase
    {
        private readonly EspecialidadLogic _eLogic;
        private readonly ILogger<EspecialidadController> _logger;
        private readonly GestionTurnosContext _context;

        public EspecialidadController(EspecialidadLogic eLogic, ILogger<EspecialidadController> logger, GestionTurnosContext context)
        {
            _eLogic = eLogic;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Especialidad>>> Get()
        {
            try
            {
                var list = await _eLogic.SpecialtyListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo lista de especialidades");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Especialidad>> Get(int id)
        {
            try
            {
                var especialidad = await _eLogic.GetSpecialtyForIdAsync(id);
                return Ok(especialidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo especialidad ID {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Post([FromBody] Especialidad oEspecialidad)
        {
            try
            {
                // Ejemplo de cómo integrar validaciones si las tuvieras para Especialidad:
                // var validations = new ValidationsMethodPost(_context);
                // var validationResult = await validations.ValidationsMethodPostSpecialty(oEspecialidad);
                // if (!validationResult.IsValid) return BadRequest(new { message = validationResult.ErrorMessage });

                await _eLogic.CreateSpecialtyAsync(oEspecialidad);
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
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Put(int id, [FromBody] Especialidad oEspecialidad)
        {
            try
            {
                await _eLogic.UpdateSpecialtyAsync(id, oEspecialidad);
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
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _eLogic.DeleteSpecialtyAsync(id);
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
        public async Task<ActionResult<List<Especialidad>>> GetListCoveredSpecialty()
        {
            try
            {
                var list = await _eLogic.CoveredSpecialtyListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo especialidades cubiertas");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}