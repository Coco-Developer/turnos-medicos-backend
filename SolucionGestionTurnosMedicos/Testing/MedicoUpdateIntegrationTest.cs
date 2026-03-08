using DataAccess.Context;
using DataAccess.Repository;
using DataAccess.Data;
using BusinessLogic.AppLogic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Models.CustomModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Testing
{
    public class MedicoUpdateIntegrationTest
    {
        [Fact]
        public async Task UpdateDoctor_WithSchedulePayload_ReproducesServerBehavior()
        {
            var options = new DbContextOptionsBuilder<GestionTurnosContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new GestionTurnosContext(options);

            // Seed Especialidad
            context.Especialidades.Add(new Especialidad { Id = 4, Nombre = "Cardio" });

            // Seed Medico with Id 43
            var medico = new Medico
            {
                Id = 43,
                Nombre = "Marcelo",
                Apellido = "Servera",
                Dni = "19456852",
                EspecialidadId = 4,
                FechaAltaLaboral = DateTime.Parse("2020-01-01"),
                Matricula = "MP45132",
                Telefono = "3516487878",
                Direccion = "Av. Alem1545"
            };

            context.Medicos.Add(medico);
            context.SaveChanges();

            var turnoRepo = new TurnoRepository(context);
            var medicoRepo = new MedicoRepository(context, turnoRepo);
            var loggerMock = new Mock<ILogger<MedicoLogic>>();
            var medicoLogic = new MedicoLogic(medicoRepo, loggerMock.Object);

            // Build DTO like the frontend
            var dto = new MedicoCustom
            {
                Id = 43,
                Nombre = "Marcelo Raul Daniel",
                Apellido = "Servera",
                Dni = "19456852",
                Telefono = "3516487878",
                Direccion = "Av. Alem1545",
                EspecialidadId = 4,
                FechaAltaLaboral = DateTime.Parse("2026-02-27"),
                Matricula = "MP45132",
                Horarios = new List<HorarioMedico>
                {
                    new HorarioMedico { MedicoId = 43, DiaSemana = 1, HorarioAtencionInicio = TimeSpan.Parse("13:00:00"), HorarioAtencionFin = TimeSpan.Parse("18:00:00") },
                    new HorarioMedico { MedicoId = 43, DiaSemana = 4, HorarioAtencionInicio = TimeSpan.Parse("09:00:00"), HorarioAtencionFin = TimeSpan.Parse("17:00:00") },
                    new HorarioMedico { MedicoId = 43, DiaSemana = 6, HorarioAtencionInicio = TimeSpan.Parse("09:00:00"), HorarioAtencionFin = TimeSpan.Parse("17:00:00") }
                }
            };

            Exception? ex = null;
            try
            {
                await medicoLogic.UpdateDoctor(43, dto);
            }
            catch (Exception e)
            {
                ex = e;
            }

            // Fail the test and include exception details if any
            if (ex != null)
            {
                Assert.True(false, "Exception during UpdateDoctor: " + ex.ToString());
            }

            // If no exception, verify horarios were updated
            var horarios = await medicoRepo.GetHorariosForDoctor(43);
            Assert.NotNull(horarios);
            Assert.Equal(3, horarios.Count);
        }
    }
}
