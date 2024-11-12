using NotificacionesAPI.Models;

namespace NotificacionesAPI.Services
{
    public class ApiKeyValidationService : IApiKeyValidationService
    {
        private readonly IConfiguration _configuration;
        private readonly List<ApiKeyConfig> _apiKeys;

        public ApiKeyValidationService(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKeys = configuration.GetSection("ApiKeys").Get<List<ApiKeyConfig>>();
        }

        public bool ValidateApiKey(string apiKey, string origin, out string companyId)
        {
            companyId = null;
            var config = _apiKeys.FirstOrDefault(x => x.Key == apiKey);

            if (config == null) return false;

            if (string.IsNullOrEmpty(origin))
            {
                companyId = config.CompanyId;
                return true;
            }

            if (config.AllowedOrigins.Any(o => origin.StartsWith(o, StringComparison.OrdinalIgnoreCase)))
            {
                companyId = config.CompanyId;
                return true;
            }

            return false;
        }

        public string GetCompanyId(string apiKey)
        {
            return _apiKeys.FirstOrDefault(x => x.Key == apiKey)?.CompanyId;
        }
    }
}
