using NotificacionesAPI.Services;

namespace NotificacionesAPI.Middleware
{
    public class ApiKeyAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IApiKeyValidationService _validationService;
        private readonly ILogger<ApiKeyAuthMiddleware> _logger;

        public ApiKeyAuthMiddleware(
            RequestDelegate next,
            IApiKeyValidationService validationService,
            ILogger<ApiKeyAuthMiddleware> logger)
        {
            _next = next;
            _validationService = validationService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "API Key is required" });
                return;
            }

            var origin = context.Request.Headers["Origin"].ToString();
            if (!_validationService.ValidateApiKey(apiKey, origin, out string companyId))
            {
                _logger.LogWarning($"Invalid API Key attempt: {apiKey} from origin: {origin}");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Invalid API Key" });
                return;
            }

            context.Items["CompanyId"] = companyId;
            await _next(context);
        }
    }
}
