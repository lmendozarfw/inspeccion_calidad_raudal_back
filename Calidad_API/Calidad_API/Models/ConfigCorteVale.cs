using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calidad_API.Models
{
    [Table("config_corte_vale")]
    public class ConfigCorteVale
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("hora_corte_1")]
        public TimeSpan HoraCorte1 { get; set; }

        [Column("hora_corte_2")]
        public TimeSpan HoraCorte2 { get; set; }

        [Column("zona_horaria")]
        public string ZonaHoraria { get; set; } = "America/Mexico_City";

        [Column("activo")]
        public bool Activo { get; set; }

        [Column("email_supervisores")]
        public string? EmailSupervisores { get; set; }
    }
}