using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("defecto")]
    public class Defecto
    {
        [Key]
        [Column("id_defecto")]
        public long IdDefecto { get; set; }

        [Column("id_operacion")]
        public long IdOperacion { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; } = null!;

        [Column("nombre")]
        public string Nombre { get; set; } = null!;

        [Column("id_criticidad")]
        public short? IdCriticidad { get; set; }

        [Column("aplica_pieza")]
        public bool AplicaPieza { get; set; }

        [Column("id_pieza")]
        public long? IdPieza { get; set; }

        [Column("ponderacion")]
        public decimal? Ponderacion { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }

        [Column("fecha_alta")]
        public DateTime FechaAlta { get; set; }

        [Column("usuario_alta")]
        public long? UsuarioAlta { get; set; }


        public Operacion Operacion { get; set; } = null!;
        public Criticidad? Criticidad { get; set; }
        public Pieza? Pieza { get; set; }
        public ICollection<DefectoTipoInspeccion> DefectoTiposInspeccion { get; set; } = new List<DefectoTipoInspeccion>();
        public ICollection<InspeccionDetalle> InspeccionDetalles { get; set; } = new List<InspeccionDetalle>();
    }
}
