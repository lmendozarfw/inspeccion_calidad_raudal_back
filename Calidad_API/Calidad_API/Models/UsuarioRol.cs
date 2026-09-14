using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("usuario_rol")]
    public class UsuarioRol
    {
        [Column("id_usuario")]
        public long IdUsuario { get; set; }
        public Usuario Usuario { get; set; } = null!;

        [Column("id_rol")]
        public short IdRol { get; set; }
        public Rol Rol { get; set; } = null!;
    }
}
