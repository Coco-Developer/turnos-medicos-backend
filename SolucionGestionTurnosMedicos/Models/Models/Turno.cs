using System;
using System.Collections.Generic;

namespace DataAccess.Data
{
    /// <summary>
    /// Representa un turno médico en el sistema de gestión de turnos.
    /// Contiene información sobre el médico, paciente, fecha, hora y estado del turno.
    /// </summary>
    public partial class Turno
    {
        /// <summary>
        /// Identificador único del turno.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador del médico asignado al turno.
        /// Hace referencia a la entidad Medico.
        /// </summary>
        public int MedicoId { get; set; }

        /// <summary>
        /// Identificador del paciente asociado al turno.
        /// Hace referencia a la entidad Paciente.
        /// </summary>
        public int PacienteId { get; set; }

        /// <summary>
        /// Fecha en la que está programado el turno.
        /// </summary>
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Hora específica del turno.
        /// Representada como un intervalo de tiempo (TimeSpan).
        /// </summary>
        public TimeSpan Hora { get; set; }

        /// <summary>
        /// Identificador del estado del turno (ej. Pendiente, Confirmado, Cancelado).
        /// Hace referencia a la entidad Estado.
        /// </summary>
        public int EstadoId { get; set; }

        /// <summary>
        /// Observaciones o notas adicionales relacionadas con el turno.
        /// Este campo es opcional y puede ser nulo.
        /// </summary>
        public string? Observaciones { get; set; }
    }
}
