using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationModule.Models.Projects
{
    /// <summary>
    /// Clase base que representa un proyecto.
    /// </summary>
    [Table("proyectos")]
    public class Proyecto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [MinLength(10, ErrorMessage = "El título debe tener al menos 10 caracteres")]
        [MaxLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        [Column("titulo")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "La problemática es obligatoria")]
        [MinLength(20, ErrorMessage = "La problemática debe tener al menos 20 caracteres")]
        [MaxLength(2000, ErrorMessage = "La problemática no puede exceder 2000 caracteres")]
        [Column("problematica")]
        public string Problematica { get; set; }

        [Required(ErrorMessage = "El objetivo general es obligatorio")]
        [MinLength(20, ErrorMessage = "El objetivo general debe tener al menos 20 caracteres")]
        [MaxLength(1000, ErrorMessage = "El objetivo general no puede exceder 1000 caracteres")]
        [Column("objetivo_general")]
        public string ObjetivoGeneral { get; set; }

        [Required(ErrorMessage = "La descripción del prototipo es obligatoria")]
        [MinLength(20, ErrorMessage = "La descripción del prototipo debe tener al menos 20 caracteres")]
        [MaxLength(2000, ErrorMessage = "La descripción del prototipo no puede exceder 2000 caracteres")]
        [Column("descripcion_prototipo")]
        public string DescripcionPrototipo { get; set; }

        [Required(ErrorMessage = "Los recursos y tecnologías son obligatorios")]
        [MinLength(10, ErrorMessage = "Los recursos y tecnologías deben tener al menos 10 caracteres")]
        [MaxLength(1000, ErrorMessage = "Los recursos y tecnologías no pueden exceder 1000 caracteres")]
        [Column("recursos_tecnologias")]
        public string RecursosTecnologias { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Propiedad calculada en SQL para identificar el tipo de proyecto.
        /// No se guarda en la base de datos.
        /// </summary>
        [NotMapped]
        public string TipoProyecto { get; set; }

        // 👇 AGREGAR PROPIEDADES DE SOCIOCOMUNITARIO
        [NotMapped] public string? Beneficiarios { get; set; }
        [NotMapped] public string? UbicacionGeografica { get; set; }
        [NotMapped] public string? ActoresInvolucrados { get; set; }
        [NotMapped] public string? ImpactoSocialEsperado { get; set; }
        [NotMapped] public string? EstrategiasSostenibilidad { get; set; }
        
        // 👇 AGREGAR PROPIEDADES DE EMPRENDIMIENTO
        [NotMapped] public string? SegmentoMercado { get; set; }
        [NotMapped] public string? PropuestaValor { get; set; }
        [NotMapped] public string? AnalisisCompetencia { get; set; }
        [NotMapped] public string? ModeloIngresos { get; set; }
        [NotMapped] public decimal? EstimacionInversionInicial { get; set; }
        [NotMapped] public string? ViabilidadEconomica { get; set; }
        [NotMapped] public string? EstrategiaComercializacion { get; set; }
        

        // Relaciones
        public virtual ICollection<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();
        public virtual ICollection<Evaluacion> Evaluaciones { get; set; } = new List<Evaluacion>();
    }
}
