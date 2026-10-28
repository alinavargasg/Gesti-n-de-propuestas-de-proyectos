using AuthenticationModule.Models.Projects;

namespace AuthenticationModule.Services.Projects;

/// <summary>
/// Interfaz del repositorio de docentes.
/// </summary>
public interface IDocenteRepository
{
    /// <summary>Obtiene todos los docentes.</summary>
    Task<List<Docente>> GetAllAsync();

    /// <summary>Obtiene un docente por su Id.</summary>
    Task<Docente> GetByIdAsync(int id);

    /// <summary>Crea un nuevo docente.</summary>
    Task<int> CreateAsync(Docente docente);

    /// <summary>Actualiza un docente existente.</summary>
    Task<bool> UpdateAsync(Docente docente);

    /// <summary>Elimina un docente.</summary>
    Task<bool> DeleteAsync(int id);
}