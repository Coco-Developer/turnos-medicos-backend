using DataAccess.Data;
using DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        public List<Especialidad> SpecialtyList()
        {
            EspecialidadRepository repSpecialty = new EspecialidadRepository(_context);
            return repSpecialty.GetAllEspecialidades();
        }

        public Especialidad GetSpecialtyForId(int id)
        {
            if (id == 0) throw new ArgumentException("Id cannot be 0");

            try
            {
                EspecialidadRepository repSpecialty = new EspecialidadRepository(_context);
                Especialidad oSpecialtyFound = repSpecialty.GetSpecialtyForId(id) ?? throw new ArgumentException("No specialty was found with that id");
                return oSpecialtyFound;
            }
            catch(Exception e)
            {
                Console.WriteLine($"Processing failed: {e.Message}");
                throw;
            }
        }

        public void CreateSpecialty(Especialidad oSpecialty)
        {
            EspecialidadRepository repSpecialty = new EspecialidadRepository(_context);

            #region Validations

            if (string.IsNullOrWhiteSpace(oSpecialty.Nombre)) throw new ArgumentException("The name field must be filled");

            if(!Regex.IsMatch(oSpecialty.Nombre, @"^[a-zA-Z\s]+$")) throw new ArgumentException("The name can only contain letters and spaces");

            #endregion

            try
            {
                repSpecialty.CreateSpecialty(oSpecialty);
            }
            catch (Exception e) 
            { 
                Console.WriteLine(e.ToString()); 
                throw; 
            }
        }

        public void UpdateSpecialty(int id, Especialidad oSpecialty)
        {
            EspecialidadRepository repSpecialty = new EspecialidadRepository(_context);

            try
            {
                Especialidad oSpecialtyFound = repSpecialty.GetSpecialtyForId(id) ?? throw new ArgumentException("No specialty was found with that id");
                oSpecialtyFound.Nombre = oSpecialty.Nombre;
                repSpecialty.UpdateSpecialty(oSpecialtyFound);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public void DeleteSpecialty(int id)
        {
            EspecialidadRepository repSpecialty = new EspecialidadRepository(_context);

            try
            {
                Especialidad oSpecialtyFound = repSpecialty.GetSpecialtyForId(id) ?? throw new ArgumentException("No specialty was found with that id");
                repSpecialty.DeleteSpecialty(oSpecialtyFound);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public List<Especialidad> CoveredSpecialtyList()
        {
            EspecialidadRepository repSpecialty = new EspecialidadRepository(_context);

            try
            {
                return repSpecialty.ReturnCoveredSpecialties();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }


        }

    }
}
