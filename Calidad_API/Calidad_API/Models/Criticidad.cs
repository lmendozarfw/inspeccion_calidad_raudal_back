using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("criticidad")]
    public class Criticidad
    {
        [Key]
        [Column("id_criticidad")]
        public short IdCriticidad { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; } = null!;

        [Column("nombre")]
        public string Nombre { get; set; } = null!;

        [Column("nivel")]
        public short Nivel { get; set; }

        public ICollection<Defecto> Defectos { get; set; } = new List<Defecto>();
    }
}
