using AuthenticationModule.Models;
using AuthenticationModule.Models.Projects;
using AuthenticationModule.Services.Exceptions;
using Dapper;
using Npgsql;

namespace AuthenticationModule.Services.Projects
{
    /// <summary>
    /// Implementación del repositorio de docentes utilizando Dapper.
    /// Proporciona operaciones CRUD para la entidad <see cref="Docente"/>.
    /// </summary>
    public class DocenteRepository : IDocenteRepository
    {
        // ============================================================
        // DEPENDENCIAS
        // ============================================================

        private readonly string _connectionString;
        private readonly ILogger<DocenteRepository> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="DocenteRepository"/>.
        /// </summary>
        /// <param name="configuration">Configuración de la aplicación.</param>
        /// <param name="logger">Logger para registrar eventos.</param>
        public DocenteRepository(
            IConfiguration configuration,
            ILogger<DocenteRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        /// <summary>
        /// Obtiene una conexión a PostgreSQL.
        /// </summary>
        private NpgsqlConnection GetConnection() => new NpgsqlConnection(_connectionString);

        // ============================================================
        // CONSULTAS
        // ============================================================

        /// <inheritdoc/>
        public async Task<List<Docente>> GetAllAsync()
        {
            const string sql = @"
                SELECT id, nombre, apellido_paterno, apellido_materno, especialidad
                FROM docentes 
                ORDER BY apellido_paterno, apellido_materno, nombre";

            try
            {
                using var connection = GetConnection();
                var result = await connection.QueryAsync<Docente>(sql);
                return result.ToList();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al obtener todos los docentes");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Docente> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT id, nombre, apellido_paterno, apellido_materno, especialidad
                FROM docentes 
                WHERE id = @Id";

            try
            {
                using var connection = GetConnection();
                return await connection.QueryFirstOrDefaultAsync<Docente>(sql, new { Id = id });
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al obtener docente con Id = {Id}", id);
                throw;
            }
        }

        // ============================================================
        // CREACIÓN
        // ============================================================

        /// <inheritdoc/>
        public async Task<int> CreateAsync(Docente docente)
        {
            if (docente == null)
                throw new BusinessException("El docente no puede ser nulo");

            // Validaciones básicas de negocio
            if (string.IsNullOrWhiteSpace(docente.Nombre))
                throw new BusinessException("El nombre del docente es obligatorio");

            if (string.IsNullOrWhiteSpace(docente.ApellidoPaterno))
                throw new BusinessException("El apellido paterno es obligatorio");

            if (string.IsNullOrWhiteSpace(docente.ApellidoMaterno))
                throw new BusinessException("El apellido materno es obligatorio");

            if (string.IsNullOrWhiteSpace(docente.Especialidad))
                throw new BusinessException("La especialidad es obligatoria");

            const string sql = @"
                INSERT INTO docentes (nombre, apellido_paterno, apellido_materno, especialidad)
                VALUES (@Nombre, @ApellidoPaterno, @ApellidoMaterno, @Especialidad)
                RETURNING id";

            try
            {
                using var connection = GetConnection();
                var id = await connection.ExecuteScalarAsync<int>(sql, new
                {
                    docente.Nombre,
                    docente.ApellidoPaterno,
                    docente.ApellidoMaterno,
                    docente.Especialidad
                });

                _logger.LogInformation(
                    "Docente creado con Id = {Id}, Nombre = {Nombre} {ApellidoPaterno}",
                    id, docente.Nombre, docente.ApellidoPaterno);

                return id;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al crear docente: {Nombre}", docente.Nombre);
                throw;
            }
        }

        // ============================================================
        // ACTUALIZACIÓN
        // ============================================================

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(Docente docente)
        {
            if (docente == null)
                throw new BusinessException("El docente no puede ser nulo");

            if (docente.Id <= 0)
                throw new BusinessException("El Id del docente no es válido");

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(docente.Nombre))
                throw new BusinessException("El nombre del docente es obligatorio");

            if (string.IsNullOrWhiteSpace(docente.ApellidoPaterno))
                throw new BusinessException("El apellido paterno es obligatorio");

            if (string.IsNullOrWhiteSpace(docente.Especialidad))
                throw new BusinessException("La especialidad es obligatoria");

            const string sql = @"
                UPDATE docentes 
                SET nombre = @Nombre,
                    apellido_paterno = @ApellidoPaterno,
                    apellido_materno = @ApellidoMaterno,
                    especialidad = @Especialidad
                WHERE id = @Id";

            try
            {
                using var connection = GetConnection();
                var rows = await connection.ExecuteAsync(sql, new
                {
                    docente.Id,
                    docente.Nombre,
                    docente.ApellidoPaterno,
                    docente.ApellidoMaterno,
                    docente.Especialidad
                });

                if (rows == 0)
                    throw new NotFoundException("Docente", docente.Id);

                _logger.LogInformation(
                    "Docente actualizado con Id = {Id}", docente.Id);

                return true;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al actualizar docente con Id = {Id}", docente.Id);
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
                throw new BusinessException("El Id del docente no es válido");

            const string sql = "DELETE FROM docentes WHERE id = @Id";

            try
            {
                using var connection = GetConnection();
                var rows = await connection.ExecuteAsync(sql, new { Id = id });

                if (rows == 0)
                    throw new NotFoundException("Docente", id);

                _logger.LogInformation("Docente eliminado con Id = {Id}", id);
                return true;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (NpgsqlException ex)
            {
                // Si el docente tiene evaluaciones asociadas, la FK lanzará error
                if (ex.SqlState == "23503") // Foreign key violation
                {
                    _logger.LogWarning(
                        "No se puede eliminar docente {Id}: tiene evaluaciones asociadas", id);

                    throw new BusinessException(
                        "No se puede eliminar el docente porque tiene evaluaciones registradas. " +
                        "Elimine primero las evaluaciones asociadas.");
                }

                _logger.LogError(ex, "Error al eliminar docente con Id = {Id}", id);
                throw;
            }
        }
    }
}