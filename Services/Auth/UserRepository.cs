using AuthenticationModule.Models.Auth;
using AuthenticationModule.Security;
using Dapper;
using Npgsql;

namespace AuthenticationModule.Services.Auth
{
    // Interfaz - Define el contrato (abstracción)
    public interface IUserRepository
    {
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> CreateUserAsync(User user, string password);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> UpdateLoginAttemptsAsync(int userId, bool success);
        Task<bool> RecordLoginAsync(int userId);
    }

    // Implementación concreta del repositorio
    public class UserRepository : IUserRepository
    {
        // Inyección de dependencias (concepto POO: composición)
        private readonly string _connectionString;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IConfiguration configuration, ILogger<UserRepository> logger)
        {
            // Obtenemos la cadena de conexión desde appsettings.json
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        // Método privado para obtener una conexión a PostgreSQL
        private NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        // Obtener usuario por nombre de usuario
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            // SQL con parámetros para prevenir inyección
            const string sql = @"
                SELECT u.*, r.* 
                FROM users u
                LEFT JOIN roles r ON u.role_id = r.id
                WHERE u.username = @Username AND u.is_active = true";

            using var connection = GetConnection();

            // Dapper mapea automáticamente los resultados a objetos
            var result = await connection.QueryAsync<User, Role, User>(
                sql,
                (user, role) =>
                {
                    user.Role = role;  // Asigna el rol al usuario
                    return user;
                },
                new { Username = username },  // ← Parámetros parametrizados
                splitOn: "id");

            return result.FirstOrDefault();
        }

        // Obtener usuario por email
        public async Task<User> GetUserByEmailAsync(string email)
        {
            const string sql = @"
                SELECT u.*, r.* 
                FROM users u
                LEFT JOIN roles r ON u.role_id = r.id
                WHERE u.email = @Email AND u.is_active = true";

            using var connection = GetConnection();
            var result = await connection.QueryAsync<User, Role, User>(
                sql,
                (user, role) =>
                {
                    user.Role = role;
                    return user;
                },
                new { Email = email },
                splitOn: "id");

            return result.FirstOrDefault();
        }

        // Crear nuevo usuario
        public async Task<bool> CreateUserAsync(User user, string password)
        {
            var (hash, salt) = PasswordHasher.HashPassword(password);

            // 👇 Agregamos todas las columnas obligatorias al INSERT
            const string sql = @"
        INSERT INTO users (
            username, 
            email, 
            password_hash, 
            password_salt, 
            role_id, 
            created_at, 
            is_active, 
            failed_login_attempts
        )
        VALUES (
            @Username, 
            @Email, 
            @PasswordHash, 
            @PasswordSalt, 
            @RoleId, 
            @CreatedAt, 
            @IsActive, 
            @FailedLoginAttempts
        )";

            try
            {
                using var connection = GetConnection();

                // 👇 Asignamos valores por defecto si vienen nulos
                var result = await connection.ExecuteAsync(sql, new
                {
                    user.Username,
                    user.Email,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    user.RoleId,
                    CreatedAt = user.CreatedAt == default ? DateTime.UtcNow : user.CreatedAt,
                    IsActive = true,         // Obligatorio
                    FailedLoginAttempts = 0   // Obligatorio
                });

                return result > 0;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error creating user: {Username}", user.Username);
                return false;
            }
        }

        // Actualizar datos de un usuario existente
        public async Task<bool> UpdateUserAsync(User user)
        {
            const string sql = @"
        UPDATE users 
        SET email = @Email,
            role_id = @RoleId,
            is_active = @IsActive,
            last_login = @LastLogin,
            failed_login_attempts = @FailedLoginAttempts,
            lockout_until = @LockoutUntil
        WHERE id = @Id";

            try
            {
                using var connection = GetConnection();
                var result = await connection.ExecuteAsync(sql, new
                {
                    user.Id,
                    user.Email,
                    user.RoleId,
                    user.IsActive,
                    user.LastLogin,
                    user.FailedLoginAttempts,
                    user.LockoutUntil
                });

                return result > 0;  // Retorna true si se actualizó al menos un registro
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error updating user: {Username}", user.Username);
                return false;
            }
        }

        // Actualizar intentos de login
        public async Task<bool> UpdateLoginAttemptsAsync(int userId, bool success)
        {
            string sql;
            if (success)
            {
                // Si el login es exitoso, reiniciamos los intentos
                sql = @"
                    UPDATE users 
                    SET failed_login_attempts = 0,
                        lockout_until = NULL,
                        last_login = @LastLogin
                    WHERE id = @UserId";
            }
            else
            {
                // Si falla, incrementamos el contador
                sql = @"
                    UPDATE users 
                    SET failed_login_attempts = failed_login_attempts + 1,
                        lockout_until = CASE 
                            WHEN failed_login_attempts + 1 >= 5 THEN @LockoutTime  -- Bloquear después de 5 intentos
                            ELSE lockout_until 
                        END
                    WHERE id = @UserId";
            }

            using var connection = GetConnection();
            var result = await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                LastLogin = DateTime.UtcNow,
                LockoutTime = DateTime.UtcNow.AddMinutes(15)  // Bloqueo de 15 minutos
            });

            return result > 0;
        }

        // Registrar login exitoso
        public async Task<bool> RecordLoginAsync(int userId)
        {
            const string sql = @"
                UPDATE users 
                SET last_login = @LastLogin,
                    failed_login_attempts = 0
                WHERE id = @UserId";

            using var connection = GetConnection();
            var result = await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                LastLogin = DateTime.UtcNow
            });

            return result > 0;
        }
    }
}