using AuthenticationModule.Models.Projects;
using AuthenticationModule.Models.Projects.Enums;
using AuthenticationModule.Services.Projects;
using AuthenticationModule.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthenticationModule.Pages.Proyectos
{
    [Authorize(Roles = "Admin,User")]
    public class CreateModel : PageModel
    {
        private readonly IProyectoService _proyectoService;

        public CreateModel(IProyectoService proyectoService)
        {
            _proyectoService = proyectoService;
        }

        [BindProperty]
        public ProyectoCreateViewModel Input { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                Proyecto nuevoProyecto;

                // Instanciamos la clase correcta según el tipo seleccionado
                if (Input.TipoProyecto == 1)
                {
                    nuevoProyecto = new ProyectoSociocomunitario
                    {
                        Titulo = Input.Titulo,
                        Problematica = Input.Problematica,
                        ObjetivoGeneral = Input.ObjetivoGeneral,
                        DescripcionPrototipo = Input.DescripcionPrototipo,
                        RecursosTecnologias = Input.RecursosTecnologias,
                        Beneficiarios = Input.Beneficiarios!,
                        UbicacionGeografica = Input.UbicacionGeografica!,
                        ActoresInvolucrados = Input.ActoresInvolucrados!,
                        ImpactoSocialEsperado = Input.ImpactoSocialEsperado!,
                        EstrategiasSostenibilidad = Input.EstrategiasSostenibilidad!
                    };
                }
                else
                {
                    nuevoProyecto = new ProyectoEmprendimiento
                    {
                        Titulo = Input.Titulo,
                        Problematica = Input.Problematica,
                        ObjetivoGeneral = Input.ObjetivoGeneral,
                        DescripcionPrototipo = Input.DescripcionPrototipo,
                        RecursosTecnologias = Input.RecursosTecnologias,
                        SegmentoMercado = Input.SegmentoMercado!,
                        PropuestaValor = Input.PropuestaValor!,
                        AnalisisCompetencia = Input.AnalisisCompetencia!,
                        ModeloIngresos = Input.ModeloIngresos!,
                        EstimacionInversionInicial = Input.EstimacionInversionInicial ?? 0,
                        ViabilidadEconomica = Input.ViabilidadEconomica!,
                        EstrategiaComercializacion = Input.EstrategiaComercializacion!
                    };
                }

                await _proyectoService.CreateAsync(nuevoProyecto);
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }
    }
}