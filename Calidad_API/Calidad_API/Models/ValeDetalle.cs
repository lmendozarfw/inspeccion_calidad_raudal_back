using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("vale_detalle")]
    public class ValeDetalle
    {
        [Key]
        [Column("id_vale_detalle")]
        public long IdValeDetalle { get; set; }

        [Column("id_vale")]
        public long IdVale { get; set; }

        [Column("id_pieza")]
        public long IdPieza { get; set; }

        [Column("cantidad")]
        public decimal Cantidad { get; set; }

        public Vale Vale { get; set; } = null!;
        public Pieza Pieza { get; set; } = null!;
    }
}