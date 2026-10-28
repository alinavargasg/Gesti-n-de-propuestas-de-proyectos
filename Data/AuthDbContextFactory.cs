using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


namespace AuthenticationModule.Data
{
    /// <summary>
    /// Fábrica de DbContext para tiempo de diseño.
    /// Permite a las herramientas de EF Core crear el contexto
    /// sin necesidad de ejecutar la aplicación completa.
    /// </summary>
    public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
    {
        public AuthDbContext CreateDbContext(string[] args)
        {
            // 1. Configurar la lectura del archivo appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // 2. Obtener la cadena de conexión
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // 3. Configurar las opciones del DbContext
            var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            // 4. Crear y retornar el contexto
            return new AuthDbContext(optionsBuilder.Options);
        }
    }
}