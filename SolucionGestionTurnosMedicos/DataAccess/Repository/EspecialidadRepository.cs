using DataAccess.Data;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiGestionTurnosMedicos.CustomModels;

namespace DataAccess.Repository
{
    public class EspecialidadRepository
    {

        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        public EspecialidadRepository(GestionTurnosContext context)
        {
            _context = context;
        }
        #endregion

        public List<Especialidad> GetAllEspecialidades()
        {
            List<Especialidad> specialty = new List<Especialidad>();
            using (_context)
            {
                specialty = _context.Especialidades
                                .OrderBy(e => e.Nombre)
                                .ToList();
            }

            return specialty;
        }


        public Especialidad GetSpecialtyForId(int id)
        {
            Especialidad oSpecialty = new Especialidad();

            oSpecialty = _context.Especialidades.Find(id);
            return oSpecialty;
        }


        public void CreateSpecialty(Especialidad oSpecialty)
        {
            using(_context)
            {
                _context.Add(oSpecialty);
                _context.SaveChanges();
            }
        }

        public void UpdateSpecialty(Especialidad oSpecialty)
        {
            using (_context)
            {
                _context.Entry(oSpecialty).State = EntityState.Modified;
                _context.SaveChanges();

            }
        }

        public void DeleteSpecialty(Especialidad oSpecialty)
        {
            using (_context)
            {
                _context.Especialidades.Remove(oSpecialty);
                _context.SaveChanges();

            }
        }

        public bool VerifyIfSpecialtyExist(int id)
        {
            return _context.Especialidades.Where(s => s.Id == id).Any();
        }

        public List<Especialidad> ReturnCoveredSpecialties()
        {
            //List<Especialidad> specialties =
            //    (from e in _context.Especialidades
            //     join m in _context.Medicos
            //     on e.Id equals m.EspecialidadId
            //     group e by e.Id into g
            //     select g.First())
            //    .OrderBy(e => e.Nombre)
            //    .ToList();

            List<Especialidad> specialties = _context.Especialidades
                .Where(e => _context.Medicos.Any(m => m.EspecialidadId == e.Id)) 
                .OrderBy(e => e.Nombre)
                .ToList();

            return specialties;
        }
    }
}
