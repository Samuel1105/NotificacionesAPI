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
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
    public class BroadcastNotificationModel
    {
        public string Message { get; set; }
        public string[] TargetCompanyIds { get; set; }
    }
}
