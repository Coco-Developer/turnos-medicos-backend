using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;

namespace DataAccess.Repository
{
    public class EspecialidadRepository
    {
        private readonly GestionTurnosContext _context;

        public EspecialidadRepository(GestionTurnosContext context)
        {
            _context = context;
        }

        // LISTAR TODO (Lo que usas en el Admin)
        public async Task<List<Especialidad>> GetAllEspecialidadesAsync()
        {
            // Quitamos el 'using' manual para que no rompa el ciclo de vida de la inyección
            return await _context.Especialidades
                .OrderBy(e => e.Nombre)
                .ToListAsync();
        }

        // BUSCAR POR ID
        public async Task<Especialidad?> GetSpecialtyForIdAsync(int id)
        {
            return await _context.Especialidades.FindAsync(id);
        }

        // CREAR
        public async Task CreateSpecialtyAsync(Especialidad oSpecialty)
        {
            await _context.Especialidades.AddAsync(oSpecialty);
            await _context.SaveChangesAsync();
        }

        // ACTUALIZAR
        public async Task UpdateSpecialtyAsync(Especialidad oSpecialty)
        {
            _context.Entry(oSpecialty).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // ELIMINAR (Recibiendo el objeto como hacías antes)
        public async Task DeleteSpecialtyAsync(Especialidad oSpecialty)
        {
            _context.Especialidades.Remove(oSpecialty);
            await _context.SaveChangesAsync();
        }

        // VERIFICAR EXISTENCIA
        public async Task<bool> VerifyIfSpecialtyExistAsync(int id)
        {
            return await _context.Especialidades.AnyAsync(s => s.Id == id);
        }

        // EL MÉTODO QUE MENCIONASTE: Especialidades con Médicos
        public async Task<List<Especialidad>> ReturnCoveredSpecialtiesAsync()
        {
            return await _context.Especialidades
                .Where(e => _context.Medicos.Any(m => m.EspecialidadId == e.Id))
                .OrderBy(e => e.Nombre)
                .ToListAsync();
        }
    }
}