using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("meta_calidad")]
    public class MetaCalidad
    {
        [Key]
        [Column("id_meta")]
        public long IdMeta { get; set; }

        [Column("id_area")]
        public long IdArea { get; set; }

        [Column("anio")]
        public short Anio { get; set; }

        [Column("semana")]
        public short? Semana { get; set; }

        [Column("meta_pct")]
        public decimal MetaPct { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }

        public Area Area { get; set; } = null!;
    }
}
