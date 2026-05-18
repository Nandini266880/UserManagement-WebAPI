using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Users.Application.Interfaces;
using Users.Domain.Entities;

namespace Users.Infrastructure.JwtServices
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenService> _logger;
        public TokenService(IConfiguration configuration, ILogger<TokenService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Generates a signed JWT access token for the specified user.
        /// </summary>
        /// <param name="user">User entity to include in the token claims.</param>
        /// <returns>A signed JWT access token string.</returns>
        public async Task<string> GetJWTAccessToken(User user)
        {
            int expiryMinutes = Convert.ToInt32(_configuration["JwtConfig:DurationInMinutes"]);
            string secretKey = _configuration["JwtConfig:Key"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim(JwtRegisteredClaimNames.Aud, "http://localhost:5135"),
                    new Claim(JwtRegisteredClaimNames.Iss, "http://localhost:5135")
                }),
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var createdToken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(createdToken);

            _logger.LogInformation("JWT created for User {Email}", user.Email);
            return token;
        }
    }
}
