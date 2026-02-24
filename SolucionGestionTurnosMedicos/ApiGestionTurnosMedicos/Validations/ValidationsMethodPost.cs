using Models.CustomModels;
using BusinessLogic;
using DataAccess.Data;
using DataAccess.Repository;
using System;
using System.Linq;
using DataAccess.Context;
using System.Threading.Tasks;

namespace ApiGestionTurnosMedicos.Validations
{
    /// <summary>
    /// Clase para realizar validaciones específicas en operaciones POST del sistema de gestión de turnos.
    /// </summary>
    public class ValidationsMethodPost
    {
        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        public ValidationsMethodPost(GestionTurnosContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        #endregion

        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }

        public ValidationsMethodPost() { }

        /// <summary>
        /// Realiza validaciones para la creación de un médico.
        /// </summary>
        public async Task<ValidationsMethodPost> ValidationsMethodPostDoctor(MedicoCustom oDoctor)
        {
            // Protección contra objeto nulo (Evita Error 500)
            if (oDoctor == null)
                return new ValidationsMethodPost { IsValid = false, ErrorMessage = "Los datos del médico son requeridos." };

            MedicoRepository repoDoctor = new MedicoRepository(_context);
            EspecialidadRepository repSpecialty = new EspecialidadRepository(_context);
            AllValidations validations = new AllValidations();

            try
            {
                #region Validaciones de campo (Null or WhiteSpace)
                if (string.IsNullOrWhiteSpace(oDoctor.Nombre))
                    throw new ArgumentException("El Nombre no puede quedar vacío.");

                if (string.IsNullOrWhiteSpace(oDoctor.Apellido))
                    throw new ArgumentException("El Apellido no puede quedar vacío.");

                if (string.IsNullOrWhiteSpace(oDoctor.Dni))
                    throw new ArgumentException("El DNI no puede quedar vacío.");
                #endregion

                #region Validaciones lógicas y formato
                if (!validations.EsSoloLetras(oDoctor.Nombre))
                    throw new ArgumentException("El Nombre solo puede contener letras y espacios.");

                if (!validations.EsSoloLetras(oDoctor.Apellido))
                    throw new ArgumentException("El Apellido solo puede contener letras y espacios.");

                if (!validations.EsSoloNumeros(oDoctor.Dni))
                    throw new ArgumentException("El DNI solo puede contener números.");

                if (!string.IsNullOrEmpty(oDoctor.Telefono) && !validations.EsSoloNumeros(oDoctor.Telefono))
                    throw new ArgumentException("El Teléfono solo puede contener números.");

                if (oDoctor.FechaAltaLaboral > DateTime.Now)
                    throw new ArgumentException("La Fecha de Alta Laboral no puede ser en el futuro.");

                if (oDoctor.EspecialidadId <= 0)
                    throw new ArgumentException("Debe seleccionar una Especialidad válida.");
                #endregion

                #region Validaciones de existencia en Base de Datos
                // Await necesario para métodos asíncronos del repositorio
                if (await repoDoctor.VerifyIfDoctorExist(oDoctor.Nombre, oDoctor.Dni))
                    throw new ArgumentException("El Médico ya se encuentra registrado en el sistema.");

                if (!await repSpecialty.VerifyIfSpecialtyExistAsync(oDoctor.EspecialidadId))
                    throw new ArgumentException("La Especialidad seleccionada no existe.");
                #endregion

                return new ValidationsMethodPost { IsValid = true };
            }
            catch (Exception e)
            {
                // Captura el mensaje exacto del throw
                return new ValidationsMethodPost { IsValid = false, ErrorMessage = e.Message };
            }
        }

        /// <summary>
        /// Realiza validaciones para la creación de un paciente.
        /// </summary>
        public async Task<ValidationsMethodPost> ValidationsMethodPostPatient(Paciente oPatient)
        {
            if (oPatient == null)
                return new ValidationsMethodPost { IsValid = false, ErrorMessage = "Los datos del paciente son requeridos." };

            AllValidations validations = new AllValidations();
            PacienteRepository repoPatient = new PacienteRepository(_context);

            try
            {
                if (string.IsNullOrWhiteSpace(oPatient.Nombre))
                    throw new ArgumentException("El Nombre no puede quedar vacío.");

                if (string.IsNullOrWhiteSpace(oPatient.Email))
                    throw new ArgumentException("El Email no puede quedar vacío.");

                if (!validations.EsFormatoEmailValido(oPatient.Email))
                    throw new ArgumentException("El formato del Email no es válido.");

                if (await repoPatient.VerifyIfPatientExistAsync(oPatient.Nombre, oPatient.Dni))
                    throw new ArgumentException("El paciente ya existe.");

                if (!validations.EsFechaNacimientoValida(oPatient.FechaNacimiento))
                    throw new ArgumentException("La Fecha de Nacimiento no puede ser en el futuro.");

                return new ValidationsMethodPost { IsValid = true };
            }
            catch (Exception e)
            {
                return new ValidationsMethodPost { IsValid = false, ErrorMessage = e.Message };
            }
        }

        /// <summary>
        /// Realiza validaciones para la creación de un turno.
        /// </summary>
        public async Task<ValidationsMethodPost> ValidationsMethodPostShift(TurnoCustom turno)
        {
            if (turno == null)
                return new ValidationsMethodPost { IsValid = false, ErrorMessage = "Los datos del turno son requeridos." };

            try
            {
                AllValidations validations = new AllValidations();
                MedicoRepository repoDoctor = new MedicoRepository(_context);
                PacienteRepository repoPatient = new PacienteRepository(_context);
                TurnoRepository repoShift = new TurnoRepository(_context);

                #region Validaciones de existencia de IDs
                if (turno.MedicoId <= 0) throw new ArgumentException("Debe elegir un médico.");
                if (turno.PacienteId <= 0) throw new ArgumentException("Debe elegir un paciente.");

                if (!await repoDoctor.VerifyIfDoctorExistReturnBool(turno.MedicoId))
                    throw new ArgumentException("El médico seleccionado no existe.");

                if (!await repoPatient.VerifyIfPatientExistByIdAsync(turno.PacienteId))
                    throw new ArgumentException("El paciente seleccionado no existe.");
                #endregion

                #region Validaciones de Fecha y Disponibilidad
                if (string.IsNullOrWhiteSpace(turno.Fecha) || string.IsNullOrWhiteSpace(turno.Hora))
                    throw new ArgumentException("La fecha y la hora son obligatorias.");

                if (!DateTime.TryParse(turno.Fecha, out DateTime fechaParsed))
                    throw new ArgumentException("Formato de fecha no válido.");

                if (!TimeSpan.TryParse(turno.Hora, out TimeSpan horaParsed))
                    throw new ArgumentException("Formato de hora no válido.");

                DateTime fechaYHoraTurno = fechaParsed.Add(horaParsed);

                if (fechaYHoraTurno < DateTime.Now)
                    throw new ArgumentException("No se pueden agendar turnos en el pasado.");

                // Verificación de choque de horarios
                if (await repoShift.VerifyIfShiftExist(turno.MedicoId, fechaParsed, horaParsed, turno.Id))
                {
                    throw new ArgumentException("El turno solicitado ya está ocupado por otro paciente.");
                }
                #endregion

                return new ValidationsMethodPost { IsValid = true };
            }
            catch (Exception e)
            {
                return new ValidationsMethodPost { IsValid = false, ErrorMessage = e.Message };
            }
        }
    }
}