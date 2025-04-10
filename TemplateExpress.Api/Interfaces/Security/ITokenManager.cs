using TemplateExpress.Api.Dto.UserDto;

namespace TemplateExpress.Api.Interfaces.Security;

public interface ITokenManager
{
    string GenerateEmailConfirmationToken(UserIdAndEmailDto userIdAndEmailDto);
}
