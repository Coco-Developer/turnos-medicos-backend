using BusinessLogic;
using DataAccess.Data;
using DataAccess.Repository;
using Models.CustomModels;
using System;
using System.Threading.Tasks;

namespace ApiGestionTurnosMedicos.Validations
{
    public class ValidationsMethodPost
    {
        private readonly MedicoRepository _repoDoctor;
        private readonly PacienteRepository _repoPatient;
        private readonly TurnoRepository _repoShift;
        private readonly EspecialidadRepository _repoSpecialty;

        public ValidationsMethodPost(
            MedicoRepository repoDoctor,
            PacienteRepository repoPatient,
            TurnoRepository repoShift,
            EspecialidadRepository repoSpecialty)
        {
            _repoDoctor = repoDoctor;
            _repoPatient = repoPatient;
            _repoShift = repoShift;
            _repoSpecialty = repoSpecialty;
        }

        #region DOCTOR

        public async Task<ValidationResult> ValidateDoctorAsync(MedicoCustom doctor)
        {
            if (doctor == null)
                return ValidationResult.Failure("Los datos del médico son requeridos.");

            try
            {
                AllValidations validations = new AllValidations();

                if (string.IsNullOrWhiteSpace(doctor.Nombre))
                    return ValidationResult.Failure("El Nombre no puede quedar vacío.");

                if (string.IsNullOrWhiteSpace(doctor.Apellido))
                    return ValidationResult.Failure("El Apellido no puede quedar vacío.");

                if (string.IsNullOrWhiteSpace(doctor.Dni))
                    return ValidationResult.Failure("El DNI no puede quedar vacío.");

                if (!validations.EsSoloLetras(doctor.Nombre))
                    return ValidationResult.Failure("El Nombre solo puede contener letras.");

                if (!validations.EsSoloLetras(doctor.Apellido))
                    return ValidationResult.Failure("El Apellido solo puede contener letras.");

                if (!validations.EsSoloNumeros(doctor.Dni))
                    return ValidationResult.Failure("El DNI solo puede contener números.");

                if (doctor.FechaAltaLaboral > DateTime.Now)
                    return ValidationResult.Failure("La Fecha de Alta no puede ser futura.");

                if (doctor.EspecialidadId <= 0)
                    return ValidationResult.Failure("Debe seleccionar una especialidad válida.");

                if (await _repoDoctor.VerifyIfDoctorExist(doctor.Nombre, doctor.Dni))
                    return ValidationResult.Failure("El médico ya existe.");

                if (!await _repoSpecialty.VerifyIfSpecialtyExistAsync(doctor.EspecialidadId))
                    return ValidationResult.Failure("La especialidad no existe.");

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
                AllValidations validations = new AllValidations();

                if (string.IsNullOrWhiteSpace(patient.Nombre))
                    return ValidationResult.Failure("El Nombre no puede quedar vacío.");

                if (string.IsNullOrWhiteSpace(patient.Email))
                    return ValidationResult.Failure("El Email no puede quedar vacío.");

                if (!validations.EsFormatoEmailValido(patient.Email))
                    return ValidationResult.Failure("Formato de Email inválido.");

                if (await _repoPatient.VerifyIfPatientExistAsync(patient.Nombre, patient.Dni))
                    return ValidationResult.Failure("El paciente ya existe.");

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

        #region SHIFT

        public async Task<ValidationResult> ValidateShiftAsync(TurnoCustom turno)
        {
            if (turno == null)
                return ValidationResult.Failure("Los datos del turno son requeridos.");

            try
            {
                if (turno.MedicoId <= 0)
                    return ValidationResult.Failure("Debe elegir un médico.");

                if (turno.PacienteId <= 0)
                    return ValidationResult.Failure("Debe elegir un paciente.");

                if (!await _repoDoctor.VerifyIfDoctorExistReturnBool(turno.MedicoId))
                    return ValidationResult.Failure("El médico no existe.");

                if (!await _repoPatient.VerifyIfPatientExistByIdAsync(turno.PacienteId))
                    return ValidationResult.Failure("El paciente no existe.");

                if (!DateTime.TryParse(turno.Fecha, out DateTime fecha))
                    return ValidationResult.Failure("Formato de fecha inválido.");

                if (!TimeSpan.TryParse(turno.Hora, out TimeSpan hora))
                    return ValidationResult.Failure("Formato de hora inválido.");

                if (fecha.Add(hora) < DateTime.Now)
                    return ValidationResult.Failure("No se pueden agendar turnos en el pasado.");

                if (await _repoShift.VerifyIfShiftExist(turno.MedicoId, fecha, hora, turno.Id))
                    return ValidationResult.Failure("El turno ya está ocupado.");

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