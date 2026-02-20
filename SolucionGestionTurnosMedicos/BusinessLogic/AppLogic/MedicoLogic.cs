using ApiGestionTurnosMedicos.CustomModels;
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
    public class MedicoLogic
    {
        #region ContextDataBase
        private readonly GestionTurnosContext _context;

        public MedicoLogic(GestionTurnosContext context)
        {
            _context = context;
        }
        #endregion

        public List<Medico> DoctorList()
        {
            MedicoRepository repDoctor = new(_context);
            return repDoctor.GetAllDoctors();
        }

        public Medico GetDoctorForId(int id)
        {
            try
            {
                MedicoRepository repDoctor = new(_context);
                Medico oDoctorFound = repDoctor.GetDoctorForId(id, repDoctor.Get_context()) ?? throw new ArgumentException("No doctor was found with that id");
                // Obtener y asignar los horarios
                oDoctorFound.Horarios = repDoctor.GetHorarioDoctorForId(id);
                return oDoctorFound;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Processing failed: {e.Message}");
                throw;
            }
        }

        public void CreateDoctor(Medico oDoctor, List<HorarioMedico> horarios)
        {
            MedicoRepository repDoctor = new(_context);

            try
            {
                repDoctor.CreateDoctor(oDoctor, horarios);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }
        

        public void UpdateDoctor(int id, MedicoCustom oDoctor)
        {
            MedicoRepository repDoctor = new(_context);
            MedicoCustom doctorCustom = new();

            try
            {
                Medico oDoctorFound = repDoctor.GetDoctorForId(id, repDoctor.Get_context()) ?? throw new ArgumentException("No doctor was found with that id");

                oDoctorFound.Nombre = oDoctor.Nombre;
                oDoctorFound.Apellido = oDoctor.Apellido;
                oDoctorFound.Dni = oDoctor.Dni;
                
                oDoctorFound.Telefono = oDoctor.Telefono;
                oDoctorFound.Direccion = oDoctor.Direccion;
                oDoctorFound.EspecialidadId = oDoctor.EspecialidadId;
                oDoctorFound.FechaAltaLaboral = oDoctor.FechaAltaLaboral;
                //oDoctorFound.HorarioAtencionInicio = doctorCustom.ModifyStartTime(oDoctor.HorarioAtencionInicio);
                //oDoctorFound.HorarioAtencionFin = doctorCustom.ModifyEndTime(oDoctor.HorarioAtencionFin);

                oDoctorFound.Foto = string.IsNullOrEmpty(oDoctor.Foto) ? oDoctorFound.Foto : Convert.FromBase64String(oDoctor.Foto);
                oDoctorFound.Matricula = oDoctor.Matricula;

                foreach (var horario in oDoctor.Horarios)
                {
                    //horario.MedicoId = oDoctorFound.Id;
                    //_context.HorariosMedicos.Update(horario);


                    // Buscar si el horario ya existe en la base de datos
                    var horarioExistente = _context.HorariosMedicos
                        .FirstOrDefault(h => h.DiaSemana == horario.DiaSemana && h.MedicoId == oDoctorFound.Id);

                    // Si alguno de los valores es nulo, eliminar el registro si existe
                    if (horario.HorarioAtencionInicio == null || horario.HorarioAtencionFin == null)
                    {
                        if (horarioExistente != null)
                        {
                            _context.HorariosMedicos.Remove(horarioExistente);
                        }
                        continue;
                    }

                    // Si el registro existe, actualizarlo; si no, agregarlo
                    if (horarioExistente != null)
                    {
                        // Sólo actualizar los campos necesarios
                        horarioExistente.HorarioAtencionInicio = horario.HorarioAtencionInicio;
                        horarioExistente.HorarioAtencionFin = horario.HorarioAtencionFin;
                    }
                    else
                    {
                        horario.Id = 0; // Asegurarse de que se trate como un nuevo registro
                        horario.MedicoId = oDoctorFound.Id; // Por las dudas
                        _context.HorariosMedicos.Add(horario);
                    }
                }

                repDoctor.UpdateDoctor(oDoctorFound);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public void DeleteDoctor(int id)
        {
            MedicoRepository repDoctor = new(_context);

            try
            {
                Medico oDoctorFound = repDoctor.GetDoctorForId(id, repDoctor.Get_context()) ?? throw new ArgumentException("No doctor was found with that id");
                repDoctor.DeleteDoctor(oDoctorFound);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public List<Medico> FindDoctorForSpecialty(int id)
        {
            MedicoRepository repDoctor = new(_context);

            if (id == 0)
            {
                throw new ArgumentException("The id can't be 0");
            }

            try
            {
                List<Medico> doctors = repDoctor.FindDoctorForSpecialty(id);

                if (doctors.Count == 0)
                {
                    // En lugar de lanzar una excepción, simplemente devuelve una lista vacía.
                    return doctors;
                }

                return doctors;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public List<MedicoCustom> ReturnAllDoctorsWithOurSpecialty()
        {
            MedicoRepository repDoctor = new(_context);

            try
            {
                return repDoctor.ReturnAllDoctorsWithOurSpecialty();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }

        }

        public int GetDoctorsQty()
        {
            MedicoRepository repDoctor = new(_context);

            try
            {
                return repDoctor.GetQtyDoctors();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }

        }

        public List<HorarioMedico> GetScheduleForDoctor(int id)
        {
            MedicoRepository repDoctor = new(_context);
            try
            {
                return repDoctor.GetHorariosForDoctor(id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public List<DateTime> GetFullScheduleForDoctor(int id)
        {
            MedicoRepository repDoctor = new(_context);

            return repDoctor.GetTurnosOcupados(id);
        }
    }
}
