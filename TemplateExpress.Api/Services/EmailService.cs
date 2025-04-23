using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Security;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Options;

namespace TemplateExpress.Api.Services;

public class EmailService : IEmailService
{

    private readonly EmailConfiguration _emailOptions;
    private readonly ITokenManager _tokenManager;
    
    public EmailService(IOptions<EmailConfiguration> emailOptions, ITokenManager tokenManager)
    {
        _emailOptions = emailOptions.Value;
        _tokenManager = tokenManager;
    }
    
    public Task SendEmailAsync(JwtConfirmationAccountTokenDto jwtConfirmationAccountTokenDto)
    {
        
        var client = new SmtpClient(_emailOptions.Host, _emailOptions.Port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_emailOptions.Username, _emailOptions.Password)
        };

        return client.SendMailAsync(
            new MailMessage(
                _emailOptions.EmailSender ?? throw new NullReferenceException("Email sender missing."),
                "userEmailDto.Email",
                "subject",
                "message"
                )
        );
    }
}