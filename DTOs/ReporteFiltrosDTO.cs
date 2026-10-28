namespace AuthenticationModule.DTOs
{
    /// <summary>
    /// Filtros aplicables a la generación de reportes.
    /// </summary>
    public class ReporteFiltrosDTO
    {
        public int? TipoProyecto { get; set; } // 1: Socio, 2: Emprende
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
    }
}