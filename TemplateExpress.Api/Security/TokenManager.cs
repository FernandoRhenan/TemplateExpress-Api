using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Security;

namespace TemplateExpress.Api.Security;

public class TokenManagerSecurity : ITokenManagerSecurity
{
    public async Task<string> GenerateEmailConfirmationTokenAsync(UserIdAndEmailDto userIdAndEmailDto)
    {
        
    }
}