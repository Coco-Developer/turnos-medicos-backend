namespace DataAccess.Data
{
    public class Medico
    {
        public int Id { get; set; }
        public string? Apellido { get; set; }
        public string? Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Dni { get; set; }
        public int EspecialidadId { get; set; }
        public DateTime FechaAltaLaboral { get; set; }
        public byte[]? Foto { get; set; }
        public string? Matricula { get; set; }


        public TimeSpan HorarioAtencionInicio { get; set; }
        public TimeSpan HorarioAtencionFin { get; set; }

        public virtual ICollection<HorarioMedico> Horarios { get; set; } = new List<HorarioMedico>();
    }


}
