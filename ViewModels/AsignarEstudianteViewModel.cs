using System.ComponentModel.DataAnnotations;

namespace AuthenticationModule.ViewModels
{
    public class AsignarEstudianteViewModel
    {
        public int ProyectoId { get; set; }

        public string ProyectoTitulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un estudiante.")]
        [Display(Name = "Estudiante")]
        public int? EstudianteId { get; set; }
    }
}