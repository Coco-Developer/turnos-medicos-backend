using ApiGestionTurnosMedicos.Validations;
using BusinessLogic.AppLogic;
using DataAccess.Data;
using DataAccess.Context;
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
        private readonly ILogger<EstadoController> _logger;
        private readonly GestionTurnosContext _context;

        public EstadoController(EstadoLogic estadoLogic, ILogger<EstadoController> logger, GestionTurnosContext context)
        {
            _estadoLogic = estadoLogic;
            _logger = logger;
            _context = context;
        }

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

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Post([FromBody] Estado estado)
        {
            try
            {
                // Si tienes un ValidationsMethodPost para Estado, deberías llamarlo aquí con await
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

        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Put(int id, [FromBody] Estado estado)
        {
            try
            {
                // Aplicamos la validación asíncrona usando await
                var validations = new ValidationsMethodPut(_context);
                var validationResult = await validations.ValidationMethodPutStatus(estado);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { message = validationResult.ErrorMessage });
                }

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