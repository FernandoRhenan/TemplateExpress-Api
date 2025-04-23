using TemplateExpress.Api.Dto.UserDto;

namespace TemplateExpress.Api.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(JwtConfirmationAccountTokenDto jwtConfirmationAccountTokenDto);
}