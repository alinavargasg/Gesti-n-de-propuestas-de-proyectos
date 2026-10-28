namespace AuthenticationModule.Services.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando una entidad no se encuentra en la base de datos.
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Inicializa una nueva instancia de <see cref="NotFoundException"/>.
        /// </summary>
        /// <param name="entityName">Nombre de la entidad no encontrada.</param>
        /// <param name="id">Identificador de la entidad.</param>
        public NotFoundException(string entityName, int id)
            : base($"No se encontró {entityName} con Id = {id}") { }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="NotFoundException"/> con un mensaje personalizado.
        /// </summary>
        /// <param name="message">Mensaje descriptivo del error.</param>
        public NotFoundException(string message) : base(message) { }
    }
}