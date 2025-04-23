using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Results;

namespace TemplateExpress.Api.Interfaces.Services;

public interface IEmailService
{
    Task<Result<string>> SendEmailAsync(JwtConfirmationAccountTokenDto jwtConfirmationAccountTokenDto);
}