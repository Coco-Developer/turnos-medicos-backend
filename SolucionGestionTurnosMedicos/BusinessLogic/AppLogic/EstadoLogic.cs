using DataAccess.Data;
using DataAccess.Repository;
using System.Collections.Generic;

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

        private readonly List<string> _validStates = new List<string>
        {
            "Activo",
            "Cancelado",
            "Realizado"/*,
            "Ausente",
            "Cancelado",
            "Reprogramado"*/
        };

        public static string UpperFirstLetter(string input)
        {
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        public List<Estado> GetAllEstados()
        {
            EstadoRepository repEstado = new EstadoRepository(_context);
            return repEstado.GetAllEstados();
        }

        public Estado GetEstadoById(int id)
        {
            if (id <= 0) throw new ArgumentException("Id must be greater than 0");

            EstadoRepository repEstado = new EstadoRepository(_context);
            Estado estado = repEstado.GetEstadoById(id);
            if (estado == null) throw new ArgumentException("No state found with the provided id");

            return estado;
        }

        public void CreateEstado(Estado estado)
        {
            if (string.IsNullOrWhiteSpace(estado.Nombre)) throw new ArgumentException("The name field must be filled");
            if (!_validStates.Contains(UpperFirstLetter(estado.Nombre))) throw new ArgumentException("Invalid state name, only admit `AGENDADO` - `REPROGRAMADO` - `EN CURSO` - `CANCELADO` - `AUSENTE` - `FINALIZADO`");

            EstadoRepository repEstado = new EstadoRepository(_context);
            repEstado.CreateEstado(estado);
        }

        public void UpdateEstado(int id, Estado estado)
        {
            if (id <= 0) throw new ArgumentException("Id must be greater than 0");
            if (string.IsNullOrWhiteSpace(estado.Nombre)) throw new ArgumentException("The name field must be filled");
            if (!_validStates.Contains(estado.Nombre)) throw new ArgumentException("Invalid state name");

            EstadoRepository repEstado = new EstadoRepository(_context);
            Estado existingEstado = repEstado.GetEstadoById(id);
            if (existingEstado == null) throw new ArgumentException("No state found with the provided id");

            existingEstado.Nombre = estado.Nombre;
            repEstado.UpdateEstado(existingEstado);
        }

        public void DeleteEstado(int id)
        {
            if (id <= 0) throw new ArgumentException("Id must be greater than 0");

            EstadoRepository repEstado = new EstadoRepository(_context);
            Estado estado = repEstado.GetEstadoById(id);
            if (estado == null) throw new ArgumentException("No state found with the provided id");

            repEstado.DeleteEstado(estado);
        }
    }
}

