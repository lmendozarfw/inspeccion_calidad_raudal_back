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
    public short IdUsuario { get; set; }
    
    [Column("codigo_hash")]
    public string CodigoHash { get; set; }
    
    [Column("activo")]
    public bool Activo { get; set; }
    
    [Column("fecha_expiracion")]
    public DateTime FechaCreacion { get; set; }
    
    [Column("fecha_expiracion")]
    public DateTime FechaExpiracion { get; set; }
}