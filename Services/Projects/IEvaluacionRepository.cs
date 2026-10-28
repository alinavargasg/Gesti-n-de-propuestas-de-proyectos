using AuthenticationModule.Models.Projects;

namespace AuthenticationModule.Services.Projects
{
    /// <summary>
    /// Interfaz del repositorio de evaluaciones.
    /// Define las operaciones CRUD para la entidad <see cref="Evaluacion"/>.
    /// </summary>
    public interface IEvaluacionRepository
    {
        // ============================================================
        // CONSULTAS
        // ============================================================

        /// <summary>
        /// Obtiene todas las evaluaciones registradas, incluyendo datos
        /// del proyecto y del docente asociado.
        /// </summary>
        /// <returns>Lista de evaluaciones.</returns>
        Task<List<Evaluacion>> GetAllAsync();

        /// <summary>
        /// Obtiene una evaluación por su identificador.
        /// </summary>
        /// <param name="id">Identificador de la evaluación.</param>
        /// <returns>La evaluación encontrada o null si no existe.</returns>
        Task<Evaluacion> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene todas las evaluaciones de un proyecto específico.
        /// </summary>
        /// <param name="proyectoId">Identificador del proyecto.</param>
        /// <returns>Lista de evaluaciones del proyecto.</returns>
        Task<List<Evaluacion>> GetByProyectoIdAsync(int proyectoId);

        /// <summary>
        /// Obtiene todas las evaluaciones realizadas por un docente.
        /// </summary>
        /// <param name="docenteId">Identificador del docente.</param>
        /// <returns>Lista de evaluaciones del docente.</returns>
        Task<List<Evaluacion>> GetByDocenteIdAsync(int docenteId);

        // ============================================================
        // CREACIÓN
        // ============================================================

        /// <summary>
        /// Crea una nueva evaluación.
        /// Valida que el proyecto y el docente existan.
        /// </summary>
        /// <param name="evaluacion">Evaluación a crear.</param>
        /// <returns>Identificador de la evaluación creada.</returns>
        /// <exception cref="BusinessException">Si los datos son inválidos.</exception>
        /// <exception cref="NotFoundException">Si el proyecto o docente no existen.</exception>
        Task<int> CreateAsync(Evaluacion evaluacion);

        // ============================================================
        // ACTUALIZACIÓN
        // ============================================================

        /// <summary>
        /// Actualiza una evaluación existente.
        /// </summary>
        /// <param name="evaluacion">Evaluación con los datos actualizados.</param>
        /// <returns>True si se actualizó correctamente.</returns>
        /// <exception cref="NotFoundException">Si la evaluación no existe.</exception>
        Task<bool> UpdateAsync(Evaluacion evaluacion);

        // ============================================================
        // ELIMINACIÓN
        // ============================================================

        /// <summary>
        /// Elimina una evaluación por su identificador.
        /// </summary>
        /// <param name="id">Identificador de la evaluación.</param>
        /// <returns>True si se eliminó correctamente.</returns>
        /// <exception cref="NotFoundException">Si la evaluación no existe.</exception>
        Task<bool> DeleteAsync(int id);

        // ============================================================
        // CONSULTAS ESPECÍFICAS
        // ============================================================

        /// <summary>
        /// Verifica si un proyecto ya tiene al menos una evaluación.
        /// Útil para validar la regla: "Un proyecto DEBE ser evaluado por 1 o más docentes".
        /// </summary>
        /// <param name="proyectoId">Identificador del proyecto.</param>
        /// <returns>True si el proyecto tiene al menos una evaluación.</returns>
        Task<bool> TieneEvaluacionesAsync(int proyectoId);

        /// <summary>
        /// Obtiene el promedio de notas de un proyecto.
        /// </summary>
        /// <param name="proyectoId">Identificador del proyecto.</param>
        /// <returns>Promedio de notas, o null si no hay evaluaciones con nota.</returns>
        Task<decimal?> GetPromedioNotasAsync(int proyectoId);
    }
}