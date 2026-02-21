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

        public List<Estado> GetAllEstados()
        {
            using (_context)
            {
                return _context.Estados.ToList();
            }
        }

        public Estado? GetEstadoById(int id)
        {
            return _context.Estados.Find(id);
        }

        public void CreateEstado(Estado oState)
        {
            using (_context)
            {
                _context.Estados.Add(oState);
                _context.SaveChanges();
            }
        }

        public void UpdateEstado(Estado oState)
        {
            using (_context)
            {
                _context.Entry(oState).State = EntityState.Modified;
                _context.SaveChanges();
            }
        }

        public void DeleteEstado(Estado oState)
        {
            using (_context)
            {
                _context.Estados.Remove(oState);
                _context.SaveChanges();
            }
        }

        public bool VerifyIfStateExist(int id)
        {
            return _context.Estados.Where(e  => e.Id == id).Any();
        }
    }
}
