using BusinessLogic;
using DataAccess.Data;
using DataAccess.Repository;
using Models.CustomModels;
using System;
using System.Threading.Tasks;

namespace ApiGestionTurnosMedicos.Validations
{
    public class ValidationsMethodPut
    {
        private readonly MedicoRepository _repoDoctor;
        private readonly PacienteRepository _repoPatient;
        private readonly TurnoRepository _repoShift;
        private readonly EspecialidadRepository _repoSpecialty;
        private readonly EstadoRepository _repoEstado;

        public ValidationsMethodPut(
            MedicoRepository repoDoctor,
            PacienteRepository repoPatient,
            TurnoRepository repoShift,
            EspecialidadRepository repoSpecialty,
            EstadoRepository repoEstado)
        {
            _repoDoctor = repoDoctor;
            _repoPatient = repoPatient;
            _repoShift = repoShift;
            _repoSpecialty = repoSpecialty;
            _repoEstado = repoEstado;
        }

        #region DOCTOR

        public async Task<ValidationResult> ValidateDoctorAsync(MedicoCustom doctor)
        {
            if (doctor == null)
                return ValidationResult.Failure("Los datos del médico son requeridos.");

            try
            {
                AllValidations validations = new();

                if (!await _repoSpecialty.VerifyIfSpecialtyExistAsync(doctor.EspecialidadId))
                    return ValidationResult.Failure("Especialidad no encontrada.");

                if (string.IsNullOrWhiteSpace(doctor.Nombre))
                    return ValidationResult.Failure("El Nombre no puede quedar vacío.");

                if (!validations.EsSoloLetras(doctor.Nombre))
                    return ValidationResult.Failure("El Nombre solo puede contener letras.");

                if (!validations.EsSoloNumeros(doctor.Dni))
                    return ValidationResult.Failure("El DNI solo puede contener números.");

                if (doctor.FechaAltaLaboral > DateTime.Now)
                    return ValidationResult.Failure("La Fecha de Alta Laboral no puede ser futura.");

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure(ex.Message);
            }
        }

        #endregion

        #region PATIENT

        public async Task<ValidationResult> ValidatePatientAsync(Paciente patient)
        {
            if (patient == null)
                return ValidationResult.Failure("Los datos del paciente son requeridos.");

            try
            {
                AllValidations validations = new();

                if (string.IsNullOrWhiteSpace(patient.Nombre))
                    return ValidationResult.Failure("El Nombre no puede quedar vacío.");

                if (!validations.EsFormatoEmailValido(patient.Email))
                    return ValidationResult.Failure("Formato de Email inválido.");

                if (!validations.EsFechaNacimientoValida(patient.FechaNacimiento))
                    return ValidationResult.Failure("Fecha de nacimiento inválida.");

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure(ex.Message);
            }
        }

        #endregion

        #region STATUS

        public async Task<ValidationResult> ValidateStatusAsync(Estado status)
        {
            if (status == null)
                return ValidationResult.Failure("El estado es requerido.");

            try
            {
                var estado = await _repoEstado.GetEstadoByIdAsync(status.Id);

                if (estado == null)
                    return ValidationResult.Failure("El Estado no existe.");

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure(ex.Message);
            }
        }

        #endregion

        #region SHIFT

        public async Task<ValidationResult> ValidateShiftAsync(TurnoCustom turno)
        {
            if (turno == null)
                return ValidationResult.Failure("Los datos del turno son requeridos.");

            try
            {
                if (!await _repoDoctor.VerifyIfDoctorExistReturnBool(turno.MedicoId))
                    return ValidationResult.Failure("El médico no existe.");

                if (!await _repoPatient.VerifyIfPatientExistByIdAsync(turno.PacienteId))
                    return ValidationResult.Failure("El paciente no existe.");

                if (!DateTime.TryParse(turno.Fecha, out DateTime fecha))
                    return ValidationResult.Failure("Formato de fecha inválido.");

                if (!TimeSpan.TryParse(turno.Hora, out TimeSpan hora))
                    return ValidationResult.Failure("Formato de hora inválido.");

                if (fecha.Add(hora) < DateTime.Now)
                    return ValidationResult.Failure("No se puede asignar turnos en el pasado.");

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure(ex.Message);
            }
        }

        #endregion
    }
}