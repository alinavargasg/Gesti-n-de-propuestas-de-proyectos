namespace AuthenticationModule.Middleware
{
    public class SecurityHeadersMiddleware
    {
        // Delegate para el siguiente middleware en el pipeline
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Método InvokeAsync se ejecuta para cada solicitud
        public async Task InvokeAsync(HttpContext context)
        {
            // === HEADERS DE SEGURIDAD ===

            // 1. Prevenir MIME sniffing
            // Evita que el navegador interprete archivos como un tipo MIME diferente
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

            // 2. Prevenir Clickjacking
            // Evita que la página sea mostrada en un iframe (ataques de "UI Redress")
            context.Response.Headers.Add("X-Frame-Options", "DENY");

            // 3. Protección XSS (navegadores antiguos)
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

            // 4. HTTPS Strict Transport Security
            // Fuerza el uso de HTTPS
            if (context.Request.IsHttps)
            {
                context.Response.Headers.Add("Strict-Transport-Security",
                    "max-age=31536000; includeSubDomains");
            }

            // 5. Referrer Policy
            // Controla cuánta información de referencia se envía
            context.Response.Headers.Add("Referrer-Policy",
                "strict-origin-when-cross-origin");

            // 6. Permissions Policy
            // Restringe características del navegador
            context.Response.Headers.Add("Permissions-Policy",
                "geolocation=(), microphone=(), camera=()");

            // 7. Content Security Policy (CSP)
            // La más importante - Controla qué recursos puede cargar la página
            context.Response.Headers.Add("Content-Security-Policy",
                "default-src 'self'; " +                    // Solo recursos locales
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; " +
                "font-src 'self' data:; " +
                "connect-src 'self';");

            // Continuar al siguiente middleware
            await _next(context);
        }
    }
}