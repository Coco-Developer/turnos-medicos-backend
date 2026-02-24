using ApiGestionTurnosMedicos.CustomModels;
using DataAccess.Context;
using DataAccess.Data;
using DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic.AppLogic
{
    public class MedicoLogic
    {
        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        public MedicoLogic(GestionTurnosContext context)
        {
            _context = context;
        }
        #endregion

        public async Task<List<Medico>> DoctorList()
        {
            MedicoRepository repDoctor = new(_context);
            return await repDoctor.GetAllDoctors();
        }

        public async Task<Medico> GetDoctorForId(int id)
        {
            try
            {
                MedicoRepository repDoctor = new(_context);
                // Mantenemos tu firma original pasando el contexto
                Medico oDoctorFound = await repDoctor.GetDoctorForId(id, _context) ?? throw new ArgumentException("No doctor was found with that id");

                // Obtener y asignar los horarios
                oDoctorFound.Horarios = await repDoctor.GetHorarioDoctorForId(id);
                return oDoctorFound;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Processing failed: {e.Message}");
                throw;
            }
        }

        public async Task CreateDoctor(Medico oDoctor, List<HorarioMedico> horarios)
        {
            MedicoRepository repDoctor = new(_context);

            try
            {
                await repDoctor.CreateDoctor(oDoctor, horarios);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public async Task UpdateDoctor(int id, MedicoCustom oDoctor)
        {
            MedicoRepository repDoctor = new(_context);

            try
            {
                // Mantenemos Get_context() tal como estaba en tu código
                Medico oDoctorFound = await repDoctor.GetDoctorForId(id, repDoctor.Get_context()) ?? throw new ArgumentException("No doctor was found with that id");

                oDoctorFound.Nombre = oDoctor.Nombre;
                oDoctorFound.Apellido = oDoctor.Apellido;
                oDoctorFound.Dni = oDoctor.Dni;
                oDoctorFound.Telefono = oDoctor.Telefono;
                oDoctorFound.Direccion = oDoctor.Direccion;
                oDoctorFound.EspecialidadId = oDoctor.EspecialidadId;
                oDoctorFound.FechaAltaLaboral = oDoctor.FechaAltaLaboral;

                oDoctorFound.Foto = string.IsNullOrEmpty(oDoctor.Foto) ? oDoctorFound.Foto : Convert.FromBase64String(oDoctor.Foto);
                oDoctorFound.Matricula = oDoctor.Matricula;

                foreach (var horario in oDoctor.Horarios)
                {
                    // Cambiamos a FirstOrDefaultAsync para no bloquear el hilo
                    var horarioExistente = await _context.HorariosMedicos
                        .FirstOrDefaultAsync(h => h.DiaSemana == horario.DiaSemana && h.MedicoId == oDoctorFound.Id);

                    if (horario.HorarioAtencionInicio == null || horario.HorarioAtencionFin == null)
                    {
                        if (horarioExistente != null)
                        {
                            _context.HorariosMedicos.Remove(horarioExistente);
                        }
                        continue;
                    }

                    if (horarioExistente != null)
                    {
                        horarioExistente.HorarioAtencionInicio = horario.HorarioAtencionInicio;
                        horarioExistente.HorarioAtencionFin = horario.HorarioAtencionFin;
                    }
                    else
                    {
                        horario.Id = 0;
                        horario.MedicoId = oDoctorFound.Id;
                        await _context.HorariosMedicos.AddAsync(horario);
                    }
                }

                await repDoctor.UpdateDoctor(oDoctorFound);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public async Task DeleteDoctor(int id)
        {
            MedicoRepository repDoctor = new(_context);

            try
            {
                Medico oDoctorFound = await repDoctor.GetDoctorForId(id, repDoctor.Get_context()) ?? throw new ArgumentException("No doctor was found with that id");
                await repDoctor.DeleteDoctor(oDoctorFound);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public async Task<List<Medico>> FindDoctorForSpecialty(int id)
        {
            MedicoRepository repDoctor = new(_context);

            if (id == 0)
            {
                throw new ArgumentException("The id can't be 0");
            }

            try
            {
                return await repDoctor.FindDoctorForSpecialty(id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public async Task<List<MedicoCustom>> ReturnAllDoctorsWithOurSpecialty()
        {
            MedicoRepository repDoctor = new(_context);

            try
            {
                return await repDoctor.ReturnAllDoctorsWithOurSpecialty();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public async Task<int> GetDoctorsQty()
        {
            MedicoRepository repDoctor = new(_context);

            try
            {
                return await repDoctor.GetQtyDoctors();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public async Task<List<HorarioMedico>> GetScheduleForDoctor(int id)
        {
            MedicoRepository repDoctor = new(_context);
            try
            {
                return await repDoctor.GetHorariosForDoctor(id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public async Task<List<DateTime>> GetFullScheduleForDoctor(int id)
        {
            MedicoRepository repDoctor = new(_context);
            return await repDoctor.GetTurnosOcupados(id);
        }
    }
}