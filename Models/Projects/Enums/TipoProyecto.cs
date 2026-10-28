namespace AuthenticationModule.Models.Projects.Enums
{
    /// <summary>
    /// Representa el tipo de proyecto en el sistema.
    /// 
    /// En TPT, este enum NO se persiste como columna en la base de datos.
    /// Se utiliza en la lógica de aplicación para:
    /// - Identificar el tipo de proyecto en ViewModels y DTOs.
    /// - Determinar qué tabla derivada consultar.
    /// - Aplicar reglas de negocio específicas por tipo.
    /// </summary>
    public enum TipoProyecto
    {
        /// <summary>
        /// Proyecto sociocomunitario productivo.
        /// Requiere 2-3 estudiantes.
        /// </summary>
        SocioComunitario = 1,

        /// <summary>
        /// Proyecto de emprendimiento productivo.
        /// Requiere 1-3 estudiantes.
        /// </summary>
        Emprendimiento = 2
    }
}
