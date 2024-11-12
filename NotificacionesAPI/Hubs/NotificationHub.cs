using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NotificacionesAPI.Services;

namespace NotificacionesAPI.Hubs
{
    
    public class NotificationHub : Hub
    {
        private readonly IApiKeyValidationService _apiKeyValidationService;
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(
            IApiKeyValidationService apiKeyValidationService,
            ILogger<NotificationHub> logger)
        {
            _apiKeyValidationService = apiKeyValidationService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                if (httpContext == null)
                {
                    _logger.LogWarning("HttpContext not available");
                    Context.Abort();
                    return;
                }

                var apiKey = httpContext.Request.Headers["X-API-Key"].ToString();
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("API Key not provided");
                    Context.Abort();
                    return;
                }

                var origin = httpContext.Request.Headers["Origin"].ToString();
                if (!_apiKeyValidationService.ValidateApiKey(apiKey, origin, out string companyId))
                {
                    _logger.LogWarning($"Invalid API Key: {apiKey} from origin: {origin}");
                    Context.Abort();
                    return;
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, companyId);
                _logger.LogInformation($"Client connected to group: {companyId}");

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync");
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var httpContext = Context.GetHttpContext();
            var companyId = httpContext.Items["CompanyId"]?.ToString();

            if (!string.IsNullOrEmpty(companyId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, companyId);
                _logger.LogInformation($"Client disconnected from group: {companyId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotification(string message)
        {
            var httpContext = Context.GetHttpContext();
            var companyId = httpContext.Items["CompanyId"]?.ToString();

            if (!string.IsNullOrEmpty(companyId))
            {
                await Clients.Group(companyId).SendAsync("ReceiveNotification", message);
                _logger.LogInformation($"Message sent to group: {companyId}");
            }
        }
    }
}
