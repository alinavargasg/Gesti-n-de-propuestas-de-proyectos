using AuthenticationModule.Models.Projects;

namespace AuthenticationModule.Services
{
    /// <summary>
    /// Interfaz del repositorio de estudiantes.
    /// Define las operaciones de acceso a datos para la entidad <see cref="Estudiante"/>.
    /// </summary>
    public interface IEstudianteRepository
    {
        /// <summary>
        /// Obtiene todos los estudiantes registrados.
        /// </summary>
        Task<List<Estudiante>> GetAllAsync();

        /// <summary>
        /// Obtiene un estudiante por su identificador.
        /// </summary>
        Task<Estudiante> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene los estudiantes asignados a un proyecto.
        /// </summary>
        Task<List<Estudiante>> GetByProyectoIdAsync(int proyectoId);

        /// <summary>
        /// Obtiene los estudiantes que no tienen proyecto asignado.
        /// </summary>
        Task<List<Estudiante>> GetSinProyectoAsync();

        /// <summary>
        /// Crea un nuevo estudiante.
        /// </summary>
        Task<int> CreateAsync(Estudiante estudiante);

        /// <summary>
        /// Actualiza un estudiante existente.
        /// </summary>
        Task<bool> UpdateAsync(Estudiante estudiante);

        /// <summary>
        /// Elimina un estudiante por su identificador.
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Asigna un estudiante a un proyecto.
        /// </summary>
        Task<bool> AsignarAProyectoAsync(int estudianteId, int proyectoId);
    }
}