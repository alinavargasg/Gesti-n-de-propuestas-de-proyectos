using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationModule.Models.Projects
{
    /// <summary>
    /// Proyecto de emprendimiento productivo.
    /// </summary>
    [Table("proyectos_emprendimientos")]
    public class ProyectoEmprendimiento : Proyecto
    {
        //[Required(ErrorMessage = "El segmento de mercado es obligatorio")]
        [MinLength(10, ErrorMessage = "Debe tener al menos 10 caracteres")]
        [MaxLength(500, ErrorMessage = "No puede exceder 500 caracteres")]
        [Column("segmento_mercado")]
        public string? SegmentoMercado { get; set; }

        //[Required(ErrorMessage = "La propuesta de valor es obligatoria")]
        [MinLength(20, ErrorMessage = "Debe tener al menos 20 caracteres")]
        [MaxLength(1000, ErrorMessage = "No puede exceder 1000 caracteres")]
        [Column("propuesta_valor")]
        public string? PropuestaValor { get; set; }

        //[Required(ErrorMessage = "El análisis de competencia es obligatorio")]
        [MinLength(20, ErrorMessage = "Debe tener al menos 20 caracteres")]
        [MaxLength(1000, ErrorMessage = "No puede exceder 1000 caracteres")]
        [Column("analisis_competencia")]
        public string? AnalisisCompetencia { get; set; }

        //[Required(ErrorMessage = "El modelo de ingresos es obligatorio")]
        [MinLength(10, ErrorMessage = "Debe tener al menos 10 caracteres")]
        [MaxLength(500, ErrorMessage = "No puede exceder 500 caracteres")]
        [Column("modelo_ingresos")]
        public string? ModeloIngresos { get; set; }

        //[Required(ErrorMessage = "La estimación de inversión es obligatoria")]
        [Range(0, 1000000, ErrorMessage = "El valor debe estar entre 0 y 1,000,000")]
        [Column("estimacion_inversion_inicial")]
        public decimal? EstimacionInversionInicial { get; set; }

        //[Required(ErrorMessage = "La viabilidad económica es obligatoria")]
        [MinLength(20, ErrorMessage = "Debe tener al menos 20 caracteres")]
        [MaxLength(1000, ErrorMessage = "No puede exceder 1000 caracteres")]
        [Column("viabilidad_economica")]
        public string? ViabilidadEconomica { get; set; }

        //[Required(ErrorMessage = "La estrategia de comercialización es obligatoria")]
        [MinLength(20, ErrorMessage = "Debe tener al menos 20 caracteres")]
        [MaxLength(1000, ErrorMessage = "No puede exceder 1000 caracteres")]
        [Column("estrategia_comercializacion")]
        public string? EstrategiaComercializacion { get; set; }
    }
}
