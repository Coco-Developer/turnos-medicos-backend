using Models.CustomModels;
using BusinessLogic.AppLogic.Services;
using DataAccess.Data;
using DataAccess.Repository;
using System.Data;

namespace BusinessLogic.AppLogic
{
    public class TurnoLogic
    {
        private readonly TurnoRepository _turnoRepository;
        private readonly MedicoRepository _medicoRepository;
        private readonly PacienteRepository _pacienteRepository;
        private readonly EmailService _emailService;

        public TurnoLogic(
            TurnoRepository turnoRepository,
            MedicoRepository medicoRepository,
            PacienteRepository pacienteRepository,
            EmailService emailService)
        {
            _turnoRepository = turnoRepository;
            _medicoRepository = medicoRepository;
            _pacienteRepository = pacienteRepository;
            _emailService = emailService;
        }

        // ================= GET ALL =================

        public async Task<List<VwTurno>> ShiftList()
        {
            return await _turnoRepository.GetAllShift();
        }

        // ================= GET BY ID =================

        public async Task<VwTurno> GetShiftForId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            return await _turnoRepository.GetDisplayShiftById(id)
                   ?? throw new ArgumentException("No se encontró el turno");
        }

        // ================= CREATE =================

        public async Task CreateShift(Turno shift)
        {
            if (shift == null)
                throw new ArgumentNullException(nameof(shift));

            if (string.IsNullOrWhiteSpace(shift.Observaciones) || shift.Observaciones == "string")
                shift.Observaciones = "N/D";

            var patient = await _pacienteRepository.GetPatientForId(shift.PacienteId)
                          ?? throw new ArgumentException("Paciente no encontrado");

            var doctor = await _medicoRepository.ReturnDoctorWithSpecialty(shift.MedicoId)
                         ?? throw new ArgumentException("Médico no encontrado");

            await _turnoRepository.CreateShift(shift);

            _emailService.SendShiftConfirmationEmail(shift, patient, doctor);
        }

        // ================= UPDATE =================

        public async Task UpdateShift(int id, Turno shift)
        {
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            var shiftFound = await _turnoRepository.GetShiftById(id)
                             ?? throw new ArgumentException("Turno no encontrado");

            shiftFound.Hora = shift.Hora;
            shiftFound.Fecha = shift.Fecha;
            shiftFound.Observaciones = shift.Observaciones;
            shiftFound.MedicoId = shift.MedicoId;
            shiftFound.PacienteId = shift.PacienteId;
            shiftFound.EstadoId = shift.EstadoId;

            await _turnoRepository.UpdateShift(shiftFound);
        }

        // ================= UPDATE STATUS =================

        public async Task UpdateShiftStatus(int id, int status, int updateSource)
        {
            var shiftFound = await _turnoRepository.GetShiftById(id)
                             ?? throw new ArgumentException("Turno no encontrado");

            if (updateSource == 1)
                shiftFound.Observaciones = "Autogestión Paciente (App Móvil)";

            shiftFound.EstadoId = status;

            await _turnoRepository.UpdateShift(shiftFound);
        }

        // ================= DELETE =================

        public async Task DeleteShift(int id)
        {
            var shiftFound = await _turnoRepository.GetShiftById(id)
                             ?? throw new ArgumentException("Turno no encontrado");

            await _turnoRepository.DeleteShift(shiftFound);
        }

        // ================= LISTS =================

        public async Task<List<HorarioTurnos>> ListOfShiftsGroupedByDay(int idDoctor)
        {
            return await _turnoRepository.ListOfShiftsGroupedByDay(idDoctor);
        }

        public async Task<List<VwTurno>> ListOfShiftsOfDate(DateTime fecha)
        {
            return await _turnoRepository.GetShiftsOfDate(fecha);
        }

        public async Task<List<DateTime>> ListOfDatesWithShiftsOfMonth(int mes)
        {
            return await _turnoRepository.GetDatesWithShiftsOfMonth(mes);
        }

        public async Task<TurnosPaciente> ListOfShiftsByPatient(int idPaciente)
        {
            return await _turnoRepository.GetShiftsByPatient(idPaciente);
        }

        public async Task<List<VwTurno>> ListOfShiftsByPatientVw(int idPaciente)
        {
            return await _turnoRepository.GetShiftsByPatientVw(idPaciente);
        }

        public async Task<List<Turno>> ListOfShiftsByDoctor(int idMedico)
        {
            return await _turnoRepository.GetShiftsByDoctor(idMedico);
        }

        // ================= DASHBOARD =================

        public async Task<List<VwTurnoCount>> ListOfShiftQtyCurrentYear()
        {
            return await _turnoRepository.GetQtyShiftYear();
        }

        public async Task<List<VwTurnoCount>> ListOfShiftQtyCurrentMonth()
        {
            return await _turnoRepository.GetQtyShiftMonth();
        }

        public async Task<List<VwTurnoXMedicoCount>> ListOfShiftByDoctorQtyCurrentYear()
        {
            return await _turnoRepository.GetQtyShiftByDoctorYear();
        }

        public List<Dictionary<string, object>> ListOfShiftStateQtyCurrentYear()
        {
            int currentYr = DateTime.Now.Year;
            DataTable dt = _turnoRepository.GetPivotTurnoCount(currentYr);

            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();

                foreach (DataColumn col in dt.Columns)
                    dict[col.ColumnName] = row[col] is DBNull ? 0 : row[col];

                list.Add(dict);
            }

            return list;
        }

        public async Task<List<CalendarEvent>> ListOfCalendarData(string start, string end)
        {
            return await _turnoRepository.GetCalendarData(start, end);
        }
    }
}