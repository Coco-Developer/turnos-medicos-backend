using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ApiGestionTurnosMedicos.Middlewares
{
    /// <summary>
    /// Middleware personalizado para el manejo global de excepciones en la API.
    /// Registra el error y devuelve un mensaje genérico al cliente.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Inicializa una nueva instancia del middleware <see cref="ExceptionHandlingMiddleware"/>.
        /// </summary>
        /// <param name="next">Delegado que representa el siguiente middleware en la canalización.</param>
        /// <param name="logger">Instancia de <see cref="ILogger{ExceptionHandlingMiddleware}"/> para el registro de errores.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Ejecuta el middleware para manejar globalmente las excepciones en la API.
        /// Registra el error y devuelve un mensaje genérico al cliente.
        /// </summary>
        /// <param name="context">El contexto HTTP actual.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var usuario = context.User?.Identity?.Name ?? "Anónimo";
                var ruta = context.Request.Path;
                _logger.LogError(ex, "Error en {Ruta} por usuario {Usuario} a las {FechaHora}", ruta, usuario, DateTime.UtcNow);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Ocurrió un error inesperado.");
            }
        }
    }
}