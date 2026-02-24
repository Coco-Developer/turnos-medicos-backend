using DataAccess.Data;
using DataAccess.Repository;
using Microsoft.AspNetCore.Identity;
using Models.CustomModels;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.AppLogic.Services
{
    public class AdminSetupService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public AdminSetupService(IUserRepository userRepository, IPasswordHasher<Usuario> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<(bool success, string message)> CrearAdminAsync(AdminRegisterDto dto)
        {
            // Normalización: Usamos el repositorio en lugar del contexto directo
            var existe = await _userRepository.GetByUsernameAsync(dto.Nombre);
            if (existe != null)
                return (false, "El usuario ya existe.");

            var usuario = new Usuario
            {
                Username = dto.Nombre,
                IsActive = true,
                // Usamos el hasher de Identity
                PasswordHash = _passwordHasher.HashPassword(null!, dto.Password),
                Rol = UserRoles.Admin,
                Email = dto.Email
            };

            await _userRepository.AddAsync(usuario);
            bool guardado = await _userRepository.SaveChangesAsync();

            if (guardado)
                return (true, "Usuario admin creado correctamente.");

            return (false, "No se pudo guardar el usuario en la base de datos.");
        }
    }
}