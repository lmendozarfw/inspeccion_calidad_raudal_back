using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models;
[Table("registros_autorizacion")]
public class RegistroAutorizacion
{
    [Key]
    [Column("id_registro_autorizacion")]
    public short IdRegistroAutorizacion { get; set; }
    
    [Column("id_usuario_solicita")]
    public long IdUsuarioSolicita { get; set; }
    
    [Column("id_usuario_autoriza")]
    public long IdUsuarioAutoriza {get; set; }
    
    [Column("id_inspeccion")]
    public long IdInspeccion { get; set; }
    
    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }
    
    public Usuario UsuarioSolicita { get; set; }
    public Usuario UsuarioAutoriza { get; set; }
    public Inspeccion Inspeccion { get; set; }
}