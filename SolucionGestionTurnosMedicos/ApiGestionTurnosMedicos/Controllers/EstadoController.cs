using BusinessLogic.AppLogic;
using DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ApiGestionTurnosMedicos.Controllers
{
    /// <summary>
    /// Controlador API para la gestión de estados en el sistema de turnos médicos
    /// </summary>
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class EstadoController : ControllerBase
    {
        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        /// <summary>
        /// Constructor del controlador de estados
        /// </summary>
        /// <param name="context">Contexto de la base de datos para operaciones CRUD</param>
        public EstadoController(GestionTurnosContext context)
        {
            _context = context;
        }
        #endregion

        /// <summary>
        /// Obtiene la lista completa de estados disponibles
        /// </summary>
        /// <returns>Lista de objetos Estado</returns>
        [HttpGet]
        public List<Estado> Get()
        {
            EstadoLogic estadoLogic = new EstadoLogic(_context);
            return estadoLogic.GetAllEstados();
        }

        /// <summary>
        /// Obtiene un estado específico por su identificador
        /// </summary>
        /// <param name="id">Identificador único del estado</param>
        /// <returns>Objeto Estado correspondiente al ID proporcionado</returns>
        [HttpGet("{id}")]
        public Estado Get(int id)
        {
            EstadoLogic estadoLogic = new EstadoLogic(_context);
            return estadoLogic.GetEstadoById(id);
        }

        /// <summary>
        /// Crea un nuevo estado en el sistema
        /// </summary>
        /// <param name="estado">Objeto Estado con los datos a crear</param>
        /// <returns>Resultado de la operación con estado HTTP: Ok si es exitoso, BadRequest si falla</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Post([FromBody] Estado estado)
        {
            try
            {
                EstadoLogic estadoLogic = new EstadoLogic(_context);
                estadoLogic.CreateEstado(estado);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza un estado existente
        /// </summary>
        /// <param name="id">Identificador del estado a actualizar</param>
        /// <param name="estado">Objeto Estado con los datos actualizados</param>
        /// <returns>Resultado de la operación con estado HTTP: Ok si es exitoso, BadRequest si falla</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Put(int id, [FromBody] Estado estado)
        {
            try
            {
                EstadoLogic estadoLogic = new EstadoLogic(_context);
                estadoLogic.UpdateEstado(id, estado);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un estado del sistema
        /// </summary>
        /// <param name="id">Identificador del estado a eliminar</param>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id, EstadoLogic estadoLogic)
        {
            try
            {
                estadoLogic.DeleteEstado(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
