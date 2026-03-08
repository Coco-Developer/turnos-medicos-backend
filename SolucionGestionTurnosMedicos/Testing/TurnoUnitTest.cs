using DataAccess.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.CustomModels;
using Xunit;
using Moq;
using ApiGestionTurnosMedicos.Controllers;
using ApiGestionTurnosMedicos.Validations;
using DataAccess.Repository;
using BusinessLogic.AppLogic.Services;
using BusinessLogic.AppLogic;
using DataAccess.Data;
using System.Collections.Generic;
using System.Linq;

namespace Testing
{
    public class TurnoUnitTest
    {
        private readonly Mock<ILogger<TurnoController>> _mockLoggerController = new();

        // Datos de escenario centralizados (reutilizados en SeedTestData)
        private static readonly List<Paciente> pacientes = new()
        {
            new() { Id = 1, Nombre = "Carlos", Apellido = "Gómez", Telefono = "555-1234", Email = "carlos@example.com", FechaNacimiento = new DateTime(1990,1,1), Dni = "11222333" }
        };

        private static readonly List<Medico> medicos = new()
        {
            new() { Id = 1, Nombre = "Prueba", Apellido = "Test", Telefono = "123456", Dni = "12345678", EspecialidadId = 1, FechaAltaLaboral = DateTime.Today.AddYears(-1), Matricula="MP-1701", Direccion = "Santa Rosa 1793" }
        };

        private static readonly List<Especialidad> especialidades = new()
        {
            new() { Id = 1, Nombre = "Medicina General" }
        };

        private static readonly List<Estado> estadosCentral = new()
        {
            new Estado { Id = 1, Nombre = "Activo", Clase = "success", Icono = "clock", Color = "green" },
            new Estado { Id = 2, Nombre = "Cancelado", Clase = "danger", Icono = "ban", Color = "red" },
            new Estado { Id = 3, Nombre = "Realizado", Clase = "info", Icono = "ok", Color = "blue" }
        };

        private static readonly List<Turno> turnos = new()
        {
            new() { Id = 1, MedicoId = 1, PacienteId = 1, Fecha = new DateTime(2025, 6, 10), Hora = new TimeSpan(10, 0, 0), EstadoId = 2 },
            new() { Id = 2, MedicoId = 1, PacienteId = 1, Fecha = new DateTime(2025, 6, 15), Hora = new TimeSpan(10, 0, 0), EstadoId = 3 },
            new() { Id = 3, MedicoId = 1, PacienteId = 1, Fecha = new DateTime(2025, 7, 1), Hora = new TimeSpan(10, 0, 0), EstadoId = 1 },
            new() { Id = 4, MedicoId = 1, PacienteId = 1, Fecha = DateTime.Today, Hora = new TimeSpan(10, 0, 0), EstadoId = 1 },
            new() { Id = 5, MedicoId = 1, PacienteId = 1, Fecha = DateTime.Now.AddDays(1), Hora = new TimeSpan(11, 0, 0), EstadoId = 1 }
        };

        private static readonly List<VwTurno> vwturnos = new()
        {
            new() {
                Id = 1,
                MedicoId = 1,
                PacienteId = 1,
                Fecha = DateTime.Today,
                Hora = "10:00",
                EstadoId = 1,
                Observaciones = "Test",
                Paciente = "Gómez, Carlos",
                PacienteTelefono = "555-1234",
                PacienteEmail = "carlos@example.com",
                PacienteDni = "11.222.333",
                Medico = "Test, Prueba",
                Estado = "Activo",
                EstadoClase = "cls",
                EstadoIcono = "icono",
                EspecialidadId = 1,
                Especialidad = "Clínica",
                Foto = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAAAAACH5BAAAAAAALAAAAAABAAEAAAICTAEAOw==")
            }
        };

        private static readonly List<VwTurnoCount> vwturnocount = new List<VwTurnoCount>
        {
            new() { Yr = 2025, Mo = 8, Estado = "Activo", Clase = "success", Color = "#00FF00", CountId = 10}
        };

        private static readonly List<VwTurnoXMedicoCount> vwturnoxmedicocount = new List<VwTurnoXMedicoCount>
        {
            new () { Yr = 2025, Medico = "Test, Prueba", Estado = "Activo", Clase = "success", Color = "#00FF00", CountId = 10 }
        };

        private static readonly List<VwTurnoCalendar> vwturnocalendar = new()
        {
            new() { Fecha = new DateTime(2025, 7, 10), Qty = 3 },
            new() { Fecha = new DateTime(2025, 7, 15), Qty = 2 }
        };

