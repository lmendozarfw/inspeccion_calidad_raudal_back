using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models;

[Table("Reparacion")]
public class Reparacion
{
    [Key]
    [Column("id_reparacion")]
    public long IdReparacion {get; set;}
    [Column("id_usuario")]
    public long IdUsuario {get; set;}
    [Column("id_inspeccion")]
    public long IdInspeccion {get; set;}
    [Column("fecha_inicio")]
    public DateTime FechaInicio {get; set;}
    [Column("fecha_fin")]
    public DateTime FechaFin {get; set;}
    
    // Navegaciones
    public Usuario Usuario {get; set;} = null!;
    public Inspeccion Inspeccion {get; set;} = null!;
}