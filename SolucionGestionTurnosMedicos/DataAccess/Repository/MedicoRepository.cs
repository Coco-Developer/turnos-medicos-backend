using Models.CustomModels;
using DataAccess.Context;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataAccess.Repository
{
    public class MedicoRepository
    {
        private const int DIAS_CALCULAR_FECHAS_COMPLETAS = 60;
        private readonly GestionTurnosContext _context;
        private readonly TurnoRepository _turnoRepository;

        public MedicoRepository(
            GestionTurnosContext context,
            TurnoRepository turnoRepository)
        {
            _context = context;
            _turnoRepository = turnoRepository;
        }

        #region Consultas Básicas

        public async Task<List<Medico>> GetAllDoctors()
        {
            return await _context.Medicos.ToListAsync();
        }

        public async Task<int> GetQtyDoctors()
        {
            return await _context.Medicos.CountAsync();
        }

        public async Task<Medico?> GetDoctorForId(int id)
        {
            return await _context.Medicos
                .Include(m => m.Horarios)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Medico?> GetDoctorForDNI(string dni)
        {
            return await _context.Medicos
                .FirstOrDefaultAsync(m => m.Dni == dni);
        }

        #endregion

        #region Validaciones

        public async Task<bool> VerifyIfDoctorExistReturnBool(int id)
        {
            return await _context.Medicos.AnyAsync(d => d.Id == id);
        }

        public async Task<bool> VerifyIfDoctorExist(string nombre, string dni)
        {
            return await _context.Medicos
                .AnyAsync(d => d.Nombre == nombre && d.Dni == dni);
        }

        #endregion

        #region CRUD

        public async Task CreateDoctor(Medico medico, List<HorarioMedico> horarios)
        {
            horarios = horarios
                .Where(h => h.HorarioAtencionInicio != null &&
                            h.HorarioAtencionFin != null)
                .ToList();

            medico.Horarios = horarios;

            await _context.Medicos.AddAsync(medico);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDoctor(Medico medico)
        {
            _context.Medicos.Update(medico);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateDoctorWithSchedules(Medico medico, List<HorarioMedico> horarios)
        {
            var provider = _context.Database.ProviderName ?? string.Empty;
            var isInMemory = provider.Contains("InMemory", StringComparison.OrdinalIgnoreCase);

            IDbContextTransaction? tx = null;
            if (!isInMemory)
            {
                try
                {
                    tx = await _context.Database.BeginTransactionAsync();
                }
                catch
                {
                    tx = null;
                }
            }

            try
            {
                // 1) Cargar médico "limpio" (sin Include Horarios)
                var doctorDb = await _context.Medicos
                    .FirstOrDefaultAsync(m => m.Id == medico.Id);

                if (doctorDb == null)
                    throw new ArgumentException($"No existe médico con Id {medico.Id}");

                // 2) Actualizar solo campos escalares
                doctorDb.Nombre = medico.Nombre;
                doctorDb.Apellido = medico.Apellido;
                doctorDb.Dni = medico.Dni;
                doctorDb.Telefono = medico.Telefono;
                doctorDb.Direccion = medico.Direccion;
                doctorDb.EspecialidadId = medico.EspecialidadId;
                doctorDb.FechaAltaLaboral = medico.FechaAltaLaboral;
                doctorDb.Matricula = medico.Matricula;
                doctorDb.Foto = medico.Foto;

                // 3) Borrar agenda anterior en servidor con un solo comando
                // (EF Core 8 soporta ExecuteDeleteAsync => evita traer entidades a memoria)
                // if (isInMemory)
                // {
                //     // InMemory provider doesn't translate ExecuteDeleteAsync. Fall back to loading and removing.
                //     var existentes = await _context.HorariosMedicos.Where(h => h.MedicoId == medico.Id).ToListAsync();
                //     if (existentes.Count > 0)
                //         _context.HorariosMedicos.RemoveRange(existentes);
                // }
                // else
                // {
                //     await _context.HorariosMedicos
                //         .Where(h => h.MedicoId == medico.Id)
                //         .ExecuteDeleteAsync();
                // }

                // Cargar horarios existentes y eliminarlos (compatible con todos los providers)
                var existentes = await _context.HorariosMedicos.Where(h => h.MedicoId == medico.Id).ToListAsync();
                if (existentes.Count > 0)
                    _context.HorariosMedicos.RemoveRange(existentes);

                // 4) Insertar agenda nueva (1 registro por día válido)
                var nuevos = (horarios ?? new List<HorarioMedico>())
                    .Where(h => h.HorarioAtencionInicio != null && h.HorarioAtencionFin != null)
                    .GroupBy(h => h.DiaSemana)
                    .Select(g => g.First())
                    .Select(h => new HorarioMedico
                    {
                        MedicoId = medico.Id,
                        DiaSemana = h.DiaSemana,
                        HorarioAtencionInicio = h.HorarioAtencionInicio,
                        HorarioAtencionFin = h.HorarioAtencionFin
                    })
                    .ToList();

                // Desactivar AutoDetectChanges durante el AddRange masivo para reducir overhead
                if (nuevos.Count > 0)
                {
                    var autoDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
                    try
                    {
                        _context.ChangeTracker.AutoDetectChangesEnabled = false;
                        await _context.HorariosMedicos.AddRangeAsync(nuevos);
                    }
                    finally
                    {
                        _context.ChangeTracker.AutoDetectChangesEnabled = autoDetect;
                    }
                }

                // 5) Persistir todos los cambios en una única llamada
                await _context.SaveChangesAsync();

                if (tx != null)
                    await tx.CommitAsync();
            }
            catch
            {
                if (tx != null)
                    await tx.RollbackAsync();
                throw;
            }
            finally
            {
                if (tx != null)
                    await tx.DisposeAsync();
            }
         }



        public async Task DeleteDoctor(Medico medico)
        {
            var horarios = await _context.HorariosMedicos
                .Where(h => h.MedicoId == medico.Id)
                .ToListAsync();

            _context.HorariosMedicos.RemoveRange(horarios);
            _context.Medicos.Remove(medico);

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Horarios y Especialidades

        public async Task<List<HorarioMedico>> GetHorarioDoctorForId(int id)
        {
            return await _context.HorariosMedicos
                .Where(h => h.MedicoId == id)
                .OrderBy(h => h.DiaSemana)
                .ToListAsync();
        }

        public async Task<List<HorarioMedico>> ReturnHorariosForDoctor(int id)
        {
            return await GetHorarioDoctorForId(id);
        }

        public async Task<List<Medico>> FindDoctorForSpecialty(int id)
        {
            return await _context.Medicos
                .Where(m => m.EspecialidadId == id)
                .ToListAsync();
        }

        public async Task<List<MedicoCustom>> ReturnAllDoctorsWithOurSpecialty()
        {
            return await _context.Medicos
                .Include(m => m.Horarios)
                .Join(_context.Especialidades,
                    m => m.EspecialidadId,
                    e => e.Id,
                    (m, e) => new MedicoCustom
                    {
                        Id = m.Id,
                        Nombre = m.Nombre,
                        Apellido = m.Apellido,
                        Telefono = m.Telefono,
                        Dni = m.Dni,
                        Direccion = m.Direccion,
                        FechaAltaLaboral = m.FechaAltaLaboral,
                        Especialidad = e.Nombre,
                        EspecialidadId = m.EspecialidadId,
                        Foto = m.Foto != null
                            ? Convert.ToBase64String(m.Foto)
                            : null,
                        Matricula = m.Matricula,
                        Horarios = m.Horarios
                            .OrderBy(h => h.DiaSemana)
                            .ToList()
                    })
                .ToListAsync();
        }

        public async Task<MedicoConEspecialidad> ReturnDoctorWithSpecialty(int id)
        {
            return await (from m in _context.Medicos
                          join e in _context.Especialidades
                          on m.EspecialidadId equals e.Id
                          where m.Id == id
                          select new MedicoConEspecialidad
                          {
                              Nombre = m.Nombre,
                              Apellido = m.Apellido,
                              Especialidad = e.Nombre
                          })
                          .FirstAsync();
        }

        #endregion

        #region Turnos y Fechas Ocupadas

        public async Task<List<HorarioMedico>> GetHorariosForDoctor(int medicoId)
        {
            return await _context.HorariosMedicos
                .Where(h => h.MedicoId == medicoId)
                .ToListAsync();
        }

        public async Task<List<DateTime>> GetTurnosOcupados(int medicoId)
        {
            const int DURACION_TURNO_MINUTOS = 60;

            var schedule = await GetHorariosForDoctor(medicoId);
            var turnos = await _turnoRepository
                .ListOfShiftsGroupedByDay(medicoId);

            List<DateTime> fechasOcupadas = new();

            DateTime hoy = DateTime.Today;
            DateTime limite = hoy.AddDays(DIAS_CALCULAR_FECHAS_COMPLETAS);

            var turnosPorDia = turnos
                .ToDictionary(t => t.Fecha_turno.Date,
                              t => t.Hora_turno);

            for (DateTime fecha = hoy; fecha <= limite; fecha = fecha.AddDays(1))
            {
                byte diaSemana = (byte)((int)fecha.DayOfWeek == 0
                    ? 7
                    : (int)fecha.DayOfWeek);

                var horarioDia = schedule
                    .FirstOrDefault(h => h.DiaSemana == diaSemana);

                if (horarioDia?.HorarioAtencionInicio == null ||
                    horarioDia?.HorarioAtencionFin == null)
                    continue;

                TimeSpan inicio = horarioDia.HorarioAtencionInicio.Value;
                TimeSpan fin = horarioDia.HorarioAtencionFin.Value;

                int totalTurnos = (int)Math.Floor(
                    (fin - inicio).TotalMinutes /
                    DURACION_TURNO_MINUTOS);

                if (turnosPorDia.TryGetValue(fecha.Date, out var horas))
                {
                    if (horas.Count >= totalTurnos)
                        fechasOcupadas.Add(fecha.Date);
                }
            }

            return fechasOcupadas;
        }

        #endregion
    }
}