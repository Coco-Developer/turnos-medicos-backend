using DataAccess.Data;
using DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Models.CustomModels;
using Models.DTOs;
using DataAccess.Context;

namespace BusinessLogic.AppLogic
{
    public class PacienteLogic
    {
        private readonly GestionTurnosContext _context;

        public PacienteLogic(GestionTurnosContext context)
        {
            _context = context;
        }

        // Obtener todos los pacientes
        public async Task<List<Paciente>> PatientsListAsync()
        {
            var repPatient = new PacienteRepository(_context);
            return await repPatient.GetAllPatientsAsync();
        }

        // Obtener paciente por ID
        public async Task<Paciente?> GetPatientForIdAsync(int id)
        {
            if (id <= 0) return null;
            var repPatient = new PacienteRepository(_context);
            return await repPatient.GetPatientForId(id);
        }

        // Obtener paciente por DNI (Crucial para validación de altas)
        public async Task<Paciente?> GetPatientForDNIAsync(string dni)
        {
            try
            {
                var repPatient = new PacienteRepository(_context);
                // Usamos el repositorio. Si no existe, devuelve null (sin lanzar excepción)
                return await repPatient.GetPatientForDNIAsync(dni);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Log: Error buscando DNI {dni}: {ex.Message}");
                return null;
            }
        }

        public async Task CreatePatientWithUserAsync(Paciente oPatient, string password)
        {
            if (oPatient == null)
                throw new ArgumentException("Datos del paciente inválidos.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña es obligatoria.");

            if (string.IsNullOrWhiteSpace(oPatient.Dni))
                throw new ArgumentException("El DNI es obligatorio.");

            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    bool userExists = await _context.Usuario
                        .AnyAsync(u => u.Username == oPatient.Dni);

                    if (userExists)
                        throw new Exception("El DNI ya se encuentra registrado.");

                    var user = new Usuario
                    {
                        Username = oPatient.Dni,
                        Rol = UserRoles.Paciente,
                        Email = oPatient.Email ?? "",
                        IsActive = true
                    };

                    var passwordHasher = new PasswordHasher<Usuario>();
                    user.PasswordHash = passwordHasher.HashPassword(user, password);

                    _context.Usuario.Add(user);
                    await _context.SaveChangesAsync();

                    oPatient.UsuarioId = user.Id;

                    _context.Pacientes.Add(oPatient);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        // Actualizar Paciente
        public async Task UpdatePatientAsync(int id, UpdatePatientDto dto)
        {
            var patient = await _context.Pacientes
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) throw new Exception("Paciente no encontrado.");

            patient.Apellido = dto.Apellido;
            patient.Nombre = dto.Nombre;
            patient.Telefono = dto.Telefono;
            patient.Email = dto.Email;
            patient.FechaNacimiento = dto.FechaNacimiento;

            // Si se envía password, se actualiza en el usuario vinculado
            if (!string.IsNullOrWhiteSpace(dto.Password) && patient.Usuario != null)
            {
                var passwordHasher = new PasswordHasher<Usuario>();
                patient.Usuario.PasswordHash = passwordHasher.HashPassword(patient.Usuario, dto.Password);
                patient.Usuario.Email = dto.Email;
            }

            await _context.SaveChangesAsync();
        }

        // Eliminar Paciente
        public async Task DeletePatientAsync(int id)
        {
            var repPatient = new PacienteRepository(_context);
            var patient = await repPatient.GetPatientForId(id);
            if (patient == null) throw new Exception("El paciente no existe.");

            await repPatient.DeletePatientAsync(id);
        }

        // Cantidad de pacientes
        public async Task<int> GetPatientsQtyAsync()
        {
            var repPatient = new PacienteRepository(_context);
            return await repPatient.GetQtyPatientsAsync();
        }
    }
}