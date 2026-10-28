using AuthenticationModule.Models.Projects;
using AuthenticationModule.Models.Projects.Enums;
using AuthenticationModule.Services.Exceptions;
using Dapper;
using Npgsql;

namespace AuthenticationModule.Services.Projects
{
    /// <summary>
    /// Implementación del repositorio de proyectos utilizando Dapper.
    /// 
    /// Estrategia TPT (Table Per Type):
    /// - La tabla base "proyectos" contiene solo los datos comunes.
    /// - Las tablas derivadas "proyectos_sociocomunitarios" y 
    ///   "proyectos_emprendimientos" contienen los datos específicos.
    /// - El tipo de proyecto se determina por la tabla en la que está el registro,
    ///   NO por una columna discriminadora.
    /// </summary>
    public class ProyectoRepository : IProyectoRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ProyectoRepository> _logger;

        public ProyectoRepository(IConfiguration configuration, ILogger<ProyectoRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        private NpgsqlConnection GetConnection() => new NpgsqlConnection(_connectionString);

        // ============================================================
        // CONSULTAS
        // ============================================================

        /// <inheritdoc/>
        /// 
        // Método auxiliar para no repetir código
        private void CopiarPropiedadesBase(Proyecto origen, Proyecto destino)
        {
            destino.Id = origen.Id;
            destino.Titulo = origen.Titulo;
            destino.Problematica = origen.Problematica;
            destino.ObjetivoGeneral = origen.ObjetivoGeneral;
            destino.DescripcionPrototipo = origen.DescripcionPrototipo;
            destino.RecursosTecnologias = origen.RecursosTecnologias;
            destino.CreatedAt = origen.CreatedAt;
            destino.TipoProyecto = origen.TipoProyecto;
        }

        public async Task<List<Proyecto>> GetAllAsync()
        {
            const string sql = @"
        SELECT p.*,
               s.beneficiarios, s.ubicacion_geografica, s.actores_involucrados,
               s.impacto_social_esperado, s.estrategias_sostenibilidad,
               e.segmento_mercado, e.propuesta_valor, e.analisis_competencia,
               e.modelo_ingresos, e.estimacion_inversion_inicial,
               e.viabilidad_economica, e.estrategia_comercializacion,
               CASE 
                   WHEN s.id IS NOT NULL THEN 1 -- SocioComunitario
                   WHEN e.id IS NOT NULL THEN 2 -- Emprendimiento
               END AS TipoProyecto
        FROM proyectos p
        LEFT JOIN proyectos_sociocomunitarios s ON p.id = s.id
        LEFT JOIN proyectos_emprendimientos e ON p.id = e.id
        ORDER BY p.created_at DESC";

            try
            {
                using var connection = GetConnection();

                // 👇 FÍJATE QUE EL TIPO FINAL ES "Proyecto", NO "ProyectoSociocomunitario"
                var result = await connection.QueryAsync<Proyecto, int, Proyecto>(
                    sql,
                    (proyectoBase, tipo) =>
                    {
                        if (tipo == 1)
                        {
                            var socio = new ProyectoSociocomunitario();
                            CopiarPropiedadesBase(proyectoBase, socio);
                            socio.TipoProyecto = "Sociocomunitario";
                            socio.Beneficiarios = proyectoBase.Beneficiarios;
                            socio.UbicacionGeografica = proyectoBase.UbicacionGeografica;
                            socio.ActoresInvolucrados = proyectoBase.ActoresInvolucrados;
                            socio.ImpactoSocialEsperado = proyectoBase.ImpactoSocialEsperado;
                            socio.EstrategiasSostenibilidad = proyectoBase.EstrategiasSostenibilidad;

                            return socio; // ✅ Retornamos el hijo, que ES UN Proyecto
                        }
                        else if (tipo == 2)
                        {
                            var empr = new ProyectoEmprendimiento();
                            CopiarPropiedadesBase(proyectoBase, empr);
                            empr.TipoProyecto = "Emprendimiento";
                            empr.SegmentoMercado = proyectoBase.SegmentoMercado;
                            empr.PropuestaValor = proyectoBase.PropuestaValor;
                            empr.AnalisisCompetencia = proyectoBase.AnalisisCompetencia;
                            empr.ModeloIngresos = proyectoBase.ModeloIngresos;
                            empr.EstimacionInversionInicial = proyectoBase.EstimacionInversionInicial;
                            empr.ViabilidadEconomica = proyectoBase.ViabilidadEconomica;
                            empr.EstrategiaComercializacion = proyectoBase.EstrategiaComercializacion;

                            return empr; // ✅ Retornamos el hijo, que ES UN Proyecto
                        }

                        return proyectoBase; // ✅ Retornamos la base
                    },
                    splitOn: "TipoProyecto"
                );

                // Como el tipo final es List<Proyecto>, no necesitas hacer Cast.
                return result.ToList();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al obtener todos los proyectos");
                throw;
            }
        }
        public async Task<Proyecto> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT p.*,
                       s.beneficiarios, s.ubicacion_geografica, s.actores_involucrados,
                       s.impacto_social_esperado, s.estrategias_sostenibilidad,
                       e.segmento_mercado, e.propuesta_valor, e.analisis_competencia,
                       e.modelo_ingresos, e.estimacion_inversion_inicial,
                       e.viabilidad_economica, e.estrategia_comercializacion,
                       CASE 
                           WHEN s.id IS NOT NULL THEN 1 -- SocioComunitario
                           WHEN e.id IS NOT NULL THEN 2 -- Emprendimiento
                       END AS TipoProyecto
                FROM proyectos p
                LEFT JOIN proyectos_sociocomunitarios s ON p.id = s.id
                LEFT JOIN proyectos_emprendimientos e ON p.id = e.id
                WHERE p.id = @Id";

            try
            {
                using var connection = GetConnection();

                // ✅ Cambiamos a QueryAsync para mantener la misma lógica que el GetAllAsync
                var result = await connection.QueryAsync<Proyecto, int, Proyecto>(
                    sql,
                    (proyectoBase, tipo) =>
                    {
                        if (tipo == 1)
                        {
                            var socio = new ProyectoSociocomunitario();
                            CopiarPropiedadesBase(proyectoBase, socio);
                            socio.TipoProyecto = "Sociocomunitario";
                            // Copiamos las propiedades específicas
                            socio.Beneficiarios = proyectoBase.Beneficiarios;
                            socio.UbicacionGeografica = proyectoBase.UbicacionGeografica;
                            socio.ActoresInvolucrados = proyectoBase.ActoresInvolucrados;
                            socio.ImpactoSocialEsperado = proyectoBase.ImpactoSocialEsperado;
                            socio.EstrategiasSostenibilidad = proyectoBase.EstrategiasSostenibilidad;

                            return socio;
                        }
                        else if (tipo == 2)
                        {
                            var empr = new ProyectoEmprendimiento();
                            CopiarPropiedadesBase(proyectoBase, empr);
                            empr.TipoProyecto = "Emprendimiento";
                            // Copiamos las propiedades específicas
                            empr.SegmentoMercado = proyectoBase.SegmentoMercado;
                            empr.PropuestaValor = proyectoBase.PropuestaValor;
                            empr.AnalisisCompetencia = proyectoBase.AnalisisCompetencia;
                            empr.ModeloIngresos = proyectoBase.ModeloIngresos;
                            empr.EstimacionInversionInicial = proyectoBase.EstimacionInversionInicial;
                            empr.ViabilidadEconomica = proyectoBase.ViabilidadEconomica;
                            empr.EstrategiaComercializacion = proyectoBase.EstrategiaComercializacion;

                            return empr;
                        }

                        return proyectoBase;
                    },
                    new { Id = id },
                    splitOn: "TipoProyecto"
                );

                return result.FirstOrDefault();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al obtener proyecto con Id = {Id}", id);
                throw;
            }
        }
        /// <inheritdoc/>
        public async Task<List<ProyectoSociocomunitario>> GetSociocomunitariosAsync()
        {
            const string sql = @"
                SELECT p.*, s.beneficiarios, s.ubicacion_geografica, 
                       s.actores_involucrados, s.impacto_social_esperado, 
                       s.estrategias_sostenibilidad
                FROM proyectos p
                INNER JOIN proyectos_sociocomunitarios s ON p.id = s.id
                ORDER BY p.created_at DESC";

            try
            {
                using var connection = GetConnection();
                var result = await connection.QueryAsync<ProyectoSociocomunitario>(sql);
                return result.ToList();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al obtener proyectos sociocomunitarios");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ProyectoEmprendimiento>> GetEmprendimientosAsync()
        {
            const string sql = @"
                SELECT p.*, e.segmento_mercado, e.propuesta_valor, 
                       e.analisis_competencia, e.modelo_ingresos, 
                       e.estimacion_inversion_inicial, e.viabilidad_economica, 
                       e.estrategia_comercializacion
                FROM proyectos p
                INNER JOIN proyectos_emprendimientos e ON p.id = e.id
                ORDER BY p.created_at DESC";

            try
            {
                using var connection = GetConnection();
                var result = await connection.QueryAsync<ProyectoEmprendimiento>(sql);
                return result.ToList();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al obtener proyectos de emprendimiento");
                throw;
            }
        }

        // ============================================================
        // CREACIÓN
        // ============================================================

        /// <inheritdoc/>
        public async Task<int> CreateAsync(Proyecto proyecto)
        {
            if (proyecto == null)
                throw new BusinessException("El proyecto no puede ser nulo");

            using var connection = GetConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1. Insertar en tabla base (SIN tipo_proyecto)
                const string sqlBase = @"
                    INSERT INTO proyectos (titulo, problematica, objetivo_general, 
                                           descripcion_prototipo, recursos_tecnologias, 
                                           created_at)
                    VALUES (@Titulo, @Problematica, @ObjetivoGeneral, 
                            @DescripcionPrototipo, @RecursosTecnologias, 
                            @CreatedAt)
                    RETURNING id";

                var id = await connection.ExecuteScalarAsync<int>(sqlBase, new
                {
                    proyecto.Titulo,
                    proyecto.Problematica,
                    proyecto.ObjetivoGeneral,
                    proyecto.DescripcionPrototipo,
                    proyecto.RecursosTecnologias,
                    proyecto.CreatedAt
                }, transaction);

                // 2. Insertar en tabla derivada según el tipo
                if (proyecto is ProyectoSociocomunitario socio)
                {
                    const string sqlSocio = @"
                        INSERT INTO proyectos_sociocomunitarios 
                            (id, beneficiarios, ubicacion_geografica, actores_involucrados,
                             impacto_social_esperado, estrategias_sostenibilidad)
                        VALUES (@Id, @Beneficiarios, @UbicacionGeografica, @ActoresInvolucrados,
                                @ImpactoSocialEsperado, @EstrategiasSostenibilidad)";

                    await connection.ExecuteAsync(sqlSocio, new
                    {
                        Id = id,
                        socio.Beneficiarios,
                        socio.UbicacionGeografica,
                        socio.ActoresInvolucrados,
                        socio.ImpactoSocialEsperado,
                        socio.EstrategiasSostenibilidad
                    }, transaction);
                }
                else if (proyecto is ProyectoEmprendimiento emprendimiento)
                {
                    const string sqlEmpr = @"
                        INSERT INTO proyectos_emprendimientos 
                            (id, segmento_mercado, propuesta_valor, analisis_competencia,
                             modelo_ingresos, estimacion_inversion_inicial,
                             viabilidad_economica, estrategia_comercializacion)
                        VALUES (@Id, @SegmentoMercado, @PropuestaValor, @AnalisisCompetencia,
                                @ModeloIngresos, @EstimacionInversionInicial,
                                @ViabilidadEconomica, @EstrategiaComercializacion)";

                    await connection.ExecuteAsync(sqlEmpr, new
                    {
                        Id = id,
                        emprendimiento.SegmentoMercado,
                        emprendimiento.PropuestaValor,
                        emprendimiento.AnalisisCompetencia,
                        emprendimiento.ModeloIngresos,
                        emprendimiento.EstimacionInversionInicial,
                        emprendimiento.ViabilidadEconomica,
                        emprendimiento.EstrategiaComercializacion
                    }, transaction);
                }
                else
                {
                    throw new BusinessException(
                        "El tipo de proyecto no es válido. Debe ser " +
                        "ProyectoSociocomunitario o ProyectoEmprendimiento");
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Proyecto creado con Id = {Id}", id);
                return id;
            }
            catch (NpgsqlException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear proyecto");
                throw;
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
        public async Task<bool> UpdateAsync(Proyecto proyecto)
        {
            if (proyecto == null)
                throw new BusinessException("El proyecto no puede ser nulo");

            using var connection = GetConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1. Actualizar tabla base
                const string sqlBase = @"
                    UPDATE proyectos 
                    SET titulo = @Titulo,
                        problematica = @Problematica,
                        objetivo_general = @ObjetivoGeneral,
                        descripcion_prototipo = @DescripcionPrototipo,
                        recursos_tecnologias = @RecursosTecnologias
                    WHERE id = @Id";

                var rows = await connection.ExecuteAsync(sqlBase, new
                {
                    proyecto.Id,
                    proyecto.Titulo,
                    proyecto.Problematica,
                    proyecto.ObjetivoGeneral,
                    proyecto.DescripcionPrototipo,
                    proyecto.RecursosTecnologias
                }, transaction);

                if (rows == 0)
                    throw new NotFoundException("Proyecto", proyecto.Id);

                // 2. Actualizar tabla derivada según tipo
                if (proyecto is ProyectoSociocomunitario socio)
                {
                    const string sqlSocio = @"
                        UPDATE proyectos_sociocomunitarios 
                        SET beneficiarios = @Beneficiarios,
                            ubicacion_geografica = @UbicacionGeografica,
                            actores_involucrados = @ActoresInvolucrados,
                            impacto_social_esperado = @ImpactoSocialEsperado,
                            estrategias_sostenibilidad = @EstrategiasSostenibilidad
                        WHERE id = @Id";

                    await connection.ExecuteAsync(sqlSocio, new
                    {
                        socio.Id,
                        socio.Beneficiarios,
                        socio.UbicacionGeografica,
                        socio.ActoresInvolucrados,
                        socio.ImpactoSocialEsperado,
                        socio.EstrategiasSostenibilidad
                    }, transaction);
                }
                else if (proyecto is ProyectoEmprendimiento empr)
                {
                    const string sqlEmpr = @"
                        UPDATE proyectos_emprendimientos 
                        SET segmento_mercado = @SegmentoMercado,
                            propuesta_valor = @PropuestaValor,
                            analisis_competencia = @AnalisisCompetencia,
                            modelo_ingresos = @ModeloIngresos,
                            estimacion_inversion_inicial = @EstimacionInversionInicial,
                            viabilidad_economica = @ViabilidadEconomica,
                            estrategia_comercializacion = @EstrategiaComercializacion
                        WHERE id = @Id";

                    await connection.ExecuteAsync(sqlEmpr, new
                    {
                        empr.Id,
                        empr.SegmentoMercado,
                        empr.PropuestaValor,
                        empr.AnalisisCompetencia,
                        empr.ModeloIngresos,
                        empr.EstimacionInversionInicial,
                        empr.ViabilidadEconomica,
                        empr.EstrategiaComercializacion
                    }, transaction);
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Proyecto actualizado con Id = {Id}", proyecto.Id);
                return true;
            }
            catch (NotFoundException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al actualizar proyecto con Id = {Id}", proyecto.Id);
                throw;
            }
        }

        // ============================================================
        // ELIMINACIÓN
        // ============================================================

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM proyectos WHERE id = @Id";

            try
            {
                using var connection = GetConnection();
                var rows = await connection.ExecuteAsync(sql, new { Id = id });

                if (rows == 0)
                    throw new NotFoundException("Proyecto", id);

                _logger.LogInformation("Proyecto eliminado con Id = {Id}", id);
                return true;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al eliminar proyecto con Id = {Id}", id);
                throw;
            }
        }

        // ============================================================
        // ASIGNACIÓN DE ESTUDIANTES
        // ============================================================

        /// <inheritdoc/>
        public async Task<bool> AsignarEstudianteAsync(int proyectoId, int estudianteId)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1. Determinar el tipo de proyecto por la tabla derivada
                var tipo = await ObtenerTipoProyectoAsync(connection, proyectoId, transaction);

                if (tipo == null)
                    throw new NotFoundException("Proyecto", proyectoId);

                // 2. Verificar que el estudiante existe y no tiene proyecto
                const string sqlEstudiante = @"
                    SELECT proyecto_id FROM estudiantes WHERE id = @Id";
                var proyectoActual = await connection.QueryFirstOrDefaultAsync<int?>(
                    sqlEstudiante, new { Id = estudianteId }, transaction);

                if (proyectoActual == null && !await ExisteEstudianteAsync(connection, estudianteId, transaction))
                    throw new NotFoundException("Estudiante", estudianteId);

                if (proyectoActual.HasValue)
                    throw new BusinessException(
                        $"El estudiante {estudianteId} ya tiene asignado el proyecto {proyectoActual.Value}");

                // 3. Contar estudiantes actuales del proyecto
                const string sqlContar = @"
                    SELECT COUNT(*) FROM estudiantes WHERE proyecto_id = @ProyectoId";
                var cantidadActual = await connection.ExecuteScalarAsync<int>(
                    sqlContar, new { ProyectoId = proyectoId }, transaction);

                // 4. Validar regla de negocio según tipo
                var (min, max) = tipo.Value switch
                {
                    TipoProyecto.SocioComunitario => (2, 3),
                    TipoProyecto.Emprendimiento => (1, 3),
                    _ => throw new BusinessException("Tipo de proyecto no válido")
                };

                if (cantidadActual >= max)
                    throw new BusinessException(
                        $"El proyecto ya tiene el máximo de {max} estudiantes permitidos");

                // 5. Asignar
                const string sqlAsignar = @"
                    UPDATE estudiantes SET proyecto_id = @ProyectoId WHERE id = @EstudianteId";
                await connection.ExecuteAsync(sqlAsignar,
                    new { ProyectoId = proyectoId, EstudianteId = estudianteId }, transaction);

                await transaction.CommitAsync();
                _logger.LogInformation("Estudiante {EstudianteId} asignado al proyecto {ProyectoId}",
                    estudianteId, proyectoId);
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ValidarCantidadEstudiantesAsync(int proyectoId)
        {
            try
            {
                using var connection = GetConnection();

                // 1. Determinar el tipo de proyecto por la tabla derivada
                var tipo = await ObtenerTipoProyectoAsync(connection, proyectoId, null);
                if (tipo == null)
                    throw new NotFoundException("Proyecto", proyectoId);

                // 2. Contar estudiantes
                const string sqlContar = @"
                    SELECT COUNT(*) FROM estudiantes WHERE proyecto_id = @ProyectoId";
                var cantidad = await connection.ExecuteScalarAsync<int>(
                    sqlContar, new { ProyectoId = proyectoId });

                // 3. Validar según tipo
                var (min, max) = tipo.Value switch
                {
                    TipoProyecto.SocioComunitario => (2, 3),
                    TipoProyecto.Emprendimiento => (1, 3),
                    _ => throw new BusinessException("Tipo de proyecto no válido")
                };

                bool esValido = cantidad >= min && cantidad <= max;

                if (!esValido)
                {
                    _logger.LogWarning(
                        "Proyecto {Id} tiene {Cantidad} estudiantes (requerido: {Min}-{Max})",
                        proyectoId, cantidad, min, max);
                }

                return esValido;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Error al validar cantidad de estudiantes del proyecto {Id}", proyectoId);
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
                        WHEN s.id IS NOT NULL THEN 1
                        WHEN e.id IS NOT NULL THEN 2
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
            NpgsqlConnection connection, int estudianteId, NpgsqlTransaction transaction)
        {
            const string sql = "SELECT COUNT(*) FROM estudiantes WHERE id = @Id";
            var count = await connection.ExecuteScalarAsync<int>(
                sql, new { Id = estudianteId }, transaction);
            return count > 0;
        }
    }
}