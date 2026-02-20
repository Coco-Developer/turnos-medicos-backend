using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Models.CustomModels;
using System.Data;
using System.Data.SqlClient;

namespace DataAccess.Repository
{
    /// <summary>
    /// Repositorio para la gestión de turnos médicos en la base de datos.
    /// Proporciona métodos para consultar, crear, actualizar y eliminar turnos.
    /// </summary>
    public class TurnoRepository
    {
        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        /// <summary>
        /// Constructor del repositorio de turnos.
        /// </summary>
        /// <param name="context">Contexto de la base de datos para operaciones CRUD</param>
        public TurnoRepository(GestionTurnosContext context)
        {
            _context = context;
        }
        #endregion

        /// <summary>
        /// Obtiene la lista completa de turnos en formato de vista extendida.
        /// </summary>
        /// <returns>Lista de objetos VwTurno</returns>
        public List<VwTurno> GetAllShift()
        {
            using (_context)
            {
                return _context.VwTurnos.ToList();
            }
        }

        /// <summary>
        /// Obtiene la lista de turnos para una fecha específica.
        /// </summary>
        /// <param name="fecha">Fecha para filtrar los turnos</param>
        /// <returns>Lista de objetos VwTurno correspondientes a la fecha</returns>
        public List<VwTurno> GetShiftsOfDate(DateTime fecha)
        {
            using (_context)
            {
                return _context.VwTurnos.Where(t => t.Fecha == fecha).ToList();
            }
        }

        /// <summary>
        /// Obtiene la lista fechas con turnos para un mes específico.
        /// </summary>
        /// <param name="mes">Mes para filtrar los turnos</param>
        /// <returns>Lista de objetos DateTime correspondientes a los turnos del mes</returns>
        public List<DateTime> GetDatesWithShiftsOfMonth(int mes)
        {
            using (_context)
            {
                return _context.Turnos
                    .Where(t => t.Fecha.Month == mes)
                    .GroupBy(t => t.Fecha)
                    .Select(g => g.Key)
                    .OrderBy(d => d)
                    .ToList();
            }
        }

        /// <summary>
        /// Obtiene un turno específico por su identificador.
        /// </summary>
        /// <param name="id">Identificador único del turno</param>
        /// <returns>Objeto Turno correspondiente al ID, o null si no se encuentra</returns>
        public Turno? GetShiftById(int id)
        {
            return _context.Turnos.Find(id);
        }

