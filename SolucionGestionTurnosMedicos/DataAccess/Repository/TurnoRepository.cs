using DataAccess.Context;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Models.CustomModels;
using System.Data;
using Microsoft.Data.SqlClient; // Cambiado a Microsoft.Data.SqlClient que es el estándar actual

namespace DataAccess.Repository
{
    public class TurnoRepository
    {
        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        public TurnoRepository(GestionTurnosContext context)
        {
            _context = context;
        }
        #endregion

        public async Task<List<VwTurno>> GetAllShift()
        {
            return await _context.VwTurnos.ToListAsync();
        }

        public async Task<List<VwTurno>> GetShiftsOfDate(DateTime fecha)
        {
            return await _context.VwTurnos.Where(t => t.Fecha == fecha).ToListAsync();
        }

        public async Task<List<DateTime>> GetDatesWithShiftsOfMonth(int mes)
        {
            return await _context.Turnos
                .Where(t => t.Fecha.Month == mes)
                .GroupBy(t => t.Fecha)
                .Select(g => g.Key)
                .OrderBy(d => d)
                .ToListAsync();
        }

        public async Task<Turno?> GetShiftById(int id)
        {
            return await _context.Turnos.FindAsync(id);
        }

        public async Task<VwTurno?> GetDisplayShiftById(int id)
        {
            return await _context.VwTurnos.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task CreateShift(Turno oShift)
        {
            await _context.Turnos.AddAsync(oShift);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateShift(Turno oShift)
        {
            _context.Entry(oShift).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteShift(Turno oShift)
        {
            _context.Turnos.Remove(oShift);
            await _context.SaveChangesAsync();
        }

        public async Task<List<HorarioTurnos>> ListOfShiftsGroupedByDay(int id_doctor)
        {
            const int estado_cancelado = 2;
            DateTime fecha_actual = DateTime.Now;

            return await (from t in _context.Turnos
                          join m in _context.Medicos on t.MedicoId equals m.Id
                          where m.Id == id_doctor
                          where t.EstadoId != estado_cancelado
                          where t.Fecha >= fecha_actual
                          group t by t.Fecha into g
                          select new HorarioTurnos
                          {
                              Fecha_turno = g.Key,
                              Hora_turno = g.Select(t => t.Hora).ToList()
                          }).ToListAsync();
        }

        public async Task<bool> VerifyIfShiftExist(int medicoId, DateTime fecha, TimeSpan hora, int? turnoId = null)
        {
            var turnoExistente = await _context.Turnos
                .FirstOrDefaultAsync(t => t.MedicoId == medicoId
                             && t.Fecha == fecha
                             && t.Hora == hora
                             && t.EstadoId == 1
                             && (!turnoId.HasValue || t.Id != turnoId));

            return turnoExistente != null;
        }

        public async Task<TurnosPaciente?> GetShiftsByPatient(int idPaciente)
        {
            var datos = await (from t in _context.Turnos
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
                               }).ToListAsync();

            if (!datos.Any()) return null;

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

        public async Task<List<VwTurno>> GetShiftsByPatientVw(int idPaciente)
        {
            return await _context.VwTurnos.Where(t => t.PacienteId == idPaciente).ToListAsync();
        }

        public async Task<List<Turno>> GetShiftsByDoctor(int idMedico)
        {
            return await _context.Turnos.Where(t => t.MedicoId == idMedico).ToListAsync();
        }

        public async Task<List<VwTurnoCount>> GetQtyShiftYear()
        {
            int currentYr = DateTime.Now.Year;

            return await _context.VwTurnoCounts
                    .Where(t => t.Yr == currentYr)
                    .GroupBy(t => new { t.Yr, t.Estado, t.Clase, t.Color })
                    .Select(g => new VwTurnoCount
                    {
                        Yr = g.Key.Yr,
                        Estado = g.Key.Estado,
                        Clase = g.Key.Clase,
                        Color = g.Key.Color,
                        CountId = g.Sum(t => t.CountId)
                    })
                    .ToListAsync();
        }

        public async Task<List<VwTurnoCount>> GetQtyShiftMonth()
        {
            int currentYr = DateTime.Now.Year;
            int currentMo = DateTime.Now.Month;
            return await _context.VwTurnoCounts
                .Where(t => t.Yr == currentYr && t.Mo == currentMo)
                .ToListAsync();
        }

        public async Task<List<VwTurnoXMedicoCount>> GetQtyShiftByDoctorYear()
        {
            int currentYr = DateTime.Now.Year;
            return await _context.VwTurnoXMedicoCounts
                .Where(t => t.Yr == currentYr)
                .ToListAsync();
        }

        // Se mantiene DataTable porque los Pivot son dinámicos, pero se limpia la lógica
        public DataTable GetPivotTurnoCount(int? year)
        {
            string connectionString = _context.Database.GetDbConnection().ConnectionString;

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
                        dt.Load(reader);
                    }
                    return dt;
                }
            }
        }

        public async Task<List<CalendarEvent>> GetCalendarData(string start, string end)
        {
            DateTime startDate = DateTime.Parse(start);
            DateTime endDate = DateTime.Parse(end);

            var data = await _context.VwTurnoCalendars
                .Where(t => t.Fecha >= startDate && t.Fecha <= endDate)
                .ToListAsync();

            return data.Select(t => new CalendarEvent
            {
                Title = t.Qty.ToString(),
                Start = t.Fecha.ToString("o"),
                End = t.Fecha.ToString("o"),
                Id = t.Fecha.ToString("yyyy-MM-dd")
            }).ToList();
        }
    }
}