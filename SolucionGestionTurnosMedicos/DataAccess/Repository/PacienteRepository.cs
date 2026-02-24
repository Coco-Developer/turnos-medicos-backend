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

        // Obtener todos los pacientes
        public async Task<List<Paciente>> GetAllPatientsAsync()
        {
            return await _context.Pacientes.ToListAsync();
        }

        // Obtener la cantidad de pacientes
        public async Task<int> GetQtyPatientsAsync()
        {
            return await _context.Pacientes.CountAsync();
        }

        // Obtener un paciente por su ID 
        // Cambiado a async para que TurnoLogic no explote al llamarlo
        public async Task<Paciente?> GetPatientForId(int id)
        {
            return await _context.Pacientes.FindAsync(id);
        }

        // Crear un nuevo paciente
        public async Task CreatePatientAsync(Paciente oPatient)
        {
            await _context.Pacientes.AddAsync(oPatient);
            await _context.SaveChangesAsync();
        }

        // Actualizar un paciente
        public async Task UpdatePatientAsync(Paciente oPatient)
        {
            _context.Pacientes.Update(oPatient);
            await _context.SaveChangesAsync();
        }

        // Eliminar un paciente por su ID
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

        // Obtener un paciente por su DNI
        public async Task<Paciente?> GetPatientForDNIAsync(string dni)
        {
            return await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Dni == dni);
        }

        // Verificar si un paciente existe por nombre y DNI
        public async Task<bool> VerifyIfPatientExistAsync(string nombre, string dni)
        {
            return await _context.Pacientes
                .AnyAsync(d => d.Nombre == nombre && d.Dni == dni);
        }

        // Buscar pacientes por apellido
        public async Task<List<Paciente>> FindPatientForLastNameAsync(string lastName)
        {
            return await _context.Pacientes
                .Where(o => o.Apellido == lastName)
                .ToListAsync();
        }

        // Verificar si un paciente existe por ID
        public async Task<bool> VerifyIfPatientExistByIdAsync(int id)
        {
            return await _context.Pacientes
                .AnyAsync(d => d.Id == id);
        }

        // Obtener paciente por ID de Usuario (útil para la App Móvil/Login)
        public async Task<Paciente?> ObtenerPacientePorIdUsuarioAsync(int id)
        {
            return await _context.Pacientes
                .FirstOrDefaultAsync(p => p.UsuarioId == id);
        }
    }
}