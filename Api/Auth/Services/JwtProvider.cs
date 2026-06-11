using Auth.Infrastructure;
using Auth.Interfaces;
using Auth.Models;
using Auth.Repositories.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Auth.Services
{
    public class JwtProvider(IOptions<JwtOptions> options) : IJwtProvider
    {
        private readonly JwtOptions _options = options.Value;

        public string GenerateToken(User user)
        {
            Claim[] claims = [new("userId", user.Id.ToString())];

            var signingCredentials = new SigningCredentials(
                RsaKeyLoader.LoadPrivateKey(_options.PrivateKeyPath),
                SecurityAlgorithms.RsaSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_options.ExpiersHours),
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
