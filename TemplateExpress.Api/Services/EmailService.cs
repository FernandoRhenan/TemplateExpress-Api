using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Security;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Options;
using TemplateExpress.Api.Results;

namespace TemplateExpress.Api.Services;

public class EmailService : IEmailService
{

    private readonly EmailConfigurationOptions _emailOptions;
    private readonly ITokenManager _tokenManager;
    
    public EmailService(IOptions<EmailConfigurationOptions> emailOptions, ITokenManager tokenManager)
    {
        _emailOptions = emailOptions.Value;
        _tokenManager = tokenManager;
    }
    
    // TODO: It wasn't unit tested.
    public async Task<Result<string>> SendEmailConfirmationTokenAsync(JwtConfirmationAccountTokenDto jwtConfirmationAccountTokenDto)
    {
        var tokenValidation = await _tokenManager.ValidateAccountConfirmationToken(jwtConfirmationAccountTokenDto);
        
        if (!tokenValidation.IsSuccess)
        {
            return Result<string>.Failure(tokenValidation.Error!);
        }
       
        var userIdAndEmail = _tokenManager.GetJwtConfirmationAccountTokenClaims(tokenValidation.Value!);
        
        var client = new SmtpClient(_emailOptions.Host, _emailOptions.Port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_emailOptions.Username, _emailOptions.Password)
        };

        var emailSender = _emailOptions.EmailSender ?? throw new NullReferenceException("Missing Email Sender.");
        // TODO: Upgrade this HTML message
        var htmlMessage = $"<a href='http://localhost:5290/email-confirmation/{jwtConfirmationAccountTokenDto.Token}'>Clique aqui para confirmar</a>";
        var message = new MailMessage(
            emailSender,
            userIdAndEmail.Email,
            "Email Confirmation",
            htmlMessage
            )
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(message);

        return Result<string>.Success(String.Empty);
    }
}