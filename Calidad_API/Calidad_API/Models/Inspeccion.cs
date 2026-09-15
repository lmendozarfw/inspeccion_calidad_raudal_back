using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("inspeccion")]
    public class Inspeccion
    {
        [Key]
        [Column("id_inspeccion")]
        public long IdInspeccion { get; set; }

        [Column("id_transfer")]
        public long IdTransfer { get; set; }

        [Column("id_operacion")]
        public long IdOperacion { get; set; }

        [Column("id_tipo_inspeccion")]
        public short IdTipoInspeccion { get; set; }

        [Column("id_usuario")]
        public long IdUsuario { get; set; }

        [Column("fecha_inspeccion")]
        public DateTime FechaInspeccion { get; set; }

        [Column("dispositivo")]
        public string? Dispositivo { get; set; }

        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "ABIERTA";   // ABIERTA | CERRADA

        public Transfer Transfer { get; set; } = null!;
        public Operacion Operacion { get; set; } = null!;
        public TipoInspeccion TipoInspeccion { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public ICollection<InspeccionDetalle> Detalles { get; set; } = new List<InspeccionDetalle>();
    }
}
