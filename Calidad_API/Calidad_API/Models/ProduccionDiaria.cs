using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("produccion_diaria")]
    public class ProduccionDiaria
    {
        [Key]
        [Column("id_produccion")]
        public long IdProduccion { get; set; }

        [Column("id_area")]
        public long IdArea { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; }   // solo fecha en BD (DATE)

        [Column("cantidad_producida")]
        public decimal CantidadProducida { get; set; }

        [Column("turno")]
        public string? Turno { get; set; }

        [Column("origen")]
        public string? Origen { get; set; }

        public Area Area { get; set; } = null!;
    }
}
