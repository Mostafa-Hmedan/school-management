using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Back.Requestes;
using Back.Responses;
using Microsoft.IdentityModel.Tokens;

namespace Back.Services;

public class JwtTokenProvider
{
    private readonly IConfiguration _configuration;
    public JwtTokenProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public TokenResponse GenerateJwtToken(GenerateTokenRequest request)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;
        var key = jwtSettings["SecretKey"]!;
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["TokenExpirationInMinutes"]!));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.Id!),
            new(JwtRegisteredClaimNames.Email, request.Email!),
            new(JwtRegisteredClaimNames.FamilyName, request.LastName!),
            new(JwtRegisteredClaimNames.GivenName, request.FirstName!),
        };
        // إضافة الأدوار
        foreach (var role in request.Roles)
            claims.Add(new(ClaimTypes.Role, role));

        // إضافة الصلاحيات
        foreach (var permission in request.Permissions)
            claims.Add(new("permission", permission));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(descriptor);

        return new TokenResponse
        {
            AccessToken = tokenHandler.WriteToken(securityToken),
            RefreshToken = Guid.NewGuid().ToString(), // مؤقت - سنحسّنه لاحقاً
            Expires = expires
        };
    }
}