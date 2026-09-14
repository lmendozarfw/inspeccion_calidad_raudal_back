using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("departamento")]
    public class Departamento
    {
        [Key]
        [Column("id_departamento")]
        public long IdDepartamento { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; } = null!;

        [Column("nombre")]
        public string Nombre { get; set; } = null!;

        [Column("activo")]
        public bool Activo { get; set; }

        [Column("fecha_alta")]
        public DateTime FechaAlta { get; set; }

        public ICollection<CentroTrabajo> CentrosTrabajo { get; set; } = new List<CentroTrabajo>();
        public ICollection<Area> Areas { get; set; } = new List<Area>();
    }
}
