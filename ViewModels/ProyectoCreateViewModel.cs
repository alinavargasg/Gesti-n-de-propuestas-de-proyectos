using System.ComponentModel.DataAnnotations;

namespace AuthenticationModule.ViewModels
{
    public class ProyectoCreateViewModel
    {
        [Required(ErrorMessage = "El tipo de proyecto es obligatorio")]
        public int TipoProyecto { get; set; } // 1: Sociocomunitario, 2: Emprendimiento

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200, MinimumLength = 10)]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "La problemática es obligatoria")]
        [StringLength(2000, MinimumLength = 20)]
        public string Problematica { get; set; }

        [Required(ErrorMessage = "El objetivo general es obligatorio")]
        [StringLength(1000, MinimumLength = 20)]
        public string ObjetivoGeneral { get; set; }

        [Required(ErrorMessage = "La descripción del prototipo es obligatoria")]
        [StringLength(2000, MinimumLength = 20)]
        public string DescripcionPrototipo { get; set; }

        [Required(ErrorMessage = "Los recursos y tecnologías son obligatorios")]
        [StringLength(1000, MinimumLength = 10)]
        public string RecursosTecnologias { get; set; }

        // --- Campos Sociocomunitario ---
        [StringLength(500, MinimumLength = 10)]
        public string? Beneficiarios { get; set; }

        [StringLength(200, MinimumLength = 5)]
        public string? UbicacionGeografica { get; set; }

        [StringLength(1000, MinimumLength = 10)]
        public string? ActoresInvolucrados { get; set; }

        [StringLength(1000, MinimumLength = 20)]
        public string? ImpactoSocialEsperado { get; set; }

        [StringLength(1000, MinimumLength = 20)]
        public string? EstrategiasSostenibilidad { get; set; }

        // --- Campos Emprendimiento ---
        [StringLength(500, MinimumLength = 10)]
        public string? SegmentoMercado { get; set; }

        [StringLength(1000, MinimumLength = 20)]
        public string? PropuestaValor { get; set; }

        [StringLength(1000, MinimumLength = 20)]
        public string? AnalisisCompetencia { get; set; }

        [StringLength(500, MinimumLength = 10)]
        public string? ModeloIngresos { get; set; }

        [Range(0, 1000000)]
        public decimal? EstimacionInversionInicial { get; set; }

        [StringLength(1000, MinimumLength = 20)]
        public string? ViabilidadEconomica { get; set; }
        [StringLength(1000, MinimumLength = 20)]
        public string? EstrategiaComercializacion {  get; set; }
    }
}