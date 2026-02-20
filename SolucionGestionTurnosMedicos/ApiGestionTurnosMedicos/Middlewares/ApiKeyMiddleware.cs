using System.Text.Json;

namespace ApiGestionTurnosMedicos.Middlewares
{
    /// <summary>
    /// Middleware para autenticar solicitudes HTTP verificando una clave API en los encabezados.
    /// Basado en https://muratsuzen.medium.com/using-api-key-authorization-with-middleware-and-attribute-on-asp-net-core-web-api-543a4955a0ef
    /// La parte de múltiples keys basada en https://stackoverflow.com/a/77021101
    /// </summary>
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ApiKeyHeader = "X-API-KEY"; 
        private const string AllowedKeysSection = "AllowedApiKeys"; // Varias keys en un array dentro de appsettings

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ApiKeyMiddleware"/>.
        /// </summary>
        /// <param name="requestDelegate">El delegado que representa el siguiente middleware en la canalización de procesamiento de solicitudes.</param>
        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Procesa la solicitud HTTP verificando la presencia y validez de una clave API en el encabezado 'X-API-KEY'.
        /// Si la clave no está presente o no es válida, devuelve un error 401 (No autorizado). Si es válida, pasa la solicitud al siguiente middleware.
        /// </summary>
        /// <param name="context">El contexto HTTP que contiene la solicitud y la respuesta.</param>
        /// <returns>Una tarea que representa la finalización del procesamiento de la solicitud.</returns>
        public async Task Invoke(HttpContext context)
        {
            // Dejar pasar preflight (CORS OPTIONS)
            if (context.Request.Method == HttpMethods.Options)
            {
                await _next(context);
                return;
            }

            // Verificar si viene la cabecera
            if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKeyVal))
            {
                //context.Response.StatusCode = 401;
                //await context.Response.WriteAsync("Api Key no encontrada!");

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new { Message = "Api Key no encontrada!" });
                await context.Response.WriteAsync(result);
                return;
            }

            // Obtener claves válidas desde appsettings
            var config = context.RequestServices.GetRequiredService<IConfiguration>();
            var allowedApiKeys = config.GetSection(AllowedKeysSection).Get<IList<string>>();


            // Para verificar en ventana Inmediato:
            //appSettings.GetSection("AllowedApiKeys").Get<IList<string>>()!.Contains(apiKeyVal)


            // Verificar si la clave es válida
            if (allowedApiKeys == null || !allowedApiKeys.Contains(apiKeyVal))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new { Message = "Cliente No Autorizado." });
                await context.Response.WriteAsync(result);
                return;
            }

            // Pasar al siguiente middleware
            await _next(context);
        }
    }
}
