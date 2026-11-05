using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DndJornada.Application.Interfaces;
using DndJornada.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DndJornada.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var secret = _configuration["Jwt:SecretKey"] ?? "DndGroupSuperSecretSecurityKey_ChangeInProd_2026!";
        var issuer = _configuration["Jwt:Issuer"] ?? "DndJornadaApi";
        var audience = _configuration["Jwt:Audience"] ?? "DndJornadaClient";
        var expirationHours = int.TryParse(_configuration["Jwt:ExpirationHours"], out var exp) ? exp : 8;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("timezone", user.DefaultTimezoneId),
            new("department", user.Department ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expirationHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

