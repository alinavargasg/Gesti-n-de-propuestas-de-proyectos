using AuthenticationModule.DTOs;
using AuthenticationModule.Models.Projects;
using AuthenticationModule.Services.Exceptions;

namespace AuthenticationModule.Services.Projects
{
    public class ProyectoService : IProyectoService
    {
        private readonly IProyectoRepository _proyectoRepository;
        private readonly IEstudianteRepository _estudianteRepository;
        private readonly IEvaluacionRepository _evaluacionRepository;
        private readonly ILogger<ProyectoService> _logger;

        public ProyectoService(
            IProyectoRepository proyectoRepository,
            IEstudianteRepository estudianteRepository,
            IEvaluacionRepository evaluacionRepository,
            ILogger<ProyectoService> logger)
        {
            _proyectoRepository = proyectoRepository;
            _estudianteRepository = estudianteRepository;
            _evaluacionRepository = evaluacionRepository;
            _logger = logger;
        }

        public async Task<List<Proyecto>> GetAllAsync()
        {
            return await _proyectoRepository.GetAllAsync();
        }

        public async Task<ProyectoDetailsDTO> GetDetailsAsync(int id)
        {
            var proyecto = await _proyectoRepository.GetByIdAsync(id);
            if (proyecto == null)
                throw new NotFoundException("Proyecto", id);

            var estudiantes = await _estudianteRepository.GetByProyectoIdAsync(id);
            var evaluaciones = await _evaluacionRepository.GetByProyectoIdAsync(id);

            // Se arma el DTO con los datos comunes
            var dto = new ProyectoDetailsDTO
            {
                Id = proyecto.Id,
                Titulo = proyecto.Titulo,
                Problematica = proyecto.Problematica,
                ObjetivoGeneral = proyecto.ObjetivoGeneral,
                DescripcionPrototipo = proyecto.DescripcionPrototipo,
                RecursosTecnologias = proyecto.RecursosTecnologias,

                // Asignamos el tipo que calculamos en el Repositorio
                TipoProyecto = proyecto.TipoProyecto,

                CreatedAt = proyecto.CreatedAt,
                Estudiantes = estudiantes.Select(e => new EstudianteDTO
                {
                    Id = e.Id,
                    NombreCompleto = $"{e.Nombre} {e.ApellidoPaterno} {e.ApellidoMaterno}",
                    Curso = e.Curso
                }).ToList(),

                Evaluaciones = evaluaciones.Select(ev => new EvaluacionDTO
                {
                    Id = ev.Id,
                    DocenteNombre = ev.Docente != null
                                ? $"{ev.Docente.Nombre} {ev.Docente.ApellidoPaterno}"
                                : "Sin docente",
                    Observaciones = ev.Observaciones,
                    Recomendaciones = ev.Recomendaciones,
                    Nota = ev.Nota,
                    FechaEvaluacion = ev.FechaEvaluacion
                }).ToList(),
            };

            // ASIGNACIÓN POLIMÓRFICA (Datos específicos)
            if (proyecto is ProyectoSociocomunitario socio)
            {
                dto.Beneficiarios = socio.Beneficiarios;
                dto.UbicacionGeografica = socio.UbicacionGeografica;
                dto.ActoresInvolucrados = socio.ActoresInvolucrados;
                dto.ImpactoSocialEsperado = socio.ImpactoSocialEsperado;
                dto.EstrategiasSostenibilidad = socio.EstrategiasSostenibilidad;
            }
            else if (proyecto is ProyectoEmprendimiento empr)
            {
                dto.SegmentoMercado = empr.SegmentoMercado;
                dto.PropuestaValor = empr.PropuestaValor;
                dto.AnalisisCompetencia = empr.AnalisisCompetencia;
                dto.ModeloIngresos = empr.ModeloIngresos;
                dto.EstimacionInversionInicial = empr.EstimacionInversionInicial;
                dto.ViabilidadEconomica = empr.ViabilidadEconomica;
                dto.EstrategiaComercializacion = empr.EstrategiaComercializacion;
            }

            return dto;
        }

        public async Task<int> CreateAsync(Proyecto proyecto)
        {
            if (proyecto == null) throw new BusinessException("El proyecto es nulo.");
            return await _proyectoRepository.CreateAsync(proyecto);
        }

