using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Security;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Interfaces.Utils;
using TemplateExpress.Api.Results;

namespace TemplateExpress.Api.Services;

public class EmailService : IEmailService
{
    private readonly ITokenManager _tokenManager;
    private readonly IEmailSender _emailSender;
    
    public EmailService(ITokenManager tokenManager, IEmailSender emailSender)
    {
        _tokenManager = tokenManager;
        _emailSender = emailSender;
    }
    
    public async Task<Result<string>> SendEmailConfirmationTokenAsync(JwtTokenDto jwtTokenDto)
    {
        var tokenValidation = await _tokenManager.ValidateAccountConfirmationTokenAsync(jwtTokenDto);
        
        if (!tokenValidation.IsSuccess)
        {
            return Result<string>.Failure(tokenValidation.Error!);
        }
        
        var userIdAndEmail = _tokenManager.GetJwtConfirmationAccountTokenClaims(tokenValidation.Value!);
        
        // TODO: Upgrade this HTML message
        var htmlMessage = $"<a href='http://localhost:5290/email-confirmation/{jwtTokenDto.Token}'>Clique aqui para confirmar</a>";

        await _emailSender.SendEmailAsync(userIdAndEmail.Email, "Email Confirmation", htmlMessage);

        return Result<string>.Success(String.Empty);
    }
}