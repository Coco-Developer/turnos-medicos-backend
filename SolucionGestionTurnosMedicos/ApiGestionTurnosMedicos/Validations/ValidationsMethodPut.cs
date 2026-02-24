using Models.CustomModels;
using BusinessLogic;
using DataAccess.Context;
using DataAccess.Data;
using DataAccess.Repository;
using System;
using System.Threading.Tasks;

namespace ApiGestionTurnosMedicos.Validations
{
    /// <summary>
    /// Clase para realizar validaciones específicas en operaciones PUT del sistema de gestión de turnos.
    /// </summary>
    public class ValidationsMethodPut
    {
        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        /// <summary>
        /// Constructor que inicializa el contexto de la base de datos.
        /// </summary>
        /// <param name="context">Contexto de la base de datos para operaciones de validación</param>
        public ValidationsMethodPut(GestionTurnosContext context)
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
        public ValidationsMethodPut() { }

        /// <summary>
        /// Realiza validaciones para la actualización de un médico en el sistema.
        /// </summary>
        /// <param name="oDoctor">Objeto MedicoCustom con los datos del médico</param>
        /// <returns>Objeto ValidationsMethodPut con el resultado de la validación</returns>
        public async Task<ValidationsMethodPut> ValidationsMethodPutDoctor(MedicoCustom oDoctor)
        {
            MedicoRepository repoDoctor = new MedicoRepository(_context);
            EspecialidadRepository repSpecialty = new EspecialidadRepository(_context);
            AllValidations validations = new AllValidations();
            Medico doctor = new Medico();

            try
            {
                #region Validaciones de existencia

                // Corregido: Agregado await y nombre de método asíncrono
                if (!await repSpecialty.VerifyIfSpecialtyExistAsync(oDoctor.EspecialidadId))
                    throw new ArgumentException("Especialidad no encontrada.");

                #endregion

                #region Validaciones de campo

                if (!validations.EsStringNoVacio(oDoctor.Nombre))
                    throw new ArgumentException("El Nombre no puede quedar vacío.");
                if (!validations.EsStringNoVacio(oDoctor.Dni))
                    throw new ArgumentException("El DNI no puede quedar vacío.");

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

                if (oDoctor.FechaAltaLaboral > DateTime.Now)
                    throw new ArgumentException("La Fecha de Alta Laboral no puede ser en el futuro.");

                if (oDoctor.EspecialidadId == 0)
                    throw new ArgumentException("El ID de la Especialidad no puede ser cero.");

                #endregion

                return new ValidationsMethodPut { IsValid = true };
            }
            catch (Exception e)
            {
                return new ValidationsMethodPut { IsValid = false, ErrorMessage = e.Message };
            }
        }

        /// <summary>
        /// Realiza validaciones para la actualización de un paciente en el sistema.
        /// </summary>
        /// <param name="oPatient">Objeto Paciente con los datos del paciente</param>
        /// <returns>Objeto ValidationsMethodPut con el resultado de la validación</returns>
        public async Task<ValidationsMethodPut> ValidationsMethodPutPatient(Paciente oPatient)
        {
            AllValidations validations = new AllValidations();

            try
            {
                PacienteRepository repoPatient = new PacienteRepository(_context);

                #region Validaciones de campo

                if (!validations.EsStringNoVacio(oPatient.Nombre))
                    throw new ArgumentException("El Nombre no puede quedar vacío.");
                if (!validations.EsStringNoVacio(oPatient.Apellido))
                    throw new ArgumentException("El Apellido no puede quedar vacío");
                if (!validations.EsStringNoVacio((oPatient.FechaNacimiento).ToString()))
                    throw new ArgumentException("La Fecha de Nacimiento no puede quedar vacía.");
                if (!validations.EsStringNoVacio(oPatient.Email))
                    throw new ArgumentException("El Email no puede quedar vacío.");
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

                return new ValidationsMethodPut { IsValid = true };
            }
            catch (Exception e)
            {
                return new ValidationsMethodPut { IsValid = false, ErrorMessage = e.Message };
            }
        }

        /// <summary>
        /// Realiza validaciones para la actualización de un estado en el sistema.
        /// </summary>
        /// <param name="oStatus">Objeto Estado con los datos del estado</param>
        /// <returns>Objeto ValidationsMethodPut con el resultado de la validación</returns>
        public async Task<ValidationsMethodPut> ValidationMethodPutStatus(Estado oStatus)
        {
            try
            {
                EstadoRepository repoEstado = new(_context);

                // Corregido: Agregado await para obtener el resultado de la base de datos
                var estado = await repoEstado.GetEstadoByIdAsync(oStatus.Id);
                if (estado == null)
                    throw new ArgumentException("El Estado no existe.");

                return new ValidationsMethodPut { IsValid = true };

            }
            catch (Exception e)
            {
                return new ValidationsMethodPut { IsValid = false, ErrorMessage = e.Message };
            }
        }

        /// <summary>
        /// Realiza validaciones para la actualización de un turno en el sistema.
        /// </summary>
        /// <param name="turno">Objeto TurnoCustom con los datos del turno</param>
        /// <returns>Objeto ValidationsMethodPut con el resultado de la validación</returns>
        public async Task<ValidationsMethodPut> ValidationsMethodPutShift(TurnoCustom turno)
        {
            try
            {
                AllValidations validations = new AllValidations();
                MedicoRepository repoDoctor = new MedicoRepository(_context);
                PacienteRepository repoPatient = new PacienteRepository(_context);
                TurnoRepository repoShift = new TurnoRepository(_context);
                EstadoRepository repoState = new EstadoRepository(_context);

                #region Validaciones de existencia

                // Corregido: Agregado await para los métodos que devuelven Task<bool>
                if (!await repoDoctor.VerifyIfDoctorExistReturnBool(turno.MedicoId))
                    throw new ArgumentException("El médico seleccionado no existe");

                if (!await repoPatient.VerifyIfPatientExistByIdAsync(turno.PacienteId))
                    throw new ArgumentException("El paciente seleccionado no existe");

                #endregion

                #region Validaciones de campo

                if (!validations.EsStringNoVacio((turno.Fecha).ToString()))
                    throw new ArgumentException("Debe elegir una fecha para su turno");
                if (!validations.EsStringNoVacio((turno.Hora).ToString()))
                    throw new ArgumentException("Debe elegir una hora para su turno");
                if (!validations.EsStringNoVacio((turno.MedicoId).ToString()))
                    throw new ArgumentException("Debe elegir un médico");
                if (!validations.EsStringNoVacio((turno.PacienteId).ToString()))
                    throw new ArgumentException("Debe elegir un paciente para el turno");

                #endregion

                #region Validaciones lógicas

                DateTime fechaTurno = DateTime.Parse(turno.Fecha);
                TimeSpan horaTurno = TimeSpan.Parse(turno.Hora);
                DateTime fechaYHoraTurno = fechaTurno.Add(horaTurno);

                if (fechaYHoraTurno < DateTime.Now)
                {
                    throw new ArgumentException("La fecha y hora del turno no pueden ser en el pasado");
                }

                #endregion

                return new ValidationsMethodPut { IsValid = true };
            }
            catch (Exception e)
            {
                return new ValidationsMethodPut { IsValid = false, ErrorMessage = e.Message };
            }
        }
    }
}