namespace AuthenticationModule.ViewModels
{
    public class ProyectoListItemViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string TipoProyecto { get; set; } // "Sociocomunitario" o "Emprendimiento"
        public DateTime CreatedAt { get; set; }
    }
}