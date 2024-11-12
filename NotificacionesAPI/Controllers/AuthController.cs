using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotificacionesAPI.Models;
using NotificacionesAPI.Services;
using System.Security.Claims;

namespace NotificacionesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginModel login)
        {
            if (!_authService.ValidateCompanyCredentials(login.CompanyId, login.Password, out string companyName))
            {
                return Unauthorized("Credenciales inválidas");
            }

            var tokenResponse = _authService.GenerateToken(login.CompanyId, companyName);
            return Ok(tokenResponse);
        }

        [HttpPost("refresh")]
        [Authorize]
        public IActionResult RefreshToken()
        {
            var companyId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var companyName = User.FindFirst("CompanyName")?.Value;

            if (string.IsNullOrEmpty(companyId) || string.IsNullOrEmpty(companyName))
            {
                return Unauthorized();
            }

            var newTokenResponse = _authService.GenerateToken(companyId, companyName);
            return Ok(newTokenResponse);
        }
    }
}
