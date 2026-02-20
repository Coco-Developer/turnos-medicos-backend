// Models/CustomModels/AdminRegisterDto.cs
using System.ComponentModel.DataAnnotations;

namespace Models.CustomModels
{
    public class AdminRegisterDto
    {
        [Required]
        [MinLength(3)]
        public string Nombre { get; set; } = default!;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = default!;

        [Required]
        public string Email { get; set; }
    }
}
