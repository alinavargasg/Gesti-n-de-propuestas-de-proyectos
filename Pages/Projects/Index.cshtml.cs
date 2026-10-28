using AuthenticationModule.Models.Projects;
using AuthenticationModule.Services.Projects;
using AuthenticationModule.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;          // ✅ necesario para IActionResult y File()
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AuthenticationModule.Pages.Proyectos
{
    [Authorize(Roles = "Admin,User")]
    public class IndexModel : PageModel
    {
        private readonly IProyectoService _proyectoService;

        public IndexModel(IProyectoService proyectoService)
        {
            _proyectoService = proyectoService;
        }

        // ✅ Inicializar con lista vacía en lugar de default!
        public IList<ProyectoListItemViewModel> Proyectos { get; set; }
            = new List<ProyectoListItemViewModel>();

        public async Task OnGetAsync()
        {
            var proyectosDb = await _proyectoService.GetAllAsync();

            Proyectos = proyectosDb.Select(p => new ProyectoListItemViewModel
            {
                Id = p.Id,
                Titulo = p.Titulo,
                CreatedAt = p.CreatedAt,
                TipoProyecto = p.TipoProyecto
            }).ToList();
        }

        // ✅ Devuelve IActionResult y llama a GeneratePdf()

        public async Task<IActionResult> OnGetDescargarPdfAsync()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var datos = await _proyectoService.GetAllAsync() ?? new List<Proyecto>();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("LISTADO DE PROYECTOS").Bold().FontSize(18).AlignCenter();
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4); // Título
                            columns.RelativeColumn(1); // Tipo
                            columns.RelativeColumn(1); // Fecha
                        });

                        // ✅ Estilo del encabezado
                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("TÍTULO");
                            header.Cell().Element(HeaderStyle).Text("TIPO");
                            header.Cell().Element(HeaderStyle).Text("FECHA");

                            static IContainer HeaderStyle(IContainer container)
                            {
                                return container
                                    .Background(Colors.Grey.Lighten2)  // Fondo gris claro
                                    .BorderBottom(1)                   // Borde inferior
                                    .BorderColor(Colors.Grey.Darken2)
                                    .Padding(8)                        // Relleno
                                    .DefaultTextStyle(x => x.Bold());  // Texto en negrita
                            }
                        });

                        // ✅ Estilo de las celdas de datos (con filas alternas)
                        for (int i = 0; i < datos.Count; i++)
                        {
                            var item = datos[i];
                            var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                            // Abreviar el tipo de proyecto
                            var tipoAbreviado = (item.TipoProyecto ?? "").ToLowerInvariant() switch
                            {
                                var t when t.Contains("sociocomunitario") => "PSCP",
                                var t when t.Contains("emprendimiento") => "PEP",
                                _ => (item.TipoProyecto ?? "").ToUpperInvariant()
                            };

                            table.Cell().Element(c => CellStyle(c, bg)).Text((item.Titulo ?? "").ToUpperInvariant());
                            table.Cell().Element(c => CellStyle(c, bg)).Text(tipoAbreviado);
                            table.Cell().Element(c => CellStyle(c, bg)).Text(item.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy"));
                        }

                        // ✅ Función de estilo reutilizable para las celdas
                        static IContainer CellStyle(IContainer container, Color bg)
                        {
                            return container
                                .Background(bg)
                                .BorderBottom(1)              // Borde inferior fino
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(8)                   // Relleno interno
                                .AlignMiddle();               // Centrado vertical (opcional)
                        }
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", "proyectos.pdf");
        }
    }
}