using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models;

[Table("defecto_operacion")]
public class DefectoOperacion
{
    [Column("id_defecto")]   public long IdDefecto { get; set; }
    [Column("id_operacion")] public long IdOperacion { get; set; }
    public Defecto Defecto { get; set; } = null!;
    public Operacion Operacion { get; set; } = null!;
}