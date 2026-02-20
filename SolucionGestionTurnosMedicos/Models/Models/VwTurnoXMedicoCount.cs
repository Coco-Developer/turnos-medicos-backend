using System;
using System.Collections.Generic;

namespace DataAccess.Data
{
    /// <summary>
    /// Vista que representa el conteo de turnos agrupados por año, médico y estado.
    /// </summary>
    public partial class VwTurnoXMedicoCount
    {
        /// <summary>
        /// Año del conteo de turnos.
        /// </summary>
        public int Yr { get; set; }

        /// <summary>
        /// Apellido y nombre del médico de los turnos contados.
        /// </summary>
        public string Medico { get; set; } = null!;

        /// <summary>
        /// Nombre del estado de los turnos contados.
        /// </summary>
        public string Estado { get; set; } = null!;

        /// <summary>
        /// Clase CSS asociada al estado para representación visual.
        /// </summary>
        public string Clase { get; set; } = null!;

        /// <summary>
        /// Color asociado al estado para representación visual.
        /// </summary>
        public string Color { get; set; } = null!;

        /// <summary>
        /// Cantidad de turnos para el estado, año y médico.
        /// </summary>
        public int CountId { get; set; }
    }
}
