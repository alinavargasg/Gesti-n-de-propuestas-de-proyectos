using AuthenticationModule.DTOs;
using AuthenticationModule.Services;
using AuthenticationModule.Services.Projects;
using AuthenticationModule.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AuthenticationModule.Pages.Proyectos
{
    [Authorize(Roles = "Admin,User")]
    public class AsignarEstudiantesModel : PageModel
    {
        private readonly IProyectoService _proyectoService;
        private readonly IEstudianteRepository _estudianteRepository;

        public AsignarEstudiantesModel(
            IProyectoService proyectoService,
            IEstudianteRepository estudianteRepository)
        {
            _proyectoService = proyectoService;
            _estudianteRepository = estudianteRepository;
        }

        public ProyectoDetailsDTO Proyecto { get; set; } = default!;
        public SelectList EstudiantesDisponibles { get; set; } = default!;

        [BindProperty]
        public AsignarEstudianteViewModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                await CargarDatosAsync(id);
                return Page();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                await CargarDatosAsync(id);
                return Page();
            }

            try
            {
                // Intentar asignar el estudiante
                await _proyectoService.AsignarEstudianteAsync(id, Input.EstudianteId!.Value);

                TempData["SuccessMessage"] = "Estudiante asignado correctamente al proyecto.";
                return RedirectToPage("./AsignarEstudiantes", new { id = id });
            }
            catch (Exception ex)
            {
                // Capturamos la excepción de negocio (ej. "El proyecto ya tiene el máximo de estudiantes")
                ModelState.AddModelError(string.Empty, ex.Message);
                await CargarDatosAsync(id);
                return Page();
            }
        }

        private async Task CargarDatosAsync(int proyectoId)
        {
            // 1. Obtener los datos del proyecto
            Proyecto = await _proyectoService.GetDetailsAsync(proyectoId);

            // 2. Obtener estudiantes que NO tienen proyecto asignado
            var estudiantesLibres = await _estudianteRepository.GetSinProyectoAsync();

            // 3. Llenar el SelectList
            EstudiantesDisponibles = new SelectList(
                estudiantesLibres.Select(e => new {
                    Id = e.Id,
                    Nombre = $"{e.Nombre} {e.ApellidoPaterno} - Curso: {e.Curso}"
                }),
                "Id",
                "Nombre"
            );

            Input.ProyectoId = Proyecto.Id;
            Input.ProyectoTitulo = Proyecto.Titulo;
        }
    }
}