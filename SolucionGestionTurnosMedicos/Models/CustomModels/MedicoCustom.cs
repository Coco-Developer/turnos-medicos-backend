using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using DataAccess.Data;

namespace ApiGestionTurnosMedicos.CustomModels
{
    public class MedicoCustom
    {
        #region Propiedades
            public int Id { get; set; }

            [Required(ErrorMessage = "Apellido is required")]
            public string Apellido { get; set; } = null!;

            [Required(ErrorMessage = "Nombre is required")]
            public string Nombre { get; set; } = null!;

            [Required(ErrorMessage = "Telefono is required")]
            public string Telefono { get; set; } = null!;

            [Required(ErrorMessage = "Direccion is required")]
            public string? Direccion { get; set; }

            [Required(ErrorMessage = "Dni is required")]
            public string Dni { get; set; } = null!;

            [Required(ErrorMessage = "Especialidad is required")]
            public int EspecialidadId { get; set; }

            [Display(Name = "Fecha de alta laboral")]
            [DataType(DataType.Date)]
            [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
            public DateTime FechaAltaLaboral { get; set; }

            //[Required(ErrorMessage = "HoraAtencionInicio is required")]
            //public string HorarioAtencionInicio { get; set; }

            //[Required(ErrorMessage = "HoraAtencionFin is required")]
            //public string HorarioAtencionFin { get; set; }

            [Required(ErrorMessage = "Horario collection is required")]
            public virtual ICollection<HorarioMedico> Horarios { get; set; } = new List<HorarioMedico>();


            [Display(Name = "Foto")]
            public string? Foto { get; set; }

            [Display(Name = "Matricula")]
            public string Matricula { get; set; } = null!;

        public string? Especialidad { get; set; }

        #endregion

        #region Constructores
            public MedicoCustom()
            {

            }        
            //public MedicoCustom(string horarioInicio, string horarioFinal)
            //{
            //    HorarioAtencionInicio = horarioInicio;
            //    HorarioAtencionFin = horarioFinal;
            //}
        #endregion

        #region Metodos

    //    public TimeSpan ModifyStartTime(string hora)
    //        {
    //            HorarioAtencionInicio = hora;
    //            string timeConvert = TimeConvert(HorarioAtencionInicio);
    //            return TimeSpan.Parse(timeConvert);
    //        }            
    //        public TimeSpan ModifyEndTime(string hora)
    //        {
    //            HorarioAtencionFin = hora;
    //            string timeConvert = TimeConvert(HorarioAtencionFin);
    //            return TimeSpan.Parse(timeConvert);
    //        }

    //        public string TimeConvert(string tiempo)
    //        {
    //            return tiempo.Replace('.', ':');
    //        }

    //        public string CalcularTiempoHoras(string fecha)
    //        {
    //            DateTime result = DateTime.ParseExact(fecha, "HH:mm:ss.fffffff",
    //                                       System.Globalization.CultureInfo.InvariantCulture);
    //            return result.ToString("HH:mm");
    //        }
        #endregion

    }
    public class MedicoConEspecialidad
    {
        public int Id { get; set; }
        public string Apellido { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Especialidad { get; set; }

        public string NombreCompletoMedico(string nombre, string apellido)
        {
            Nombre = nombre;
            Apellido = apellido;

            return ($"{Nombre} {Apellido}");
        }

    }
}
