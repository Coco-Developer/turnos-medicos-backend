using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace DataAccess.Data
{
    /// <summary>
    /// Representa un médico en el sistema de gestión de turnos médicos.
    /// </summary>
    public partial class Medico
    {
        /// <summary>
        /// Identificador único del médico.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Apellido del médico.
        /// Campo obligatorio.
        /// </summary>
        [Required(ErrorMessage = "Apellido is required")]
        public string Apellido { get; set; } = null!;

        /// <summary>
        /// Nombre del médico.
        /// Campo obligatorio.
        /// </summary>
        [Required(ErrorMessage = "Nombre is required")]
        public string Nombre { get; set; } = null!;

        /// <summary>
        /// Número de teléfono del médico.
        /// Campo obligatorio.
        /// </summary>
        [Required(ErrorMessage = "Telefono is required")]
        public string Telefono { get; set; } = null!;

        /// <summary>
        /// Dirección del médico.
        /// Campo opcional.
        /// </summary>
        [Required(ErrorMessage = "Direccion is required")]
        public string? Direccion { get; set; }

        /// <summary>
        /// Número de DNI del médico.
        /// Campo obligatorio.
        /// </summary>
        [Required(ErrorMessage = "Dni is required")]
        public string Dni { get; set; } = null!;

        /// <summary>
        /// Identificador de la especialidad del médico.
        /// Hace referencia a la entidad Especialidad. Campo obligatorio.
        /// </summary>
        [Required(ErrorMessage = "Especialidad is required")]
        public int EspecialidadId { get; set; }

        /// <summary>
        /// Fecha de alta laboral del médico en el sistema.
        /// </summary>
        [Display(Name = "Fecha de alta laboral")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaAltaLaboral { get; set; }

        /// <summary>
        /// Hora de inicio del horario de atención del médico.
        /// </summary>
        //[Display(Name = "Hora de inicio de atención")]
        //public TimeSpan HorarioAtencionInicio { get; set; }

        /// <summary>
        /// Hora de fin del horario de atención del médico.
        /// </summary>
        //[Display(Name = "Hora de fin de atención")]
        //public TimeSpan HorarioAtencionFin { get; set; }

        /// <summary>
        /// Horario de atención semanal (día por día) del médico.
        /// </summary>
        public virtual ICollection<HorarioMedico> Horarios { get; set; } = new List<HorarioMedico>();

        /// <summary>
        /// Foto del médico. Opcional
        /// Se almacenará la imagen directamente en la tabla.
        /// </summary>
        public byte[]? Foto { get; set; }

        /// <summary>
        /// Matrícula Profesional o Nacional del médico.
        /// DEBE empezar con "MP" o "MN" seguido de números.
        /// </summary>
        public string Matricula { get; set; } = null!;

        /// <summary>
        /// Constructor por defecto de la clase Medico.
        /// </summary>
        public Medico()
        {
        }

        /// <summary>
        /// Constructor parametrizado para inicializar el horario de atención del médico.
        /// </summary>
        /// <param name="horarioInicio">Hora de inicio de atención</param>
        /// <param name="horarioFinal">Hora de fin de atención</param>
        //public Medico(TimeSpan horarioInicio, TimeSpan horarioFinal)
        //{
        //    HorarioAtencionInicio = horarioInicio;
        //    HorarioAtencionFin = horarioFinal;
        //}



    }
}
