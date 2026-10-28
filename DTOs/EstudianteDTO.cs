namespace AuthenticationModule.DTOs
{
    /// <summary>
    /// DTO con los datos básicos de un estudiante para mostrar en vistas y reportes.
    /// </summary>
    public class EstudianteDTO
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Curso { get; set; }
    }
}