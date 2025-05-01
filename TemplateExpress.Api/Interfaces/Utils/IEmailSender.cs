namespace TemplateExpress.Api.Interfaces.Utils;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string htmlMessage);
}