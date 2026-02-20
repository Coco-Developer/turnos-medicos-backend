using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DataAccess.Data
{
    /// <summary>
    /// Representa el horario de un médico en el sistema de gestión de turnos médicos.
    /// </summary>
    public class HorarioMedico
    {
        /// <summary>
        /// Identificador único del horario.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Identificador del médico. Es una clave externa.
        /// </summary>
        //[ForeignKey("Medico")]
        public int MedicoId { get; set; }

        //[JsonIgnore]
        //public Medico? Medico { get; set; }

        /// <summary>
        /// Número de día de semana del horario de atención del médico (1=Lunes, 7=Domingo)
        /// </summary>
        [Range(1, 7, ErrorMessage = "El día de la semana debe estar entre 1 y 7")]
        public byte DiaSemana { get; set; } // 1=Lunes, 7=Domingo

        /// <summary>
        /// Hora de inicio del horario de atención del médico en un día de semana específico
        /// </summary>
        [Display(Name = "Hora de inicio de atención")]
        public TimeSpan? HorarioAtencionInicio { get; set; }

        /// <summary>
        /// Hora de fin del horario de atención del médico en un día de semana específico.
        /// </summary>
        [Display(Name = "Hora de fin de atención")]
        public TimeSpan? HorarioAtencionFin { get; set; }
    }
}