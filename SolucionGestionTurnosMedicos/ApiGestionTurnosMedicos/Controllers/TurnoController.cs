using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using ApiGestionTurnosMedicos.CustomModels;
using ApiGestionTurnosMedicos.Services;
using ApiGestionTurnosMedicos.Validations;
using BusinessLogic.AppLogic;
using BusinessLogic.AppLogic.Services;
using DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.CustomModels;

namespace ApiGestionTurnosMedicos.Controllers
{
    /// <summary>
    /// Controlador API para la gestión de turnos médicos.
    /// Provee endpoints para crear, leer, actualizar y eliminar turnos.
    /// </summary>
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class TurnoController : ControllerBase
    {
        private readonly ILogger<TurnoController> _logger;

        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        /// <summary>
        /// Constructor del controlador que inicializa el contexto de la base de datos.
        /// </summary>
        /// <param name="context">Contexto de la base de datos para operaciones CRUD.</param>
        /// <param name="logger"></param>
        public TurnoController(GestionTurnosContext context, ILogger<TurnoController> logger)
        {
            _context = context;
            /* 
             Código para generar LOG de métricas y registro de actualización.
             */
            _logger = logger;
        }
        #endregion

        /// <summary>
        /// Obtiene la lista completa de turnos cargados.
        /// </summary>
        /// <returns>Lista de objetos <see cref="VwTurno"/> con información de turnos.</returns>
        [HttpGet]
        public List<VwTurno> Get()
        {
            TurnoLogic sLogic = new (_context);
            return sLogic.ShiftList();   
        }

        /// <summary>
        /// Obtiene un turno específico por su ID.
        /// </summary>
        /// <param name="id">Identificador único del turno.</param>
        /// <returns>Objeto <see cref="VwTurno"/> con los detalles del turno solicitado.</returns>
        [HttpGet("{id}")]
        public VwTurno Get(int id)
        {
            TurnoLogic sLogic = new (_context);
            return sLogic.GetShiftForId(id);
        }

        /// <summary>
        /// Crea un nuevo turno médico en el sistema.
        /// </summary>
        /// <param name="oShift">Objeto <see cref="TurnoCustom"/> con los datos del turno a crear.</param>
        /// <returns>Respuesta HTTP indicando éxito o fallo con mensaje correspondiente.</returns>
        [HttpPost]
        //public IActionResult Post([FromBody] TurnoCustom oShift)
        public async Task<IActionResult> Post([FromBody] TurnoCustom oShift)
        {
            const int EstadoActivo = 1;

            ValidationsMethodPost validations = new (_context);
            ValidationsMethodPost validationResult = validations.ValidationsMethodPostShift(oShift);

            if (validationResult.IsValid == false) return BadRequest(new { Message = validationResult.ErrorMessage });

            try
            {
                TurnoLogic sLogic = new (_context);
                TurnoCustom shiftCustom = new ();
                Turno shift = new ();

                shift.MedicoId = oShift.MedicoId;
                shift.PacienteId = oShift.PacienteId;
                shift.Fecha = shiftCustom.ModifyDate(oShift.Fecha);
                shift.Hora = shiftCustom.ModifyHour(oShift.Hora);
                shift.EstadoId = oShift.EstadoId; //EstadoActivo;
                shift.Observaciones = oShift.Observaciones;

                try {
                    await sLogic.CreateShift(shift);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }

                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza un turno existente en el sistema.
        /// </summary>
        /// <param name="id">Identificador único del turno a actualizar.</param>
        /// <param name="turnoUpdate">Objeto <see cref="TurnoCustom"/> con los datos actualizados.</param>
        /// <returns>Respuesta HTTP indicando éxito o fallo con mensaje correspondiente.</returns>
        [HttpPut("{id}")]
        public IActionResult UpdateTurno(int id, [FromBody] TurnoCustom turnoUpdate)
        {
            if (turnoUpdate == null)
            {
                return BadRequest(new { Message = "Los datos del turno no pueden ser nulos." });
            }

            // Verificar si el turno existe
            var turno = _context.Turnos.Find(id);
            if (turno == null)
            {
                return NotFound(new { Message = "Turno no encontrado." });
            }

            // Verificar si el médico existe
            var medicoExistente = _context.Medicos.Find(turnoUpdate.MedicoId);
            if (medicoExistente == null)
            {
                return BadRequest(new { Message = "Médico no encontrado." });
            }

            // Validar datos de entrada
            var validations = new ValidationsMethodPut(_context);
            var validationResult = validations.ValidationsMethodPutShift(turnoUpdate);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Message = validationResult.ErrorMessage });
            }

            // Intentar convertir Fecha y Hora
            if (!DateTime.TryParse(turnoUpdate.Fecha, out DateTime fecha) ||
                !TimeSpan.TryParse(turnoUpdate.Hora, out TimeSpan hora))
            {
                return BadRequest(new { Message = "Formato de fecha u hora inválido." });
            }

            // Actualizar campos del turno
            turno.Fecha = fecha;
            turno.Hora = hora;
            turno.MedicoId = turnoUpdate.MedicoId;
            turno.PacienteId = turnoUpdate.PacienteId;
            turno.EstadoId = turnoUpdate.EstadoId;
            turno.Observaciones = turnoUpdate.Observaciones;

            // Guardar cambios en la base de datos
            _context.SaveChanges();

            _logger.LogInformation("El usuario {Usuario} modificó el Turno {id} a las {FechaHora}",
                User?.Identity?.Name ?? "Anónimo", id, DateTime.UtcNow);


            return Ok(new { Message = "Turno actualizado correctamente." });
        }


