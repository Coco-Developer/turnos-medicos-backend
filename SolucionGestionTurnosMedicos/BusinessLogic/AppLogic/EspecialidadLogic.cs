using DataAccess.Context;
using DataAccess.Data;
using DataAccess.Repository;
using System.Text.RegularExpressions;

namespace BusinessLogic.AppLogic
{
    public class EspecialidadLogic
    {
        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        public EspecialidadLogic(GestionTurnosContext context)
        {
            _context = context;
        }
        #endregion

        public async Task<List<Especialidad>> SpecialtyListAsync()
        {
            var repSpecialty = new EspecialidadRepository(_context);
            return await repSpecialty.GetAllEspecialidadesAsync();
        }

        public async Task<Especialidad> GetSpecialtyForIdAsync(int id)
        {
            if (id == 0) throw new ArgumentException("Id cannot be 0");

            try
            {
                var repSpecialty = new EspecialidadRepository(_context);
                var oSpecialtyFound = await repSpecialty.GetSpecialtyForIdAsync(id);

                return oSpecialtyFound ?? throw new KeyNotFoundException("No specialty was found with that id");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Processing failed: {e.Message}");
                throw;
            }
        }

        public async Task CreateSpecialtyAsync(Especialidad oSpecialty)
        {
            var repSpecialty = new EspecialidadRepository(_context);

            #region Validations
            if (string.IsNullOrWhiteSpace(oSpecialty.Nombre))
                throw new ArgumentException("The name field must be filled");

            if (!Regex.IsMatch(oSpecialty.Nombre, @"^[a-zA-Z\s]+$"))
                throw new ArgumentException("The name can only contain letters and spaces");
            #endregion

            try
            {
                await repSpecialty.CreateSpecialtyAsync(oSpecialty);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public async Task UpdateSpecialtyAsync(int id, Especialidad oSpecialty)
        {
            var repSpecialty = new EspecialidadRepository(_context);

            try
            {
                var oSpecialtyFound = await repSpecialty.GetSpecialtyForIdAsync(id)
                    ?? throw new KeyNotFoundException("No specialty was found with that id");

                oSpecialtyFound.Nombre = oSpecialty.Nombre;
                await repSpecialty.UpdateSpecialtyAsync(oSpecialtyFound);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public async Task DeleteSpecialtyAsync(int id)
        {
            var repSpecialty = new EspecialidadRepository(_context);

            try
            {
                var oSpecialtyFound = await repSpecialty.GetSpecialtyForIdAsync(id)
                    ?? throw new KeyNotFoundException("No specialty was found with that id");

                await repSpecialty.DeleteSpecialtyAsync(oSpecialtyFound);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public async Task<List<Especialidad>> CoveredSpecialtyListAsync()
        {
            var repSpecialty = new EspecialidadRepository(_context);

            try
            {
                return await repSpecialty.ReturnCoveredSpecialtiesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }
    }
}