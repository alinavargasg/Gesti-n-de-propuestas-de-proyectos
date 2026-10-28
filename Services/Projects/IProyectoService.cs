using AuthenticationModule.DTOs;
using AuthenticationModule.Models.Projects;

namespace AuthenticationModule.Services.Projects
{
    public interface IProyectoService
    {
        Task<List<Proyecto>> GetAllAsync();
        Task<ProyectoDetailsDTO> GetDetailsAsync(int id);
        Task<int> CreateAsync(Proyecto proyecto);
        Task<bool> UpdateAsync(Proyecto proyecto);
        Task<bool> DeleteAsync(int id);

        // Asignación y validaciones
        Task<bool> AsignarEstudianteAsync(int proyectoId, int estudianteId);
        Task<ValidationResult> ValidarCantidadEstudiantesAsync(int proyectoId);

        // Preparación para reportes
        Task<List<ReporteProyectoDTO>> GetReporteProyectosAsync(ReporteFiltrosDTO filtros);
    }
}