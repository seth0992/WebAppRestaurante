using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAppRestaurante.Models.Models;

namespace WebAppRestaurante.ApiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController: ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpPost("login")]
        public ActionResult<LoginResponseModel> Login([FromBody] LoginModel loginModel)
        {
            if (loginModel.Username == "Admin" && loginModel.Password == "Admin" ||
               loginModel.Username == "User" && loginModel.Password == "User")
            {
                var token = GenerateJwtToken(loginModel.Username , isRefreshToken:false);
                var resfreshToken = GenerateJwtToken(loginModel.Username, isRefreshToken: true);
                return Ok(new LoginResponseModel { 
                    Token = token,
                    RefreshToken = resfreshToken,
                    TokenExpired = DateTimeOffset.UtcNow.AddMinutes(9).ToUnixTimeSeconds()
                });
            }
            return Unauthorized();
        }

        private string GenerateJwtToken(string username, bool isRefreshToken)
        {
            var claims = new[] {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role,  username == "Admin" ? "Admin" : "User")
              };

            string secret = _configuration.GetValue<string>($"Jwt:{((isRefreshToken)? "RefreshTokenSecret" : "Secret")}") ?? throw new InvalidOperationException("JWT secret is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "doseHiue",
                audience: "doseHiue",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(isRefreshToken ? 24*60 :  9),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        [HttpGet("loginByRefreshToken")]
        public ActionResult<LoginResponseModel> LoginByRefreshToken(string refreshToken) {
            var secret = _configuration.GetValue<string>("Jwt:RefreshTokenSecret");
            var claimsPrincipal = GetClaimsPrincipalFromToken(refreshToken, secret);
            if (claimsPrincipal == null) {
                return new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }

            var username = claimsPrincipal.FindFirstValue(ClaimTypes.Name);
            //Call to db to check user valid

            var newToken = GenerateJwtToken(username, isRefreshToken: false);
            var newRefreshToken = GenerateJwtToken(username, isRefreshToken: true);

            return new LoginResponseModel
            {
                Token = newToken,
                TokenExpired = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
                RefreshToken = newRefreshToken
            };
        }

        private ClaimsPrincipal GetClaimsPrincipalFromToken(string token, string? secret)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secret);
            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = "doseHiue",
                    ValidateIssuer = true,
                    ValidIssuer = "doseHiue",
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                }, out var validatedToken);
                return principal;
            }
            catch {
                return null;
            }
        }
    }
}

