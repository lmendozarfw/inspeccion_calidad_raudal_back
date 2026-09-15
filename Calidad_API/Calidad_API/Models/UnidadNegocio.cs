using Azure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("unidad_negocio")]
    public class UnidadNegocio
    {
        [Key]
        [Column("id_unidad_negocio")]
        public long IdUnidadNegocio { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; } = null!;

        [Column("nombre")]
        public string Nombre { get; set; } = null!;

        [Column("activo")]
        public bool Activo { get; set; }

        [Column("fecha_alta")]
        public DateTime FechaAlta { get; set; }

        public ICollection<Departamento> Departamentos { get; set; } = new List<Departamento>();
        public ICollection<Operacion> Operaciones { get; set; } = new List<Operacion>();
    }
}