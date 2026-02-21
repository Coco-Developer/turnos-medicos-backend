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
    public class EstadoController : ControllerBase
    {
        private readonly EstadoLogic _estadoLogic;
        private readonly ILogger<EstadoController> _logger;

        public EstadoController(GestionTurnosContext context, ILogger<EstadoController> logger)
        {
            _estadoLogic = new EstadoLogic(context);
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<List<Estado>> Get()
        {
            try
            {
                return Ok(_estadoLogic.GetAllEstados());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo lista de estados");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public ActionResult<Estado> Get(int id)
        {
            try
            {
                var estado = _estadoLogic.GetEstadoById(id);
                if (estado == null)
                    return NotFound(new { message = "Estado no encontrado" });

                return Ok(estado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estado ID {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Post([FromBody] Estado estado)
        {
            try
            {
                _estadoLogic.CreateEstado(estado);
                _logger.LogInformation("Estado creado: {Nombre}", estado.Nombre);
                return Ok(new { message = "Estado creado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando estado");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Put(int id, [FromBody] Estado estado)
        {
            try
            {
                _estadoLogic.UpdateEstado(id, estado);
                _logger.LogInformation("Estado ID {Id} actualizado", id);
                return Ok(new { message = "Estado actualizado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando estado ID {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            try
            {
                _estadoLogic.DeleteEstado(id);
                _logger.LogInformation("Estado ID {Id} eliminado", id);
                return Ok(new { message = "Estado eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando estado ID {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}