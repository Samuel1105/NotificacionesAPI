using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NotificacionesAPI.Hubs;
using NotificacionesAPI.Models;
using NotificacionesAPI.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace NotificacionesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly NotificationDbContext _context;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            IHubContext<NotificationHub> hubContext,
            NotificationDbContext context,
            ILogger<NotificationController> logger)
        {
            _hubContext = hubContext;
            _context = context;
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
                notification.UserId = companyId;
                notification.CreatedAt = DateTime.UtcNow;
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Enviar la notificación a través de SignalR
                await _hubContext.Clients.Group(companyId).SendAsync("ReceiveNotification", new
                {
                    message = notification.Message,
                    timestamp = notification.CreatedAt,
                    companyId = companyId
                });

                _logger.LogInformation($"Notificación enviada y guardada para la empresa: {companyId}");
                return Ok(new { message = "Notificación enviada y guardada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar y guardar notificación para la empresa {companyId}");
                return StatusCode(500, new { message = "Error al enviar la notificación", error = ex.Message });
            }
        }

        [HttpGet("GetNotifications")]
        public async Task<IActionResult> GetNotifications(string userId, bool? isRead = null)
        {
            // Validar que el usuario autenticado tiene acceso a las notificaciones del `userId` solicitado
            

            // Consulta de notificaciones filtradas por `userId` y, opcionalmente, por estado de lectura
            var notificationsQuery = _context.Notifications
                .Where(n => n.UserId == userId);

            if (isRead.HasValue)
            {
                notificationsQuery = notificationsQuery.Where(n => n.IsRead == isRead.Value);
            }

            var notifications = await notificationsQuery
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }


        [HttpPost("MarkAsRead/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var companyId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(companyId))
            {
                return Unauthorized();
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && (n.UserId == companyId || n.UserId == "Global"));

            if (notification == null)
            {
                return NotFound(new { message = "Notificación no encontrada o no pertenece al usuario." });
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            // Notificar al cliente que la notificación ha sido marcada como leída
            await _hubContext.Clients.Group(companyId).SendAsync("NotificationMarkedAsRead", new
            {
                notificationId = notification.Id,
                isRead = notification.IsRead
            });

            return Ok(new { message = "Notificación marcada como leída." });
        }
    }
}
