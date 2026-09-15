using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("usuario")]
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public long IdUsuario { get; set; }

        [Column("usuario")]
        public string Username { get; set; } = null!;

        [Column("nombre")]
        public string Nombre { get; set; } = null!;

        [Column("password_hash")]
        public string PasswordHash { get; set; } = null!;

        [Column("activo")]
        public bool Activo { get; set; }

        [Column("fecha_alta")]
        public DateTime FechaAlta { get; set; }

        public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
        public ICollection<PermisoOperacion> PermisosOperacion { get; set; } = new List<PermisoOperacion>();
        public ICollection<Inspeccion> Inspecciones { get; set; } = new List<Inspeccion>();
        public ICollection<InspeccionDetalle> InspeccionDetalles { get; set; } = new List<InspeccionDetalle>();
        public ICollection<Vale> ValesSolicitados { get; set; } = new List<Vale>();
    }
}
