using Azure;
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

        [Column("id_unidad_negocio")]
        public long? IdUnidadNegocio { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }

        [Column("fecha_alta")]
        public DateTime FechaAlta { get; set; }

        public UnidadNegocio? UnidadNegocio { get; set; }
        public ICollection<Operacion> Operaciones { get; set; } = new List<Operacion>();
    }
}