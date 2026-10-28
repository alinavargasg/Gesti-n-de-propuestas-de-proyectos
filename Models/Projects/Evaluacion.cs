using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationModule.Models.Projects
{
    [Table("evaluaciones")]
    public class Evaluacion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("proyecto_id")]
        public int ProyectoId { get; set; }

        [ForeignKey("ProyectoId")]
        public virtual Proyecto Proyecto { get; set; }

        [Required]
        [Column("docente_id")]
        public int DocenteId { get; set; }

        [ForeignKey("DocenteId")]
        public virtual Docente Docente { get; set; }

        [Required(ErrorMessage = "Las observaciones son obligatorias")]
        [MinLength(10, ErrorMessage = "Debe tener al menos 10 caracteres")]
        [MaxLength(2000, ErrorMessage = "No puede exceder 2000 caracteres")]
        [Column("observaciones")]
        public string Observaciones { get; set; }

        [Required(ErrorMessage = "Las recomendaciones son obligatorias")]
        [MinLength(10, ErrorMessage = "Debe tener al menos 10 caracteres")]
        [MaxLength(2000, ErrorMessage = "No puede exceder 2000 caracteres")]
        [Column("recomendaciones")]
        public string Recomendaciones { get; set; }

        [Column("fecha_evaluacion")]
        public DateTime FechaEvaluacion { get; set; } = DateTime.UtcNow;

        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100")]
        [Column("nota")]
        public decimal? Nota { get; set; }
    }
}
