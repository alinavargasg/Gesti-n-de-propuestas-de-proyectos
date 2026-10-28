using AuthenticationModule.Models.Projects;
using AuthenticationModule.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationModule.Data
{
    // AuthDbContext hereda de DbContext (herencia)
    public class AuthDbContext : DbContext
    {
        // Constructor que recibe las opciones de configuración
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options) { }

        // DbSet representa una tabla en la base de datos
        // Al usar DbSet, Entity Framework mapea automáticamente
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Proyecto> Proyectos { get; set; }
        public DbSet<ProyectoSociocomunitario> ProyectosSociocomunitarios { get; set; }
        public DbSet<ProyectoEmprendimiento> ProyectosEmprendimientos { get; set; }
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Docente> Docentes { get; set; }
        public DbSet<Evaluacion> Evaluaciones { get; set; }

        // Este método se ejecuta cuando el modelo se está construyendo
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de índices para mejorar rendimiento
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)      // ← Crear índice en Username
                .IsUnique();                    // ← Asegurar que sea único

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // 👇 AGREGAR LA CONFIGURACIÓN DE LA RELACIÓN 1 A 1
            modelBuilder.Entity<User>()
                .HasOne(u => u.Docente)
                .WithOne() // Un docente tiene un solo usuario
                .HasForeignKey<User>(u => u.DocenteId)
                .OnDelete(DeleteBehavior.SetNull); // Si se borra el docente, el usuario no se borra, solo pierde el enlace

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();
            // Configuración explícita de TPT (Table Per Type)
            modelBuilder.Entity<Proyecto>().ToTable("proyectos");
            modelBuilder.Entity<ProyectoSociocomunitario>().ToTable("proyectos_sociocomunitarios");
            modelBuilder.Entity<ProyectoEmprendimiento>().ToTable("proyectos_emprendimientos");
            modelBuilder.Entity<Estudiante>()
                .HasIndex(u => u.Id)
                .IsUnique();
            modelBuilder.Entity<Docente>()
                .HasIndex(u => u.Id)
                .IsUnique();
            modelBuilder.Entity<Evaluacion>()
                .HasIndex(u => u.Id) 
                .IsUnique();

            // === 🆕 Configuraciones del módulo de proyectos ===

            // Configuración explícita de TPT (Table Per Type)
            // Esto le dice a EF Core que use estas tablas para cada clase
            modelBuilder.Entity<Proyecto>().ToTable("proyectos");
            modelBuilder.Entity<ProyectoSociocomunitario>().ToTable("proyectos_sociocomunitarios");
            modelBuilder.Entity<ProyectoEmprendimiento>().ToTable("proyectos_emprendimientos");

            // Un proyecto puede tener 1-3 estudiantes
            modelBuilder.Entity<Estudiante>()
                .HasOne(e => e.Proyecto)
                .WithMany(p => p.Estudiantes)
                .HasForeignKey(e => e.ProyectoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relación N:M entre Proyecto y Docente mediante Evaluacion
            modelBuilder.Entity<Evaluacion>()
                .HasOne(ev => ev.Proyecto)
                .WithMany(p => p.Evaluaciones)
                .HasForeignKey(ev => ev.ProyectoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Evaluacion>()
                .HasOne(ev => ev.Docente)
                .WithMany(d => d.Evaluaciones)
                .HasForeignKey(ev => ev.DocenteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Insertar datos iniciales (seed data)
            SeedData(modelBuilder);
        }

        // Método para cargar datos iniciales
        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = 1,
                    Name = "Admin",
                    Description = "Administrator with full access",
                    Permissions = "{\"all\": true}"
                },
                new Role
                {
                    Id = 2,
                    Name = "User",
                    Description = "Standard user with limited access",
                    Permissions = "{\"read\": true, \"write\": false}"
                },
                new Role
                {
                    Id = 3,
                    Name = "Guest",
                    Description = "Guest user with read-only access",
                    Permissions = "{\"read\": true, \"write\": false, \"delete\": false}"
                }
            );

            modelBuilder.Entity<User>().HasData(
              
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@authmodule.local",
                    PasswordHash = "a5j+a55i0n894t53qaxb3Ziupf0hWUFsGIwhPTcF1DY=",
                    PasswordSalt = "meESooV31rXnzcHS4eRDlA==",
                    RoleId = 1,  // ← Rol Admin
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    FailedLoginAttempts = 0
                },
                new User
                {
                    Id = 2,
                    Username = "user",
                    Email = "user@authmodule.local",
                    PasswordHash = "40nIGT0qFiytP95YSZtCFNY1DjZJLXMOh92MQ/hcbKI=",
                    PasswordSalt = "x0F1Y4kGuD1kd7ciNaW+RQ==",
                    RoleId = 2,  // ← Rol User
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    FailedLoginAttempts = 0
                }
            );
        }
    }
}