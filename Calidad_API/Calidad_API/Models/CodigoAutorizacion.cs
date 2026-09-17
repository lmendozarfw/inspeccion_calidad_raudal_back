using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models;

[Table("codigo_autorizacion")]
public class CodigoAutorizacion
{
    [Key]
    [Column("id_codigo_autorizacion")]
    public short IdCodigoAutorizacion { get; set; }
    
    [Column("is_usuario")]
    public long IdUsuario { get; set; }
    
    [Column("codigo_hash")]
    public string CodigoHash { get; set; }
    
    [Column("activo")]
    public bool Activo { get; set; }
    
    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }
    
    [Column("fecha_expiracion")]
    public DateTime FechaExpiracion { get; set; }
    
    public Usuario Usuario { get; set; } = null!;
}