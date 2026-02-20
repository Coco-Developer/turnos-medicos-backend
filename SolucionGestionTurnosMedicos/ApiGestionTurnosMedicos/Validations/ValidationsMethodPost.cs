using ApiGestionTurnosMedicos.CustomModels;
using BusinessLogic;
using DataAccess.Data;
using DataAccess.Repository;
using Models.CustomModels;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Linq;

namespace ApiGestionTurnosMedicos.Validations
{
    /// <summary>
    /// Clase para realizar validaciones específicas en operaciones POST del sistema de gestión de turnos.
    /// </summary>
    public class ValidationsMethodPost
    {

        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        /// <summary>
        /// Constructor que inicializa el contexto de la base de datos.
        /// </summary>
        /// <param name="context">Contexto de la base de datos para operaciones de validación</param>
        public ValidationsMethodPost(GestionTurnosContext context)
        {
            _context = context;
        }
        #endregion

        /// <summary>
        /// Indica si las validaciones fueron exitosas.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Mensaje de error si las validaciones fallan.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public ValidationsMethodPost()
        {
        }

        /// <summary>
        /// Realiza validaciones para la creación de un médico en el sistema.
        /// </summary>
        /// <param name="oDoctor">Objeto MedicoCustom con los datos del médico</param>
        /// <returns>Objeto ValidationsMethodPost con el resultado de la validación</returns>
        public ValidationsMethodPost ValidationsMethodPostDoctor(MedicoCustom oDoctor)
        {
            MedicoRepository repoDoctor = new MedicoRepository(_context);
            EspecialidadRepository repSpecialty = new EspecialidadRepository(_context);
            AllValidations validations = new AllValidations();
            Medico doctor = new Medico();

            //doctor.HorarioAtencionInicio = oDoctor.ModifyStartTime(oDoctor.HorarioAtencionInicio);
            //doctor.HorarioAtencionFin = oDoctor.ModifyEndTime(oDoctor.HorarioAtencionFin);
            try
            {
                #region Validaciones de existencia

                if (repoDoctor.VerifyIfDoctorExist(oDoctor.Nombre, oDoctor.Dni))
                    throw new ArgumentException("El Médico ya existe.");
                if (!repSpecialty.VerifyIfSpecialtyExist(oDoctor.EspecialidadId))
                    throw new ArgumentException("Especialidad no encontrada.");

                #endregion

                #region Validaciones de campo

                if (!validations.EsStringNoVacio(oDoctor.Nombre))
                    throw new ArgumentException("El Nombre no puede quedar vacío.");
                if (!validations.EsStringNoVacio(oDoctor.Dni))
                    throw new ArgumentException("EL DNI no puede quedar vacío.");

                #endregion

                #region Validaciones lógicas
                if (!validations.EsSoloLetras(oDoctor.Nombre))
                    throw new ArgumentException("El Nombre solo puede contener letras y espacios.");
                if (!validations.EsSoloLetras(oDoctor.Apellido))
                    throw new ArgumentException("El Apellido solo puede contener letras y espacios.");
                if (!validations.EsSoloNumeros(oDoctor.Dni))
                    throw new ArgumentException("El DNI solo puede contener números.");
                if (!validations.EsSoloNumeros(oDoctor.Telefono))
                    throw new ArgumentException("El Teléfono solo puede contener números.");

                // Comentado por ahora (20250921) - Requiere revisión
                // Habrá que revisar toda la lista de horarios para que cumplan
                // con el horario permitido que tendría que ir hasta las 20 más o menos
                //if (doctor.HorarioAtencionInicio < new TimeSpan(7, 0, 0) || doctor.HorarioAtencionInicio > new TimeSpan(18, 0, 0))
                //{
                //    throw new ArgumentException("El Inicio de Atención debe estar entre las 07:00 y las 18:00");
                //}
                if (oDoctor.FechaAltaLaboral > DateTime.Now)
                    throw new ArgumentException("La Fecha de Alta Laboral no puede ser en el futuro.");

                if (oDoctor.EspecialidadId == 0)
                    throw new ArgumentException("El ID de la Especialidad no puede ser cero.");

                #endregion

                return new ValidationsMethodPost { IsValid = true };
            }
            catch (Exception e)
            {
                return new ValidationsMethodPost { IsValid = false, ErrorMessage = e.Message };
            }
        }

