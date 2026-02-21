using DataAccess.Context;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class PacienteRepository
    {
        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        public PacienteRepository(GestionTurnosContext context)
        {
            _context = context;
        }
        #endregion

        // Obtener todos los pacientes (asíncrono)
        public async Task<List<Paciente>> GetAllPatientsAsync()
        {
            return await _context.Pacientes.ToListAsync();
        }

        // Obtener la cantidad de pacientes (asíncrono)
        public async Task<int> GetQtyPatientsAsync()
        {
            return await _context.Pacientes.CountAsync();
        }

        // Obtener un paciente por su ID (asíncrono)
        public Paciente GetPatientForId(int id)

        {
            return _context.Pacientes.Find(id);
        }

        // Crear un nuevo paciente (asíncrono)
        public async Task CreatePatientAsync(Paciente oPatient)
        {
            await _context.Pacientes.AddAsync(oPatient);
            await _context.SaveChangesAsync();
        }

        // Actualizar un paciente (asíncrono)
        public async Task UpdatePatientAsync(Paciente oPatient)
        {
            _context.Pacientes.Update(oPatient);
            await _context.SaveChangesAsync();
        }

        // Eliminar un paciente por su ID (asíncrono)
        public async Task DeletePatientAsync(int id)
        {
            var patient = await _context.Pacientes.FindAsync(id);
            if (patient != null)
            {
                _context.Pacientes.Remove(patient);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Paciente no encontrado.");
            }
        }

        // Obtener un paciente por su DNI (asíncrono)
        public async Task<Paciente?> GetPatientForDNIAsync(string dni)
        {
            return await _context.Pacientes
                .Where(p => p.Dni == dni)
                .FirstOrDefaultAsync();
        }

        // Verificar si un paciente existe por nombre y DNI (asíncrono)
        public async Task<bool> VerifyIfPatientExistAsync(string nombre, string dni)
        {
            return await _context.Pacientes
                .AnyAsync(d => d.Nombre == nombre && d.Dni == dni);
        }

        // Verificar si un paciente existe por nombre y DNI (síncrono)
        public bool VerifyIfPatientExist(string nombre, string dni)
        {
            return _context.Pacientes
                .Any(d => d.Nombre == nombre && d.Dni == dni);
        }

        // Buscar pacientes por apellido (asíncrono)
        public async Task<List<Paciente>> FindPatientForLastNameAsync(string lastName)
        {
            return await _context.Pacientes
                .Where(o => o.Apellido == lastName)
                .ToListAsync();
        }

        // Verificar si un paciente existe por ID (asíncrono)
        public async Task<bool> VerifyIfPatientExistByIdAsync(int id)
        {
            return await _context.Pacientes
                .AnyAsync(d => d.Id == id);
        }

        // Verificar si un paciente existe por ID (síncrono)
        public bool VerifyIfPatientExistById(int id)
        {
            return _context.Pacientes
                .Any(d => d.Id == id);
        }

        public Paciente ObtenerPacientePorIdUsuario(int id)
        {
            var resultado = _context.Pacientes.Where(p => p.UsuarioId == id).FirstOrDefault();

            return resultado;
        }
    }
}
