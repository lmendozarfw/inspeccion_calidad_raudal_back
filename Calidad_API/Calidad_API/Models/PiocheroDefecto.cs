using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models;

[Table("piochero_defecto")]
public class PiocheroDefecto
{
    [Column("id_usuario")] 
    public long IdUsuario { get; set; }
    [Column("id_defecto")] 
    public long IdDefecto { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Defecto Defecto { get; set; } = null!;
}