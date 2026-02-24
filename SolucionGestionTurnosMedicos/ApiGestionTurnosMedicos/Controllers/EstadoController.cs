using ApiGestionTurnosMedicos.Validations;
using BusinessLogic.AppLogic;
using DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.CustomModels;

namespace ApiGestionTurnosMedicos.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class EstadoController : ControllerBase
    {
        private readonly EstadoLogic _estadoLogic;
        private readonly ValidationsMethodPut _validations;
        private readonly ILogger<EstadoController> _logger;

        public EstadoController(
            EstadoLogic estadoLogic,
            ValidationsMethodPut validations,
            ILogger<EstadoController> logger)
        {
            _estadoLogic = estadoLogic;
            _validations = validations;
            _logger = logger;
        }

        // ================= GET ALL =================

        [HttpGet]
        public async Task<ActionResult<List<Estado>>> Get()
        {
            try
            {
                var estados = await _estadoLogic.GetAllEstadosAsync();
                return Ok(estados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo lista de estados");
                return BadRequest(new { message = ex.Message });
            }
        }

        // ================= GET BY ID =================

        [HttpGet("{id}")]
        public async Task<ActionResult<Estado>> Get(int id)
        {
            try
            {
                var estado = await _estadoLogic.GetEstadoByIdAsync(id);
                return Ok(estado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estado ID {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        // ================= CREATE =================

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Post([FromBody] Estado estado)
        {
            try
            {
                await _estadoLogic.CreateEstadoAsync(estado);

                _logger.LogInformation("Estado creado: {Nombre}", estado.Nombre);

                return Ok(new { message = "Estado creado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando estado");
                return BadRequest(new { message = ex.Message });
            }
        }

        // ================= UPDATE =================

        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Put(int id, [FromBody] Estado estado)
        {
            try
            {
                var validationResult = await _validations.ValidateStatusAsync(estado);

                if (!validationResult.IsValid)
                    return BadRequest(new { message = validationResult.ErrorMessage });

                await _estadoLogic.UpdateEstadoAsync(id, estado);

                _logger.LogInformation("Estado ID {Id} actualizado", id);

                return Ok(new { message = "Estado actualizado correctamente" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando estado ID {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        // ================= DELETE =================

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _estadoLogic.DeleteEstadoAsync(id);

                _logger.LogInformation("Estado ID {Id} eliminado", id);

                return Ok(new { message = "Estado eliminado correctamente" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando estado ID {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}