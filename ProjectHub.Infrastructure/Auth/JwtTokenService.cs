using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProjectHub.Application.Dtos;
using ProjectHub.Application.Features.Users.Login;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjectHub.Infrastructure.Auth
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _config;
        public JwtTokenService(IConfiguration config) => _config = config;

        public TokenResponseDto GenerateForUser(int userId, string email, string username)
        {

            var issuer = _config["Jwt:Issuer"]!;
            var audience = _config["Jwt:Audience"]!;
            var key = _config["Jwt:Key"]!;
            var minutes = int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "60");

            var claims = new[]
            {
                // มาตรฐาน JWT
                new Claim("sub",   userId.ToString()),
                new Claim("email", email),

                // แบบ .NET (หลายจุดใน ASP.NET Core อ่านจากอันนี้)
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email,          email),
                new Claim(ClaimTypes.Name,           username),
            };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(minutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new TokenResponseDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expires
            };

            
        }
    }
}
