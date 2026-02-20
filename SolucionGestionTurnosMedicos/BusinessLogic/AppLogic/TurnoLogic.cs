using ApiGestionTurnosMedicos.CustomModels;
using ApiGestionTurnosMedicos.Services;
using BusinessLogic.AppLogic.Services;
using DataAccess.Data;
using DataAccess.Repository;
using Microsoft.Extensions.DependencyInjection;
using Models.CustomModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace BusinessLogic.AppLogic
{
    public class TurnoLogic
    {

        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        public TurnoLogic(GestionTurnosContext context)
        {
            _context = context;
        }
        #endregion

        private readonly HttpClient _httpClient;

        public TurnoLogic(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public List<VwTurno> ShiftList()
        {
            TurnoRepository repShift = new TurnoRepository(_context);
            return repShift.GetAllShift();
        }

        public VwTurno GetShiftForId(int id)
        {
            if (id == 0) throw new ArgumentException("Id cannot be 0");

            try
            {
                TurnoRepository repShift = new TurnoRepository(_context);
                VwTurno oShiftFound = repShift.GetDisplayShiftById(id) ?? throw new ArgumentException("No shift was found with that id");
                return oShiftFound;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Processing failed: {e.Message}");
                throw;
            }
        }

        public async Task CreateShift(Turno oShift)
        {

            #region Validations

            if (oShift.Observaciones == "" || oShift.Observaciones == "string")
            {
                oShift.Observaciones = "N/D";
            }

            #endregion

            // Se pasó el control de errores al Controlador.

            TurnoRepository repShift = new(_context);
            MedicoRepository repDoctor = new(_context);
            PacienteRepository repPatient = new(_context);
            MedicoConEspecialidad doctor = new();

            Paciente patient = repPatient.GetPatientForId(oShift.PacienteId);
            MedicoConEspecialidad Odoctor = repDoctor.ReturnDoctorWithSpecialty(oShift.MedicoId);

            await repShift.CreateShift(oShift);

            var emailService = new EmailService(new SmtpEmailSender());
            emailService.SendShiftConfirmationEmail(oShift, patient, Odoctor);
        }

        public void UpdateShift(int id, Turno oShift)
        {
            TurnoRepository repShift = new(_context);

            try
            {
                Turno oShiftFound = repShift.GetShiftById(id) ?? throw new ArgumentException("No Shift was found with that id");
                oShiftFound.Hora = oShift.Hora;
                oShiftFound.Fecha = oShift.Fecha;
                oShiftFound.Observaciones = oShift.Observaciones;
                repShift.UpdateShift(oShiftFound);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Processing failed: {e.Message}");
                throw;
            }
        }

        public void UpdateShiftStatus(int id, int status, int updateSource)
        {
            TurnoRepository repShift = new(_context);

            try
            {
                Turno oShiftFound = repShift.GetShiftById(id) ?? throw new ArgumentException("No Shift was found with that id");

                if (updateSource == 1) // Cambio de estado desde la aplicación móvil
                {
                    oShiftFound.Observaciones = "Autogestión Paciente (App Movil)";
                }

                oShiftFound.EstadoId = status;
                repShift.UpdateShift(oShiftFound);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Processing failed: {e.Message}");
                throw;
            }
        }

        public void DeleteShift(int id)
        {
            TurnoRepository repShift = new TurnoRepository(_context);

            try
            {
                Turno oShiftFound = repShift.GetShiftById(id) ?? throw new ArgumentException("No shift was found with that id");
                repShift.DeleteShift(oShiftFound);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Processing failed: {e.Message}");
                throw;
            }
        }

        public List<HorarioTurnos> ListOfShiftsGroupedByDay(int idDoctor)
        //Devuelve la lista de turnos ocupados por dia
        {
            TurnoRepository repoTurno = new(_context);
            return repoTurno.ListOfShiftsGroupedByDay(idDoctor);
        }

        //public List<HorarioTurnos> ListOfAvailableShifts(int idDoctor)
        ////Devuelve la lista de turnos disponibles por dia
        //// Este no es usado
        //{
        //    MedicoRepository repTurno = new(_context);

        //    Medico medico = repTurno.GetDoctorForId(idDoctor, repTurno.Get_context()) ?? throw new ArgumentException("El médico no existe");
        //    TimeSpan hora_llegada = medico.HorarioAtencionInicio;
        //    TimeSpan hora_salida = medico.HorarioAtencionFin;

        //    List<TimeSpan> horarios_disponibles = new List<TimeSpan>();

        //    TimeSpan _INTERVALO = TimeSpan.FromHours(1);

        //    for (TimeSpan hora = hora_llegada; hora < hora_salida; hora += _INTERVALO)
        //    {
        //        horarios_disponibles.Add(hora);
        //    }

        //    List<HorarioTurnos> turnos_ocupados = ListOfShiftsGroupedByDay(idDoctor);
        //    List<HorarioTurnos> turnos_disponibles = new();

        //    foreach (var h in turnos_ocupados)
        //    {
        //        List<TimeSpan> horarios_libres = horarios_disponibles.Except(h.Hora_turno).ToList();

        //        HorarioTurnos busyShifts = new()
        //        {
        //            Fecha_turno = h.Fecha_turno,
        //            Hora_turno = horarios_libres
        //        };

        //        turnos_disponibles.Add(busyShifts);
        //    }
        //    return turnos_disponibles;
        //}

        public List<VwTurno> ListOfShiftsOfDate(DateTime fecha)
        //Devuelve la lista de turnos de una fecha
        {
            TurnoRepository repoTurno = new(_context);
            return repoTurno.GetShiftsOfDate(fecha);
        }
      
        public List<DateTime> ListOfDatesWithShiftsOfMonth(int mes)
        //Devuelve la lista de turnos de un mes
        {
            TurnoRepository repoTurno = new(_context);

            return repoTurno.GetDatesWithShiftsOfMonth(mes);
        }
        public TurnosPaciente ListOfShiftsByPatient(int idPaciente)
        //Devuelve la lista de turnos de un paciente
        {
            TurnoRepository repoTurno = new(_context);
            return repoTurno.GetShiftsByPatient(idPaciente);
        }

        /// <summary>
        /// Obtiene la lista de turnos asociados a un paciente específico.
        /// Formato largo. 
        /// </summary>
        /// <param name="idPaciente">Identificador del paciente</param>
        /// <returns>Lista de objetos VwTurno</returns>
        public List<VwTurno> ListOfShiftsByPatientVw(int idPaciente)
        //Devuelve la lista de turnos de un paciente
        {
            TurnoRepository repoTurno = new(_context);
            return repoTurno.GetShiftsByPatientVw(idPaciente);
        }

        public List<Turno> ListOfShiftsByDoctor(int idMedico)
        //Devuelve la lista de turnos de un médico
        {
            TurnoRepository repoTurno = new(_context);
            return repoTurno.GetShiftsByDoctor(idMedico);
        }
        public List<VwTurnoCount> ListOfShiftQtyCurrentYear()
        {
            TurnoRepository repShift = new (_context);
            return repShift.GetQtyShiftYear();
        }

        public List<VwTurnoCount> ListOfShiftQtyCurrentMonth()
        {
            TurnoRepository repShift = new TurnoRepository(_context);
            return repShift.GetQtyShiftMonth();
        }

        public List<VwTurnoXMedicoCount> ListOfShiftByDoctorQtyCurrentYear()
        {
            TurnoRepository repShift = new TurnoRepository(_context);
            return repShift.GetQtyShiftByDoctorYear();
        }

        public List<Dictionary<string, object>> ListOfShiftStateQtyCurrentYear()
        {
            TurnoRepository repShift = new TurnoRepository(_context);
            
            int currentYr = DateTime.Now.Year;

            DataTable dt = repShift.GetPivotTurnoCount(currentYr);

            // Convertir DataTable en Lista
            // Código sugerido por ChatGPT
            List<Dictionary<string, object>> lista = new List<Dictionary<string, object>>();

            foreach (DataRow row in dt.Rows)
            {
                Dictionary<string, object> fila = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    fila[col.ColumnName] = row[col] is DBNull ? 0 : row[col];
                }
                lista.Add(fila);
            }

            return lista;
        }

        public List<CalendarEvent> ListOfCalendarData(string start, string end)
        {
            TurnoRepository repShift = new TurnoRepository(_context);
            return repShift.GetCalendarData(start, end);
        }
    }
}