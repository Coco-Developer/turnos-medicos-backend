using System;
using System.Collections.Generic;

namespace DataAccess.Data
{
    /// <summary>
    /// Vista que representa el conteo de turnos agrupados por año, mes y estado.
    /// </summary>
    public partial class VwTurnoCount
    {
        /// <summary>
        /// Año del conteo de turnos.
        /// </summary>
        public int Yr { get; set; }

        /// <summary>
        /// Mes del conteo de turnos (1-12).
        /// </summary>
        public int Mo { get; set; }

        /// <summary>
        /// Nombre del estado de los turnos contados.
        /// Campo obligatorio.
        /// </summary>
        public string Estado { get; set; } = null!;

        /// <summary>
        /// Clase CSS asociada al estado para representación visual.
        /// Campo obligatorio.
        /// </summary>
        public string Clase { get; set; } = null!;

        /// <summary>
        /// Color asociado al estado para representación visual.
        /// Campo obligatorio.
        /// </summary>
        public string Color { get; set; } = null!;

        /// <summary>
        /// Cantidad de turnos para el estado, año y mes especificados.
        /// </summary>
        public int CountId { get; set; }
    }
}
