using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace STDMS.Filters
{
    public class ActionLoggingFilter : IActionFilter
    {
        private readonly ILogger<ActionLoggingFilter> _logger;

        public ActionLoggingFilter(ILogger<ActionLoggingFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation(
                "Executing MVC action {Controller}.{Action} using {HttpMethod}. Authenticated: {IsAuthenticated}",
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"],
                context.HttpContext.Request.Method,
                context.HttpContext.User.Identity?.IsAuthenticated == true);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation(
                "Executed MVC action {Controller}.{Action}",
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"]);
        }
    }
}
