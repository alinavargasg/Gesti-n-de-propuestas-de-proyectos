using AuthenticationModule.Decorators;
using AuthenticationModule.Models.Auth;
using AuthenticationModule.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace AuthenticationModule.Services.Auth
{
    // Interfaz del servicio - Define el contrato
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(string username, string password);
        Task LogoutAsync();
        Task<User> GetCurrentUserAsync(ClaimsPrincipal principal);
        bool ValidatePassword(string password);
        // Método para registrar nuevos usuarios
        Task<RegisterResult> RegisterAsync(
        string username,
        string email,
        string password,
        string confirmPassword);

    }

    // 👇 AGREGAR: Clase de resultado del registro
    public class RegisterResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public User User { get; set; }
        public List<string> Errors { get; set; } = new();
    }



    // Implementación del servicio
    public class AuthService : IAuthService
    {
        // Dependencias - Inyección de dependencias
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;
        private readonly ILoggingService _loggingService;

        public AuthService(
            IUserRepository userRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthService> logger,
            ILoggingService loggingService)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _loggingService = loggingService;
        }

        /// <summary>
        /// Proceso de login
        /// </summary>
        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                ?? "desconocida";
            try
            {
                // 1. Buscar usuario en la base de datos
                var user = await _userRepository.GetUserByUsernameAsync(username);

                // 2. Verificar si existe
                if (user == null)
                {
                    // Simular demora para prevenir timing attacks
                    await Task.Delay(1000);
                    await _loggingService.LogAsync(
                        "LoginFailed",
                        username,
                        $"Usuario no encontrado. IP: {ipAddress}",
                        LogLevel.Warning);
                    return new LoginResult { Success = false, Message = "Credenciales inválidas " };
                }

                // 3. Verificar si la cuenta está bloqueada
                if (user.LockoutUntil.HasValue && user.LockoutUntil > DateTime.UtcNow)
                {
                    await _loggingService.LogAsync(
                        "LoginBlocked",
                        username,
                        $"Cuenta bloqueada hasta {user.LockoutUntil.Value.ToLocalTime():HH:mm}. IP: {ipAddress}",
                        LogLevel.Warning);
                    return new LoginResult
                    {
                        Success = false,
                        Message = $"Cuenta bloqueada hasta {user.LockoutUntil.Value.ToLocalTime():HH:mm}"
                    };
                }

                // 4. Verificar la contraseña
                if (!PasswordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                {
                    // Incrementar intentos fallidos
                    await _userRepository.UpdateLoginAttemptsAsync(user.Id, false);
                    await _loggingService.LogAsync(
                        "LoginFailed",
                        username,
                        $"Contraseña incorrecta. IP: {ipAddress}",
                        LogLevel.Warning);
                    return new LoginResult { Success = false, Message = "Credenciales inválidas" };
                }

                // =========================================================
                // 👇 NUEVA VALIDACIÓN: ¿EL USUARIO ESTÁ VINCULADO A UN DOCENTE?
                // =========================================================
                if (user.Role.Name != "Admin" && user.DocenteId == null)
                {
                    await _loggingService.LogAsync(
                        "LoginBlocked",
                        username,
                        $"Intento de login denegado: Usuario sin docente asignado. IP: {ipAddress}",
                        LogLevel.Warning);

                    return new LoginResult
                    {
                        Success = false,
                        Message = "Su cuenta está registrada, pero aún no ha sido habilitada. Por favor, comuníquese con el administrador para solicitar la activación de su cuenta docente."
                    };
                }

                // 5. Login exitoso - Actualizar registros
                await _userRepository.UpdateLoginAttemptsAsync(user.Id, true);
                await _userRepository.RecordLoginAsync(user.Id);

                // 6. Crear los claims (datos de la sesión)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.Name),
                    new Claim("UserId", user.Id.ToString())
                };

                // 7. Crear la identidad y el principal
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // 8. Iniciar sesión (crear cookie)
                await _httpContextAccessor.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(7),
                        IssuedUtc = DateTime.UtcNow
                    });

                await _loggingService.LogAsync(
                    "Login",
                    username,
                    $"Login exitoso. Rol: {user.Role.Name}. IP: {ipAddress}",
                    LogLevel.Information);
                // 9. Registrar en logs
                _logger.LogInformation("Usuario logueado: {Username}", username);

                return new LoginResult { Success = true, User = user };
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync(
                    "LoginError",
                    username,
                    $"Error del sistema: {ex.Message}. IP: {ipAddress}",
                    LogLevel.Error);

                _logger.LogError(ex, "Error de login para: {Username}", username);
                return new LoginResult { Success = false, Message = "Error en el sistema" };
            }
        }

        /// <summary>
        /// Cerrar sesión
        /// </summary>
        public async Task LogoutAsync()
        {
            // Obtener el nombre del usuario antes de cerrar sesión
            var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "desconocido";
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                            ?? "desconocida";

            await _httpContextAccessor.HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            await _loggingService.LogAsync(
                "Logout",
                username,
                $"Sesión cerrada. IP: {ipAddress}",
                LogLevel.Information);
        }

        /// <summary>
        /// Obtener el usuario actual
        /// </summary>
        public async Task<User> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            if (principal?.Identity?.IsAuthenticated != true)
                return null;

            var userIdClaim = principal.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return null;

            return await _userRepository.GetUserByUsernameAsync(principal.Identity.Name);
        }

        /// <summary>
        /// Validar política de contraseñas
        /// </summary>
        public bool ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            // Política de contraseñas:
            // - Al menos 8 caracteres
            // - Al menos una mayúscula
            // - Al menos una minúscula
            // - Al menos un número
            // - Al menos un carácter especial
            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        public async Task<RegisterResult> RegisterAsync(
            string username,
            string email,
            string password,
            string confirmPassword)
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                            ?? "desconocida";

            var result = new RegisterResult();

            try
            {
                // === 1. VALIDACIONES DE ENTRADA ===

                // Validar campos vacíos
                if (string.IsNullOrWhiteSpace(username))
                    result.Errors.Add("El nombre de usuario es obligatorio");

                if (string.IsNullOrWhiteSpace(email))
                    result.Errors.Add("El email es obligatorio");

                if (string.IsNullOrWhiteSpace(password))
                    result.Errors.Add("La contraseña es obligatoria");

                // Validar formato de email
                if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
                    result.Errors.Add("El formato del email no es válido");

                // Validar longitud del username
                if (!string.IsNullOrWhiteSpace(username) && username.Length < 3)
                    result.Errors.Add("El nombre de usuario debe tener al menos 3 caracteres");

                // Validar política de contraseña
                if (!ValidatePassword(password))
                    result.Errors.Add("La contraseña debe tener al menos 8 caracteres, " +
                                      "una mayúscula, una minúscula, un número y un carácter especial");

                // Validar que las contraseñas coincidan
                if (password != confirmPassword)
                    result.Errors.Add("Las contraseñas no coinciden");

                // Si hay errores de validación, retornar sin continuar
                if (result.Errors.Any())
                {
                    result.Success = false;
                    result.Message = "Corrige los errores antes de continuar";
                    return result;
                }

                // === 2. VERIFICAR DUPLICADOS ===

                // Verificar si el username ya existe
                var existingUserByUsername = await _userRepository.GetUserByUsernameAsync(username);
                if (existingUserByUsername != null)
                {
                    result.Errors.Add("El nombre de usuario ya está en uso");
                }

                // Verificar si el email ya existe
                var existingUserByEmail = await _userRepository.GetUserByEmailAsync(email);
                if (existingUserByEmail != null)
                {
                    result.Errors.Add("El email ya está registrado");
                }

                if (result.Errors.Any())
                {
                    result.Success = false;
                    result.Message = "El usuario ya existe";

                    await _loggingService.LogAsync(
                        "RegisterFailed",
                        username,
                        $"Intento de registro con datos duplicados. IP: {ipAddress}",
                        LogLevel.Warning);

                    return result;
                }

                // === 3. CREAR EL USUARIO ===

                var newUser = new User
                {
                    Username = username.Trim(),
                    Email = email.Trim().ToLowerInvariant(),
                    RoleId = 2,  // Rol "User" por defecto
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    FailedLoginAttempts = 0
                };

                // El repositorio se encarga de generar el hash y salt
                var created = await _userRepository.CreateUserAsync(newUser, password);

                if (!created)
                {
                    result.Success = false;
                    result.Message = "No se pudo crear el usuario. Intenta nuevamente";

                    await _loggingService.LogAsync(
                        "RegisterError",
                        username,
                        $"Error al crear usuario en BD. IP: {ipAddress}",
                        LogLevel.Error);

                    return result;
                }

                // === 4. REGISTRO EXITOSO ===

                result.Success = true;
                result.User = newUser;
                result.Message = "Usuario registrado exitosamente";

                await _loggingService.LogAsync(
                    "Register",
                    username,
                    $"Usuario registrado con rol User. IP: {ipAddress}",
                    LogLevel.Information);

                _logger.LogInformation("Usuario registrado: {Username}", username);

                return result;
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync(
                    "RegisterError",
                    username,
                    $"Error del sistema: {ex.Message}. IP: {ipAddress}",
                    LogLevel.Error);

                _logger.LogError(ex, "Error de registro para: {Username}", username);

                result.Success = false;
                result.Message = "Error en el sistema. Intenta nuevamente";
                return result;
            }
        }

        /// <summary>
        /// Validar formato de email
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}

// Clase para el resultado del login
public class LoginResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public User User { get; set; }
}
