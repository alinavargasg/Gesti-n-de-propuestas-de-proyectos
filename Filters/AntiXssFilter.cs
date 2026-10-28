using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace AuthenticationModule.Filters
{
    // Filtro de acción - se ejecuta antes/después de cada acción
    public class AntiXssFilter : IActionFilter
    {
        private readonly HtmlEncoder _htmlEncoder;

        public AntiXssFilter(HtmlEncoder htmlEncoder)
        {
            _htmlEncoder = htmlEncoder;
        }

        // Se ejecuta ANTES de la acción
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Iterar sobre todos los parámetros de la acción
            foreach (var parameter in context.ActionArguments)
            {
                if (parameter.Value is string stringValue)
                {
                    // Sanitizar el input
                    stringValue = SanitizeInput(stringValue);
                    context.ActionArguments[parameter.Key] = stringValue;
                }
                else if (parameter.Value is IDictionary<string, object> dict)
                {
                    // Sanitizar diccionarios (formularios complejos)
                    SanitizeDictionary(dict);
                }
            }
        }

        // Se ejecuta DESPUÉS de la acción
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No necesitamos hacer nada después
        }

        /// <summary>
        /// Sanitiza el input eliminando posibles scripts maliciosos
        /// </summary>
        private string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // 1. HTML Encode - Convierte caracteres especiales a entidades HTML
            // Ejemplo: <script> → &lt;script&gt;
            input = _htmlEncoder.Encode(input);

            // 2. Patrones de XSS comunes
            var xssPatterns = new[]
            {
                @"<script.*?>.*?</script>",
                @"javascript:",
                @"onerror=",
                @"onload=",
                @"onclick=",
                @"eval\(",
                @"expression\("
            };

            // Eliminar patrones maliciosos con Regex
            foreach (var pattern in xssPatterns)
            {
                input = Regex.Replace(input, pattern, "", RegexOptions.IgnoreCase);
            }

            return input.Trim();
        }

        /// <summary>
        /// Sanitiza recursivamente un diccionario
        /// </summary>
        private void SanitizeDictionary(IDictionary<string, object> dict)
        {
            foreach (var key in dict.Keys.ToList())
            {
                if (dict[key] is string stringValue)
                {
                    dict[key] = SanitizeInput(stringValue);
                }
                else if (dict[key] is IDictionary<string, object> nestedDict)
                {
                    SanitizeDictionary(nestedDict);
                }
            }
        }
    }
}