using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("corte_vale")]
    public class CorteVale
    {
        [Key]
        [Column("id_corte")]
        public long IdCorte { get; set; }

        [Column("id_operacion")]
        public long IdOperacion { get; set; }

        [Column("fecha_corte")]
        public DateTime FechaCorte { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "PENDIENTE_AUTH";

        [Column("id_usuario_autoriza")]
        public long? IdUsuarioAutoriza { get; set; }

        [Column("fecha_autorizacion")]
        public DateTime? FechaAutorizacion { get; set; }

        [Column("id_vale")]
        public long? IdVale { get; set; }

        public Operacion Operacion { get; set; } = null!;
        public Usuario? UsuarioAutoriza { get; set; }
        public Vale? Vale { get; set; }
        public ICollection<SolicitudMaterial> Solicitudes { get; set; } = new List<SolicitudMaterial>();
    }
}