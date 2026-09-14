using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("area")]
    public class Area
    {
        [Key]
        [Column("id_area")]
        public long IdArea { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; } = null!;

        [Column("nombre")]
        public string Nombre { get; set; } = null!;

        [Column("id_centro_trabajo")]
        public long? IdCentroTrabajo { get; set; }

        [Column("id_departamento")]
        public long? IdDepartamento { get; set; }

        [Column("proceso")]
        public string Proceso { get; set; } = null!;

        [Column("activo")]
        public bool Activo { get; set; }

        [Column("fecha_alta")]
        public DateTime FechaAlta { get; set; }

        [Column("usuario_alta")]
        public long? UsuarioAlta { get; set; }

        public CentroTrabajo? CentroTrabajo { get; set; }
        public Departamento? Departamento { get; set; }
        public ICollection<Defecto> Defectos { get; set; } = new List<Defecto>();
        public ICollection<PermisoArea> PermisosArea { get; set; } = new List<PermisoArea>();
        public ICollection<Inspeccion> Inspecciones { get; set; } = new List<Inspeccion>();
        public ICollection<MetaCalidad> MetasCalidad { get; set; } = new List<MetaCalidad>();
        public ICollection<ProduccionDiaria> ProduccionesDiarias { get; set; } = new List<ProduccionDiaria>();
    }
}
