namespace AuthenticationModule.Services.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando se viola una regla de negocio.
    /// Ejemplo: asignar más estudiantes de los permitidos a un proyecto.
    /// </summary>
    public class BusinessException : Exception
    {
        /// <summary>
        /// Inicializa una nueva instancia de <see cref="BusinessException"/>.
        /// </summary>
        /// <param name="message">Mensaje descriptivo del error de negocio.</param>
        public BusinessException(string message) : base(message) { }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="BusinessException"/> con una excepción interna.
        /// </summary>
        /// <param name="message">Mensaje descriptivo del error de negocio.</param>
        /// <param name="innerException">Excepción interna que causó el error.</param>
        public BusinessException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}