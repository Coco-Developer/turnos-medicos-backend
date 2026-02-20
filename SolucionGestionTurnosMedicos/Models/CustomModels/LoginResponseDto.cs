using System.ComponentModel.DataAnnotations;

namespace Models.CustomModels
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;

    }
}