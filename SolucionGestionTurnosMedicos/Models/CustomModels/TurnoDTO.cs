using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.CustomModels
{
    public class TurnoDTO
    {
        public DateTime Fecha { get; set; }
        public string Hora { get; set; }

        public string NombreMedico { get; set; }
        public string ApellidoMedico { get; set; }
        public string NombreEspecialidad { get; set; }
    }
}
