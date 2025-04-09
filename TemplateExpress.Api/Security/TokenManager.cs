using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Security;
using TemplateExpress.Api.Options;

namespace TemplateExpress.Api.Security;

public class TokenManager : ITokenManager
{
    
    private readonly JwtOptions _jwtOptions;
    public TokenManager(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }
    public string GenerateEmailConfirmationToken(UserIdAndEmailDto userIdAndEmailDto)
    {
        // Create a new instance of TokenHandler
        var handler = new JwtSecurityTokenHandler();

        // Get JwtSecret
        var jwtSecret = _jwtOptions.Secret;
        if (string.IsNullOrWhiteSpace(jwtSecret)) throw new InvalidOperationException("Missing JWT Secret.");

        var key = Encoding.UTF8.GetBytes(jwtSecret);
        
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            SigningCredentials = credentials,
            Expires = DateTime.UtcNow.AddHours(12),
            Subject = GenerateClaims(userIdAndEmailDto)
        };
        
        // Generate a Token
        var token = handler.CreateJwtSecurityToken(tokenDescriptor);
        
        // Generate and return a string from type object SecurityToken
        return handler.WriteToken(token);

    }

    private static ClaimsIdentity GenerateClaims(UserIdAndEmailDto userIdAndEmailDto)
    {
        var ci = new ClaimsIdentity();
        ci.AddClaim(new Claim(ClaimTypes.Email, userIdAndEmailDto.Email));
        ci.AddClaim(new Claim(ClaimTypes.Name, userIdAndEmailDto.Id.ToString()));
        return ci;
    }
    
}