        /// <summary>
        /// Realiza validaciones para la creación de un paciente en el sistema.
        /// </summary>
        /// <param name="oPatient">Objeto Paciente con los datos del paciente</param>
        /// <returns>Objeto ValidationsMethodPost con el resultado de la validación</returns>
        public ValidationsMethodPost ValidationsMethodPostPatient(Paciente oPatient)
        {
            AllValidations validations = new AllValidations();

            try
            {
                PacienteRepository repoPatient = new PacienteRepository(_context);

                #region Validaciones de existencia
                if (repoPatient.VerifyIfPatientExist(oPatient.Nombre, oPatient.Dni))
                    throw new ArgumentException("El paciente ya existe.");

                #endregion

                #region Validaciones de campo

                if (!validations.EsStringNoVacio(oPatient.Nombre))
                    throw new ArgumentException("El Nombre no puede quedar vacío.");
                if (!validations.EsStringNoVacio(oPatient.Apellido))
                    throw new ArgumentException("El Apellido no puede quedar vacío");
                if (!validations.EsStringNoVacio(oPatient.FechaNacimiento.ToString()))
                    throw new ArgumentException("La Fecha de Nacimiento no puede quedar vacía.");
                if (!validations.EsStringNoVacio(oPatient.Email))
                //    throw new ArgumentException("El Email no puede quedar vacío.");
                //if (!validations.EsStringNoVacio(oPatient.Dni))
                    throw new ArgumentException("EL DNI no puede quedar vacío.");
                if (!validations.EsStringNoVacio(oPatient.Telefono))
                    throw new ArgumentException("El Teléfono no puede quedar vacío.");

                #endregion

                #region Validaciones lógicas
                if (!validations.EsFormatoEmailValido(oPatient.Email))
                    throw new ArgumentException("El Email no tiene un formato válido.");
                if (!validations.EsSoloLetras(oPatient.Nombre))
                    throw new ArgumentException("El Nombre solo puede contener letras y espacios.");
                if (!validations.EsSoloLetras(oPatient.Apellido))
                    throw new ArgumentException("El Apellido solo puede contener letras y espacios.");
                if (!int.TryParse(oPatient.Dni.ToString(), out _))
                {
                    throw new ArgumentException("El DNI solo puede contener números.");
                }
                if (!validations.EsSoloNumeros(oPatient.Telefono))
                    throw new ArgumentException("El Teléfono solo puede contener números.");
                if (!validations.EsFechaNacimientoValida(oPatient.FechaNacimiento))
                    throw new ArgumentException("La Fecha de Nacimiento no puede ser en el futuro.");

                #endregion

                return new ValidationsMethodPost { IsValid = true };
            }
            catch (Exception e)
            {
                return new ValidationsMethodPost { IsValid = false, ErrorMessage = e.Message };
            }
        }

        /// <summary>
        /// Realiza validaciones para la creación de un turno en el sistema.
        /// </summary>
        /// <param name="turno">Objeto TurnoCustom con los datos del turno</param>
        /// <returns>Objeto ValidationsMethodPost con el resultado de la validación</returns>
        public ValidationsMethodPost ValidationsMethodPostShift(TurnoCustom turno)
        {
            try
            {
                AllValidations validations = new AllValidations();
                MedicoRepository repoDoctor = new MedicoRepository(_context);
                PacienteRepository repoPatient = new PacienteRepository(_context);
                TurnoRepository repoShift = new TurnoRepository(_context);
                EstadoRepository repoState = new EstadoRepository(_context);

                #region Validaciones de existencia

                if (!repoDoctor.VerifyIfDoctorExistReturnBool(turno.MedicoId))
                    throw new ArgumentException("El médico seleccionado no existe");
                if (!repoPatient.VerifyIfPatientExistById(turno.PacienteId))
                    throw new ArgumentException("El paciente seleccionado no existe");

                #endregion

                #region Validaciones de campo

                if (!validations.EsStringNoVacio(turno.Fecha.ToString()))
                    throw new ArgumentException("Debe elegir una fecha para su turno");
                if (!validations.EsStringNoVacio(turno.Hora.ToString()))
                    throw new ArgumentException("Debe elegir una hora para su turno");
                if (!validations.EsStringNoVacio(turno.MedicoId.ToString()))
                    throw new ArgumentException("Debe elegir un médico");
                if (!validations.EsStringNoVacio(turno.PacienteId.ToString()))
                    throw new ArgumentException("Debe elegir un paciente para el turno");

                #endregion

                #region Validaciones lógicas

                DateTime fechaTurno = DateTime.Parse(turno.Fecha); // Fecha del turno
                TimeSpan horaTurno = TimeSpan.Parse(turno.Hora);   // Hora del turno
                DateTime fechaYHoraTurno = fechaTurno.Add(horaTurno); // Combinas fecha y hora

                // Comparación de fecha y hora del turno con el momento actual
                if (fechaYHoraTurno < DateTime.Now)
                {
                    throw new ArgumentException("La fecha y hora del turno no pueden ser en el pasado");
                }

                // Valida que la hora esté dentro del rango horario del médico                

                // (20250921 - AML) - Revisar
                // Acá habría que tomar el día de semana de la fecha y verificar
                // si el horario está dentro del rango que el médico trabaja ese día
                //Medico doctorHorario = repoDoctor.ReturnHorariosForDoctor(turno.MedicoId);
                //if (horaTurno < doctorHorario.HorarioAtencionInicio || horaTurno > doctorHorario.HorarioAtencionFin)
                //{
                //    throw new ArgumentException("La fecha del turno debe estar dentro del rango horario del médico");
                //}

                // Valida que no existan dos turnos con la misma fecha y hora, excluyendo el turno actual si está siendo modificado
                if (repoShift.VerifyIfShiftExist(turno.MedicoId, fechaTurno, horaTurno, turno.Id))
                {
                    throw new ArgumentException("El turno solicitado ya está ocupado");
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
