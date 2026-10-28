using AuthenticationModule.Services.Auth;
using System.Diagnostics;

namespace AuthenticationModule.Decorators
{
    // Interfaz del servicio de logging
    public interface ILoggingService
    {
        Task LogAsync(string action, string user, string details, LogLevel level);
    }

    // Implementación base - Sin logging
    public class BaseLoggingService : ILoggingService
    {
        public async Task LogAsync(string action, string user, string details, LogLevel level)
        {
            // Implementación base (vacía)
            await Task.CompletedTask;
        }
    }

    // Decorator - Añade funcionalidad de logging
    public class LoggingDecorator : ILoggingService
    {
        // El decorator envuelve al objeto original
        private readonly ILogger<LoggingDecorator> _logger;
        private readonly IUserRepository _userRepository;

        public LoggingDecorator(ILogger<LoggingDecorator> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        public async Task LogAsync(string action, string user, string details, LogLevel level)
        {
            // 1. Crear el objeto de log
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Action = action,
                User = user,
                Details = details,
                Level = level.ToString(),
                Source = "AuthenticationModule",
                MachineName = Environment.MachineName,
                ProcessId = Process.GetCurrentProcess().Id
            };

            // 2. Log según el nivel
            switch (level)
            {
                case LogLevel.Information:
                    _logger.LogInformation("{Action} by {User}: {Details}",
                        action, user, details);
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning("{Action} by {User}: {Details}",
                        action, user, details);
                    break;
                case LogLevel.Error:
                    _logger.LogError("{Action} by {User}: {Details}",
                        action, user, details);
                    break;
                case LogLevel.Critical:
                    _logger.LogCritical("{Action} by {User}: {Details}",
                        action, user, details);
                    break;
                default:
                    _logger.LogDebug("{Action} by {User}: {Details}",
                        action, user, details);
                    break;
            }

            // 3. Guardar en base de datos (opcional)
            await LogToDatabaseAsync(action, user, details, level);
        }

        // Método privado para guardar logs en la base de datos
        private async Task LogToDatabaseAsync(string action, string user, string details, LogLevel level)
        {
            const string sql = @"
                INSERT INTO logs (timestamp, action, user, details, level, source, machine_name, process_id)
                VALUES (@Timestamp, @Action, @User, @Details, @Level, @Source, @MachineName, @ProcessId)";

            try
            {
                // Implementar con Dapper o EF Core
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando log en base de datos");
            }
        }
    }
}
