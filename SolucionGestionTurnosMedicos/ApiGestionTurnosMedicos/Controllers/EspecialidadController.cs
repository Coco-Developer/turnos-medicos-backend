using BusinessLogic.AppLogic;
using DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGestionTurnosMedicos.Controllers
{
    /// <summary>
    /// Controlador API para la gestión de especialidades médicas
    /// </summary>
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class EspecialidadController : ControllerBase
    {
        #region ContextDataBase
        private readonly EspecialidadLogic _eLogic;

        /// <summary>
        /// Constructor del controlador de especialidades
        /// </summary>
        /// <param name="context">Contexto de la base de datos para operaciones CRUD</param>
        public EspecialidadController(GestionTurnosContext context)
        {
            _eLogic = new EspecialidadLogic(context);
        }
        #endregion

        /// <summary>
        /// Obtiene la lista completa de especialidades
        /// </summary>
        /// <returns>Lista de objetos Especialidad</returns>
        [HttpGet]
        public List<Especialidad> Get()
        {
            return _eLogic.SpecialtyList();
        }

        /// <summary>
        /// Obtiene una especialidad específica por su identificador
        /// </summary>
        /// <param name="id">Identificador único de la especialidad</param>
        /// <returns>Objeto Especialidad correspondiente al ID proporcionado</returns>
        [HttpGet("{id}")]
        public Especialidad Get(int id)
        {
            return _eLogic.GetSpecialtyForId(id);
        }

        /// <summary>
        /// Crea una nueva especialidad en el sistema
        /// </summary>
        /// <param name="oEspecialidad">Objeto Especialidad con los datos a crear</param>
        /// <returns>Resultado de la operación con estado HTTP</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Post([FromBody] Especialidad oEspecialidad)
        {
            _eLogic.CreateSpecialty(oEspecialidad);
            return Ok();
        }

        /// <summary>
        /// Actualiza una especialidad existente
        /// </summary>
        /// <param name="id">Identificador de la especialidad a actualizar</param>
        /// <param name="oEspecialidad">Objeto Especialidad con los datos actualizados</param>
        /// <returns>Resultado de la operación con estado HTTP</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Put(int id, [FromBody] Especialidad oEspecialidad)
        {
            _eLogic.UpdateSpecialty(id, oEspecialidad);
            return Ok();
        }

        /// <summary>
        /// Elimina una especialidad del sistema
        /// </summary>
        /// <param name="id">Identificador de la especialidad a eliminar</param>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            _eLogic.DeleteSpecialty(id);
            return Ok();
        }

        /// <summary>
        /// Obtiene la lista de especialidades cubiertas
        /// </summary>
        /// <returns>Lista de objetos Especialidad que están cubiertas</returns>
        [HttpGet("list-covered-specialty")]
        public List<Especialidad> GetListCoveredSpecialty()
        {
            return _eLogic.CoveredSpecialtyList();
        }
    }
}
