using AuthenticationModule.DTOs;
using AuthenticationModule.Services.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthenticationModule.Pages.Proyectos
{
    [Authorize(Roles = "Admin,User")]
    public class DetailsModel : PageModel
    {
        private readonly IProyectoService _proyectoService;

        public DetailsModel(IProyectoService proyectoService)
        {
            _proyectoService = proyectoService;
        }

        public ProyectoDetailsDTO Proyecto { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                Proyecto = await _proyectoService.GetDetailsAsync(id.Value);
                return Page();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}