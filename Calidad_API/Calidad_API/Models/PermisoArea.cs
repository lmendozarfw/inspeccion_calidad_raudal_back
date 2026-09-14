using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("permiso_area")]
    public class PermisoArea
    {
        [Column("id_usuario")]
        public long IdUsuario { get; set; }
        public Usuario Usuario { get; set; } = null!;

        [Column("id_area")]
        public long IdArea { get; set; }

        [Column("puede_capturar")]
        public bool PuedeCapturar { get; set; }

        [Column("puede_consultar")]
        public bool PuedeConsultar { get; set; }

        [Column("puede_generar_vale")]
        public bool PuedeGenerarVale { get; set; }

        public Area Area { get; set; } = null!;
    }
}
