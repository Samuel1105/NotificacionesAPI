namespace NotificacionesAPI.Services
{
    public interface IApiKeyValidationService
    {
        bool ValidateApiKey(string apiKey, string origin, out string companyId);
        string GetCompanyId(string apiKey);
    }
}
