using AuthenticationModule.Models.Projects;

namespace AuthenticationModule.Services.Projects
{
    /// <summary>
    /// Interfaz del repositorio de proyectos.
    /// Define las operaciones de acceso a datos para la entidad <see cref="Proyecto"/>.
    /// </summary>
    public interface IProyectoRepository
    {
        /// <summary>
        /// Obtiene todos los proyectos registrados.
        /// </summary>
        /// <returns>Lista de proyectos.</returns>
        Task<List<Proyecto>> GetAllAsync();

        /// <summary>
        /// Obtiene un proyecto por su identificador.
        /// </summary>
        /// <param name="id">Identificador del proyecto.</param>
        /// <returns>El proyecto encontrado o null.</returns>
        Task<Proyecto> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene todos los proyectos sociocomunitarios.
        /// </summary>
        /// <returns>Lista de proyectos sociocomunitarios.</returns>
        Task<List<ProyectoSociocomunitario>> GetSociocomunitariosAsync();

        /// <summary>
        /// Obtiene todos los proyectos de emprendimiento.
        /// </summary>
        /// <returns>Lista de proyectos de emprendimiento.</returns>
        Task<List<ProyectoEmprendimiento>> GetEmprendimientosAsync();

        /// <summary>
        /// Crea un nuevo proyecto en la base de datos.
        /// </summary>
        /// <param name="proyecto">Proyecto a crear.</param>
        /// <returns>Identificador del proyecto creado.</returns>
        Task<int> CreateAsync(Proyecto proyecto);

        /// <summary>
        /// Actualiza un proyecto existente.
        /// </summary>
        /// <param name="proyecto">Proyecto con los datos actualizados.</param>
        /// <returns>True si se actualizó correctamente.</returns>
        Task<bool> UpdateAsync(Proyecto proyecto);

        /// <summary>
        /// Elimina un proyecto por su identificador.
        /// </summary>
        /// <param name="id">Identificador del proyecto.</param>
        /// <returns>True si se eliminó correctamente.</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Asigna un estudiante a un proyecto.
        /// Valida que el proyecto no exceda la cantidad máxima de estudiantes según su tipo.
        /// </summary>
        /// <param name="proyectoId">Identificador del proyecto.</param>
        /// <param name="estudianteId">Identificador del estudiante.</param>
        /// <returns>True si se asignó correctamente.</returns>
        Task<bool> AsignarEstudianteAsync(int proyectoId, int estudianteId);

        /// <summary>
        /// Valida que un proyecto cumpla con la cantidad de estudiantes permitida según su tipo.
        /// </summary>
        /// <param name="proyectoId">Identificador del proyecto.</param>
        /// <returns>True si la cantidad es válida.</returns>
        Task<bool> ValidarCantidadEstudiantesAsync(int proyectoId);
    }
}