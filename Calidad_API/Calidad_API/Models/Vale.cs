using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("vale")]
    public class Vale
    {
        [Key]
        [Column("id_vale")]
        public long IdVale { get; set; }

        [Column("folio")]
        public string Folio { get; set; } = null!;

        [Column("id_inspeccion_detalle")]
        public long IdInspeccionDetalle { get; set; }

        [Column("id_pieza")]
        public long IdPieza { get; set; }

        [Column("cantidad")]
        public decimal Cantidad { get; set; }

        [Column("id_usuario_solicita")]
        public long IdUsuarioSolicita { get; set; }

        [Column("fecha_generacion")]
        public DateTime FechaGeneracion { get; set; }

        [Column("ruta_pdf")]
        public string? RutaPdf { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "GENERADO";   // GENERADO | ENTREGADO | CANCELADO

        public InspeccionDetalle InspeccionDetalle { get; set; } = null!;
        public Pieza Pieza { get; set; } = null!;
        public Usuario UsuarioSolicita { get; set; } = null!;
    }
}
