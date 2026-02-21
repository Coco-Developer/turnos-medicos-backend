using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Linq;

namespace ApiGestionTurnosMedicos.Middlewares
{
    /// <summary>
    /// Atributo para requerir API Key en endpoints específicos.
    /// </summary>
    public class ApiKeyAttribute : ActionFilterAttribute
    {
        private const string HeaderName = "X-API-KEY";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var allowedApiKeys = context.HttpContext.RequestServices.GetRequiredService<IList<string>>();

            if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var extractedApiKey) ||
                !allowedApiKeys.Contains(extractedApiKey.First()))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "API Key inválida o no proporcionada." });
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}