using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("solicitud_material")]
    public class SolicitudMaterial
    {
        [Key]
        [Column("id_solicitud")]
        public long IdSolicitud { get; set; }

        [Column("id_inspeccion")]
        public long IdInspeccion { get; set; }

        [Column("id_inspeccion_detalle")]
        public long? IdInspeccionDetalle { get; set; }

        [Column("id_operacion")]
        public long IdOperacion { get; set; }

        [Column("programa")]
        public string? Programa { get; set; }

        [Column("lote")]
        public string Lote { get; set; } = null!;

        [Column("lista")]
        public string? Lista { get; set; }

        [Column("punto")]
        public string? Punto { get; set; }

        [Column("lado")]
        public string? Lado { get; set; }

        [Column("id_pieza")]
        public long IdPieza { get; set; }

        [Column("cantidad")]
        public decimal Cantidad { get; set; }

        [Column("id_usuario_solicita")]
        public long IdUsuarioSolicita { get; set; }

        [Column("fecha_solicitud")]
        public DateTime FechaSolicitud { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "PENDIENTE";

        [Column("id_corte")]
        public long? IdCorte { get; set; }

        [Column("id_vale")]
        public long? IdVale { get; set; }

        public Inspeccion Inspeccion { get; set; } = null!;
        public Operacion Operacion { get; set; } = null!;
        public Pieza Pieza { get; set; } = null!;
        public Usuario UsuarioSolicita { get; set; } = null!;
        public CorteVale? Corte { get; set; }
        public Vale? Vale { get; set; }
    }
}