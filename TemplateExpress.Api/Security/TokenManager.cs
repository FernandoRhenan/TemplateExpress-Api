using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Security;
using TemplateExpress.Api.Options;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Results.EnumResponseTypes;

namespace TemplateExpress.Api.Security;

public class TokenManager : ITokenManager
{
    
    private readonly JwtAccountConfirmationOptions _jwtAccountConfirmationOptions;
    public TokenManager(IOptions<JwtAccountConfirmationOptions> jwtAccountConfirmationOptions)
    {
        _jwtAccountConfirmationOptions = jwtAccountConfirmationOptions.Value;
    }
    public string GenerateAccountConfirmationToken(UserIdAndEmailDto userIdAndEmailDto)
    {
        // Create a new instance of TokenHandler
        var handler = new JwtSecurityTokenHandler();

        // Get JwtSecret
        var jwtSecret = _jwtAccountConfirmationOptions.Secret;
        if (string.IsNullOrWhiteSpace(jwtSecret)) throw new InvalidOperationException("Missing JWT Secret.");

        var key = Encoding.UTF8.GetBytes(jwtSecret);
        
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            SigningCredentials = credentials,
            Expires = DateTime.UtcNow.AddHours(12),
            Subject = GenerateAccountConfirmationTokenClaims(userIdAndEmailDto)
        };
        
        // Generate a Token
        var token = handler.CreateJwtSecurityToken(tokenDescriptor);
        
        // Generate and return a string from type object SecurityToken
        return handler.WriteToken(token);

    }

    private static ClaimsIdentity GenerateAccountConfirmationTokenClaims(UserIdAndEmailDto userIdAndEmailDto)
    {
        var ci = new ClaimsIdentity();
        ci.AddClaim(new Claim(ClaimTypes.Email, userIdAndEmailDto.Email));
        ci.AddClaim(new Claim(ClaimTypes.Name, userIdAndEmailDto.Id.ToString()));
        return ci;
    }

    public async Task<Result<TokenValidationResult>> ValidateAccountConfirmationTokenAsync(JwtConfirmationAccountTokenDto jwtConfirmationAccountTokenDto)
    {
        var jwtSecret = _jwtAccountConfirmationOptions.Secret;
        if (string.IsNullOrWhiteSpace(jwtSecret)) throw new InvalidOperationException("Missing JWT Secret.");
        
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        var validationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = key,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidateAudience = false,
            ValidateIssuer = false,
            // ValidIssuer = "http://localhost:",
            ClockSkew = TimeSpan.Zero
        };
        
        var tokenValidation = await handler.ValidateTokenAsync(jwtConfirmationAccountTokenDto.Token, validationParameters);

        if (!tokenValidation.IsValid)
        {
            List<IErrorMessage> errorMessages = [new ErrorMessage("You do not have authorization for continue.", "Confirm your credentials.")];
            return Result<TokenValidationResult>.Failure(new Error((byte)ErrorCodes.InvalidJwtToken, (byte)ErrorTypes.Unauthorized, errorMessages));
        }

        return Result<TokenValidationResult>.Success(tokenValidation);
    }
    
    public UserIdAndEmailDto GetJwtConfirmationAccountTokenClaims(TokenValidationResult tokenValidationResult)
    {
        
        var identity = tokenValidationResult.ClaimsIdentity;

        if (identity == null || !identity.Claims.Any())
            throw new SecurityTokenException("Missing token claims.");

        var idStr = identity.FindFirst(ClaimTypes.Name)?.Value;
        var email = identity.FindFirst(ClaimTypes.Email)?.Value;

        if (!long.TryParse(idStr, out var id))
            throw new SecurityTokenException("Missing userId claim.");
        
        if(email == null)
            throw new SecurityTokenException("Missing email claim.");

        return new UserIdAndEmailDto(id, email);
    }
    
}
