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

        [Column("id_inspeccion")]
        public long IdInspeccion { get; set; }

        [Column("id_usuario_solicita")]
        public long IdUsuarioSolicita { get; set; }

        [Column("fecha_generacion")]
        public DateTime FechaGeneracion { get; set; }

        [Column("ruta_pdf")]
        public string? RutaPdf { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "GENERADO";

        public Inspeccion Inspeccion { get; set; } = null!;
        public Usuario UsuarioSolicita { get; set; } = null!;
        public ICollection<ValeDetalle> Detalles { get; set; } = new List<ValeDetalle>();
    }
}