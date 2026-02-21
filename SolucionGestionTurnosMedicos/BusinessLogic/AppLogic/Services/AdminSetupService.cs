using DataAccess.Context;
using DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Models.CustomModels;


namespace BusinessLogic.AppLogic.Services
{
    public class AdminSetupService
    {
        private readonly GestionTurnosContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public AdminSetupService(GestionTurnosContext context, IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<(bool success, string message)> CrearAdminAsync(AdminRegisterDto dto)
        {
            if (_context.Usuario.Any(u => u.Username == dto.Nombre))
                return (false, "El usuario ya existe.");

            var usuario = new Usuario
            {
                Username = dto.Nombre,
                IsActive = true,
                PasswordHash = _passwordHasher.HashPassword(null!, dto.Password),
                Rol = UserRoles.Admin,  // Asignamos el rol de "Admin"
                Email = dto.Email
            };

            _context.Usuario.Add(usuario);
            await _context.SaveChangesAsync();
            return (true, "Usuario admin creado correctamente.");
        }
    }
}
