using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Xml;

namespace Calidad_API.Models
{
    [Table("modelo")]
    public class Modelo
    {
        [Key]
        [Column("id_modelo")]
        public int IdModelo { get; set; }

        [Column("codigo_modelo_base")]
        public string CodigoMB { get; set; } = null!;

        [Column("codigo_combinacion")]
        public string CodigoCombinacion { get; set; } = null!;

        [Column("descripcion")]
        public string Descripcion { get; set; } = null!;       

        [Column("estatus")]
        public bool Estatus { get; set; }

        public ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();
    }
}
