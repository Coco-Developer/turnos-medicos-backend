// DataAccess/Repository/UserRepository.cs
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly GestionTurnosContext _context;

        public UserRepository(GestionTurnosContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> GetByNombreAsync(string nombre)
        {
            return await _context.Usuario
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(u => u.Username == nombre);
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            return await _context.Usuario
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario?> GetByIdTrackingAsync(int id)
        {
            return await _context.Usuario
                                 .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario?> GetByUsernameAsync(string username)
        {
            return await _context.Usuario
                                 .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task AddAsync(Usuario usuario)
            => await _context.Usuario.AddAsync(usuario);

        public async Task<bool> SaveChangesAsync()
            => await _context.SaveChangesAsync() > 0;
    }
}