        /// <summary>
        /// Obtiene un turno en formato de vista extendida por su identificador.
        /// </summary>
        /// <param name="id">Identificador único del turno</param>
        /// <returns>Objeto VwTurno correspondiente al ID, o null si no se encuentra</returns>
        public VwTurno GetDisplayShiftById(int id)
        {
            return _context.VwTurnos.Where(t => t.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Crea un nuevo turno en la base de datos.
        /// </summary>
        /// <param name="oShift">Objeto Turno con los datos a crear</param>
        public async Task CreateShift(Turno oShift)
        {
            await _context.Turnos.AddAsync(oShift);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Actualiza un turno existente en la base de datos.
        /// </summary>
        /// <param name="oShift">Objeto Turno con los datos actualizados</param>
        public void UpdateShift(Turno oShift)
        {
            using (_context)
            {
                _context.Entry(oShift).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _context.SaveChanges();
            }
        }
        
        /// <summary>
        /// Elimina un turno de la base de datos.
        /// </summary>
        /// <param name="oShift">Objeto Turno a eliminar</param>
        public void DeleteShift(Turno oShift)
        {
            using (_context)
            {
                _context.Turnos.Remove(oShift);
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Obtiene una lista de turnos agrupados por día para un médico específico, excluyendo turnos cancelados.
        /// </summary>
        /// <param name="id_doctor">Identificador del médico</param>
        /// <returns>Lista de objetos HorarioTurnos con fechas y horas de turnos</returns>
        public List<HorarioTurnos> ListOfShiftsGroupedByDay(int id_doctor)
            // Lista de turnos agrupados por dia.
        {
            TurnoCustom turnoCustom = new();
            List<HorarioTurnos> horario = new();
            const int estado_cancelado = 2;
            DateTime fecha_actual = DateTime.Now;

            horario = (from t in _context.Turnos
                       join m in _context.Medicos on t.MedicoId equals m.Id
                       where m.Id == id_doctor
                       where t.EstadoId != estado_cancelado
                       where t.Fecha >= fecha_actual
                       group t by t.Fecha into g
                       select new HorarioTurnos
                       {
                           Fecha_turno = g.Key,
                           Hora_turno = g.Select(t => t.Hora).ToList()
                       }).ToList();

            return horario;
        }

        /// <summary>
        /// Verifica si ya existe un turno para un médico en una fecha y hora específicas.
        /// </summary>
        /// <param name="medicoId">Identificador del médico</param>
        /// <param name="fecha">Fecha del turno</param>
        /// <param name="hora">Hora del turno</param>
        /// <param name="turnoId">ID del turno a excluir (opcional, usado en actualizaciones)</param>
        /// <returns>True si existe un turno, False si no</returns>
        public bool VerifyIfShiftExist(int medicoId, DateTime fecha, TimeSpan hora, int? turnoId = null)
        {
            // Busca si existe algún turno en la misma fecha, hora y activo, pero excluye el turno con el ID actual
            var turnoExistente = _context.Turnos
                .Where(t => t.MedicoId == medicoId
                            && t.Fecha == fecha
                            && t.Hora == hora
                            && t.EstadoId == 1
                            && (!turnoId.HasValue || t.Id != turnoId)) // Excluir el turno actual que se está modificando
                .FirstOrDefault();

            // Retorna true si encuentra un turno, false si no lo encuentra
            return turnoExistente != null;
        }

        /// <summary>
        /// Obtiene la lista de turnos asociados a un paciente específico.
        /// </summary>
        /// <param name="idPaciente">Identificador del paciente</param>
        /// <returns>Lista de objetos Turno</returns>
        public TurnosPaciente GetShiftsByPatient(int idPaciente)
        {
            using (_context)
            {
                var datos = (from t in _context.Turnos
                             join m in _context.Medicos on t.MedicoId equals m.Id
                             join e in _context.Especialidades on m.EspecialidadId equals e.Id
                             join p in _context.Pacientes on t.PacienteId equals p.Id
                             where t.PacienteId == idPaciente
                             select new
                             {
                                 Paciente = p,
                                 Medico = m,
                                 Especialidad = e,
                                 Turno = t
                             }).ToList();

                if (!datos.Any())
                    return null;

                var paciente = datos.First().Paciente;

                return new TurnosPaciente
                {
                    Id = paciente.Id,
                    Nombre = paciente.Nombre,
                    Apellido = paciente.Apellido,
                    Turnos = datos.Select(x => new TurnoDTO
                    {
                        Fecha = x.Turno.Fecha,
                        Hora = x.Turno.Hora.ToString(),
                        NombreMedico = x.Medico.Nombre,
                        ApellidoMedico = x.Medico.Apellido,
                        NombreEspecialidad = x.Especialidad.Nombre
                    }).ToList()
                };
            }
        }


        /// <summary>
        /// Obtiene la lista de turnos asociados a un paciente específico.
        /// Formato largo. 
        /// </summary>
        /// <param name="idPaciente">Identificador del paciente</param>
        /// <returns>Lista de objetos VwTurno</returns>
        public List<VwTurno> GetShiftsByPatientVw(int idPaciente)
        {
            using (_context)
            {
                return _context.VwTurnos.Where(t => t.PacienteId == idPaciente).ToList();
            }
        }

        /// <summary>
        /// Obtiene la lista de turnos asociados a un médico específico.
        /// </summary>
        /// <param name="idMedico">Identificador del médico</param>
        /// <returns>Lista de objetos Turno</returns>
        public List<Turno> GetShiftsByDoctor(int idMedico)
        {
            using (_context)
            {
                return _context.Turnos.Where(t => t.MedicoId == idMedico).ToList();
            }
        }

        /// <summary>
        /// Obtiene el conteo de turnos agrupados por estado para el año actual.
        /// </summary>
        /// <returns>Lista de objetos VwTurnoCount con el conteo por estado</returns>
        public List<VwTurnoCount> GetQtyShiftYear()
        {
            int currentYr = DateTime.Now.Year;

            return _context.VwTurnoCounts
                    .Where(t => t.Yr == currentYr) // Filtrar por el año actual
                    .GroupBy(t => new 
                    {
                        Yr = t.Yr,
                        Estado = t.Estado,
                        t.Clase,
                        t.Color
                    })
                    .Select(g => new VwTurnoCount
                    {
                        Yr = g.Key.Yr,
                        Estado = g.Key.Estado,
                        Clase = g.Key.Clase,
                        Color = g.Key.Color,
                        CountId = g.Sum(t => t.CountId)
                    })
                    .ToList();
        }

        /// <summary>
        /// Obtiene el conteo de turnos para el mes y año actuales.
        /// </summary>
        /// <returns>Lista de objetos VwTurnoCount</returns>
        public List<VwTurnoCount> GetQtyShiftMonth()
        {
            int currentYr = DateTime.Now.Year;
            int currentMo = DateTime.Now.Month;
            return _context.VwTurnoCounts
                .Where(t => t.Yr == currentYr
                            && t.Mo == currentMo)
            .ToList();
        }

        /// <summary>
        /// Obtiene el conteo de turnos para el mes y año actuales.
        /// </summary>
        /// <returns>Lista de objetos VwTurnoCount</returns>
        public List<VwTurnoXMedicoCount> GetQtyShiftByDoctorYear()
        {
            int currentYr = DateTime.Now.Year;
            return _context.VwTurnoXMedicoCounts
                .Where(t => t.Yr == currentYr)
            .ToList();
        }

        /// <summary>
        /// Obtiene una tabla dinámica de conteo de turnos usando un procedimiento almacenado.
        /// </summary>
        /// <param name="year">Año para filtrar los datos (opcional)</param>
        /// <returns>DataTable con los resultados dinámicos del conteo</returns>
        public DataTable GetPivotTurnoCount(int? year)
        {
            // Leer el appsettings.json manualmente
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory) // Base path de la app
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Sacar la connection string
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("La cadena de conexión no está configurada o es nula.");
            }
            // Código sugerido por ChatGPT para obtener los datos sin tener que
            // crear un modelo, ya que las consultas "pivot" devuelven columnas
            // dinámicas (es decir, pueden ser una, dos tres, etc)
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("spPivotTurnoCount", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@year", (object)year ?? DBNull.Value);

                    DataTable dt = new DataTable();
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader); // Carga los datos dinámicos sin necesidad de un modelo
                    }
                    return dt;
                }
            }
        }

        /// Obtiene el conteo de turnos por día entre start y end.
        /// </summary>
        /// <param name="start">Fecha de inicio del conjunto de datos (formato ISO8601)</param>
        /// <param name="end">Fecha de fin del conjunto de datos (formato ISO8601)</param>
        /// <returns>Lista de objetos VwTurnoCount</returns>
        public List<CalendarEvent> GetCalendarData(string start, string end)
        {
            DateTime startDate = DateTime.Parse(start);
            DateTime endDate = DateTime.Parse(end);

            var data = _context.VwTurnoCalendars
                .Where(t => t.Fecha >= startDate && t.Fecha <= endDate)
                .ToList();

            return data.Select(t => new CalendarEvent
            {
                Title = t.Qty.ToString(),
                Start = t.Fecha.ToString("o"), // ISO 8601
                End = t.Fecha.ToString("o"),
                Id = t.Fecha.ToString("yyyy-MM-dd")
            }).ToList();

        }
    }
}