using DataAccess.Context;
using DataAccess.Data;
using DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.AppLogic
{
    public class EstadoLogic
    {
        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        public EstadoLogic(GestionTurnosContext context)
        {
            _context = context;
        }
        #endregion

        // Lista de estados permitidos según tu regla de negocio
        private readonly List<string> _validStates = new List<string>
        {
            "Activo",
            "Cancelado",
            "Realizado",
            "Ausente",
            "Reprogramado",
            "Agendado",
            "En Curso",
            "Finalizado"
        };

        private static string UpperFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }

        public async Task<List<Estado>> GetAllEstadosAsync()
        {
            var repEstado = new EstadoRepository(_context);
            return await repEstado.GetAllEstadosAsync();
        }

        public async Task<Estado> GetEstadoByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Id must be greater than 0");

            var repEstado = new EstadoRepository(_context);
            var estado = await repEstado.GetEstadoByIdAsync(id);

            if (estado == null)
                throw new KeyNotFoundException("No state found with the provided id");

            return estado;
        }

        public async Task CreateEstadoAsync(Estado estado)
        {
            if (string.IsNullOrWhiteSpace(estado.Nombre))
                throw new ArgumentException("The name field must be filled");

            string normalizedName = UpperFirstLetter(estado.Nombre);

            if (!_validStates.Contains(normalizedName))
                throw new ArgumentException($"Invalid state name. Allowed: {string.Join(", ", _validStates)}");

            var repEstado = new EstadoRepository(_context);
            estado.Nombre = normalizedName; // Guardamos siempre normalizado
            await repEstado.CreateEstadoAsync(estado);
        }

        public async Task UpdateEstadoAsync(int id, Estado estado)
        {
            if (id <= 0) throw new ArgumentException("Id must be greater than 0");
            if (string.IsNullOrWhiteSpace(estado.Nombre)) throw new ArgumentException("The name field must be filled");

            var repEstado = new EstadoRepository(_context);
            var existingEstado = await repEstado.GetEstadoByIdAsync(id);

            if (existingEstado == null)
                throw new KeyNotFoundException("No state found with the provided id");

            string normalizedName = UpperFirstLetter(estado.Nombre);
            if (!_validStates.Contains(normalizedName))
                throw new ArgumentException("Invalid state name");

            existingEstado.Nombre = normalizedName;
            await repEstado.UpdateEstadoAsync(existingEstado);
        }

        public async Task DeleteEstadoAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Id must be greater than 0");

            var repEstado = new EstadoRepository(_context);
            var estado = await repEstado.GetEstadoByIdAsync(id);

            if (estado == null)
                throw new KeyNotFoundException("No state found with the provided id");

            await repEstado.DeleteEstadoAsync(estado);
        }
    }
}