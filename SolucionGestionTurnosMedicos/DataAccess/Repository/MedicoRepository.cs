using ApiGestionTurnosMedicos.CustomModels;
using DataAccess.Context;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class MedicoRepository
    {
        private const int DIAS_CALCULAR_FECHAS_COMPLETAS = 60;

        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        public MedicoRepository(GestionTurnosContext context)
        {
            _context = context;
        }
        #endregion

        // Eliminamos el using() y agregamos Task/await/ToListAsync
        public async Task<List<Medico>> GetAllDoctors()
        {
            return await _context.Medicos.ToListAsync();
        }

        public async Task<int> GetQtyDoctors()
        {
            return await _context.Medicos.CountAsync();
        }

        public GestionTurnosContext Get_context()
        {
            return _context;
        }

        // Respetamos los dos parámetros que usas
        public async Task<Medico> GetDoctorForId(int id, GestionTurnosContext _context)
        {
            return await _context.Medicos.FindAsync(id);
        }

        public async Task<List<HorarioMedico>> GetHorarioDoctorForId(int id)
        {
            return await _context.HorariosMedicos
                .Where(h => h.MedicoId == id)
                .OrderBy(h => h.DiaSemana)
                .ToListAsync();
        }

        public async Task CreateDoctor(Medico oDoctor, List<HorarioMedico> horarios)
        {
            horarios.RemoveAll(h => h.HorarioAtencionInicio == null || h.HorarioAtencionFin == null);
            oDoctor.Horarios = horarios;

            await _context.Medicos.AddAsync(oDoctor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDoctor(Medico oDoctor)
        {
            _context.Entry(oDoctor).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDoctor(Medico oDoctor)
        {
            var horarios = await _context.HorariosMedicos.Where(h => h.MedicoId == oDoctor.Id).ToListAsync();
            _context.HorariosMedicos.RemoveRange(horarios);
            _context.Medicos.Remove(oDoctor);
            await _context.SaveChangesAsync();
        }

        public async Task<Medico> GetDoctorForDNI(string dni)
        {
            // Nota: Find() busca por PK. Si DNI no es PK, mejor usar FirstOrDefaultAsync
            return await _context.Medicos.FirstOrDefaultAsync(m => m.Dni == dni);
        }

        public async Task<bool> VerifyIfDoctorExist(string nombre, string dni)
        {
            return await _context.Medicos.AnyAsync(d => d.Nombre == nombre && d.Dni == dni);
        }

        public async Task<List<Medico>> FindDoctorForSpecialty(int id)
        {
            // Eliminamos el using() que cerraba el contexto
            return await _context.Medicos.Where(o => o.EspecialidadId == id).ToListAsync();
        }

        public async Task<bool> VerifyIfDoctorExistReturnBool(int id)
        {
            return await _context.Medicos.AnyAsync(d => d.Id == id);
        }

        public async Task<List<HorarioMedico>> ReturnHorariosForDoctor(int id)
        {
            return await _context.HorariosMedicos
                .Where(h => h.MedicoId == id)
                .OrderBy(h => h.DiaSemana)
                .ToListAsync();
        }

        public async Task<List<MedicoCustom>> ReturnAllDoctorsWithOurSpecialty()
        {
            var all_doctors = await (from m in _context.Medicos
                                     join e in _context.Especialidades
                                     on m.EspecialidadId equals e.Id
                                     select new MedicoCustom
                                     {
                                         Id = m.Id,
                                         Nombre = m.Nombre,
                                         Apellido = m.Apellido,
                                         Telefono = m.Telefono,
                                         Dni = m.Dni,
                                         Direccion = m.Direccion,
                                         FechaAltaLaboral = m.FechaAltaLaboral,
                                         Especialidad = e.Nombre,
                                         Foto = m.Foto != null ? Convert.ToBase64String(m.Foto) : null,
                                         Matricula = m.Matricula
                                     }).ToListAsync();

            foreach (var m in all_doctors)
            {
                m.Horarios = await _context.HorariosMedicos
                    .Where(h => h.MedicoId == m.Id)
                    .OrderBy(h => h.DiaSemana)
                    .ToListAsync();
            }

            return all_doctors;
        }

        public async Task<MedicoConEspecialidad> ReturnDoctorWithSpecialty(int id)
        {
            return await (from m in _context.Medicos
                          join e in _context.Especialidades on m.EspecialidadId equals e.Id
                          where m.Id == id
                          select new MedicoConEspecialidad
                          {
                              Nombre = m.Nombre,
                              Apellido = m.Apellido,
                              Especialidad = e.Nombre
                          }).FirstAsync();
        }

        public async Task<List<HorarioMedico>> GetHorariosForDoctor(int medicoId)
        {
            return await _context.HorariosMedicos.Where(h => h.MedicoId == medicoId).ToListAsync();
        }

        public async Task<List<DateTime>> GetTurnosOcupados(int medicoId)
        {
            const int DURACION_TURNO_MINUTOS = 60;

            // Suponiendo que normalizaremos TurnoRepository después
            TurnoRepository tRepo = new(_context);

            var schedule = await GetHorariosForDoctor(medicoId);
            var turnos = await tRepo.ListOfShiftsGroupedByDay(medicoId);

            List<DateTime> fechasOcupadas = new();
            DateTime hoy = DateTime.Today;
            DateTime limite = hoy.AddDays(DIAS_CALCULAR_FECHAS_COMPLETAS);

            var turnosPorDia = turnos.ToDictionary(t => t.Fecha_turno.Date, t => t.Hora_turno);

            for (DateTime fecha = hoy; fecha <= limite; fecha = fecha.AddDays(1))
            {
                byte diaSemana = (byte)((int)fecha.DayOfWeek == 0 ? 7 : (int)fecha.DayOfWeek);
                var horarioDia = schedule.FirstOrDefault(h => h.DiaSemana == diaSemana);

                if (horarioDia == null || horarioDia.HorarioAtencionInicio == null || horarioDia.HorarioAtencionFin == null)
                    continue;

                TimeSpan inicio = horarioDia.HorarioAtencionInicio.Value;
                TimeSpan fin = horarioDia.HorarioAtencionFin.Value;

                int totalTurnos = (int)Math.Floor((fin - inicio).TotalMinutes / DURACION_TURNO_MINUTOS);

                turnosPorDia.TryGetValue(fecha.Date, out var horas);
                int ocupados = horas?.Count ?? 0;

                if (ocupados >= totalTurnos)
                    fechasOcupadas.Add(fecha.Date);
            }

            return fechasOcupadas;
        }
    }
}