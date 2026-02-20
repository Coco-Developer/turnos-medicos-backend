// DataAccess/Repository/IUserRepository.cs
using DataAccess.Data;

namespace DataAccess.Repository
{
    public interface IUserRepository
    {
        Task<Usuario?> GetByNombreAsync(string nombre);
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario?> GetByIdTrackingAsync(int id);
        Task<Usuario?> GetByUsernameAsync(string username);
        Task AddAsync(Usuario usuario);
        Task<bool> SaveChangesAsync();
    }
}
