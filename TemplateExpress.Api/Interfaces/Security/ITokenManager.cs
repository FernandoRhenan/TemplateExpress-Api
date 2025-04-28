using Microsoft.IdentityModel.Tokens;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Results;

namespace TemplateExpress.Api.Interfaces.Security;

public interface ITokenManager
{
    string GenerateAccountConfirmationToken(UserIdAndEmailDto userIdAndEmailDto);
    Task<Result<TokenValidationResult>> ValidateAccountConfirmationToken(JwtConfirmationAccountTokenDto jwtConfirmationAccountTokenDto);
    UserIdAndEmailDto GetJwtConfirmationAccountTokenClaims(TokenValidationResult tokenValidationResult);
}
