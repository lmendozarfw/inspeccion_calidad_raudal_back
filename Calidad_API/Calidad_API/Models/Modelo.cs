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
        public long IdModelo { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; } = null!;

        [Column("nombre")]
        public string Nombre { get; set; } = null!;

        [Column("familia")]
        public string? Familia { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }

        [Column("fecha_alta")]
        public DateTime FechaAlta { get; set; }

        [Column("usuario_alta")]
        public long? UsuarioAlta { get; set; }

        public ICollection<Transfer> Transfers { get; set; } = new List<Transfer>();
    }
}
