using Microsoft.EntityFrameworkCore;
using DataAccess.Context;

namespace Testing
{
    // DbContext de prueba que extiende el original y añade claves para las entidades keyless
    public class TestGestionTurnosContext : GestionTurnosContext
    {
        public TestGestionTurnosContext(DbContextOptions<GestionTurnosContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ejecuta la configuración original (mapeos, vistas, etc.)
            base.OnModelCreating(modelBuilder);

            // Añadimos claves sólo para el contexto de tests para permitir Add/Track en InMemory
            // Ajusta las propiedades si tu modelo tiene nombres distintos.
            modelBuilder.Entity<VwTurno>().HasKey(v => v.Id);
            modelBuilder.Entity<VwTurnoCount>().HasKey(v => v.CountId);
            modelBuilder.Entity<VwTurnoXMedicoCount>().HasKey(v => v.CountId);
            modelBuilder.Entity<VwTurnoCalendar>().HasKey(c => c.Fecha);
        }
    }
}