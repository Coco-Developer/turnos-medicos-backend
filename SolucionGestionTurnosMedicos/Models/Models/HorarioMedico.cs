using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Data
{
    [Table("HorarioMedico")]
    public class HorarioMedico
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("medicold")] // Según la foto de tu DB es medicold
        public int MedicoId { get; set; }

        [Column("diaSemana")]
        public byte DiaSemana { get; set; }

        [Column("horario_atencion_inicio")]
        public TimeSpan? HorarioAtencionInicio { get; set; }

        [Column("horario_atencion_fin")]
        public TimeSpan? HorarioAtencionFin { get; set; }
    }
}