using DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context
{
    public partial class GestionTurnosContext : DbContext
    {
        public GestionTurnosContext(DbContextOptions<GestionTurnosContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Especialidad> Especialidades { get; set; } = null!;
        public virtual DbSet<Estado> Estados { get; set; } = null!;
        public virtual DbSet<Medico> Medicos { get; set; } = null!;
        public virtual DbSet<HorarioMedico> HorariosMedicos { get; set; } = null!;
        public virtual DbSet<Paciente> Pacientes { get; set; } = null!;
        public virtual DbSet<Turno> Turnos { get; set; } = null!;
        public virtual DbSet<Usuario> Usuario { get; set; } = null!;
        public virtual DbSet<VwTurno> VwTurnos { get; set; } = null!;
        public virtual DbSet<VwTurnoCount> VwTurnoCounts { get; set; } = null!;
        public virtual DbSet<VwTurnoXMedicoCount> VwTurnoXMedicoCounts { get; set; } = null!;
        public virtual DbSet<VwTurnoCalendar> VwTurnoCalendars { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Especialidad>(entity =>
            {
                entity.ToTable("Especialidad");

                entity.HasIndex(e => e.Nombre, "IX_Especialidad");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("nombre");
            });

            modelBuilder.Entity<Estado>(entity =>
            {
                entity.ToTable("Estado");

                entity.HasIndex(e => e.Nombre, "IX_Estado");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("nombre");

                entity.Property(e => e.Clase)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("clase");

                entity.Property(e => e.Icono)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("icono");

                entity.Property(e => e.Color)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("color");
            });

            modelBuilder.Entity<Medico>(entity =>
            {
                entity.ToTable("Medico");

                entity.HasIndex(e => new { e.Apellido, e.Nombre }, "IX_Medico_ApeNom");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Apellido)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("apellido");

                entity.Property(e => e.Direccion)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("direccion");

                entity.Property(e => e.Dni)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("dni");

                entity.Property(e => e.EspecialidadId).HasColumnName("especialidadId");

                entity.Property(e => e.FechaAltaLaboral)
                    .HasColumnType("date")
                    .HasColumnName("fecha_alta_laboral");

                //entity.Property(e => e.HorarioAtencionFin).HasColumnName("horario_atencion_fin");

                //entity.Property(e => e.HorarioAtencionInicio).HasColumnName("horario_atencion_inicio");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("nombre");

                entity.Property(e => e.Telefono)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("telefono");


                entity.Property(e => e.Matricula)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("matricula");
            });

            modelBuilder.Entity<HorarioMedico>(entity =>
            {
                //entity.HasOne(h => h.Medico)
                //    .WithMany(m => m.Horarios)
                //    .HasForeignKey(h => h.MedicoId)
                //    .OnDelete(DeleteBehavior.Cascade);

                entity.ToTable("HorarioMedico");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.MedicoId).HasColumnName("medicoId");

                entity.Property(e => e.DiaSemana).HasColumnName("diaSemana");

                entity.Property(e => e.HorarioAtencionFin).HasColumnName("horario_atencion_fin");

                entity.Property(e => e.HorarioAtencionInicio).HasColumnName("horario_atencion_inicio");

            });


            modelBuilder.Entity<Paciente>(entity =>
            {
                entity.ToTable("Paciente");

                entity.HasIndex(e => e.Dni, "IX_DNI");

                entity.HasIndex(e => new { e.Apellido, e.Nombre }, "IX_Paciente_ApeNom");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Apellido)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("apellido");

                entity.Property(e => e.Dni)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("dni");

                entity.Property(e => e.Email)
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .HasColumnName("email");

                entity.Property(e => e.FechaNacimiento)
                    .HasColumnType("date")
                    .HasColumnName("fecha_nacimiento");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("nombre");

                entity.Property(e => e.Telefono)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("telefono");

                entity.Property(e => e.UsuarioId)
                    .HasColumnName("usuarioid");

                // 👇 RELACIÓN CORRECTA
                entity.HasOne(p => p.Usuario)
                    .WithMany()
                    .HasForeignKey(p => p.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Turno>(entity =>
            {
                entity.ToTable("Turno");

                entity.HasIndex(e => new { e.MedicoId, e.Fecha, e.Hora }, "IX_Medico_Fecha_Hora");

                entity.HasIndex(e => new { e.PacienteId, e.Fecha, e.Hora }, "IX_Paciente_Fecha_Hora");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.EstadoId).HasColumnName("estadoId");

                entity.Property(e => e.Fecha)
                    .HasColumnType("date")
                    .HasColumnName("fecha");

                entity.Property(e => e.Hora).HasColumnName("hora");

                entity.Property(e => e.MedicoId).HasColumnName("medicoId");

                entity.Property(e => e.Observaciones)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("observaciones");

                entity.Property(e => e.PacienteId).HasColumnName("pacienteId");
            });

            modelBuilder.Entity<VwTurno>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vwTurno");

                entity.Property(e => e.EstadoId).HasColumnName("estadoId");

                entity.Property(e => e.Fecha)
                    .HasColumnType("date")
                    .HasColumnName("fecha");

                entity.Property(e => e.Hora).HasColumnName("hora");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Medico)
                    .HasMaxLength(302)
                    .IsUnicode(false)
                    .HasColumnName("medico");

                entity.Property(e => e.MedicoId).HasColumnName("medicoId");

                entity.Property(e => e.Observaciones)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("observaciones");

                entity.Property(e => e.Paciente)
                    .HasMaxLength(302)
                    .IsUnicode(false)
                    .HasColumnName("paciente");

                entity.Property(e => e.PacienteEmail)
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .HasColumnName("paciente_email");

                entity.Property(e => e.PacienteId).HasColumnName("pacienteId");

                entity.Property(e => e.PacienteTelefono)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("paciente_telefono");
                
                entity.Property(e => e.PacienteDni)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("paciente_dni");

                entity.Property(e => e.Estado)
                   .HasMaxLength(200)
                   .IsUnicode(false)
                   .HasColumnName("estado");

                entity.Property(e => e.EstadoClase)
                   .HasMaxLength(50)
                   .IsUnicode(false)
                   .HasColumnName("estado_clase");

                entity.Property(e => e.EstadoIcono)
                   .HasMaxLength(50)
                   .IsUnicode(false)
                   .HasColumnName("estado_icono");

                entity.Property(e => e.EspecialidadId).HasColumnName("especialidadId");

                entity.Property(e => e.Especialidad)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("especialidad");
            });

            modelBuilder.Entity<VwTurnoCount>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vwTurnoCount");

                entity.Property(e => e.Yr).HasColumnName("yr");

                entity.Property(e => e.Mo).HasColumnName("mo");

                entity.Property(e => e.Estado)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("Estado");

                entity.Property(e => e.Clase)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("clase");

                entity.Property(e => e.Color)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("color");

                entity.Property(e => e.CountId).HasColumnName("Count_Id");
            });

            modelBuilder.Entity<VwTurnoXMedicoCount>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vwTurnoXMedicoCount");

                entity.Property(e => e.Yr).HasColumnName("yr");

                entity.Property(e => e.Medico)
                    .HasMaxLength(302)
                    .IsUnicode(false)
                    .HasColumnName("medico");

                entity.Property(e => e.Estado)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("estado");

                entity.Property(e => e.Clase)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("clase");

                entity.Property(e => e.Color)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("color");

                entity.Property(e => e.CountId).HasColumnName("count_id");
            });


            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");

                entity.Property(e => e.Id).HasColumnName("Id");

                entity.Property(e => e.Username)
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .HasColumnName("UserName");

                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("PasswordHash");

                entity.Property(e => e.IsActive)
                    .HasColumnName("IsActive")
                    .HasDefaultValue(true);
            });

            modelBuilder.Entity<VwTurnoCalendar>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vwTurnoCalendar");

                entity.Property(e => e.Fecha)
                .HasColumnType("date")
                .HasColumnName("fecha");

                entity.Property(e => e.Qty).HasColumnName("qty");

            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
