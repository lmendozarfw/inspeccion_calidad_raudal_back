using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("transfer")]
    public class Transfer
    {
        [Key]
        [Column("id_transfer")]
        public long IdTransfer { get; set; }

        [Column("qr_raw")]
        public string QrRaw { get; set; } = null!;

        [Column("programa")]
        public string? Programa { get; set; }

        [Column("lista")]
        public string? Lista { get; set; }

        [Column("lote")]
        public string Lote { get; set; } = null!;

        [Column("punto")]
        public string? Punto { get; set; }

        [Column("id_modelo")]
        public int? IdModelo { get; set; }

        [Column("fecha_primer_scan")]
        public DateTime FechaPrimerScan { get; set; }

        [Column("fecha_ultimo_scan")]
        public DateTime FechaUltimoScan { get; set; }

        public Modelo? Modelo { get; set; }
        public ICollection<Inspeccion> Inspecciones { get; set; } = new List<Inspeccion>();
    }
}
