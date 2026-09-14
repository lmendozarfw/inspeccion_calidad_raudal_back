using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("tipo_inspeccion")]
    public class TipoInspeccion
    {
        [Key]
        [Column("id_tipo_inspeccion")]
        public short IdTipoInspeccion { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; } = null!;

        [Column("nombre")]
        public string Nombre { get; set; } = null!;

        [Column("activo")]
        public bool Activo { get; set; }

        public ICollection<DefectoTipoInspeccion> DefectoTiposInspeccion { get; set; } = new List<DefectoTipoInspeccion>();
        public ICollection<Inspeccion> Inspecciones { get; set; } = new List<Inspeccion>();
    }
}
