 using Microsoft.IdentityModel.Tokens;
using NotificacionesAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NotificacionesAPI.Services
{

    public interface IAuthService
    {
        TokenResponse GenerateToken(string companyId, string companyName);
        bool ValidateToken(string token, out string companyId);
        bool ValidateCompanyCredentials(string companyId, string password, out string companyName);
    }

    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly List<CompanyConfig> _companies;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _companies = configuration.GetSection("Companies").Get<List<CompanyConfig>>();
        }

        public bool ValidateCompanyCredentials(string companyId, string password, out string companyName)
        {
            companyName = null;
            var company = _companies.FirstOrDefault(x =>
                x.CompanyId == companyId &&
                x.Password == password);  // En producción, usar hash

            if (company != null)
            {
                companyName = company.Name;
                return true;
            }

            return false;
        }

        public TokenResponse GenerateToken(string companyId, string companyName)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtConfig:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddHours(1);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, companyId),
                new Claim("CompanyName", companyName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtConfig:Issuer"],
                audience: _configuration["JwtConfig:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return new TokenResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration,
                CompanyId = companyId,
                CompanyName = companyName
            };
        }

        public bool ValidateToken(string token, out string companyId)
        {
            companyId = null;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["JwtConfig:Secret"]);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JwtConfig:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JwtConfig:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                companyId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return false;
            }
        }
    }

}
