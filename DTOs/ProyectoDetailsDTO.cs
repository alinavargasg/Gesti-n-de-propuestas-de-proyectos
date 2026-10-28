namespace AuthenticationModule.DTOs
{
    /// <summary>
    /// DTO con los detalles completos de un proyecto.
    /// </summary>
    public class ProyectoDetailsDTO
    {
        // Datos comunes
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Problematica { get; set; }
        public string ObjetivoGeneral { get; set; }
        public string DescripcionPrototipo { get; set; }
        public string RecursosTecnologias { get; set; }
        public string TipoProyecto { get; set; } // "Sociocomunitario" o "Emprendimiento"
        public DateTime CreatedAt { get; set; }

        // Datos Sociocomunitario
        public string Beneficiarios { get; set; }
        public string UbicacionGeografica { get; set; }
        public string ActoresInvolucrados { get; set; }
        public string ImpactoSocialEsperado { get; set; }
        public string EstrategiasSostenibilidad { get; set; }

        // Datos Emprendimiento
        public string SegmentoMercado { get; set; }
        public string PropuestaValor { get; set; }
        public string AnalisisCompetencia { get; set; }
        public string ModeloIngresos { get; set; }
        public decimal? EstimacionInversionInicial { get; set; }
        public string ViabilidadEconomica { get; set; }
        public string EstrategiaComercializacion { get; set; }

        // Relaciones
        public List<EstudianteDTO> Estudiantes { get; set; } = new();
        public List<EvaluacionDTO> Evaluaciones { get; set; } = new();
    }
}