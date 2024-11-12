namespace NotificacionesAPI.Models
{
    public class UserLoginModel
    {
        public string CompanyId { get; set; }
        public string Password { get; set; }
    }

    public class CompanyConfig
    {
        public string CompanyId { get; set; }
        public string Password { get; set; }  // En producción, usar hash
        public string Name { get; set; }
        public string[] AllowedOrigins { get; set; }
    }

    public class TokenResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
    }

    public class NotificationModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int Tipo { get; set; }
        public bool IsRead { get; set; } = false;
    }
    public class BroadcastNotificationModel
    {
        public string Message { get; set; }
        public string[] TargetCompanyIds { get; set; }
    }
}
