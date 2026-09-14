using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("defecto_tipo_inspeccion")]
    public class DefectoTipoInspeccion
    {
        [Column("id_defecto")]
        public long IdDefecto { get; set; }

        [Column("id_tipo_inspeccion")]
        public short IdTipoInspeccion { get; set; }

        public Defecto Defecto { get; set; } = null!;
        public TipoInspeccion TipoInspeccion { get; set; } = null!;
    }
}
