using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Data
{
    /// <summary>
    /// Representa una especialidad médica en el sistema de gestión de turnos.
    /// </summary>
    public partial class Especialidad
    {
        /// <summary>
        /// Identificador único de la especialidad.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre de la especialidad médica.
        /// Campo obligatorio.
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        public string Nombre { get; set; } = null!;
    }
}
