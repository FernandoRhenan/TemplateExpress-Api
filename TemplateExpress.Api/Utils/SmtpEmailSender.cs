using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using TemplateExpress.Api.Options;
using TemplateExpress.Api.Interfaces.Utils;

namespace TemplateExpress.Api.Utils;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailConfigurationOptions _emailOptions;

    public SmtpEmailSender(IOptions<EmailConfigurationOptions> options)
    {
        _emailOptions = options.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlMessage)
    {
        if (_emailOptions.Host == null || _emailOptions.Port == 0 || _emailOptions.Username == null ||
            _emailOptions.Password == null)
        {
            throw new NullReferenceException("Missing SendEmail Configuration.");
        }
        
        using var client = new SmtpClient(_emailOptions.Host, _emailOptions.Port);
        client.EnableSsl = true;
        client.UseDefaultCredentials = false;
        client.Credentials = new NetworkCredential(_emailOptions.Username, _emailOptions.Password);

        var from = _emailOptions.EmailSender ?? throw new NullReferenceException("Missing Email Sender.");

        var mailMessage = new MailMessage(from, to, subject, htmlMessage)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(mailMessage);
    }
}