        // ---------- Helpers para InMemory ----------
        private GestionTurnosContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<GestionTurnosContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new TestGestionTurnosContext(options);
        }

        private void SeedTestData(GestionTurnosContext context)
        {
            if (!context.Especialidades.Any()) context.Especialidades.AddRange(especialidades);
            if (!context.Estados.Any()) context.Estados.AddRange(estadosCentral);
            if (!context.Medicos.Any()) context.Medicos.AddRange(medicos);
            if (!context.Pacientes.Any()) context.Pacientes.AddRange(pacientes);
            if (!context.Turnos.Any()) context.Turnos.AddRange(turnos);
            if (!context.VwTurnos.Any()) context.VwTurnos.AddRange(vwturnos);
            if (!context.VwTurnoCounts.Any()) context.VwTurnoCounts.AddRange(vwturnocount);
            if (!context.VwTurnoXMedicoCounts.Any()) context.VwTurnoXMedicoCounts.AddRange(vwturnoxmedicocount);
            if (!context.VwTurnoCalendars.Any()) context.VwTurnoCalendars.AddRange(vwturnocalendar);
            if (!context.HorariosMedicos.Any()) context.HorariosMedicos.Add(new HorarioMedico { MedicoId = 1, DiaSemana = 1, HorarioAtencionInicio = new TimeSpan(8,0,0), HorarioAtencionFin = new TimeSpan(17,0,0) });

            context.SaveChanges();
        }

        private TurnoController BuildController(GestionTurnosContext context)
        {
            var turnoRepo = new TurnoRepository(context);
            var medicoRepo = new MedicoRepository(context, turnoRepo);
            var pacienteRepo = new PacienteRepository(context);
            var especialidadRepo = new EspecialidadRepository(context);
            var estadoRepo = new EstadoRepository(context);

            var mockEmailSender = new Mock<IEmailSender>();
            var mockLoggerEmail = new Mock<ILogger<EmailService>>();
            var emailService = new EmailService(mockEmailSender.Object, mockLoggerEmail.Object);

            var mockLoggerTurnoLogic = new Mock<ILogger<TurnoLogic>>();
            var turnoLogic = new TurnoLogic(turnoRepo, medicoRepo, pacienteRepo, emailService, mockLoggerTurnoLogic.Object);

            var validations = new ValidationsMethodPut(medicoRepo, pacienteRepo, turnoRepo, especialidadRepo, estadoRepo);

            return new TurnoController(turnoLogic, validations, _mockLoggerController.Object);
        }

        // ---------- Tests adaptados a controller actual ----------

        [Fact]
        public async Task Smoke_GetAndGetById()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = BuildController(context);

            var listResult = await controller.Get();
            var okList = Xunit.Assert.IsType<OkObjectResult>(listResult.Result);
            var list = Xunit.Assert.IsType<List<VwTurno>>(okList.Value);
            Xunit.Assert.NotEmpty(list);

            var itemResult = await controller.Get(1);
            var okItem = Xunit.Assert.IsType<OkObjectResult>(itemResult.Result);
            var item = Xunit.Assert.IsType<VwTurno>(okItem.Value);
            Xunit.Assert.Equal(1, item.Id);
        }

        [Fact]
        public async Task GetDatesWithShiftsOfMonth_ReturnsList()
        {
            const int mesATestear = 6;
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = BuildController(context);
            var actionResult = await controller.GetDatesWithShifts(mesATestear);
            var ok = Xunit.Assert.IsType<OkObjectResult>(actionResult.Result);
            var list = Xunit.Assert.IsType<List<DateTime>>(ok.Value);

            var fechasMes = turnos.Where(t => t.Fecha.Month == mesATestear).Select(t => t.Fecha).ToList();
            foreach (var fecha in fechasMes)
                Xunit.Assert.Contains(fecha, list);
        }

        [Fact]
        public async Task GetCalendarData_ReturnsActionResultList()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = BuildController(context);

            var actionResult = await controller.GetCalendarData("2025-07-01", "2025-07-31");
            var ok = Xunit.Assert.IsType<OkObjectResult>(actionResult.Result);
            var list = Xunit.Assert.IsType<List<Models.CustomModels.CalendarEvent>>(ok.Value);
            Xunit.Assert.NotNull(list);
        }

        [Fact]
        public async Task GetDashboardData_ReturnsDictionary()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = BuildController(context);

            var actionResult = await controller.GetDashboardData();
            var ok = Xunit.Assert.IsType<OkObjectResult>(actionResult.Result);
            var dict = Xunit.Assert.IsType<Dictionary<string, object>>(ok.Value);
            Xunit.Assert.NotNull(dict);
        }
    }
}