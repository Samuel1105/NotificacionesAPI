using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NotificacionesAPI.Hubs;
using NotificacionesAPI.Models;
using System.Security.Claims;

namespace NotificacionesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationController> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationModel notification)
        {
            var companyId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(companyId))
            {
                return Unauthorized();
            }

            try
            {
                await _hubContext.Clients.Group(companyId).SendAsync("ReceiveNotification", new
                {
                    message = notification.Message,
                    timestamp = DateTime.UtcNow,
                    companyId = companyId
                });

                _logger.LogInformation($"Notificación enviada a la empresa: {companyId}");
                return Ok(new { message = "Notificación enviada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar notificación a la empresa {companyId}");
                return StatusCode(500, new { message = "Error al enviar la notificación" });
            }
        }

        [HttpPost("broadcast")]
        [Authorize(Roles = "Admin")] // Si implementas roles
        public async Task<IActionResult> BroadcastNotification([FromBody] BroadcastNotificationModel notification)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
                {
                    message = notification.Message,
                    timestamp = DateTime.UtcNow,
                    isGlobal = true
                });

                _logger.LogInformation("Notificación global enviada");
                return Ok(new { message = "Notificación global enviada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación global");
                return StatusCode(500, new { message = "Error al enviar la notificación global" });
            }
        }
    }
}
