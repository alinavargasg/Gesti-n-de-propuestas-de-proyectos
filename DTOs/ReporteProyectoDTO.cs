namespace AuthenticationModule.DTOs
{
    /// <summary>
    /// DTO diseñado específicamente para la generación de reportes (PDF, Excel, CSV).
    /// Aplana los datos para facilitar la tabulación.
    /// </summary>
    public class ReporteProyectoDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string TipoProyecto { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string ObjetivoGeneral { get; set; }

        // Datos específicos combinados
        public string PublicoObjetivo { get; set; } // Beneficiarios o Segmento Mercado
        public string UbicacionOModelo { get; set; } // Ubicación o Modelo de Ingresos

        public int CantidadEstudiantes { get; set; }
        public string EstudiantesAsignados { get; set; } // Nombres separados por coma
        public decimal? PromedioNotas { get; set; }
    }
}