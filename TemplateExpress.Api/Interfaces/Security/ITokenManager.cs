using Microsoft.IdentityModel.Tokens;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Results;

namespace TemplateExpress.Api.Interfaces.Security;

public interface ITokenManager
{
    string GenerateEmailConfirmationToken(UserIdAndEmailDto userIdAndEmailDto);
    Task<Result<TokenValidationResult>> TokenValidation(JwtConfirmationAccountTokenDto jwtConfirmationAccountTokenDto);
    UserIdAndEmailDto GetJwtConfirmationAccountTokenClaims(TokenValidationResult tokenValidationResult);
}
