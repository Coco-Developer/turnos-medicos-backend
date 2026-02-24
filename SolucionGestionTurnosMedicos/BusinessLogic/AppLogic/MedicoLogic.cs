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
        #region Context and Repository
        private readonly GestionTurnosContext _context;
        private readonly MedicoRepository _repDoctor;

        public MedicoLogic(GestionTurnosContext context)
        {
            _context = context;
            // Centralizamos el repositorio para evitar múltiples instancias innecesarias
            _repDoctor = new MedicoRepository(_context);
        }
        #endregion

        public async Task<List<Medico>> DoctorList()
        {
            return await _repDoctor.GetAllDoctors();
        }

        public async Task<Medico> GetDoctorForId(int id)
        {
            try
            {
                // Buscamos el médico usando el repositorio
                Medico oDoctorFound = await _repDoctor.GetDoctorForId(id, _context)
                    ?? throw new ArgumentException($"No se encontró un médico con el ID: {id}");

                // Obtenemos y asignamos los horarios
                oDoctorFound.Horarios = await _repDoctor.GetHorarioDoctorForId(id);
                return oDoctorFound;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error en GetDoctorForId: {e.Message}");
                throw;
            }
        }

        public async Task CreateDoctor(Medico oDoctor, List<HorarioMedico> horarios)
        {
            try
            {
                // Nos aseguramos de que horarios no sea nulo para evitar errores en el bucle del repositorio
                var listaHorarios = horarios ?? new List<HorarioMedico>();
                await _repDoctor.CreateDoctor(oDoctor, listaHorarios);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error en CreateDoctor: {e.ToString()}");
                throw;
            }
        }

        public async Task UpdateDoctor(int id, MedicoCustom oDoctor)
        {
            try
            {
                // Obtenemos el médico actual desde la DB
                Medico oDoctorFound = await _repDoctor.GetDoctorForId(id, _context)
                    ?? throw new ArgumentException("No se encontró el médico para actualizar");

                // Actualización de campos básicos
                oDoctorFound.Nombre = oDoctor.Nombre;
                oDoctorFound.Apellido = oDoctor.Apellido;
                oDoctorFound.Dni = oDoctor.Dni;
                oDoctorFound.Telefono = oDoctor.Telefono;
                oDoctorFound.Direccion = oDoctor.Direccion;
                oDoctorFound.EspecialidadId = oDoctor.EspecialidadId;
                oDoctorFound.FechaAltaLaboral = oDoctor.FechaAltaLaboral;
                oDoctorFound.Matricula = oDoctor.Matricula;

                // Procesamiento seguro de la foto (Base64)
                if (!string.IsNullOrWhiteSpace(oDoctor.Foto))
                {
                    try
                    {
                        oDoctorFound.Foto = Convert.FromBase64String(oDoctor.Foto);
                    }
                    catch
                    {
                        // Si el formato es inválido, mantenemos la foto anterior o null
                        Console.WriteLine("Advertencia: El formato de imagen Base64 no es válido.");
                    }
                }

                // Gestión de Horarios
                if (oDoctor.Horarios != null)
                {
                    foreach (var horario in oDoctor.Horarios)
                    {
                        var horarioExistente = await _context.HorariosMedicos
                            .FirstOrDefaultAsync(h => h.DiaSemana == horario.DiaSemana && h.MedicoId == oDoctorFound.Id);

                        // Si el horario viene sin horas, se interpreta como eliminación
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
                            horario.Id = 0; // Aseguramos que sea una inserción nueva
                            horario.MedicoId = oDoctorFound.Id;
                            await _context.HorariosMedicos.AddAsync(horario);
                        }
                    }
                }

                // Guardamos los cambios a través del repositorio
                await _repDoctor.UpdateDoctor(oDoctorFound);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error en UpdateDoctor: {e.ToString()}");
                throw;
            }
        }

        public async Task DeleteDoctor(int id)
        {
            try
            {
                Medico oDoctorFound = await _repDoctor.GetDoctorForId(id, _context)
                    ?? throw new ArgumentException("No se encontró el médico para eliminar");

                await _repDoctor.DeleteDoctor(oDoctorFound);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error en DeleteDoctor: {e.ToString()}");
                throw;
            }
        }

        public async Task<List<Medico>> FindDoctorForSpecialty(int id)
        {
            if (id <= 0) throw new ArgumentException("El ID de especialidad debe ser mayor a 0");

            try
            {
                return await _repDoctor.FindDoctorForSpecialty(id);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error en FindDoctorForSpecialty: {e.ToString()}");
                throw;
            }
        }

        public async Task<List<MedicoCustom>> ReturnAllDoctorsWithOurSpecialty()
        {
            try
            {
                return await _repDoctor.ReturnAllDoctorsWithOurSpecialty();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error en ReturnAllDoctorsWithOurSpecialty: {e.ToString()}");
                throw;
            }
        }

        public async Task<int> GetDoctorsQty()
        {
            try
            {
                return await _repDoctor.GetQtyDoctors();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error en GetDoctorsQty: {e.ToString()}");
                throw;
            }
        }

        public async Task<List<HorarioMedico>> GetScheduleForDoctor(int id)
        {
            try
            {
                return await _repDoctor.GetHorariosForDoctor(id);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error en GetScheduleForDoctor: {e.ToString()}");
                throw;
            }
        }

        public async Task<List<DateTime>> GetFullScheduleForDoctor(int id)
        {
            try
            {
                return await _repDoctor.GetTurnosOcupados(id);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error en GetFullScheduleForDoctor: {e.Message}");
                throw;
            }
        }
    }
}