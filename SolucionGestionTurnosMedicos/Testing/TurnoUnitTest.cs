using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.CustomModels;

namespace Testing
{
    public class TurnoUnitTest
    {
        private readonly Mock<ILogger<TurnoController>> _mockLogger = new();

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

            // Usar el contexto de prueba que añade claves a las vistas
            return new TestGestionTurnosContext(options);
        }

        private void SeedTestData(GestionTurnosContext context)
        {
            // Idempotente: sólo añadir si no existen
            if (!context.Especialidades.Any())
                context.Especialidades.AddRange(especialidades);

            if (!context.Estados.Any())
                context.Estados.AddRange(estadosCentral);

            if (!context.Medicos.Any())
                context.Medicos.AddRange(medicos);

            if (!context.Pacientes.Any())
                context.Pacientes.AddRange(pacientes);

            if (!context.Turnos.Any())
                context.Turnos.AddRange(turnos);

            if (!context.VwTurnos.Any())
                context.VwTurnos.AddRange(vwturnos);

            if (!context.VwTurnoCounts.Any())
                context.VwTurnoCounts.AddRange(vwturnocount);

            if (!context.VwTurnoXMedicoCounts.Any())
                context.VwTurnoXMedicoCounts.AddRange(vwturnoxmedicocount);

            if (!context.VwTurnoCalendars.Any())
                context.VwTurnoCalendars.AddRange(vwturnocalendar);

            context.SaveChanges();
        }

        // ---------- Tests: todos adaptados para usar InMemory (sin borrar ninguno) ----------

        [Fact]
        public void Get_ReturnsListOfVwTurno()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = new TurnoController(context, _mockLogger.Object);

            var result = controller.Get();

