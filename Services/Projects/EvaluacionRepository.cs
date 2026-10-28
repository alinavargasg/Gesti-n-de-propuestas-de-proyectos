using AuthenticationModule.Models.Projects;
using AuthenticationModule.Services.Exceptions;
using Dapper;
using Npgsql;

namespace AuthenticationModule.Services.Projects
{
    /// <summary>
    /// Implementación del repositorio de evaluaciones utilizando Dapper.
    /// Proporciona operaciones CRUD para la entidad <see cref="Evaluacion"/>.
    /// </summary>
    public class EvaluacionRepository : IEvaluacionRepository
    {
        // ============================================================
        // DEPENDENCIAS
        // ============================================================

        private readonly string _connectionString;
        private readonly ILogger<EvaluacionRepository> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="EvaluacionRepository"/>.
        /// </summary>
        public EvaluacionRepository(
            IConfiguration configuration,
            ILogger<EvaluacionRepository> logger)
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
        public async Task<List<Evaluacion>> GetAllAsync()
        {
            const string sql = @"
                SELECT 
                    e.id,
                    e.proyecto_id,
                    e.docente_id,
                    e.observaciones,
                    e.recomendaciones,
                    e.fecha_evaluacion,
                    e.nota,
                    d.id AS DocenteId,
                    d.nombre AS DocenteNombre,
                    d.apellido_paterno AS DocenteApellidoPaterno,
                    d.especialidad AS DocenteEspecialidad
                    p.titulo AS ProyectoTitulo,
                FROM evaluaciones e
                INNER JOIN proyectos p ON e.proyecto_id = p.id
                INNER JOIN docentes d ON e.docente_id = d.id
                ORDER BY e.fecha_evaluacion DESC";

            try
            {
                using var connection = GetConnection();
                var result = await connection.QueryAsync<Evaluacion, Docente, Evaluacion>(
                    sql,
                    (evaluacion, docente) =>
                    {
                        evaluacion.Docente = docente;
                        return evaluacion;
                    },
                    splitOn: "DocenteId"
                );
                return result.ToList();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al obtener todas las evaluaciones");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Evaluacion> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT 
                    e.id,
                    e.proyecto_id,
                    e.docente_id,
                    e.observaciones,
                    e.recomendaciones,
                    e.fecha_evaluacion,
                    e.nota,
                    p.titulo AS ProyectoTitulo,
                    d.nombre AS DocenteNombre,
                    d.apellido_paterno AS DocenteApellidoPaterno,
                    d.especialidad AS DocenteEspecialidad
                FROM evaluaciones e
                INNER JOIN proyectos p ON e.proyecto_id = p.id
                INNER JOIN docentes d ON e.docente_id = d.id
                WHERE e.id = @Id";

            try
            {
                using var connection = GetConnection();
                return await connection.QueryFirstOrDefaultAsync<Evaluacion>(
                    sql, new { Id = id });
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al obtener evaluación con Id = {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<Evaluacion>> GetByProyectoIdAsync(int proyectoId)
        {
            const string sql = @"
                SELECT 
                    e.id,
                    e.proyecto_id,
                    e.docente_id,
                    e.observaciones,
                    e.recomendaciones,
                    e.fecha_evaluacion,
                    e.nota,
                    d.id AS DocenteId,              -- Mapeo del Docente
                    d.nombre AS Nombre,      -- Mapeo del Docente
                    d.apellido_paterno AS ApellidoPaterno,
                    d.apellido_materno AS ApellidoMaterno,
                    d.especialidad AS Especialidad
                FROM evaluaciones e
                INNER JOIN docentes d ON e.docente_id = d.id
                WHERE e.proyecto_id = @ProyectoId";

            try
            {
                using var connection = GetConnection();
                // ✅ Usamos Multi-Mapping de Dapper para llenar la propiedad Docente
                var result = await connection.QueryAsync<Evaluacion, Docente, Evaluacion>(
                    sql,
                    (evaluacion, docente) =>
                    {
                        evaluacion.Docente = docente;
                        return evaluacion;
                    },
                    new { ProyectoId = proyectoId },
                    splitOn: "DocenteId" // 👈 Coincide con el alias
                );
                return result.ToList();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex,
                    "Error al obtener evaluaciones del proyecto {ProyectoId}", proyectoId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<Evaluacion>> GetByDocenteIdAsync(int docenteId)
        {
            const string sql = @"
                SELECT 
                    e.id,
                    e.proyecto_id,
                    e.docente_id,
                    e.observaciones,
                    e.recomendaciones,
                    e.fecha_evaluacion,
                    e.nota,
                    p.titulo AS ProyectoTitulo
                FROM evaluaciones e
                INNER JOIN proyectos p ON e.proyecto_id = p.id
                WHERE e.docente_id = @DocenteId
                ORDER BY e.fecha_evaluacion DESC";

            try
            {
                using var connection = GetConnection();
                var result = await connection.QueryAsync<Evaluacion>(
                    sql, new { DocenteId = docenteId });
                return result.ToList();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex,
                    "Error al obtener evaluaciones del docente {DocenteId}", docenteId);
                throw;
            }
        }

        // ============================================================
        // CREACIÓN
        // ============================================================

        /// <inheritdoc/>
        public async Task<int> CreateAsync(Evaluacion evaluacion)
        {
            if (evaluacion == null)
                throw new BusinessException("La evaluación no puede ser nula");

            // Validaciones básicas de negocio
            if (string.IsNullOrWhiteSpace(evaluacion.Observaciones))
                throw new BusinessException("Las observaciones son obligatorias");

            if (evaluacion.Observaciones.Length < 10)
                throw new BusinessException("Las observaciones deben tener al menos 10 caracteres");

            if (string.IsNullOrWhiteSpace(evaluacion.Recomendaciones))
                throw new BusinessException("Las recomendaciones son obligatorias");

            if (evaluacion.Recomendaciones.Length < 10)
                throw new BusinessException("Las recomendaciones deben tener al menos 10 caracteres");

            if (evaluacion.Nota.HasValue && (evaluacion.Nota < 0 || evaluacion.Nota > 100))
                throw new BusinessException("La nota debe estar entre 0 y 100");

            if (evaluacion.ProyectoId <= 0)
                throw new BusinessException("El proyecto es obligatorio");

            if (evaluacion.DocenteId <= 0)
                throw new BusinessException("El docente es obligatorio");

            using var connection = GetConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1. Verificar que el proyecto existe
                const string sqlProyecto = @"
                    SELECT COUNT(*) FROM proyectos WHERE id = @Id";
                var proyectoExiste = await connection.ExecuteScalarAsync<int>(
                    sqlProyecto, new { Id = evaluacion.ProyectoId }, transaction);

                if (proyectoExiste == 0)
                    throw new NotFoundException("Proyecto", evaluacion.ProyectoId);

                // 2. Verificar que el docente existe
                const string sqlDocente = @"
                    SELECT COUNT(*) FROM docentes WHERE id = @Id";
                var docenteExiste = await connection.ExecuteScalarAsync<int>(
                    sqlDocente, new { Id = evaluacion.DocenteId }, transaction);

                if (docenteExiste == 0)
                    throw new NotFoundException("Docente", evaluacion.DocenteId);

                // 3. Verificar que el mismo docente no haya evaluado ya este proyecto
                const string sqlDuplicado = @"
                    SELECT COUNT(*) FROM evaluaciones 
                    WHERE proyecto_id = @ProyectoId AND docente_id = @DocenteId";
                var duplicado = await connection.ExecuteScalarAsync<int>(
                    sqlDuplicado,
                    new { ProyectoId = evaluacion.ProyectoId, DocenteId = evaluacion.DocenteId },
                    transaction);

                if (duplicado > 0)
                    throw new BusinessException(
                        "Este docente ya ha evaluado este proyecto. " +
                        "Puede actualizar la evaluación existente.");

                // 4. Insertar
                const string sqlInsert = @"
                    INSERT INTO evaluaciones 
                        (proyecto_id, docente_id, observaciones, recomendaciones, 
                         fecha_evaluacion, nota)
                    VALUES 
                        (@ProyectoId, @DocenteId, @Observaciones, @Recomendaciones, 
                         @FechaEvaluacion, @Nota)
                    RETURNING id";

                var id = await connection.ExecuteScalarAsync<int>(sqlInsert, new
                {
                    evaluacion.ProyectoId,
                    evaluacion.DocenteId,
                    evaluacion.Observaciones,
                    evaluacion.Recomendaciones,
                    FechaEvaluacion = evaluacion.FechaEvaluacion == default
                        ? DateTime.UtcNow
                        : evaluacion.FechaEvaluacion,
                    evaluacion.Nota
                }, transaction);

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Evaluación creada con Id = {Id} para proyecto {ProyectoId} por docente {DocenteId}",
                    id, evaluacion.ProyectoId, evaluacion.DocenteId);

                return id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ============================================================
        // ACTUALIZACIÓN
        // ============================================================

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(Evaluacion evaluacion)
        {
            if (evaluacion == null)
                throw new BusinessException("La evaluación no puede ser nula");

            if (evaluacion.Id <= 0)
                throw new BusinessException("El Id de la evaluación no es válido");

            if (string.IsNullOrWhiteSpace(evaluacion.Observaciones))
                throw new BusinessException("Las observaciones son obligatorias");

            if (string.IsNullOrWhiteSpace(evaluacion.Recomendaciones))
                throw new BusinessException("Las recomendaciones son obligatorias");

            if (evaluacion.Nota.HasValue && (evaluacion.Nota < 0 || evaluacion.Nota > 100))
                throw new BusinessException("La nota debe estar entre 0 y 100");

            const string sql = @"
                UPDATE evaluaciones 
                SET observaciones = @Observaciones,
                    recomendaciones = @Recomendaciones,
                    nota = @Nota,
                    fecha_evaluacion = @FechaEvaluacion
                WHERE id = @Id";

            try
            {
                using var connection = GetConnection();
                var rows = await connection.ExecuteAsync(sql, new
                {
                    evaluacion.Id,
                    evaluacion.Observaciones,
                    evaluacion.Recomendaciones,
                    evaluacion.Nota,
                    FechaEvaluacion = evaluacion.FechaEvaluacion == default
                        ? DateTime.UtcNow
                        : evaluacion.FechaEvaluacion
                });

                if (rows == 0)
                    throw new NotFoundException("Evaluacion", evaluacion.Id);

                _logger.LogInformation(
                    "Evaluación actualizada con Id = {Id}", evaluacion.Id);

                return true;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex,
                    "Error al actualizar evaluación con Id = {Id}", evaluacion.Id);
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
                throw new BusinessException("El Id de la evaluación no es válido");

            const string sql = "DELETE FROM evaluaciones WHERE id = @Id";

            try
            {
                using var connection = GetConnection();
                var rows = await connection.ExecuteAsync(sql, new { Id = id });

                if (rows == 0)
                    throw new NotFoundException("Evaluacion", id);

                _logger.LogInformation("Evaluación eliminada con Id = {Id}", id);
                return true;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al eliminar evaluación con Id = {Id}", id);
                throw;
            }
        }

        // ============================================================
        // CONSULTAS ESPECÍFICAS
        // ============================================================

        /// <inheritdoc/>
        public async Task<bool> TieneEvaluacionesAsync(int proyectoId)
        {
            const string sql = @"
                SELECT COUNT(*) FROM evaluaciones WHERE proyecto_id = @ProyectoId";

            try
            {
                using var connection = GetConnection();
                var count = await connection.ExecuteScalarAsync<int>(
                    sql, new { ProyectoId = proyectoId });

                return count > 0;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex,
                    "Error al verificar evaluaciones del proyecto {ProyectoId}", proyectoId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<decimal?> GetPromedioNotasAsync(int proyectoId)
        {
            const string sql = @"
                SELECT AVG(nota) 
                FROM evaluaciones 
                WHERE proyecto_id = @ProyectoId AND nota IS NOT NULL";

            try
            {
                using var connection = GetConnection();
                var promedio = await connection.ExecuteScalarAsync<decimal?>(
                    sql, new { ProyectoId = proyectoId });

                return promedio;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex,
                    "Error al calcular promedio de notas del proyecto {ProyectoId}", proyectoId);
                throw;
            }
        }
    }
}