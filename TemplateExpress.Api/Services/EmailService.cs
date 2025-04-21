using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Options;

namespace TemplateExpress.Api.Services;

public class EmailService : IEmailService
{

    private EmailConfiguration _emailOptions;
    
    public EmailService(IOptions<EmailConfiguration> emailOptions)
    {
        _emailOptions = emailOptions.Value;
    }
    
    public Task SendEmailAsync(UserEmailDto userEmailDto, string subject, string message)
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
                userEmailDto.Email,
                subject,
                message
                )
        );
    }
}