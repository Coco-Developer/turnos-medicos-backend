using DataAccess.Data;

public partial class Paciente
{
    public int Id { get; set; }
    public string? Apellido { get; set; } // Permitir nulo para validación manual
    public string? Nombre { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public string? Dni { get; set; }
    public int UsuarioId { get; set; }
    public virtual Usuario? Usuario { get; set; } // La relación puede ser nula al crear

    public string NombreCompletoPaciente() => $"{Nombre} {Apellido}";

    public int EdadDelPaciente(DateTime fechaNacimiento)
    {
        DateTime fechaActual = DateTime.Now;
        int edad = fechaActual.Year - FechaNacimiento.Year;
        if (fechaActual < FechaNacimiento.AddYears(edad)) edad--;
        return edad;
    }
}