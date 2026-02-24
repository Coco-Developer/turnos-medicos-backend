using System;
using System.Collections.Generic;
using DataAccess.Data;

namespace Models.CustomModels // Usaremos este namespace para ser consistentes
{
    public class MedicoCustom
    {
        public int Id { get; set; }
        public string? Apellido { get; set; }
        public string? Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Dni { get; set; }
        public int EspecialidadId { get; set; }
        public DateTime FechaAltaLaboral { get; set; }
        public virtual ICollection<HorarioMedico> Horarios { get; set; } = new List<HorarioMedico>();
        public string? Foto { get; set; }
        public string? Matricula { get; set; }

        // CORRECCIÓN: Esta es la propiedad que te faltaba
        public string? Especialidad { get; set; }
    }

    // CORRECCIÓN: Clase para que el compilador la encuentre
    public class MedicoConEspecialidad
    {
        public int Id { get; set; }
        public string? Apellido { get; set; }
        public string? Nombre { get; set; }
        public string? Especialidad { get; set; }

        public string NombreCompletoMedico(string nombre, string apellido)
        {
            return $"{nombre} {apellido}";
        }
    }
}