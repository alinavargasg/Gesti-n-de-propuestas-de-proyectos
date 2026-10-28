using AuthenticationModule.Models.Projects;
using AuthenticationModule.Models.Projects.Enums;
using AuthenticationModule.Services.Exceptions;
using Dapper;
using Npgsql;

namespace AuthenticationModule.Services.Projects
{
    /// <summary>
    /// Implementación del repositorio de estudiantes utilizando Dapper.
    /// 
    /// Estrategia TPT (Table Per Type):
    /// - La tabla "estudiantes" tiene una FK a "proyectos".
    /// - El tipo de proyecto se determina por JOIN con las tablas derivadas.
    /// </summary>
    public class EstudianteRepository : IEstudianteRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<EstudianteRepository> _logger;

        public EstudianteRepository(
            IConfiguration configuration,
            ILogger<EstudianteRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        private NpgsqlConnection GetConnection() => new NpgsqlConnection(_connectionString);

        // ============================================================
        // CONSULTAS
        // ============================================================

        /// <inheritdoc/>
        public async Task<List<Estudiante>> GetAllAsync()
        {
            const string sql = @"
                SELECT e.*, p.titulo AS ProyectoTitulo
                FROM estudiantes e
                LEFT JOIN proyectos p ON e.proyecto_id = p.id
                ORDER BY e.apellido_paterno, e.apellido_materno, e.nombre";

            try
            {
                using var connection = GetConnection();
                var result = await connection.QueryAsync<Estudiante>(sql);
                return result.ToList();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al obtener todos los estudiantes");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Estudiante> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT e.*, p.titulo AS ProyectoTitulo
                FROM estudiantes e
                LEFT JOIN proyectos p ON e.proyecto_id = p.id
                WHERE e.id = @Id";

            try
            {
                using var connection = GetConnection();
                return await connection.QueryFirstOrDefaultAsync<Estudiante>(
                    sql, new { Id = id });
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al obtener estudiante con Id = {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<Estudiante>> GetByProyectoIdAsync(int proyectoId)
        {
            const string sql = @"
                SELECT * FROM estudiantes 
                WHERE proyecto_id = @ProyectoId
                ORDER BY apellido_paterno, apellido_materno, nombre";

            try
            {
                using var connection = GetConnection();
                var result = await connection.QueryAsync<Estudiante>(
                    sql, new { ProyectoId = proyectoId });
                return result.ToList();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex,
                    "Error al obtener estudiantes del proyecto {Id}", proyectoId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<Estudiante>> GetSinProyectoAsync()
        {
            const string sql = @"
                SELECT * FROM estudiantes 
                WHERE proyecto_id IS NULL
                ORDER BY apellido_paterno, apellido_materno, nombre";

            try
            {
                using var connection = GetConnection();
                var result = await connection.QueryAsync<Estudiante>(sql);
                return result.ToList();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al obtener estudiantes sin proyecto");
                throw;
            }
        }

        // ============================================================
        // CREACIÓN
        // ============================================================

        /// <inheritdoc/>
        public async Task<int> CreateAsync(Estudiante estudiante)
        {
            if (estudiante == null)
                throw new BusinessException("El estudiante no puede ser nulo");

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(estudiante.Nombre))
                throw new BusinessException("El nombre es obligatorio");

            if (string.IsNullOrWhiteSpace(estudiante.ApellidoPaterno))
                throw new BusinessException("El apellido paterno es obligatorio");

            if (string.IsNullOrWhiteSpace(estudiante.ApellidoMaterno))
                throw new BusinessException("El apellido materno es obligatorio");

            if (string.IsNullOrWhiteSpace(estudiante.Curso))
                throw new BusinessException("El curso es obligatorio");

            const string sql = @"
                INSERT INTO estudiantes 
                    (nombre, apellido_paterno, apellido_materno, curso, proyecto_id)
                VALUES 
                    (@Nombre, @ApellidoPaterno, @ApellidoMaterno, @Curso, @ProyectoId)
                RETURNING id";

            try
            {
                using var connection = GetConnection();
                var id = await connection.ExecuteScalarAsync<int>(sql, new
                {
                    estudiante.Nombre,
                    estudiante.ApellidoPaterno,
                    estudiante.ApellidoMaterno,
                    estudiante.Curso,
                    estudiante.ProyectoId
                });

                _logger.LogInformation(
                    "Estudiante creado con Id = {Id}, Nombre = {Nombre} {ApellidoPaterno}",
                    id, estudiante.Nombre, estudiante.ApellidoPaterno);

                return id;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al crear estudiante: {Nombre}",
                    estudiante.Nombre);
                throw;
            }
        }

        // ============================================================
        // ACTUALIZACIÓN
        // ============================================================

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(Estudiante estudiante)
        {
            if (estudiante == null)
                throw new BusinessException("El estudiante no puede ser nulo");

            if (estudiante.Id <= 0)
                throw new BusinessException("El Id del estudiante no es válido");

            const string sql = @"
                UPDATE estudiantes 
                SET nombre = @Nombre,
                    apellido_paterno = @ApellidoPaterno,
                    apellido_materno = @ApellidoMaterno,
                    curso = @Curso
                WHERE id = @Id";

            try
            {
                using var connection = GetConnection();
                var rows = await connection.ExecuteAsync(sql, new
                {
                    estudiante.Id,
                    estudiante.Nombre,
                    estudiante.ApellidoPaterno,
                    estudiante.ApellidoMaterno,
                    estudiante.Curso
                });

                if (rows == 0)
                    throw new NotFoundException("Estudiante", estudiante.Id);

                _logger.LogInformation(
                    "Estudiante actualizado con Id = {Id}", estudiante.Id);

                return true;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex,
                    "Error al actualizar estudiante con Id = {Id}", estudiante.Id);
                throw;
            }
        }

        // ============================================================
        // ELIMINACIÓN
        // ============================================================

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new BusinessException("El Id del estudiante no es válido");

            const string sql = "DELETE FROM estudiantes WHERE id = @Id";

            try
            {
                using var connection = GetConnection();
                var rows = await connection.ExecuteAsync(sql, new { Id = id });

                if (rows == 0)
                    throw new NotFoundException("Estudiante", id);

                _logger.LogInformation("Estudiante eliminado con Id = {Id}", id);
                return true;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex,
                    "Error al eliminar estudiante con Id = {Id}", id);
                throw;
            }
        }

        // ============================================================
        // ASIGNACIÓN A PROYECTO
        // ============================================================

        /// <inheritdoc/>
        public async Task<bool> AsignarAProyectoAsync(int estudianteId, int proyectoId)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1. Verificar que el estudiante existe y no tiene proyecto
                const string sqlVerificar = @"
                    SELECT proyecto_id FROM estudiantes WHERE id = @Id";
                var proyectoActual = await connection.QueryFirstOrDefaultAsync<int?>(
                    sqlVerificar, new { Id = estudianteId }, transaction);

                if (proyectoActual == null && !await ExisteEstudianteAsync(
                        connection, estudianteId, transaction))
                {
                    throw new NotFoundException("Estudiante", estudianteId);
                }

                if (proyectoActual.HasValue)
                    throw new BusinessException(
                        $"El estudiante ya tiene asignado el proyecto {proyectoActual.Value}");

                // 2. ✅ Determinar el tipo de proyecto por la tabla derivada
                var tipo = await ObtenerTipoProyectoAsync(
                    connection, proyectoId, transaction);

                if (tipo == null)
                    throw new NotFoundException("Proyecto", proyectoId);

                // 3. Contar estudiantes actuales del proyecto
                const string sqlContar = @"
                    SELECT COUNT(*) FROM estudiantes WHERE proyecto_id = @ProyectoId";
                var cantidad = await connection.ExecuteScalarAsync<int>(
                    sqlContar, new { ProyectoId = proyectoId }, transaction);

                // 4. Validar regla de negocio según tipo
                var (min, max) = tipo.Value switch
                {
                    TipoProyecto.SocioComunitario => (2, 3),
                    TipoProyecto.Emprendimiento => (1, 3),
                    _ => throw new BusinessException("Tipo de proyecto no válido")
                };

                if (cantidad >= max)
                    throw new BusinessException(
                        $"El proyecto ya tiene el máximo de {max} estudiantes permitidos");

                // 5. Asignar
                const string sqlAsignar = @"
                    UPDATE estudiantes 
                    SET proyecto_id = @ProyectoId 
                    WHERE id = @EstudianteId";

                await connection.ExecuteAsync(sqlAsignar,
                    new { ProyectoId = proyectoId, EstudianteId = estudianteId },
                    transaction);

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Estudiante {EstudianteId} asignado al proyecto {ProyectoId}",
                    estudianteId, proyectoId);

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ============================================================
        // MÉTODOS AUXILIARES
        // ============================================================

        /// <summary>
        /// Determina el tipo de proyecto consultando las tablas derivadas.
        /// En TPT, el tipo se determina por la tabla en la que está el registro.
        /// </summary>
        /// <param name="connection">Conexión a PostgreSQL.</param>
        /// <param name="proyectoId">Identificador del proyecto.</param>
        /// <param name="transaction">Transacción activa (opcional).</param>
        /// <returns>Tipo de proyecto o null si no existe.</returns>
        private async Task<TipoProyecto?> ObtenerTipoProyectoAsync(
            NpgsqlConnection connection,
            int proyectoId,
            NpgsqlTransaction transaction)
        {
            const string sql = @"
                SELECT 
                    CASE 
                        WHEN s.id IS NOT NULL THEN 1  -- SocioComunitario
                        WHEN e.id IS NOT NULL THEN 2  -- Emprendimiento
                        ELSE NULL
                    END AS tipo_proyecto
                FROM proyectos p
                LEFT JOIN proyectos_sociocomunitarios s ON p.id = s.id
                LEFT JOIN proyectos_emprendimientos e ON p.id = e.id
                WHERE p.id = @Id";

            var tipo = await connection.QueryFirstOrDefaultAsync<int?>(
                sql, new { Id = proyectoId }, transaction);

            return tipo.HasValue ? (TipoProyecto)tipo.Value : null;
        }

        /// <summary>
        /// Verifica si un estudiante existe en la base de datos.
        /// </summary>
        private async Task<bool> ExisteEstudianteAsync(
            NpgsqlConnection connection,
            int estudianteId,
            NpgsqlTransaction transaction)
        {
            const string sql = "SELECT COUNT(*) FROM estudiantes WHERE id = @Id";
            var count = await connection.ExecuteScalarAsync<int>(
                sql, new { Id = estudianteId }, transaction);
            return count > 0;
        }
    }
}