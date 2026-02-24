namespace Models.DTOs
{
    public class UpdatePatientDto
    {
        public string? Apellido { get; set; }
        public string? Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string? Password { get; set; }
    }
}