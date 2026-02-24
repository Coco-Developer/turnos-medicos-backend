using DataAccess.Data;
using DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using Microsoft.AspNetCore.Identity;
using Models.CustomModels;
using Models.DTOs;
using DataAccess.Context;

namespace BusinessLogic.AppLogic
{
    public class PacienteLogic
    {
        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        public PacienteLogic(GestionTurnosContext context)
        {
            _context = context;
        }
        #endregion

        public async Task<List<Paciente>> PatientsListAsync()
        {
            var repPatient = new PacienteRepository(_context);
            return await repPatient.GetAllPatientsAsync();
        }

        public async Task<Paciente> GetPatientForIdAsync(int id)
        {
            if (id == 0) throw new ArgumentException("Id cannot be 0");

            try
            {
                var repPatient = new PacienteRepository(_context);
                // Ahora usamos await porque el Repo es asíncrono
                var patient = await repPatient.GetPatientForId(id);

                if (patient == null)
                    throw new NotFoundException("No patient found with the given ID");

                return patient;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                throw new ApplicationException("An error occurred while fetching the patient.", e);
            }
        }

        public async Task CreatePatientWithUserAsync(Paciente oPatient, string password)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (oPatient == null)
                    throw new ArgumentNullException(nameof(oPatient), "El paciente no puede ser nulo.");

                // Verificación asíncrona de existencia de usuario
                bool userExists = await _context.Usuario.AnyAsync(u => u.Username == oPatient.Dni.ToString());
                if (userExists)
                    throw new ApplicationException("Ya existe un usuario con el mismo DNI.");

                var passwordHasher = new PasswordHasher<Usuario>();
                var hashedPassword = passwordHasher.HashPassword(null!, password);

                var user = new Usuario
                {
                    Username = oPatient.Dni.ToString(),
                    PasswordHash = hashedPassword,
                    Rol = UserRoles.Paciente,
                    Email = oPatient.Email,
                    IsActive = true
                };

                _context.Usuario.Add(user);
                await _context.SaveChangesAsync();

                oPatient.UsuarioId = user.Id;
                oPatient.Usuario = user;

                _context.Pacientes.Add(oPatient);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error: {e.Message}");
                throw new ApplicationException("Ocurrió un error al crear el paciente y el usuario.", e);
            }
        }

        public async Task UpdatePatientAsync(int id, UpdatePatientDto pacienteDto)
        {
            // Buscamos el paciente incluyendo su usuario de forma eficiente
            var patient = await _context.Pacientes
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                throw new KeyNotFoundException($"Paciente con ID {id} no encontrado.");
            }

            // Mapeo de datos
            patient.Apellido = pacienteDto.Apellido;
            patient.Nombre = pacienteDto.Nombre;
            patient.Telefono = pacienteDto.Telefono;
            patient.Email = pacienteDto.Email;
            patient.FechaNacimiento = pacienteDto.FechaNacimiento;

            if (!string.IsNullOrWhiteSpace(pacienteDto.Password))
            {
                if (patient.Usuario == null)
                {
                    throw new ApplicationException("El paciente no tiene un usuario asociado para cambiar contraseña.");
                }

                var passwordHasher = new PasswordHasher<Usuario>();
                patient.Usuario.PasswordHash = passwordHasher.HashPassword(patient.Usuario, pacienteDto.Password);
                patient.Usuario.Email = pacienteDto.Email; // Sincronizamos email del usuario
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeletePatientAsync(int id)
        {
            try
            {
                var repPatient = new PacienteRepository(_context);
                var patientFound = await repPatient.GetPatientForId(id);

                if (patientFound == null)
                    throw new NotFoundException("No patient found with the given ID");

                await repPatient.DeletePatientAsync(id);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                throw new ApplicationException("An error occurred while deleting the patient.", e);
            }
        }

        public async Task<Paciente> GetPatientForDNIAsync(string dni)
        {
            try
            {
                var repPatient = new PacienteRepository(_context);
                var patient = await repPatient.GetPatientForDNIAsync(dni);

                if (patient == null)
                    throw new NotFoundException("No patient found with the given DNI");

                return patient;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                throw new ApplicationException("An error occurred while fetching the patient by DNI.", e);
            }
        }

        public async Task<int> GetPatientsQtyAsync()
        {
            try
            {
                var repPatient = new PacienteRepository(_context);
                return await repPatient.GetQtyPatientsAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                throw new ApplicationException("An error occurred while counting the patients.", e);
            }
        }
    }
}