using System;
using System.Collections.Generic;

namespace DataAccess.Data
{
    /// <summary>
    /// Vista que representa la cantidad de turnos por fecha.
    /// </summary>
    public partial class VwTurnoCalendar
    {
        /// <summary>
        /// Fecha de los turnos.
        /// </summary>
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Cantidad de turnos en la Fecha.
        /// </summary>
        public int Qty { get; set; }
    }
}
