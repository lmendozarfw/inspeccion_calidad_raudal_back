using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("centro_trabajo")]
    public class CentroTrabajo
    {
        [Key]
        [Column("id_centro_trabajo")]
        public long IdCentroTrabajo { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; } = null!;

        [Column("nombre")]
        public string Nombre { get; set; } = null!;

        [Column("id_departamento")]
        public long? IdDepartamento { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }

        [Column("fecha_alta")]
        public DateTime FechaAlta { get; set; }

        public Departamento? Departamento { get; set; }
        public ICollection<Area> Areas { get; set; } = new List<Area>();
    }
}
