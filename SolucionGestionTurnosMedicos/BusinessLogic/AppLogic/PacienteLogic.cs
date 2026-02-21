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
                var patient = repPatient.GetPatientForId(id);
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
                // Validar que el paciente no sea nulo
                if (oPatient == null)
                    throw new ArgumentNullException(nameof(oPatient), "El paciente no puede ser nulo.");

                // Verificar que no exista un usuario con el mismo Dni (o username)
                if (_context.Usuario.Any(u => u.Username == oPatient.Dni.ToString()))
                    throw new ApplicationException("Ya existe un usuario con el mismo DNI.");

                // Instanciar el PasswordHasher
                var passwordHasher = new PasswordHasher<Usuario>();

                // Hash de la contraseña
                var hashedPassword = passwordHasher.HashPassword(null, password);

                // Crear el usuario
                var user = new Usuario
                {
                    Username = oPatient.Dni.ToString(), // El Username será el DNI automático
                    PasswordHash = hashedPassword,
                    Rol = UserRoles.Paciente, // Rol Paciente
                    Email = oPatient.Email, 
                    IsActive = true            // Activar el usuario por defecto
                };

                // Guardar el usuario en la base de datos
                _context.Usuario.Add(user);
                await _context.SaveChangesAsync();

                // Asociar el usuario al paciente
                oPatient.UsuarioId = user.Id;
                oPatient.Usuario = user; // Mantener la navegación en memoria

                // Guardar el paciente en la base de datos
                _context.Pacientes.Add(oPatient);
                await _context.SaveChangesAsync();

                // Confirmar la transacción
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                // Deshacer la transacción en caso de error
                await transaction.RollbackAsync();
                Console.WriteLine($"Error: {e.Message}");
                throw new ApplicationException("Ocurrió un error al crear el paciente y el usuario.", e);
            }
        }



        public async Task UpdatePatientAsync(int id, UpdatePatientDto pacienteDto)
        {
            var dummy = await _context.Pacientes
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    Paciente = p,
                    Usuario = _context.Usuario.FirstOrDefault(u => u.Id == p.UsuarioId)
                })
                .FirstOrDefaultAsync();


            if (dummy == null)
            {
                throw new KeyNotFoundException($"Paciente con ID {id} no encontrado.");
            }

            // Mapeo manual de los datos
            dummy.Paciente.Apellido = pacienteDto.Apellido;
            dummy.Paciente.Nombre = pacienteDto.Nombre;
            dummy.Paciente.Telefono = pacienteDto.Telefono;
            dummy.Paciente.Email = pacienteDto.Email;
            dummy.Paciente.FechaNacimiento = pacienteDto.FechaNacimiento;

            if (!string.IsNullOrWhiteSpace(pacienteDto.Password))
            {
                if (dummy.Usuario == null)
                {
                    throw new ApplicationException("El paciente no tiene un usuario asociado para cambiar contraseña.");
                }

                var passwordHasher = new PasswordHasher<Usuario>();

                // Sin "sal" adicional:
                var hashedPassword = passwordHasher.HashPassword(null, pacienteDto.Password);

                // Con "sal" adicional:
                //var hashedPassword = passwordHasher.HashPassword(patient.Usuario, pacienteDto.Password);

                dummy.Usuario.PasswordHash = hashedPassword;

                dummy.Usuario.Email = pacienteDto.Email;
            }

            // Guardamos los cambios en la base de datos
            await _context.SaveChangesAsync();
        }


        public async Task DeletePatientAsync(int id)
        {
            try
            {
                var repPatient = new PacienteRepository(_context);
                var patientFound =  repPatient.GetPatientForId(id);

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
