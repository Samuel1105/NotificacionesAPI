using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NotificacionesAPI.Hubs;
using NotificacionesAPI.Models;

namespace NotificacionesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
 
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
            var companyId = HttpContext.Items["CompanyId"]?.ToString();

            if (string.IsNullOrEmpty(companyId))
            {
                return Unauthorized();
            }

            await _hubContext.Clients.Group(companyId).SendAsync("ReceiveNotification", notification.Message);
            _logger.LogInformation($"Notification sent to company: {companyId}");

            return Ok(new { message = "Notification sent successfully" });
        }
    }
}
