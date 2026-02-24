using DataAccess.Context;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class EstadoRepository
    {
        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        public EstadoRepository(GestionTurnosContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        #endregion

        // Obtener todos los estados (Asíncrono y sin Dispose manual)
        public async Task<List<Estado>> GetAllEstadosAsync()
        {
            return await _context.Estados.ToListAsync();
        }

        // Obtener por ID
        public async Task<Estado?> GetEstadoByIdAsync(int id)
        {
            return await _context.Estados.FindAsync(id);
        }

        // Crear nuevo estado
        public async Task CreateEstadoAsync(Estado oState)
        {
            await _context.Estados.AddAsync(oState);
            await _context.SaveChangesAsync();
        }

        // Actualizar estado existente
        public async Task UpdateEstadoAsync(Estado oState)
        {
            _context.Entry(oState).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Eliminar estado
        public async Task DeleteEstadoAsync(Estado oState)
        {
            _context.Estados.Remove(oState);
            await _context.SaveChangesAsync();
        }

        // Verificar existencia (Asíncrono)
        public async Task<bool> VerifyIfStateExistAsync(int id)
        {
            return await _context.Estados.AnyAsync(e => e.Id == id);
        }
    }
}