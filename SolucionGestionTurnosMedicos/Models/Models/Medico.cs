using DataAccess.Data;

public partial class Medico
{
    public int Id { get; set; }
    public string? Apellido { get; set; }
    public string? Nombre { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Dni { get; set; }
    public int EspecialidadId { get; set; }
    public DateTime FechaAltaLaboral { get; set; }
    public virtual ICollection<HorarioMedico> Horarios { get; set; } = new List<HorarioMedico>();
    public byte[]? Foto { get; set; }
    public string? Matricula { get; set; }
    public TimeSpan HorarioAtencionInicio { get; set; } // Agregado para Azure
    public TimeSpan HorarioAtencionFin { get; set; }    // Agregado para Azure
}