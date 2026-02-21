using ApiGestionTurnosMedicos.CustomModels;
using DataAccess.Context;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class MedicoRepository
    {
        private const int DIAS_CALCULAR_FECHAS_COMPLETAS = 60; // Se usa MUCHO más abajo

        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        public MedicoRepository(GestionTurnosContext context)
        {
            _context = context;
        }
        #endregion

        public List<Medico> GetAllDoctors()
        {
            List<Medico> doctors = new();
            using (_context)
            {
                doctors = _context.Medicos.ToList();
            }

            return doctors;
        }

        public int GetQtyDoctors()
        {
            int qty = 0;
            using (_context)
            {
                qty = _context.Medicos.Count();
            }

            return qty;
        }


        public GestionTurnosContext Get_context()
        {
            return _context;
        }

        public Medico GetDoctorForId(int id, GestionTurnosContext _context)
        {
            Medico oDoctor = new();

            oDoctor = _context.Medicos.Find(id);
            return oDoctor;
        }

        public List<HorarioMedico> GetHorarioDoctorForId(int id)
        {
            return _context.HorariosMedicos
                .Where(h => h.MedicoId == id)
                .OrderBy(h => h.DiaSemana)
                .ToList();
        }


        public void CreateDoctor(Medico oDoctor, List<HorarioMedico> horarios)
        {
            // Eliminado el using ya que causa problemas

            // Limpiar horarios nulos (días donde el Médico no trabaja)
            horarios.RemoveAll(h => h.HorarioAtencionInicio == null || h.HorarioAtencionFin == null);

            oDoctor.Horarios = horarios;

            _context.Medicos.Add(oDoctor);

            _context.SaveChanges();
        }

        public void UpdateDoctor(Medico oDoctor)
        {
            // Eliminado el using ya que causa problemas
            _context.Entry(oDoctor).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void DeleteDoctor(Medico oDoctor)
        {
            // Eliminado el using ya que causa problemas
            var horarios = _context.HorariosMedicos.Where(h => h.MedicoId == oDoctor.Id).ToList();
            _context.HorariosMedicos.RemoveRange(horarios);
            _context.Medicos.Remove(oDoctor);
            _context.SaveChanges();
        }

        public Medico GetDoctorForDNI(string dni)
        {
            Medico oDoctor = new Medico();

            oDoctor = _context.Medicos.Find(dni);
            return oDoctor;
        }


        public bool VerifyIfDoctorExist(string nombre, string dni)
        {
            return _context.Medicos.Any(d => d.Nombre == nombre && d.Dni == dni);
        }

        public List<Medico> FindDoctorForSpecialty(int id)
        {
            List<Medico> listDoctorForSpecialty  = new();
            using(_context)
            {
                listDoctorForSpecialty = _context.Medicos.Where(o => o.EspecialidadId == id).ToList();
            }

            return listDoctorForSpecialty;
        }

        public bool VerifyIfDoctorExistReturnBool(int id)
        {
            return _context.Medicos.Any(d=> d.Id == id);
        }

        public List<HorarioMedico> ReturnHorariosForDoctor(int id)
        {
            return _context.HorariosMedicos
                .Where(h => h.MedicoId == id)
                .OrderBy(h => h.DiaSemana)
                .ToList();
        }

        public List<MedicoCustom> ReturnAllDoctorsWithOurSpecialty()
        {

            MedicoCustom medico = new();
            List<MedicoCustom> all_doctors = new();

            all_doctors = (from m in _context.Medicos
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
                           }).ToList();

            // Asignar la colección de horarios a cada médico
            foreach (var m in all_doctors)
            {
                m.Horarios = _context.HorariosMedicos
                    .Where(h => h.MedicoId == m.Id)
                    .OrderBy(h => h.DiaSemana)
                    .ToList();
            }


            return all_doctors;
        }

        public MedicoConEspecialidad ReturnDoctorWithSpecialty(int id)
        {
            MedicoConEspecialidad medico = new();

            medico = (from m in _context.Medicos
                      join e in _context.Especialidades on m.EspecialidadId equals e.Id
                      where m.Id == id
                      select new MedicoConEspecialidad
                      {
                          Nombre = m.Nombre,
                          Apellido = m.Apellido,
                          Especialidad = e.Nombre
                      }).First();
            return medico;
        }
        public List<HorarioMedico> GetHorariosForDoctor(int medicoId)
        {
            return _context.HorariosMedicos.Where(h => h.MedicoId == medicoId).ToList();
        }

        public List<DateTime> GetTurnosOcupados(int medicoId)
        {
            const int DURACION_TURNO_MINUTOS = 60;

            TurnoRepository tRepo = new(_context);

            var schedule = GetHorariosForDoctor(medicoId);
            var turnos = tRepo.ListOfShiftsGroupedByDay(medicoId);

            List<DateTime> fechasOcupadas = new();

            DateTime hoy = DateTime.Today;
            DateTime limite = hoy.AddDays(DIAS_CALCULAR_FECHAS_COMPLETAS);

            // Recorrer desde hoy hasta dentro de [DIAS_CALCULAR_FECHAS_COMPLETAS] días
            for (DateTime fecha = hoy; fecha <= limite; fecha = fecha.AddDays(1))
            {
                // Día de la semana actual (1=Lunes, 7=Domingo)
                byte diaSemana = (byte)((int)fecha.DayOfWeek == 0 ? 7 : (int)fecha.DayOfWeek);

                // Verificar si el médico trabaja ese día
                var horarioDia = schedule.FirstOrDefault(h => h.DiaSemana == diaSemana);
                if (horarioDia == null)
                    continue; // no trabaja ese día

                if (horarioDia.HorarioAtencionInicio == null || horarioDia.HorarioAtencionFin == null)
                    continue; // horario incompleto

                // Calcular cuántos turnos debería tener disponibles
                TimeSpan inicio = horarioDia.HorarioAtencionInicio.Value;
                TimeSpan fin = horarioDia.HorarioAtencionFin.Value;

                //int totalTurnos = (int)(fin - inicio).TotalHours; // Se asume que cada turno dura 1 hora

                // Para poder definir duración de turno en minutos se usa una
                // constante al inicio del método. Inicialmente 60 minutos.
                int totalTurnos = (int)Math.Floor((fin - inicio).TotalMinutes / DURACION_TURNO_MINUTOS);

                // Obtener los turnos ocupados para ese día
                //var turnosDia = turnos.FirstOrDefault(t => t.Fecha_turno.Date == fecha.Date);
                //int ocupados = turnosDia?.Hora_turno.Count ?? 0;


                // Optimización: Crear un diccionario para acceso rápido
                // (sugerido por ChatGPT)
                var turnosPorDia = turnos.ToDictionary(t => t.Fecha_turno.Date, t => t.Hora_turno);
                turnosPorDia.TryGetValue(fecha.Date, out var horas);
                int ocupados = horas?.Count ?? 0;

                // Si tiene todos los turnos ocupados -> día completo
                if (ocupados >= totalTurnos)
                    fechasOcupadas.Add(fecha.Date);

            }

            return fechasOcupadas;
        }

    }
}