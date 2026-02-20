// Models/CustomModels/LoginRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace Models.CustomModels
{
    public class LoginRequestDto
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; } = default!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = default!;
    }
}
