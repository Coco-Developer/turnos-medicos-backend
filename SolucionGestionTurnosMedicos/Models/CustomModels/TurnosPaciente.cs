using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Data;

namespace Models.CustomModels
{
    public class TurnosPaciente
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }        
        public List<TurnoDTO> Turnos { get; set; }
    }
}
