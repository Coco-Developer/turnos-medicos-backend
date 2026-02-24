using DataAccess.Data;
using DataAccess.Repository;
using Microsoft.Extensions.Logging;
using Models.CustomModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic.AppLogic
{
    public class MedicoLogic
    {
        private readonly MedicoRepository _repDoctor;
        private readonly ILogger<MedicoLogic> _logger;

        public MedicoLogic(
            MedicoRepository repDoctor,
            ILogger<MedicoLogic> logger)
        {
            _repDoctor = repDoctor;
            _logger = logger;
        }

        #region Consultas

        public async Task<List<Medico>> DoctorList()
        {
            return await _repDoctor.GetAllDoctors();
        }

        public async Task<Medico> GetDoctorForId(int id)
        {
            var doctor = await _repDoctor.GetDoctorForId(id);

            if (doctor == null)
                throw new ArgumentException($"No se encontró un médico con ID {id}");

            return doctor;
        }

        public async Task<List<Medico>> FindDoctorForSpecialty(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Especialidad inválida");

            return await _repDoctor.FindDoctorForSpecialty(id);
        }

        public async Task<List<MedicoCustom>> ReturnAllDoctorsWithOurSpecialty()
        {
            return await _repDoctor.ReturnAllDoctorsWithOurSpecialty();
        }

        public async Task<int> GetDoctorsQty()
        {
            return await _repDoctor.GetQtyDoctors();
        }

        public async Task<List<HorarioMedico>> GetScheduleForDoctor(int id)
        {
            return await _repDoctor.GetHorariosForDoctor(id);
        }

        public async Task<List<DateTime>> GetFullScheduleForDoctor(int id)
        {
            return await _repDoctor.GetTurnosOcupados(id);
        }

        #endregion

        #region CRUD

        public async Task CreateDoctor(MedicoCustom dto)
        {
            // Sacamos el primer horario de la lista para llenar la tabla Medico
            var primerHorario = dto.Horarios?.FirstOrDefault();

            var medico = new Medico
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                EspecialidadId = dto.EspecialidadId,
                FechaAltaLaboral = dto.FechaAltaLaboral,
                Direccion = dto.Direccion,
                Dni = dto.Dni,
                Telefono = dto.Telefono,
                Matricula = dto.Matricula,

                // IMPORTANTE: Aquí llenamos las columnas NOT NULL de la foto
                // Si el front no manda nada, le ponemos un default para que no explote
                HorarioAtencionInicio = primerHorario?.HorarioAtencionInicio ?? new TimeSpan(8, 0, 0),
                HorarioAtencionFin = primerHorario?.HorarioAtencionFin ?? new TimeSpan(17, 0, 0)
            };

            if (!string.IsNullOrWhiteSpace(dto.Foto))
            {
                try
                {
                    string base64Data = dto.Foto.Contains(",") ? dto.Foto.Split(',')[1] : dto.Foto;
                    medico.Foto = Convert.FromBase64String(base64Data);
                }
                catch { medico.Foto = null; }
            }

            // Mapeo para la tabla HorarioMedico (la otra tabla de la foto)
            var horarios = dto.Horarios?.Select(h => new HorarioMedico
            {
                DiaSemana = h.DiaSemana,
                HorarioAtencionInicio = h.HorarioAtencionInicio.GetValueOrDefault(new TimeSpan(8, 0, 0)),
                HorarioAtencionFin = h.HorarioAtencionFin.GetValueOrDefault(new TimeSpan(17, 0, 0))
            }).ToList() ?? new List<HorarioMedico>();

            await _repDoctor.CreateDoctor(medico, horarios);
        }

        public async Task UpdateDoctor(int id, MedicoCustom dto)
        {
            var doctor = await _repDoctor.GetDoctorForId(id);

            if (doctor == null)
                throw new ArgumentException("No se encontró el médico para actualizar");

            doctor.Nombre = dto.Nombre;
            doctor.Apellido = dto.Apellido;
            doctor.Dni = dto.Dni;
            doctor.Telefono = dto.Telefono;
            doctor.Direccion = dto.Direccion;
            doctor.EspecialidadId = dto.EspecialidadId;
            doctor.FechaAltaLaboral = dto.FechaAltaLaboral;
            doctor.Matricula = dto.Matricula;

            if (!string.IsNullOrWhiteSpace(dto.Foto))
            {
                try
                {
                    string base64Data = dto.Foto.Contains(",") ? dto.Foto.Split(',')[1] : dto.Foto;
                    doctor.Foto = Convert.FromBase64String(base64Data);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Imagen Base64 inválida en UpdateDoctor");
                }
            }

            await _repDoctor.UpdateDoctor(doctor);
        }

        public async Task DeleteDoctor(int id)
        {
            var doctor = await _repDoctor.GetDoctorForId(id);

            if (doctor == null)
                throw new ArgumentException("No se encontró el médico para eliminar");

            await _repDoctor.DeleteDoctor(doctor);
        }

        #endregion
    }
}