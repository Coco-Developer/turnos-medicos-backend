using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.CustomModels
{
    public class TurnoCustom
    {
        public int Id { get; set; }
        public int MedicoId { get; set; }
        public int PacienteId { get; set; }
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public int EstadoId { get; set; }
        public string? Observaciones { get; set; }

        public TimeSpan ModifyHour(string hora)
        {
            Hora = hora;
            string timeConvert = TimeConvert(Hora);
            return TimeSpan.Parse(timeConvert);
        }
        public DateTime ModifyDate(string fecha)
        {
            Fecha = fecha;
            string dateConvert = Fecha;
            return DateTime.Parse(dateConvert);
        }

        public string TimeConvert(string tiempo)
        {
            return tiempo.Replace('.', ':');
        }
    }

    public class HorarioTurnos
    {
        public DateTime Fecha_turno { get; set; }
        public List<TimeSpan> Hora_turno { get; set; }

    }
}
