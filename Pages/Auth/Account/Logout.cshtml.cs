using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AuthenticationModule.Services.Auth;

namespace AuthenticationModule.Pages.Account
{
    /// <summary>
    /// PageModel para el cierre de sesión.
    /// 
    /// Proporciona dos manejadores:
    /// - OnGetAsync:  invocado desde un enlace o URL directa (vulnerable a CSRF).
    /// - OnPostAsync: invocado desde un formulario con token anti-CSRF (recomendado).
    /// 
    /// En ambos casos, el servicio de autenticación elimina la cookie
    /// y este PageModel redirige al usuario a la página de Login.
    /// </summary>
    public class LogoutModel : PageModel
    {
        // ============================================================
        // DEPENDENCIAS (Inyección de Dependencias)
        // ============================================================
        private readonly IAuthService _authService;

        public LogoutModel(IAuthService authService)
        {
            _authService = authService;
        }

        // ============================================================
        // MANEJADOR GET: OnGetAsync
        // ============================================================
        /// <summary>
        /// Cierra la sesión cuando se accede vía GET.
        /// 
        /// Advertencia de seguridad:
        /// Este manejador es vulnerable a CSRF, porque un sitio externo
        /// podría forzar el cierre de sesión incluyendo una imagen o iframe
        /// que apunte a /Auth/Account/Logout.
        /// 
        /// Uso recomendado: solo para casos excepcionales, como enlaces
        /// de confirmación enviados por correo electrónico.
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // 1. Delegar el cierre de sesión al servicio
            //    (elimina la cookie + registra el evento en logs)
            await _authService.LogoutAsync();

            // 2. Guardar mensaje de confirmación para mostrarlo en Login
            TempData["LogoutMessage"] = "Has cerrado sesión correctamente";

            // 3. Redirigir a la página de Login
            return RedirectToPage("/Auth/Account/Login");
        }

        // ============================================================
        // MANEJADOR POST: OnPostAsync
        // ============================================================
        /// <summary>
        /// Cierra la sesión cuando se envía un formulario vía POST.
        /// 
        /// Recomendado para el botón de "Cerrar Sesión":
        /// Razor Pages valida automáticamente el token anti-CSRF
        /// generado por @Html.AntiForgeryToken() en el formulario.
        /// Si el token no es válido, el servidor responde 400 Bad Request
        /// y este manejador nunca se ejecuta.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Delegar el cierre de sesión al servicio
            await _authService.LogoutAsync();

            // 2. Guardar mensaje de confirmación para mostrarlo en Login
            TempData["LogoutMessage"] = "Has cerrado sesión correctamente";

            // 3. Redirigir a la página de Login
            return RedirectToPage("/Auth/Account/Login");
        }
    }
}
