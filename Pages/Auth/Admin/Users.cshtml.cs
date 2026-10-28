using AuthenticationModule.Data;
using AuthenticationModule.Models.Auth;
using AuthenticationModule.Models.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationModule.Pages.Admin
{
    // 👇 Solo los administradores pueden ver esta página
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly AuthDbContext _context;

        public UsersModel(AuthDbContext context)
        {
            _context = context;
        }

        public IList<User> Usuarios { get; set; } = default!;

        // Lista para el <select> de docentes
        public SelectList DocentesDisponibles { get; set; } = default!;

        public async Task OnGetAsync()
        {
            // 1. Cargar usuarios con su Rol y su Docente (si lo tienen)
            Usuarios = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Docente)
                .OrderBy(u => u.Username)
                .ToListAsync();

            // 2. Cargar solo los docentes que NO tienen un usuario asignado aún
            var docentesLibres = await _context.Docentes
                .Where(d => !_context.Users.Any(u => u.DocenteId == d.Id))
                .OrderBy(d => d.ApellidoPaterno)
                .ToListAsync();

            DocentesDisponibles = new SelectList(docentesLibres, "Id", "ApellidoPaterno");
        }

        // Manejador para vincular al usuario con el docente
        public async Task<IActionResult> OnPostVincularAsync(int userId, int docenteId)
        {
            if (docenteId == 0)
            {
                TempData["Error"] = "Debe seleccionar un docente válido.";
                return RedirectToPage();
            }

            var usuario = await _context.Users.FindAsync(userId);
            if (usuario == null)
            {
                return NotFound();
            }

            // Validar que el docente no haya sido asignado a otro usuario mientras la página estaba abierta
            var yaAsignado = await _context.Users.AnyAsync(u => u.DocenteId == docenteId);
            if (yaAsignado)
            {
                TempData["Error"] = "Este docente ya ha sido vinculado a otro usuario.";
                return RedirectToPage();
            }

            // Hacer el enlace
            usuario.DocenteId = docenteId;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Usuario '{usuario.Username}' vinculado exitosamente.";
            return RedirectToPage();
        }
    }
}
