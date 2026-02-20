using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Data;

namespace DataAccess.Data
{
    [Table("Usuario")]
    public class Usuario
    {
        public int Id { get; set; }

        // Nombre de Usuario
        public string Username { get; set; } = default!;

        // Contraseña encriptada
        [Column("PasswordHash")]
        public string PasswordHash { get; set; }

        // Estado de la cuenta (pensado para el futuro)
        [Column("IsActive")]
        public bool IsActive { get; set; }

        [Column("Rol")]
        public string Rol { get; set; } = "Paciente"; // Por defecto

        [Column("Email")]
        public string Email { get; set; }
    }
}
