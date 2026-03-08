using BenchmarkDotNet.Attributes;
using DataAccess.Context;
using DataAccess.Repository;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class MedicoRepositoryBenchmark
    {
        private DbContextOptions<GestionTurnosContext> _options;
        private GestionTurnosContext _context;
        private MedicoRepository _repo;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _options = new DbContextOptionsBuilder<GestionTurnosContext>()
                .UseInMemoryDatabase(databaseName: "bench_medico_db")
                .Options;
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _context?.Dispose();
            _context = new GestionTurnosContext(_options);

            // Ensure clean state per iteration
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            SeedMinimumData();

            var turnoRepo = new TurnoRepository(_context);
            _repo = new MedicoRepository(_context, turnoRepo);
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            _context?.Dispose();
            _context = null;
            _repo = null;
        }

        private void SeedMinimumData()
        {
            if (!_context.Especialidades.Any())
            {
                _context.Especialidades.Add(new Especialidad { Id = 1, Nombre = "General" });
            }

            if (!_context.Medicos.Any())
            {
                _context.Medicos.Add(new Medico
                {
                    Id = 1,
                    Nombre = "Prueba",
                    Apellido = "Test",
                    Dni = "12345678",
                    EspecialidadId = 1,
                    FechaAltaLaboral = DateTime.Today
                });
            }

            if (!_context.HorariosMedicos.Any())
            {
                _context.HorariosMedicos.Add(new HorarioMedico
                {
                    MedicoId = 1,
                    DiaSemana = 1,
                    HorarioAtencionInicio = new TimeSpan(8, 0, 0),
                    HorarioAtencionFin = new TimeSpan(17, 0, 0)
                });
            }

            _context.SaveChanges();
        }

        [Benchmark]
        public async Task UpdateDoctorWithSchedules_Run()
        {
            var medico = new Medico
            {
                Id = 1,
                Nombre = "PruebaMod",
                Apellido = "Test",
                Dni = "12345678",
                EspecialidadId = 1,
                FechaAltaLaboral = DateTime.Today
            };

            var horarios = new List<HorarioMedico>
            {
                new HorarioMedico { DiaSemana = 1, HorarioAtencionInicio = new TimeSpan(9,0,0), HorarioAtencionFin = new TimeSpan(18,0,0) },
                new HorarioMedico { DiaSemana = 2, HorarioAtencionInicio = new TimeSpan(9,0,0), HorarioAtencionFin = new TimeSpan(18,0,0) }
            };

            await _repo.UpdateDoctorWithSchedules(medico, horarios);
        }
    }
}
