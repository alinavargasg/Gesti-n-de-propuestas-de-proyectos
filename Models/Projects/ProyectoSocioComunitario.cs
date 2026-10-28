using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationModule.Models.Projects
{
    /// <summary>
    /// Proyecto sociocomunitario productivo.
    /// </summary>
    [Table("proyectos_sociocomunitarios")]
    public class ProyectoSociocomunitario : Proyecto
    {
        //[Required(ErrorMessage = "Los beneficiarios son obligatorios")]
        [MinLength(10, ErrorMessage = "Debe tener al menos 10 caracteres")]
        [MaxLength(500, ErrorMessage = "No puede exceder 500 caracteres")]
        [Column("beneficiarios")]
        public string? Beneficiarios { get; set; }

        //[Required(ErrorMessage = "La ubicación geográfica es obligatoria")]
        [MinLength(5, ErrorMessage = "Debe tener al menos 5 caracteres")]
        [MaxLength(200, ErrorMessage = "No puede exceder 200 caracteres")]
        [Column("ubicacion_geografica")]
        public string? UbicacionGeografica { get; set; }

        //[Required(ErrorMessage = "Los actores involucrados son obligatorios")]
        [MinLength(10, ErrorMessage = "Debe tener al menos 10 caracteres")]
        [MaxLength(1000, ErrorMessage = "No puede exceder 1000 caracteres")]
        [Column("actores_involucrados")]
        public string? ActoresInvolucrados { get; set; }

        //[Required(ErrorMessage = "El impacto social esperado es obligatorio")]
        [MinLength(20, ErrorMessage = "Debe tener al menos 20 caracteres")]
        [MaxLength(1000, ErrorMessage = "No puede exceder 1000 caracteres")]
        [Column("impacto_social_esperado")]
        public string? ImpactoSocialEsperado { get; set; }

        //[Required(ErrorMessage = "Las estrategias de sostenibilidad son obligatorias")]
        [MinLength(20, ErrorMessage = "Debe tener al menos 20 caracteres")]
        [MaxLength(1000, ErrorMessage = "No puede exceder 1000 caracteres")]
        [Column("estrategias_sostenibilidad")]
        public string? EstrategiasSostenibilidad { get; set; }
    }
}
