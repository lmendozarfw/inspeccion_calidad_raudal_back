using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models;

[Table("codigos_autorizacion")]
public class CodigoAutorizacion
{
    [Key]
    [Column("id_codigo_autorizacion")]
    public long IdCodigoAutorizacion { get; set; }
    
    [Column("id_usuario")]
    public long IdUsuario { get; set; }
    
    [Column("codigo_cifrado")]
    public string? CodigoCifrado { get; set; }
    
    [Column("activo")]
    public bool Activo { get; set; }
    
    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }
    
    [Column("fecha_expiracion")]
    public DateTime FechaExpiracion { get; set; }
    
    public Usuario Usuario { get; set; } = null!;
}