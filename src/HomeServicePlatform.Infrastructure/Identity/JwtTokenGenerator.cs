using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Domain.Modules.Identity.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Infrastructure.Identity
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration) => _configuration = configuration;

        public (string Token, DateTime ExpiresAtUtc) GenerateToken(User user, IList<string> roles)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
            foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresAtUtc = DateTime.UtcNow.AddMinutes(ResolveExpiryMinutes());

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: creds
            );
            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
        }

        private double ResolveExpiryMinutes()
        {
            const double defaultMinutes = 15;
            var raw = _configuration["JwtSettings:ExpiryMinutes"];

            return double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var minutes)
                   && minutes > 0
                ? minutes
                : defaultMinutes;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
