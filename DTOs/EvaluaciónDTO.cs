namespace AuthenticationModule.DTOs
{
    /// <summary>
    /// DTO con los datos de una evaluación para mostrar en vistas y reportes.
    /// </summary>
    public class EvaluacionDTO
    {
        public int Id { get; set; }
        public string DocenteNombre { get; set; }
        public string Observaciones { get; set; }
        public string Recomendaciones { get; set; }
        public decimal? Nota { get; set; }
        public DateTime FechaEvaluacion { get; set; }
    }
}