            Assert.NotNull(result);
            Assert.IsType<List<VwTurno>>(result);
            Assert.True(result.Count > 0);
        }

        [Fact]
        public void Get_ById_ReturnsVwTurno()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = new TurnoController(context, _mockLogger.Object);

            var result = controller.Get(1);

            Assert.NotNull(result);
            Assert.IsType<VwTurno>(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void Post_ReturnsBadRequest_WhenValidationFails()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = new TurnoController(context, _mockLogger.Object);

            var turnoCustom = new TurnoCustom();
            // Ejecuta Post (es síncrono en el controlador original) y valida que sea BadRequest
            var result = controller.Post(turnoCustom).Result;

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void UpdateTurno_ReturnsBadRequest_WhenTurnoUpdateIsNull()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = new TurnoController(context, _mockLogger.Object);

            var result = controller.UpdateTurno(1, null);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("no pueden ser nulos", badRequest.Value.ToString());
        }

        [Fact]
        public void UpdateTurno_ReturnsNotFound_WhenTurnoDoesNotExist()
        {
            var dbName = Guid.NewGuid().ToString();
            // Crear un contexto con sólo un turno Id = 1 para reproducir el caso
            using var context = CreateInMemoryContext(dbName);
            if (!context.Turnos.Any())
            {
                context.Turnos.Add(new Turno { Id = 1, MedicoId = 1, PacienteId = 1, Fecha = DateTime.Today, Hora = new TimeSpan(10,0,0), EstadoId = 1 });
                context.SaveChanges();
            }

            var controller = new TurnoController(context, _mockLogger.Object);

            var turnoUpdate = new TurnoCustom();

            var result = controller.UpdateTurno(2, turnoUpdate);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Turno no encontrado", notFound.Value?.ToString() ?? "");
        }

        [Fact]
        public void UpdateTurno_ReturnsOK()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = new TurnoController(context, _mockLogger.Object);

            var turnoUpdate = new TurnoCustom
            {
                MedicoId = 1,
                PacienteId = 1,
                Fecha = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"),
                Hora = "10:00",
                EstadoId = 1,
                Observaciones = "Prueba"
            };

            var result = controller.UpdateTurno(1, turnoUpdate);

            var ok = Assert.IsType<OkObjectResult>(result);

            var message = ok.Value?.GetType().GetProperty("Message")?.GetValue(ok.Value) as string;

            Assert.Equal("Turno actualizado correctamente.", message);

            // Verificar persistencia
            var updated = context.Turnos.Find(1);
            Assert.NotNull(updated);
            Assert.Equal(TimeSpan.Parse("10:00"), updated.Hora);
        }

        [Fact]
        public void Put_ReturnsBadRequest_WhenValidationFails()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = new TurnoController(context, _mockLogger.Object);

            var result = controller.Put(1, 0);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Delete_CallsDeleteShift()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = new TurnoController(context, _mockLogger.Object);

            var ex = Record.Exception(() => controller.Delete(1));
            Assert.Null(ex);
        }

        [Fact]
        public void GetBusyShiftsGroupedByDay_ReturnsList()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = new TurnoController(context, _mockLogger.Object);

            var result = controller.GetBusyShiftsGroupedByDay(1);

            Assert.NotNull(result);
            Assert.IsType<List<HorarioTurnos>>(result);
            Assert.True(result.Count > 0);
        }

        [Fact]
        public void GetListOfShiftsByPatient_ReturnsTurnosPaciente()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = new TurnoController(context, _mockLogger.Object);

            var result = controller.GetListOfShiftsByPatient(1);

            Assert.NotNull(result);
            Assert.IsType<TurnosPaciente>(result);
            Assert.True(result.Turnos.Count > 0);
        }

        [Fact]
        public void GetListOfShiftsByPatientVw_ReturnsList()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = new TurnoController(context, _mockLogger.Object);

            var result = controller.GetListOfShiftsByPatientVw(1);

            Assert.NotNull(result);
            Assert.IsType<List<VwTurno>>(result);
        }

        [Fact]
        public void GetListOfShiftsByDoctor_ReturnsList()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = new TurnoController(context, _mockLogger.Object);

            var result = controller.GetListOfShiftsByDoctor(1);

            Assert.NotNull(result);
            Assert.IsType<List<Turno>>(result);
        }

        [Fact]
        public void GetShiftsOfDate_ReturnsList()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = new TurnoController(context, _mockLogger.Object);

            var result = controller.GetShiftsOfDate(DateTime.Now);

            Assert.NotNull(result);
            Assert.IsType<List<VwTurno>>(result);
        }

        [Fact]
        public void GetDatesWithShiftsOfMonth_ReturnsList()
        {
            // En los datos del escenario (declarados arriba de todo), en la lista
            // "turnos" debe haber 1 o más fechas del "mesATestear" y 1 o más de
            // otro mes. Y se debe ajustar el valor de la siguiente constante:
            const int mesATestear = 6;

            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = new TurnoController(context, _mockLogger.Object);
            
            // Obtener las fechas del mes a testear y de otro mes
            var fechasMes = turnos.Where(t => t.Fecha.Month == mesATestear).Select(t => t.Fecha).ToList();
            var fechasOtroMes = turnos.Where(t => t.Fecha.Month != mesATestear).Select(t => t.Fecha).ToList();

            var result = controller.GetDatesWithShiftsOfMonth(mesATestear);

            Assert.NotNull(result);
            Assert.IsType<List<DateTime>>(result);

            // Verificar que las fechas del mes a testear están en el resultado
            foreach (var fecha in fechasMes)
                Assert.Contains(fecha, result);

            // Verificar que las fechas de otro mes NO están en el resultado
            foreach (var fecha in fechasOtroMes)
                Assert.DoesNotContain(fecha, result);
        }

        [Fact]
        public void GetDashboardData_ReturnsDictionary()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            SeedTestData(context);

            var controller = new TurnoController(context, _mockLogger.Object);

            var result = controller.GetDashboardData();

            Assert.NotNull(result);
            Assert.IsType<Dictionary<string, object>>(result);
        }

        [Fact]
        public void GetCalendarData_ReturnsActionResultList()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);

            // Sembramos datos concretos de calendario para este test
            if (!context.VwTurnoCalendars.Any())
            {
                context.VwTurnoCalendars.AddRange(vwturnocalendar);
                context.SaveChanges();
            }

            var controller = new TurnoController(context, _mockLogger.Object);

            var result = controller.GetCalendarData("2025-07-01", "2025-07-31");

            Assert.NotNull(result);
            Assert.IsType<ActionResult<List<CalendarEvent>>>(result);
        }
    }
}