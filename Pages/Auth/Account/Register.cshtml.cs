using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AuthenticationModule.Services.Auth;
using AuthenticationModule.Decorators;

namespace AuthenticationModule.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILoggingService _loggingService;

        public RegisterModel(IAuthService authService, ILoggingService loggingService)
        {
            _authService = authService;
            _loggingService = loggingService;
        }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string ErrorMessage { get; set; }
        public List<string> Errors { get; set; } = new();

        // GET: Mostrar la página de registro
        public IActionResult OnGet()
        {
            // Si ya está autenticado, redirigir al inicio
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }

        // POST: Procesar el registro
        public async Task<IActionResult> OnPostAsync()
        {
            // Validar el modelo
            if (!ModelState.IsValid)
            {
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Page();
            }

            // Llamar al servicio de registro
            var result = await _authService.RegisterAsync(
                Username,
                Email,
                Password,
                ConfirmPassword);

            if (result.Success)
            {
                // Redirigir al login con mensaje de éxito
                TempData["SuccessMessage"] = "Cuenta creada exitosamente. Comunícate con el administrador para activar tu cuenta docente.";
                return RedirectToPage("/Auth/Account/Login");
            }

            // Mostrar errores
            ErrorMessage = result.Message;
            Errors = result.Errors;
            return Page();
        }
    }
}
