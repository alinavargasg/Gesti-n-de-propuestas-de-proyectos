using AuthenticationModule.Models.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthenticationModule.Data
{
    /// <summary>
    /// Clase encargada de sembrar datos de prueba para el módulo de proyectos.
    /// </summary>
    public static class SeedDataProyectos
    {
        public static async Task SeedProyectosAsync(AuthDbContext context, ILogger logger)
        {
            try
            {
                // Verificar si ya existen datos para evitar duplicados
                if (await context.Proyectos.AnyAsync())
                {
                    logger.LogInformation("Los datos de proyectos ya existen. Se omite el seed.");
                    return;
                }

                logger.LogInformation("Iniciando seed de datos de proyectos...");

                // ============================================================
                // 1. CREAR DOCENTES
                // ============================================================
                var docentes = new List<Docente>
                {
                    new Docente { Nombre = "Juan Carlos", ApellidoPaterno = "Pérez", ApellidoMaterno = "Gómez", Especialidad = "Ingeniería de Software" },
                    new Docente { Nombre = "María Elena", ApellidoPaterno = "Rodríguez", ApellidoMaterno = "López", Especialidad = "Gestión de Proyectos" },
                    new Docente { Nombre = "Carlos Andrés", ApellidoPaterno = "Mamani", ApellidoMaterno = "Quispe", Especialidad = "Emprendimiento e Innovación" }
                };

                await context.Docentes.AddRangeAsync(docentes);
                await context.SaveChangesAsync();
                logger.LogInformation("Se crearon {Count} docentes", docentes.Count);

                // ============================================================
                // 2. CREAR ESTUDIANTES (17 en total: 9 para proyectos, 8 libres)
                // ============================================================
                var estudiantes = new List<Estudiante>
                {
                    // Asignados a proyectos
                    new Estudiante { Nombre = "Ana Belén", ApellidoPaterno = "Flores", ApellidoMaterno = "Vargas", Curso = "3er Año A" },
                    new Estudiante { Nombre = "Luis Fernando", ApellidoPaterno = "Chávez", ApellidoMaterno = "Rojas", Curso = "3er Año A" },
                    new Estudiante { Nombre = "Carla Sofía", ApellidoPaterno = "Gutiérrez", ApellidoMaterno = "Mendoza", Curso = "3er Año A" },
                    new Estudiante { Nombre = "Diego Alejandro", ApellidoPaterno = "Herrera", ApellidoMaterno = "Castro", Curso = "3er Año A" },
                    new Estudiante { Nombre = "Valeria Nicole", ApellidoPaterno = "Jiménez", ApellidoMaterno = "Ortiz", Curso = "3er Año A" },
                    new Estudiante { Nombre = "Sebastián Mateo", ApellidoPaterno = "Romero", ApellidoMaterno = "Silva", Curso = "3er Año A" },
                    new Estudiante { Nombre = "Camila Andrea", ApellidoPaterno = "Torres", ApellidoMaterno = "Navarro", Curso = "3er Año A" },
                    new Estudiante { Nombre = "Gabriel Ignacio", ApellidoPaterno = "Vargas", ApellidoMaterno = "Peña", Curso = "3er Año A" },
                    new Estudiante { Nombre = "Isabella Fernanda", ApellidoPaterno = "Morales", ApellidoMaterno = "Ríos", Curso = "3er Año A" },
                    
                    // Libres (sin proyecto)
                    new Estudiante { Nombre = "Mateo", ApellidoPaterno = "García", ApellidoMaterno = "Fernández", Curso = "1er Año A" },
                    new Estudiante { Nombre = "Lucía", ApellidoPaterno = "Martínez", ApellidoMaterno = "Sánchez", Curso = "1er Año A" },
                    new Estudiante { Nombre = "Santiago", ApellidoPaterno = "López", ApellidoMaterno = "Pérez", Curso = "1er Año B" },
                    new Estudiante { Nombre = "Valentina", ApellidoPaterno = "Gómez", ApellidoMaterno = "Díaz", Curso = "1er Año B" },
                    new Estudiante { Nombre = "Sebastián", ApellidoPaterno = "Ruiz", ApellidoMaterno = "Romero", Curso = "2do Año A" },
                    new Estudiante { Nombre = "Camila", ApellidoPaterno = "Hernández", ApellidoMaterno = "Torres", Curso = "2do Año A" },
                    new Estudiante { Nombre = "Alejandro", ApellidoPaterno = "Díaz", ApellidoMaterno = "Ramírez", Curso = "2do Año B" },
                    new Estudiante { Nombre = "Mariana", ApellidoPaterno = "Álvarez", ApellidoMaterno = "Jiménez", Curso = "2do Año B" }
                };

                await context.Estudiantes.AddRangeAsync(estudiantes);
                await context.SaveChangesAsync();
                logger.LogInformation("Se crearon {Count} estudiantes", estudiantes.Count);

                // ============================================================
                // 3. CREAR PROYECTOS SOCIOCOMUNITARIOS (3)
                // ============================================================
                var socio1 = new ProyectoSociocomunitario
                {
                    Titulo = "Sistema de Riego Comunitario en la Zona Sur",
                    Problematica = "La comunidad de la zona sur enfrenta escasez de agua para sus cultivos, lo que afecta la producción agrícola.",
                    ObjetivoGeneral = "Implementar un sistema de riego automatizado que optimice el uso del agua y mejore la producción agrícola de la zona.",
                    DescripcionPrototipo = "Sistema de riego por goteo con sensores de humedad y control automático mediante Arduino y panel solar.",
                    RecursosTecnologias = "Arduino, sensores de humedad, panel solar, válvulas solenoides, tuberías PVC",
                    CreatedAt = DateTime.UtcNow,
                    Beneficiarios = "50 familias productoras de la zona sur",
                    UbicacionGeografica = "Zona Sur, Comunidad de Huayllani",
                    ActoresInvolucrados = "Gobierno Municipal, Asociación de Regantes, Universidad, Comunidad",
                    ImpactoSocialEsperado = "Mejora del 40% en la producción agrícola y reducción del 30% en el consumo de agua",
                    EstrategiasSostenibilidad = "Capacitación a los comunarios, mantenimiento comunitario, fondo de reserva"
                };

                var socio2 = new ProyectoSociocomunitario
                {
                    Titulo = "Centro de Reciclaje Comunitario",
                    Problematica = "La comunidad genera grandes cantidades de residuos sólidos sin un manejo adecuado.",
                    ObjetivoGeneral = "Establecer un centro de reciclaje comunitario que reduzca la contaminación y genere ingresos.",
                    DescripcionPrototipo = "Planta de clasificación de residuos con contenedores diferenciados y prensa hidráulica.",
                    RecursosTecnologias = "Contenedores diferenciados, prensa hidráulica, báscula industrial, software de gestión",
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    Beneficiarios = "200 familias del barrio San Antonio",
                    UbicacionGeografica = "Barrio San Antonio, Calle 12",
                    ActoresInvolucrados = "Junta Vecinal, Alcaldía, Empresas recicladoras",
                    ImpactoSocialEsperado = "Reducción del 60% de residuos sólidos y generación de 5 empleos directos",
                    EstrategiasSostenibilidad = "Venta de materiales reciclados, convenios con empresas, educación ambiental"
                };

                var socio3 = new ProyectoSociocomunitario
                {
                    Titulo = "Biblioteca Digital Comunitaria",
                    Problematica = "Los estudiantes de la comunidad no tienen acceso a material bibliográfico actualizado.",
                    ObjetivoGeneral = "Crear una biblioteca digital accesible para todos los estudiantes de la comunidad.",
                    DescripcionPrototipo = "Plataforma web con repositorio de libros digitales y acceso mediante tablets.",
                    RecursosTecnologias = "Servidor web, tablets, software de gestión bibliotecaria, conexión a internet",
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    Beneficiarios = "150 estudiantes de primaria y secundaria",
                    UbicacionGeografica = "Comunidad de Villa Adela",
                    ActoresInvolucrados = "Dirección de Educación, Universidad, Padres de familia",
                    ImpactoSocialEsperado = "Mejora del rendimiento académico en un 25% y reducción de la deserción escolar",
                    EstrategiasSostenibilidad = "Donaciones de libros, convenios con editoriales, voluntariado universitario"
                };

                await context.Proyectos.AddRangeAsync(socio1, socio2, socio3);
                await context.SaveChangesAsync();
                logger.LogInformation("Se crearon 3 proyectos sociocomunitarios");

                // ============================================================
                // 4. CREAR PROYECTOS DE EMPRENDIMIENTO (3)
                // ============================================================
                var empr1 = new ProyectoEmprendimiento
                {
                    Titulo = "App de Delivery para Restaurantes Locales",
                    Problematica = "Los restaurantes locales no tienen presencia digital y pierden clientes.",
                    ObjetivoGeneral = "Desarrollar una aplicación móvil de delivery que conecte restaurantes locales con clientes.",
                    DescripcionPrototipo = "Aplicación móvil con registro de restaurantes, menú digital, pedidos y seguimiento.",
                    RecursosTecnologias = "Flutter, Firebase, Google Maps API, pasarela de pagos",
                    CreatedAt = DateTime.UtcNow,
                    SegmentoMercado = "Restaurantes locales y clientes de 20-45 años en zonas urbanas",
                    PropuestaValor = "Comisión reducida (10% vs 30% de competidores) y soporte personalizado",
                    AnalisisCompetencia = "Competidores: PedidosYa, Glovo. Diferenciación: comisión reducida",
                    ModeloIngresos = "Comisión del 10% por pedido + publicidad destacada",
                    EstimacionInversionInicial = 15000,
                    ViabilidadEconomica = "Punto de equilibrio en 8 meses con 500 pedidos diarios.",
                    EstrategiaComercializacion = "Alianzas con cámaras de restaurantes, marketing digital, referidos"
                };

                var empr2 = new ProyectoEmprendimiento
                {
                    Titulo = "Tienda Virtual de Café Boliviano",
                    Problematica = "Los productores de café boliviano no tienen canales directos de comercialización.",
                    ObjetivoGeneral = "Crear una tienda virtual que conecte productores de café con consumidores.",
                    DescripcionPrototipo = "E-commerce con catálogo de cafés, suscripciones mensuales y envío a domicilio.",
                    RecursosTecnologias = "Shopify, pasarela de pagos, logística de envíos",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    SegmentoMercado = "Amantes del café de 25-50 años, cafeterías especializadas",
                    PropuestaValor = "Café de especialidad boliviano con trazabilidad directa del productor",
                    AnalisisCompetencia = "Competidores: tiendas físicas y marketplaces. Diferenciación: origen directo",
                    ModeloIngresos = "Venta directa + suscripciones mensuales + ventas al por mayor",
                    EstimacionInversionInicial = 8000,
                    ViabilidadEconomica = "Margen del 35% por venta. Punto de equilibrio en 6 meses.",
                    EstrategiaComercializacion = "Redes sociales, influencers, ferias de café, alianzas con cafeterías"
                };

                var empr3 = new ProyectoEmprendimiento
                {
                    Titulo = "Plataforma de Tutorías Académicas Online",
                    Problematica = "Los estudiantes tienen dificultades para encontrar tutores calificados.",
                    ObjetivoGeneral = "Crear una plataforma que conecte estudiantes con tutores especializados.",
                    DescripcionPrototipo = "Plataforma web con perfiles de tutores, sistema de reservas y videollamadas.",
                    RecursosTecnologias = "ASP.NET Core, WebRTC, Stripe, PostgreSQL",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    SegmentoMercado = "Estudiantes universitarios y de secundaria de 15-25 años",
                    PropuestaValor = "Tutores verificados con calificaciones reales y precios accesibles",
                    AnalisisCompetencia = "Competidores: plataformas genéricas. Diferenciación: enfoque académico",
                    ModeloIngresos = "Comisión del 15% por sesión + suscripción premium",
                    EstimacionInversionInicial = 12000,
                    ViabilidadEconomica = "Punto de equilibrio en 10 meses con 300 sesiones mensuales.",
                    EstrategiaComercializacion = "Alianzas con universidades, marketing en redes, referidos"
                };

                await context.Proyectos.AddRangeAsync(empr1, empr2, empr3);
                await context.SaveChangesAsync();
                logger.LogInformation("Se crearon 3 proyectos de emprendimiento");

                // ============================================================
                // 5. ASIGNAR ESTUDIANTES A PROYECTOS
                // ============================================================

                // Volvemos a consultar los estudiantes para que EF Core los rastree en este contexto
                var estudiantesDb = await context.Estudiantes.OrderBy(e => e.Id).ToListAsync();

                // Sociocomunitario 1 (2 estudiantes)
                estudiantesDb[0].ProyectoId = socio1.Id;
                estudiantesDb[1].ProyectoId = socio1.Id;

                // Sociocomunitario 2 (3 estudiantes)
                estudiantesDb[2].ProyectoId = socio2.Id;
                estudiantesDb[3].ProyectoId = socio2.Id;
                estudiantesDb[4].ProyectoId = socio2.Id;

                // Sociocomunitario 3 (2 estudiantes)
                estudiantesDb[5].ProyectoId = socio3.Id;
                estudiantesDb[6].ProyectoId = socio3.Id;

                // Emprendimiento 1 (1 estudiante)
                estudiantesDb[7].ProyectoId = empr1.Id;

                // Emprendimiento 2 (1 estudiante)
                estudiantesDb[8].ProyectoId = empr2.Id;

                // Emprendimiento 3 (0 estudiantes) y los índices del 9 en adelante quedan libres

                await context.SaveChangesAsync();
                logger.LogInformation("Se asignaron 9 estudiantes a los proyectos");

                // ============================================================
                // 6. CREAR EVALUACIONES
                // ============================================================
                var evaluaciones = new List<Evaluacion>
                {
                    new Evaluacion { ProyectoId = socio1.Id, DocenteId = docentes[0].Id, Observaciones = "El proyecto aborda una problemática real. Se recomienda detallar el presupuesto.", Recomendaciones = "Incluir un plan de mantenimiento a largo plazo.", FechaEvaluacion = DateTime.UtcNow.AddDays(-5), Nota = 85 },
                    new Evaluacion { ProyectoId = socio1.Id, DocenteId = docentes[1].Id, Observaciones = "Buena identificación de beneficiarios. El impacto social está bien definido.", Recomendaciones = "Fortalecer la estrategia de sostenibilidad con indicadores medibles.", FechaEvaluacion = DateTime.UtcNow.AddDays(-3), Nota = 88 },
                    new Evaluacion { ProyectoId = socio2.Id, DocenteId = docentes[1].Id, Observaciones = "Excelente enfoque ambiental. La propuesta es pertinente.", Recomendaciones = "Considerar la normativa ambiental vigente y obtener permisos.", FechaEvaluacion = DateTime.UtcNow.AddDays(-8), Nota = 90 },
                    new Evaluacion { ProyectoId = socio3.Id, DocenteId = docentes[0].Id, Observaciones = "El proyecto tiene un alto impacto educativo.", Recomendaciones = "Definir mejor los derechos de autor de los libros digitales.", FechaEvaluacion = DateTime.UtcNow.AddDays(-12), Nota = 82 },
                    new Evaluacion { ProyectoId = empr1.Id, DocenteId = docentes[2].Id, Observaciones = "El modelo de negocio es viable y el análisis de competencia es acertado.", Recomendaciones = "Realizar un estudio de mercado más profundo antes de invertir.", FechaEvaluacion = DateTime.UtcNow.AddDays(-2), Nota = 87 },
                    new Evaluacion { ProyectoId = empr2.Id, DocenteId = docentes[2].Id, Observaciones = "La propuesta de valor es clara y el segmento está bien identificado.", Recomendaciones = "Validar la logística de envíos a nivel internacional.", FechaEvaluacion = DateTime.UtcNow.AddDays(-7), Nota = 85 }
                };

                await context.Evaluaciones.AddRangeAsync(evaluaciones);
                await context.SaveChangesAsync();
                logger.LogInformation("Se crearon {Count} evaluaciones", evaluaciones.Count);

                logger.LogInformation("Seed de proyectos completado exitosamente");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al sembrar datos de proyectos");
                throw;
            }
        }
    }
}