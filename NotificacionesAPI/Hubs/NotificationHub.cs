using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace NotificacionesAPI.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var companyId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(companyId))
                {
                    _logger.LogWarning("CompanyId no encontrado en el token");
                    Context.Abort();
                    return;
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, companyId);
                _logger.LogInformation($"Cliente conectado al grupo: {companyId}");
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en OnConnectedAsync");
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var companyId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(companyId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, companyId);
                _logger.LogInformation($"Cliente desconectado del grupo: {companyId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotification(string message)
        {
            var companyId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(companyId))
            {
                throw new HubException("No autorizado");
            }

            await Clients.Group(companyId).SendAsync("ReceiveNotification", new
            {
                message = message,
                timestamp = DateTime.UtcNow,
                companyId = companyId
            });

            _logger.LogInformation($"Mensaje enviado al grupo: {companyId}");
        }

        // Método para enviar notificación a una empresa específica (solo para uso interno/admin)
        public async Task SendNotificationToCompany(string targetCompanyId, string message)
        {
            var senderCompanyId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderCompanyId))
            {
                throw new HubException("No autorizado");
            }

            // Aquí podrías agregar lógica adicional para verificar permisos

            await Clients.Group(targetCompanyId).SendAsync("ReceiveNotification", new
            {
                message = message,
                timestamp = DateTime.UtcNow,
                senderCompanyId = senderCompanyId
            });

            _logger.LogInformation($"Mensaje enviado de {senderCompanyId} a {targetCompanyId}");
        }
    }
}