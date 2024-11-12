using Microsoft.EntityFrameworkCore;
using NotificacionesAPI.Models;

namespace NotificacionesAPI.Data
{
    public class NotificationDbContext : DbContext
    {

        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

        public DbSet<NotificationModel> Notifications { get; set; }

    }
}