        /// <summary>
        /// Actualiza el estado de un turno específico.
        /// </summary>
        /// <param name="id">Identificador único del turno.</param>
        /// <param name="st">ID del nuevo estado a asignar.</param>
        /// <param name="o"></param>Origen del cambio (opcional).
        /// <returns>Respuesta HTTP indicando éxito o fallo con mensaje correspondiente.</returns>
        [HttpPut("set-turno-status/{id}")]
        public IActionResult Put(int id, [FromQuery] int st, [FromQuery] int? o = null)
        {

            Estado oStatus = new()
            {
                Id = st
            };

            // Si "o" no viene, src será 0. Si viene, tomará su valor.
            int src = o ?? 0;

            ValidationsMethodPut validations = new (_context);
            ValidationsMethodPut validationResult = validations.ValidationMethodPutStatus(oStatus);

            // Para que el FrontEnd pueda mostrar un mensaje más específico se
            // retorna un estatus HTML 400 con el mensaje de error que viene del
            // validador.
            if (validationResult.IsValid == false) return BadRequest(new { Message = validationResult.ErrorMessage });

            TurnoLogic sLogic = new (_context);
            sLogic.UpdateShiftStatus(id, oStatus.Id, src);

            return Ok();
        }

        /// <summary>
        /// Elimina un turno específico del sistema.
        /// </summary>
        /// <param name="id">Identificador único del turno a eliminar.</param>
        // DELETE /<TurnoController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            TurnoLogic sLogic = new (_context);
            sLogic.DeleteShift(id);
        }

        /// <summary>
        /// Obtiene los turnos ocupados agrupados por día para un doctor específico.
        /// </summary>
        /// <param name="idDoctor">Identificador único del doctor.</param>
        /// <returns>Lista de <see cref="HorarioTurnos"/> con turnos agrupados por día.</returns>
        [HttpGet("get-turnos-for-doctor")]
        [Authorize(Roles = "Admin,Medico,Paciente")]
        public List<HorarioTurnos> GetBusyShiftsGroupedByDay(int idDoctor)
        {
            TurnoLogic sLogic = new (_context);
            return sLogic.ListOfShiftsGroupedByDay(idDoctor);
        }

        /// <summary>
        /// Obtiene la lista de turnos asociados a un paciente específico.
        /// </summary>
        /// <param name="idPaciente">Identificador único del paciente.</param>
        /// <returns>Lista de <see cref="Turno"/> con los turnos del paciente.</returns>
        [HttpGet("get-turnos-of-patient/{idPaciente}")]
        public TurnosPaciente GetListOfShiftsByPatient(int idPaciente)
        {
            TurnoLogic sLogic = new (_context);
            return sLogic.ListOfShiftsByPatient(idPaciente);
        }

        /// <summary>
        /// Obtiene la lista de turnos asociados a un paciente específico.
        /// Formato largo
        /// </summary>
        /// <param name="idPaciente">Identificador único del paciente.</param>
        /// <returns>Lista de <see cref="VwTurno"/> con los turnos del paciente.</returns>
        [HttpGet("get-turnos-by-patient/{idPaciente}")]
        public List<VwTurno> GetListOfShiftsByPatientVw(int idPaciente)
        {
            TurnoLogic sLogic = new(_context);
            return sLogic.ListOfShiftsByPatientVw(idPaciente);
        }

        /// <summary>
        /// Obtiene la lista de turnos asociados a un doctor específico.
        /// </summary>
        /// <param name="idMedico">Identificador único del doctor.</param>
        /// <returns>Lista de <see cref="Turno"/> con los turnos del doctor.</returns>
        [HttpGet("get-turnos-of-doctor/{idMedico}")]
        public List<Turno> GetListOfShiftsByDoctor(int idMedico)
        {
            TurnoLogic sLogic = new (_context);
            return sLogic.ListOfShiftsByDoctor(idMedico);
        }

        /// <summary>
        /// Obtiene la lista de turnos disponibles para un doctor específico (no utilizado).
        /// </summary>
        /// <param name="idDoctor">Identificador único del doctor.</param>
        /// <returns>Lista de <see cref="HorarioTurnos"/> con turnos disponibles.</returns>
        //[HttpGet("get-turnos-disponibles")]
        //public List<HorarioTurnos> GetListOfAvailableShifts(int idDoctor)
        //{
        //    // Este no es usado

        //    TurnoLogic sLogic = new (_context);
        //    return sLogic.ListOfAvailableShifts(idDoctor);
        //}

        /// <summary>
        /// Obtiene los turnos de una fecha específica.
        /// </summary>
        /// <param name="fecha">Fecha para filtrar los turnos.</param>
        /// <returns>Lista de <see cref="VwTurno"/> con los turnos de la fecha indicada.</returns>
        [HttpGet("get-turnos-of-date/{fecha}")]
        public List<VwTurno> GetShiftsOfDate(DateTime fecha)
        {
            TurnoLogic sLogic = new (_context);
            return sLogic.ListOfShiftsOfDate(fecha);
        }

        /// <summary>
        /// Obtiene las fechas del mes con turnos.
        /// </summary>
        /// <param name="mes">Número del mes para filtrar los datos.</param>
        /// <returns>Lista de DateTime con turnos en el mes.</returns>
        [HttpGet("get-dates-with-turnos-of-month/{mes}")]
        public List<DateTime> GetDatesWithShiftsOfMonth(int mes)
        {
            TurnoLogic sLogic = new (_context);
            return sLogic.ListOfDatesWithShiftsOfMonth(mes);
        }

        /// <summary>
        /// Obtiene los datos del dashboard: cantidad de turnos del año actual, del mes actual y por estado.
        /// </summary>
        /// <returns>Un diccionario con los datos del dashboard.</returns>
        [HttpGet("get-dashboard-data")]
        public Dictionary<string, object> GetDashboardData()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                TurnoLogic sLogic = new(_context);

                var result = new Dictionary<string, object>
            {
                { "qtyTurnosYr", sLogic.ListOfShiftQtyCurrentYear() },
                { "qtyTurnosMo", sLogic.ListOfShiftQtyCurrentMonth() },
                { "qtyTurnosXMedico", sLogic.ListOfShiftByDoctorQtyCurrentYear() },
                { "qtyStatesYr", sLogic.ListOfShiftStateQtyCurrentYear() }
            };

                stopwatch.Stop();
                _logger.LogInformation("Generación datos Dashboard completada en {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "Error de conexión SQL al obtener datos del Dashboard");
                return new Dictionary<string, object> { { "ErrorMessage", "Se requiere conexión con el servidor de datos" } };
            }
            catch (Microsoft.Data.SqlClient.SqlException msqlEx)
            {
                _logger.LogError(msqlEx, "Error de conexión SQL (Microsoft.Data) al obtener datos del Dashboard");
                return new Dictionary<string, object> { { "ErrorMessage", "Se requiere conexión con el servidor de datos" } };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener datos del Dashboard");
                return new Dictionary<string, object> { { "ErrorMessage", ex.Message } };
            }
        }

        /// <summary>
        /// Obtiene datos para mostrar en el calendario. 
        /// Inicialmente será la cantidad de turnos por día.
        /// </summary>
        /// <param name="start">Fecha de inicio del conjunto de datos (formato ISO8601).</param>
        /// <param name="end">Fecha de fin del conjunto de datos (formato ISO8601).</param>
        /// <returns>Lista de DateTime con turnos en el mes.</returns>
        [HttpGet("get-calendar-data")]
        public ActionResult<List<CalendarEvent>> GetCalendarData([FromQuery] string start, [FromQuery] string end)
        {
            TurnoLogic sLogic = new(_context);
            return sLogic.ListOfCalendarData(start, end);
        }


        /// <summary>
        /// SOLO PARA PRUEBAS
        /// </summary>
        /// <returns></returns>
        //[AllowAnonymous]
        //[HttpGet("test-email")]
        //public IActionResult TestEmail()
        //{
        //    // Datos de prueba
        //    var paciente = new Paciente
        //    {
        //        Nombre = "Pepe",
        //        Apellido = "Pérez",
        //        Dni = "12345678",
        //        FechaNacimiento = new DateTime(1990, 5, 20),
        //        Email = "chronomedpaciente@gmail.com"
        //    };

        //    var medico = new MedicoConEspecialidad
        //    {
        //        Nombre = "Pedro",
        //        Apellido = "Piermontti",
        //        Especialidad = "Medicina General"
        //    };

        //    var turno = new Turno
        //    {
        //        Fecha = DateTime.Today.AddDays(2),
        //        Hora = TimeSpan.Parse("10:30")
        //    };

        //    // Llamás directamente a tu método
        //    _ = new EmailService();
        //    EmailService.SendShiftConfirmationEmail(turno, paciente, medico);

        //    return Ok("Correo de prueba enviado.");
        //}
    }
}
