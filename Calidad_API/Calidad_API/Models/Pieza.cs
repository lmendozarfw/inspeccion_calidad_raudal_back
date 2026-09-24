using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("pieza")]
    public class Pieza
    {
        [Key]
        [Column("id_pieza")]
        public long IdPieza { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; } = null!;

        [Column("nombre")]
        public string Nombre { get; set; } = null!;

        [Column("unidad")]
        public string? Unidad { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }

        [Column("fecha_alta")]
        public DateTime FechaAlta { get; set; }

        public ICollection<Defecto> Defectos { get; set; } = new List<Defecto>();
        public ICollection<ValeDetalle> ValeDetalles { get; set; } = new List<ValeDetalle>();
    }

}
