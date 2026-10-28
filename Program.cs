using AuthenticationModule.Data;
using AuthenticationModule.Decorators;
using AuthenticationModule.Middleware;
using AuthenticationModule.Models.Auth;
using AuthenticationModule.Security;
using AuthenticationModule.Services;
using AuthenticationModule.Services.Auth;
using AuthenticationModule.Services.Projects;
using Dapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
DefaultTypeMap.MatchNamesWithUnderscores = true;

// === 1. CONFIGURACIÓN DE SERVICIOS ===

// DbContext - Conexión a PostgreSQL
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositorio - Acceso a datos
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Servicio de autenticación - Lógica de negocio
builder.Services.AddScoped<IAuthService, AuthService>();

// Decorator de logging - Auditoría de eventos
builder.Services.AddScoped<ILoggingService, LoggingDecorator>();

builder.Services.AddScoped<IProyectoRepository, ProyectoRepository>();

builder.Services.AddScoped<IEstudianteRepository, EstudianteRepository>();
builder.Services.AddScoped<IDocenteRepository, DocenteRepository>();
builder.Services.AddScoped<IEvaluacionRepository, EvaluacionRepository>();

builder.Services.AddScoped<IProyectoService, ProyectoService>();

// HttpContextAccessor - Necesario para IP y cookies
builder.Services.AddHttpContextAccessor();

// === 2. AUTENTICACIÓN CON COOKIES ===
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Account/Login";
        options.LogoutPath = "/Auth/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.Name = "AuthCookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

// === 3. AUTORIZACIÓN POR POLÍTICAS ===
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("UserOnly", policy =>
        policy.RequireRole("User", "Admin"));
});

// === 4. RAZOR PAGES CON CONVENCIONES ===
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AuthorizePage("/Admin/Users", "AdminOnly");
    });

// === 5. HSTS - HTTP STRICT TRANSPORT SECURITY ===
builder.Services.Configure<HstsOptions>(options =>
{
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

var app = builder.Build();

// === 6. PIPELINE DE MIDDLEWARE ===

// HSTS solo en producción
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Redirigir HTTP → HTTPS
app.UseHttpsRedirection();

// Headers de seguridad personalizados (CSP, X-Frame-Options, etc.)
app.UseMiddleware<SecurityHeadersMiddleware>();

// Archivos estáticos (CSS, JS, imágenes)
app.UseStaticFiles();

// Autenticación (¿quién es el usuario?)
app.UseAuthentication();

// Autorización (¿tiene permiso?)
app.UseAuthorization();

// Mapeo de Razor Pages
app.MapRazorPages();

// === 7. INICIALIZACIÓN DE BASE DE DATOS ===

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Aplicar migraciones (crea la BD si no existe)
    dbContext.Database.Migrate();
    try
    {
         // 1. Aplicar migraciones pendientes
        await dbContext.Database.MigrateAsync();

        // 2. Sembrar datos de proyectos (Docentes, Estudiantes, Proyectos)
        // Nota: Los usuarios Admin y User ya se crean por HasData en AuthDbContext
        await SeedDataProyectos.SeedProyectosAsync(dbContext, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");
        throw;
    }

    // === SEED PROGRAMÁTICO ===

    // Sembrar usuario admin
    if (!dbContext.Users.Any(u => u.Username == "admin"))
        {
            var (hash, salt) = PasswordHasher.HashPassword("Admin123!");

            dbContext.Users.Add(new User
            {
                Username = "admin",
                Email = "admin@authmodule.local",
                PasswordHash = hash,     // ✅ Generado en runtime
                PasswordSalt = salt,
                RoleId = 1,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                FailedLoginAttempts = 0
            });
        }

        // Sembrar usuario user
        /*if (!dbContext.Users.Any(u => u.Username == "user"))
        {
            var (hash, salt) = PasswordHasher.HashPassword("User123!");

            dbContext.Users.Add(new User
            {
                Username = "user",
                Email = "user@authmodule.local",
                PasswordHash = hash,
                PasswordSalt = salt,
                RoleId = 2,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                FailedLoginAttempts = 0
            });
        }*/

        dbContext.SaveChanges();
    }

// === 8. EJECUTAR APLICACIÓN ===
app.Run();