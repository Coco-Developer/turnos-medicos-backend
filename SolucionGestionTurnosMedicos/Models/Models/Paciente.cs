using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Data
{
    /// <summary>
    /// Representa un paciente en el sistema de gestión de turnos médicos.
    /// </summary>
    public partial class Paciente
    {
        /// <summary>
        /// Identificador único del paciente.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Apellido del paciente.
        /// Campo obligatorio.
        /// </summary>
        public string Apellido { get; set; } = null!;

        /// <summary>
        /// Nombre del paciente.
        /// Campo obligatorio.
        /// </summary>
        public string Nombre { get; set; } = null!;

        /// <summary>
        /// Número de teléfono del paciente.
        /// Campo obligatorio.
        /// </summary>
        public string Telefono { get; set; } = null!;

        /// <summary>
        /// Correo electrónico del paciente.
        /// Campo obligatorio.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Fecha de nacimiento del paciente.
        /// </summary>
        public DateTime FechaNacimiento { get; set; }

        /// <summary>
        /// Número de DNI del paciente.
        /// Campo obligatorio.
        /// </summary>
        public string Dni { get; set; } 

        /// <summary>
        /// Clave foránea al usuario de login.
        /// </summary>
        public int UsuarioId { get; set; }

        /// <summary>
        /// Relación de navegación al usuario.
        /// </summary>
        public virtual Usuario Usuario { get; set; } = null!;

        /// <summary>
        /// Obtiene el nombre completo del paciente combinando nombre y apellido.
        /// </summary>
        /// <returns>Cadena con el nombre completo del paciente</returns>
        public string NombreCompletoPaciente()
        {
            return $"{Nombre} {Apellido}";
        }

        /// <summary>
        /// Calcula la edad del paciente basada en su fecha de nacimiento.
        /// </summary>
        /// <returns>Edad del paciente en años</returns>
        public int EdadDelPaciente(DateTime fechaNacimiento)
        {
            DateTime fechaActual = DateTime.Now;
            int edad = fechaActual.Year - FechaNacimiento.Year;

            if (fechaActual < FechaNacimiento.AddYears(edad))
            {
                edad--;
            }

            return edad;
        }
    }
}
