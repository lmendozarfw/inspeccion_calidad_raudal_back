using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("inspeccion_detalle")]
    public class InspeccionDetalle
    {
        [Key]
        [Column("id_detalle")]
        public long IdDetalle { get; set; }

        [Column("id_inspeccion")]
        public long IdInspeccion { get; set; }

        [Column("id_defecto")]
        public long IdDefecto { get; set; }

        [Column("tipo_registro")]
        public string TipoRegistro { get; set; } = null!;   // PIOCHA | REPROCESO

        [Column("lado")]
        public string? Lado { get; set; }                   // IZQUIERDO | DERECHO | PAR

        [Column("cantidad")]
        public decimal Cantidad { get; set; }

        // Cuantitativa
        [Column("valor_objetivo")]
        public decimal? ValorObjetivo { get; set; }

        [Column("valor_min")]
        public decimal? ValorMin { get; set; }

        [Column("valor_max")]
        public decimal? ValorMax { get; set; }

        [Column("unidad")]
        public string? Unidad { get; set; }

        [Column("valor_obtenido")]
        public decimal? ValorObtenido { get; set; }

        // Cualitativa
        [Column("resultado_cualitativo")]
        public string? ResultadoCualitativo { get; set; }

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; }

        [Column("id_usuario")]
        public long IdUsuario { get; set; }

        public Inspeccion Inspeccion { get; set; } = null!;
        public Defecto Defecto { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public ICollection<Vale> Vales { get; set; } = new List<Vale>();
    }
}
