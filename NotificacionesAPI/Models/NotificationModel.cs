namespace NotificacionesAPI.Models
{
    public class ApiKeyConfig
    {
        public string Key { get; set; }
        public string CompanyId { get; set; }
        public string Name { get; set; }
        public string[] AllowedOrigins { get; set; }
    }


    public class NotificationModel
    {
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
