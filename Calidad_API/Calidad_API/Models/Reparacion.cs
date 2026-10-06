using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models;

[Table("Reparaciones")]
public class Reparacion
{
    [Key]
    [Column("id_reparacion")]
    public long IdReparacion {get; set;}
    [Column("id_usuario")]
    public long IdUsuario {get; set;}
    [Column("id_inspeccion_detalle")]
    public long IdInspeccionDetalle {get; set;}
    [Column("fecha_inicio")]
    public DateTime FechaInicio {get; set;}
    [Column("fecha_fin")]
    public DateTime FechaFin {get; set;}
    
    // Navegaciones
    public Usuario Usuario {get; set;} = null!;
    public InspeccionDetalle InspeccionDetalle {get; set;} = null!;
}