        public async Task<bool> UpdateAsync(Proyecto proyecto)
        {
            if (proyecto == null) throw new BusinessException("El proyecto es nulo.");
            return await _proyectoRepository.UpdateAsync(proyecto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _proyectoRepository.DeleteAsync(id);
        }

        public async Task<bool> AsignarEstudianteAsync(int proyectoId, int estudianteId)
        {
            return await _proyectoRepository.AsignarEstudianteAsync(proyectoId, estudianteId);
        }

        public async Task<ValidationResult> ValidarCantidadEstudiantesAsync(int proyectoId)
        {
            var proyecto = await _proyectoRepository.GetByIdAsync(proyectoId);
            if (proyecto == null) throw new NotFoundException("Proyecto", proyectoId);

            var estudiantes = await _estudianteRepository.GetByProyectoIdAsync(proyectoId);
            int cantidad = estudiantes.Count;

            // ✅ CORRECCIÓN: Obtenemos las reglas evaluando la instancia
            var (min, max) = proyecto switch
            {
                ProyectoSociocomunitario => (2, 3),
                ProyectoEmprendimiento => (1, 3),
                _ => throw new BusinessException("Tipo de proyecto no válido")
            };

            var result = new ValidationResult
            {
                CantidadActual = cantidad,
                Minimo = min,
                Maximo = max,
                IsValid = cantidad >= min && cantidad <= max
            };

            if (!result.IsValid)
            {
                result.Message = cantidad < min
                    ? $"Faltan estudiantes. Mínimo requerido: {min}."
                    : $"Excede el límite. Máximo permitido: {max}.";
            }

            return result;
        }

        public async Task<List<ReporteProyectoDTO>> GetReporteProyectosAsync(ReporteFiltrosDTO filtros)
        {
            var proyectos = await _proyectoRepository.GetAllAsync();
            var reporte = new List<ReporteProyectoDTO>();

            foreach (var p in proyectos)
            {
                // ✅ CORRECCIÓN: Filtro basado en la instancia de la clase
                if (filtros.TipoProyecto.HasValue)
                {
                    if (filtros.TipoProyecto.Value == 1 && p is not ProyectoSociocomunitario) continue;
                    if (filtros.TipoProyecto.Value == 2 && p is not ProyectoEmprendimiento) continue;
                }

                if (filtros.FechaDesde.HasValue && p.CreatedAt < filtros.FechaDesde.Value)
                    continue;
                if (filtros.FechaHasta.HasValue && p.CreatedAt > filtros.FechaHasta.Value)
                    continue;

                var estudiantes = await _estudianteRepository.GetByProyectoIdAsync(p.Id);
                var evaluaciones = await _evaluacionRepository.GetByProyectoIdAsync(p.Id);

                decimal? promedio = null;
                if (evaluaciones.Any(e => e.Nota.HasValue))
                {
                    promedio = evaluaciones.Where(e => e.Nota.HasValue).Average(e => e.Nota.Value);
                }

                var item = new ReporteProyectoDTO
                {
                    Id = p.Id,
                    Titulo = p.Titulo,
                    // ✅ CORRECCIÓN: Tipo inferido por la instancia
                    TipoProyecto = (p is ProyectoSociocomunitario) ? "Sociocomunitario" : "Emprendimiento",
                    FechaCreacion = p.CreatedAt,
                    ObjetivoGeneral = p.ObjetivoGeneral,
                    CantidadEstudiantes = estudiantes.Count,
                    EstudiantesAsignados = string.Join(", ", estudiantes.Select(e => e.Nombre)),
                    PromedioNotas = promedio
                };

                if (p is ProyectoSociocomunitario socio)
                {
                    item.PublicoObjetivo = socio.Beneficiarios;
                    item.UbicacionOModelo = socio.UbicacionGeografica;
                }
                else if (p is ProyectoEmprendimiento empr)
                {
                    item.PublicoObjetivo = empr.SegmentoMercado;
                    item.UbicacionOModelo = empr.ModeloIngresos;
                }

                reporte.Add(item);
            }

            return reporte;
        }
    }
}