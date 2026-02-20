using System;
using System.Collections.Generic;

namespace DataAccess.Data
{
    /// <summary>
    /// Representa el estado de un turno en el sistema (ej. Activo, Realizado, Cancelado).
    /// </summary>
    public partial class Estado
    {
        /// <summary>
        /// Identificador único del estado.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre descriptivo del estado.
        /// Campo obligatorio.
        /// </summary>
        public string Nombre { get; set; } = null!;

        /// <summary>
        /// Clase CSS asociada al estado para su representación visual.
        /// Campo obligatorio.
        /// </summary>
        public string Clase { get; set; } = null!;

        /// <summary>
        /// Icono asociado al estado para su representación visual.
        /// Campo obligatorio.
        /// </summary>
        public string Icono { get; set; } = null!;

        /// <summary>
        /// Color asociado al estado para su representación visual.
        /// Campo obligatorio.
        /// </summary>
        public string Color { get; set; } = null!;
    }
}
