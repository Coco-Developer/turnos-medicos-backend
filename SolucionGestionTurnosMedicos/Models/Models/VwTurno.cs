using System;
using System.Collections.Generic;

namespace DataAccess.Data
{
    /// <summary>
    /// Vista que representa un turno médico con información extendida de paciente, médico y estado.
    /// </summary>
    public partial class VwTurno
    {
        /// <summary>
        /// Identificador único del turno.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador del médico asignado al turno.
        /// </summary>
        public int MedicoId { get; set; }

        /// <summary>
        /// Identificador del paciente asociado al turno.
        /// </summary>
        public int PacienteId { get; set; }

        /// <summary>
        /// Fecha del turno.
        /// </summary>
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Hora del turno representada como cadena.
        /// Campo obligatorio.
        /// </summary>
        public string Hora { get; set; } = null!;

        /// <summary>
        /// Identificador del estado del turno.
        /// </summary>
        public int EstadoId { get; set; }

        /// <summary>
        /// Observaciones adicionales del turno.
        /// Campo opcional.
        /// </summary>
        public string? Observaciones { get; set; }

        /// <summary>
        /// Nombre completo del paciente.
        /// Campo obligatorio.
        /// </summary>
        public string Paciente { get; set; } = null!;

        /// <summary>
        /// Teléfono del paciente.
        /// Campo obligatorio.
        /// </summary>
        public string PacienteTelefono { get; set; } = null!;

        /// <summary>
        /// Correo electrónico del paciente.
        /// Campo obligatorio.
        /// </summary>
        public string PacienteEmail { get; set; } = null!;

        /// <summary>
        /// Número de DNI del paciente.
        /// Campo obligatorio.
        /// </summary>
        public string PacienteDni { get; set; } = null!;

        /// <summary>
        /// Nombre completo del médico.
        /// Campo obligatorio.
        /// </summary>
        public string Medico { get; set; } = null!;

        /// <summary>
        /// Nombre del estado del turno.
        /// Campo obligatorio.
        /// </summary>
        public string Estado { get; set; } = null!;

        /// <summary>
        /// Clase CSS del estado para representación visual.
        /// Campo obligatorio.
        /// </summary>
        public string EstadoClase { get; set; } = null!;

        /// <summary>
        /// Icono del estado para representación visual.
        /// Campo obligatorio.
        /// </summary>
        public string EstadoIcono { get; set; } = null!;

        /// <summary>
        /// Identificador de la especialidad del médico.
        /// </summary>
        public int EspecialidadId { get; set; }

        /// <summary>
        /// Nombre de la especialidad del médico.
        /// Campo obligatorio.
        /// </summary>
        public string Especialidad { get; set; } = null!;

        /// <summary>
        /// Foto del médico.
        /// </summary>
        public byte[] Foto { get; set; } = null!;
    }
}
