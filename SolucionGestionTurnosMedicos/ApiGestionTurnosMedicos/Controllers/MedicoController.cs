using BusinessLogic.AppLogic;
using DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using ApiGestionTurnosMedicos.CustomModels;
using ApiGestionTurnosMedicos.Validations;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace ApiGestionTurnosMedicos.Controllers
{
    /// <summary>
    /// Controlador API para la gestión de médicos.
    /// Provee endpoints para crear, leer, actualizar y eliminar médicos en el sistema.
    /// </summary>
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class MedicoController : ControllerBase
    {
        private readonly ILogger<MedicoController> _logger;


        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        /// <summary>
        /// Constructor del controlador que inicializa el contexto de la base de datos.
        /// </summary>
        /// <param name="context">Contexto de la base de datos para operaciones CRUD.</param>
        /// <param name="logger"></param>
        public MedicoController(GestionTurnosContext context, ILogger<MedicoController> logger)
        {
            _context = context;
            /* 
             Código para generar LOG de registro de actualización.
             */
            _logger = logger;
        }
        #endregion

        /// <summary>
        /// Obtiene la lista completa de médicos registrados. Solo accesible por Administradores.
        /// </summary>
        /// <returns>Lista de objetos <see cref="Medico"/> con información de los médicos.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")] // Solo accesible para usuarios con el rol 'Admin'
        public List<Medico> Get()
        {
            MedicoLogic dLogic = new(_context);
            return dLogic.DoctorList();
        }
        /// <summary>
        /// Obtiene un médico específico por su ID. Accesible para Administradores y Médicos.
        /// </summary>
        /// <param name="id">Identificador único del médico.</param>
        /// <returns>Objeto <see cref="Medico"/> con los detalles del médico solicitado.</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Medico")] // Accesible para Administradores y Médicos
        public Medico Get(int id)
        {
            MedicoLogic dLogic = new(_context);
            return dLogic.GetDoctorForId(id);
        }

        /// <summary>
        /// Crea un nuevo médico en el sistema. Solo accesible por Administradores.
        /// </summary>
        /// <param name="oDoctor">Objeto <see cref="MedicoCustom"/> con los datos del médico a crear.</param>
        /// <returns>Respuesta HTTP indicando éxito o fallo con mensaje correspondiente.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")] // Solo accesible para usuarios con el rol 'Admin'
        public IActionResult Post([FromBody] MedicoCustom oDoctor)
        {
            MedicoLogic dLogic = new(_context);
            MedicoCustom medicoCustom = new();
            ValidationsMethodPost validations = new(_context);
            ValidationsMethodPost validationResult = validations.ValidationsMethodPostDoctor(oDoctor);

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

            // Mapea la colección de horarios
            var horarios = oDoctor.Horarios.Select(h => new HorarioMedico
            {
                DiaSemana = h.DiaSemana,
                HorarioAtencionInicio = h.HorarioAtencionInicio,
                HorarioAtencionFin = h.HorarioAtencionFin
            }).ToList();

            // Guarda el médico y sus horarios
            dLogic.CreateDoctor(medico, horarios);
            return Ok();
        }
        /// <summary>
        /// Actualiza los datos de un médico existente. Accesible para Administradores y el propio médico.
        /// </summary>
        /// <param name="id">Identificador único del médico a actualizar.</param>
        /// <param name="oDoctor">Objeto <see cref="MedicoCustom"/> con los datos actualizados.</param>
        /// <returns>Respuesta HTTP indicando éxito o fallo con mensaje correspondiente.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Medico")] // Accesible para Administradores y el propio médico
        public IActionResult Put(int id, [FromBody] MedicoCustom oDoctor)
        {
            ValidationsMethodPut validations = new ValidationsMethodPut(_context);
            ValidationsMethodPut validationResult = validations.ValidationsMethodPutDoctor(oDoctor);

            if (validationResult.IsValid == false) return BadRequest(new { validationResult.ErrorMessage });

            MedicoLogic dLogic = new(_context);
            dLogic.UpdateDoctor(id, oDoctor);

            _logger.LogInformation("El usuario {Usuario} modificó el Médico {id} a las {FechaHora}", 
                User?.Identity?.Name ?? "Anónimo", id, DateTime.UtcNow);

            return Ok();
        }

        /// <summary>
        /// Elimina un médico específico del sistema. Solo accesible por Administradores.
        /// </summary>
        /// <param name="id">Identificador único del médico a eliminar.</param>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Solo accesible para usuarios con el rol 'Admin'
        public void Delete(int id)
        {
            MedicoLogic dLogic = new(_context);
            dLogic.DeleteDoctor(id);
        }

        /// <summary>
        /// Obtiene la lista de médicos asociados a una especialidad específica. Accesible por todos los usuarios autenticados.
        /// </summary>
        /// <param name="id">Identificador único de la especialidad.</param>
        /// <returns>Respuesta HTTP indicando éxito o fallo con mensaje correspondiente. El éxito adjunta lista de <see cref="Medico"/> con los médicos de la especialidad indicada.</returns>
        [HttpGet("list-for-specialty/{id}")]
        [Authorize] // Accesible por cualquier usuario autenticado
        public IActionResult GetListDoctorForSpecialty(int id)
        {
            try
            {
                MedicoLogic dLogic = new(_context);
                var doctors = dLogic.FindDoctorForSpecialty(id);

                if (doctors == null || !doctors.Any())
                {
                    return NotFound($"No se encuentran médicos con la ID {id}"); // 404 Not Found
                }

                return Ok(doctors); // 200 OK
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error recuperando médicos por especialidad: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene todos los médicos junto con información de su especialidad. Accesible por Administradores y Médicos.
        /// </summary>
        /// <returns>Lista de <see cref="MedicoCustom"/> con datos extendidos de los médicos.</returns>
        [HttpGet("get-all-doctors")]
        [Authorize(Roles = "Admin,Medico")] // Accesible para Administradores y Médicos
        public List<MedicoCustom> ReturnAllDoctorsWithOurSpecialty()
        {
            MedicoLogic dLogic = new(_context);
            return dLogic.ReturnAllDoctorsWithOurSpecialty();
        }

        /// <summary>
        /// Obtiene la cantidad total de médicos registrados en el sistema. Solo accesible por Administradores.
        /// </summary>
        /// <returns>Número entero representando la cantidad de médicos o un error en caso de fallo.</returns>
        [HttpGet("get-qty")]
        [Authorize(Roles = "Admin")] // Solo accesible para usuarios con el rol 'Admin'
        public ActionResult<int> GetDoctorsQtyi()
        {
            try
            {
                MedicoLogic pLogic = new(_context);
                return pLogic.GetDoctorsQty();

            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el horario semanal del médico
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("get-schedule/{id}")]
        public IActionResult GetScheduleForDoctor(int id)
        {
            try
            {
                MedicoLogic dLogic = new(_context);
                var schedule = dLogic.GetScheduleForDoctor(id);
                if (schedule == null || !schedule.Any())
                {
                    return NotFound($"No se encontraron horarios para el médico con ID {id}"); // 404 Not Found
                }
                return Ok(schedule); // 200 OK
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error recuperando horarios del médico: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene las fechas COMPLETAS del médico. Es decir, las fechas que 
        /// trabaja y NO tiene turnos disponibles.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("get-full-schedule/{id}")]
        public IActionResult GetFullScheduleForDoctor(int id)
        {
            try
            {
                MedicoLogic dLogic = new(_context);

                var fechasOcupadas = dLogic.GetFullScheduleForDoctor(id);

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
