using Microsoft.IdentityModel.Tokens;
using TemplateExpress.Api.Dto.UserDto;

namespace TemplateExpress.Api.Interfaces.Security;

public interface ITokenManager
{
    string GenerateEmailConfirmationToken(UserIdAndEmailDto userIdAndEmailDto);
    Task<TokenValidationResult> TokenValidation(JwtConfirmationAccountTokenDto jwtConfirmationAccountTokenDto);
    UserIdAndEmailDto GetJwtConfirmationAccountTokenClaims(TokenValidationResult tokenValidationResult);
}
