using Models.CustomModels;
using BusinessLogic.AppLogic.Services;
using DataAccess.Context;
using DataAccess.Data;
using DataAccess.Repository;
using System.Data;

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

        public async Task<List<VwTurno>> ShiftList()
        {
            TurnoRepository repShift = new TurnoRepository(_context);
            return await repShift.GetAllShift();
        }

        public async Task<VwTurno> GetShiftForId(int id)
        {
            if (id == 0) throw new ArgumentException("Id cannot be 0");

            try
            {
                TurnoRepository repShift = new TurnoRepository(_context);
                VwTurno oShiftFound = await repShift.GetDisplayShiftById(id) ?? throw new ArgumentException("No shift was found with that id");
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
            if (string.IsNullOrEmpty(oShift.Observaciones) || oShift.Observaciones == "string")
            {
                oShift.Observaciones = "N/D";
            }
            #endregion

            TurnoRepository repShift = new(_context);
            MedicoRepository repDoctor = new(_context);
            PacienteRepository repPatient = new(_context);

            // Obtenemos los datos necesarios para el mail (se asume que estos métodos en Repo serán asíncronos)
            Paciente patient = await repPatient.GetPatientForId(oShift.PacienteId);
            MedicoConEspecialidad Odoctor = await repDoctor.ReturnDoctorWithSpecialty(oShift.MedicoId);

            await repShift.CreateShift(oShift);

            // Envío de email (Se mantiene síncrono según tu implementación de EmailService)
            var emailService = new EmailService(new SmtpEmailSender());
            emailService.SendShiftConfirmationEmail(oShift, patient, Odoctor);
        }

        public async Task UpdateShift(int id, Turno oShift)
        {
            TurnoRepository repShift = new(_context);

            try
            {
                Turno oShiftFound = await repShift.GetShiftById(id) ?? throw new ArgumentException("No Shift was found with that id");
                oShiftFound.Hora = oShift.Hora;
                oShiftFound.Fecha = oShift.Fecha;
                oShiftFound.Observaciones = oShift.Observaciones;

                await repShift.UpdateShift(oShiftFound);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Processing failed: {e.Message}");
                throw;
            }
        }

        public async Task UpdateShiftStatus(int id, int status, int updateSource)
        {
            TurnoRepository repShift = new(_context);

            try
            {
                Turno oShiftFound = await repShift.GetShiftById(id) ?? throw new ArgumentException("No Shift was found with that id");

                if (updateSource == 1) // Cambio de estado desde la aplicación móvil
                {
                    oShiftFound.Observaciones = "Autogestión Paciente (App Movil)";
                }

                oShiftFound.EstadoId = status;
                await repShift.UpdateShift(oShiftFound);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Processing failed: {e.Message}");
                throw;
            }
        }

        public async Task DeleteShift(int id)
        {
            TurnoRepository repShift = new TurnoRepository(_context);

            try
            {
                Turno oShiftFound = await repShift.GetShiftById(id) ?? throw new ArgumentException("No shift was found with that id");
                await repShift.DeleteShift(oShiftFound);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Processing failed: {e.Message}");
                throw;
            }
        }

        public async Task<List<HorarioTurnos>> ListOfShiftsGroupedByDay(int idDoctor)
        {
            TurnoRepository repoTurno = new(_context);
            return await repoTurno.ListOfShiftsGroupedByDay(idDoctor);
        }

        public async Task<List<VwTurno>> ListOfShiftsOfDate(DateTime fecha)
        {
            TurnoRepository repoTurno = new(_context);
            return await repoTurno.GetShiftsOfDate(fecha);
        }

        public async Task<List<DateTime>> ListOfDatesWithShiftsOfMonth(int mes)
        {
            TurnoRepository repoTurno = new(_context);
            return await repoTurno.GetDatesWithShiftsOfMonth(mes);
        }

        public async Task<TurnosPaciente> ListOfShiftsByPatient(int idPaciente)
        {
            TurnoRepository repoTurno = new(_context);
            return await repoTurno.GetShiftsByPatient(idPaciente);
        }

        public async Task<List<VwTurno>> ListOfShiftsByPatientVw(int idPaciente)
        {
            TurnoRepository repoTurno = new(_context);
            return await repoTurno.GetShiftsByPatientVw(idPaciente);
        }

        public async Task<List<Turno>> ListOfShiftsByDoctor(int idMedico)
        {
            TurnoRepository repoTurno = new(_context);
            return await repoTurno.GetShiftsByDoctor(idMedico);
        }

        public async Task<List<VwTurnoCount>> ListOfShiftQtyCurrentYear()
        {
            TurnoRepository repShift = new(_context);
            return await repShift.GetQtyShiftYear();
        }

        public async Task<List<VwTurnoCount>> ListOfShiftQtyCurrentMonth()
        {
            TurnoRepository repShift = new TurnoRepository(_context);
            return await repShift.GetQtyShiftMonth();
        }

        public async Task<List<VwTurnoXMedicoCount>> ListOfShiftByDoctorQtyCurrentYear()
        {
            TurnoRepository repShift = new TurnoRepository(_context);
            return await repShift.GetQtyShiftByDoctorYear();
        }

        public List<Dictionary<string, object>> ListOfShiftStateQtyCurrentYear()
        {
            TurnoRepository repShift = new TurnoRepository(_context);
            int currentYr = DateTime.Now.Year;

            // Este sigue siendo síncrono porque devuelve un DataTable dinámico
            DataTable dt = repShift.GetPivotTurnoCount(currentYr);

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

        public async Task<List<CalendarEvent>> ListOfCalendarData(string start, string end)
        {
            TurnoRepository repShift = new TurnoRepository(_context);
            return await repShift.GetCalendarData(start, end);
        }
    }
}