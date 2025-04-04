using TemplateExpress.Api.Dto.UserDto;

namespace TemplateExpress.Api.Interfaces.Security;

public interface ITokenManagerSecurity
{
    Task<string> GenerateEmailConfirmationTokenAsync(UserIdAndEmailDto userIdAndEmailDto);
}
