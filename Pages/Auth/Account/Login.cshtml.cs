using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AuthenticationModule.Decorators;
using AuthenticationModule.Services.Auth;

namespace AuthenticationModule.Pages.Account
{
    // PageModel - Controlador de la página
    public class LoginModel : PageModel
    {
        // Dependencias
        private readonly IAuthService _authService;
        private readonly ILoggingService _loggingService;

        // Constructor - Inyección de dependencias
        public LoginModel(IAuthService authService, ILoggingService loggingService)
        {
            _authService = authService;
            _loggingService = loggingService;
        }

        // Propiedades vinculadas al formulario
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        // Propiedad para mensajes de error
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        // GET: Muestra la página de login
        public IActionResult OnGet()
        {
            // Si ya está autenticado, redirigir al inicio
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Index");
            }
            // 👇 Mostrar mensaje de éxito tras un registro
            if (TempData["SuccessMessage"] != null)
            {
                SuccessMessage = TempData["SuccessMessage"].ToString();
            }

            return Page();
        }

        // POST: Procesa el login
        public async Task<IActionResult> OnPostAsync()
        {
            // Validar que los campos no estén vacíos
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Usuario y contraseña son obligatorios";
                return Page();
            }

            // Llamar al servicio de autenticación
            var result = await _authService.LoginAsync(Username, Password);

            if (result.Success)
            {
                // Log exitoso
                await _loggingService.LogAsync(
                    "Login",
                    Username,
                    $"Login exitoso desde IP: {HttpContext.Connection.RemoteIpAddress}",
                    LogLevel.Information);

                // Redirigir al inicio
                return RedirectToPage("/Index");
            }
            else
            {
                // Log fallido
                await _loggingService.LogAsync(
                    "LoginFailed",
                    Username,
                    $"Intento fallido desde IP: {HttpContext.Connection.RemoteIpAddress}",
                    LogLevel.Warning);

                ErrorMessage = result.Message;
                return Page();
            }
        }
    }
}
