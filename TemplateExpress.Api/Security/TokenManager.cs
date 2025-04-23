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
    
    private readonly JwtConfirmationOptions _jwtConfirmationOptions;
    public TokenManager(IOptions<JwtConfirmationOptions> jwtOptions)
    {
        _jwtConfirmationOptions = jwtOptions.Value;
    }
    public string GenerateEmailConfirmationToken(UserIdAndEmailDto userIdAndEmailDto)
    {
        // Create a new instance of TokenHandler
        var handler = new JwtSecurityTokenHandler();

        // Get JwtSecret
        var jwtSecret = _jwtConfirmationOptions.Secret;
        if (string.IsNullOrWhiteSpace(jwtSecret)) throw new InvalidOperationException("Missing JWT Secret.");

        var key = Encoding.UTF8.GetBytes(jwtSecret);
        
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            SigningCredentials = credentials,
            Expires = DateTime.UtcNow.AddHours(12),
            Subject = GenerateEmailConfirmationTokenClaims(userIdAndEmailDto)
        };
        
        // Generate a Token
        var token = handler.CreateJwtSecurityToken(tokenDescriptor);
        
        // Generate and return a string from type object SecurityToken
        return handler.WriteToken(token);

    }

    private static ClaimsIdentity GenerateEmailConfirmationTokenClaims(UserIdAndEmailDto userIdAndEmailDto)
    {
        var ci = new ClaimsIdentity();
        ci.AddClaim(new Claim(ClaimTypes.Email, userIdAndEmailDto.Email));
        ci.AddClaim(new Claim(ClaimTypes.Name, userIdAndEmailDto.Id.ToString()));
        return ci;
    }

    // TODO: It wasn't unit tested.
    public async Task<TokenValidationResult> TokenValidation(JwtConfirmationAccountTokenDto jwtConfirmationAccountTokenDto)
    {
        var jwtSecret = _jwtConfirmationOptions.Secret;
        if (string.IsNullOrWhiteSpace(jwtSecret)) throw new InvalidOperationException("Missing JWT Secret.");
        
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        var validationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = key,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            // ValidIssuer = "http://localhost:",
            // ValidateIssuer = true,
            ClockSkew = TimeSpan.Zero
        };
        
        var tokenValidation = await handler.ValidateTokenAsync(jwtConfirmationAccountTokenDto.Token, validationParameters);

        return tokenValidation;
    }
    
    // TODO: It wasn't unit tested.
    public UserIdAndEmailDto GetJwtConfirmationAccountTokenClaims(TokenValidationResult tokenValidationResult)
    {
        return new UserIdAndEmailDto(0, "");
    }

    
    // TODO: It wasn't unit tested.

}
