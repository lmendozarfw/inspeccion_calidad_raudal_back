using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("permiso_operacion")]
    public class PermisoOperacion
    {
        [Column("id_usuario")]
        public long IdUsuario { get; set; }

        [Column("id_operacion")]
        public long IdOperacion { get; set; }

        [Column("puede_capturar")]
        public bool PuedeCapturar { get; set; }

        [Column("puede_consultar")]
        public bool PuedeConsultar { get; set; }

        [Column("puede_generar_vale")]
        public bool PuedeGenerarVale { get; set; }

        public Usuario Usuario { get; set; } = null!;
        public Operacion Operacion { get; set; } = null!;
    }
}