using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Security;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Options;
using TemplateExpress.Api.Results;
using TemplateExpress.Api.Results.EnumResponseTypes;

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
    
    public async Task<Result<string>> SendEmailAsync(JwtConfirmationAccountTokenDto jwtConfirmationAccountTokenDto)
    {
        var tokenValidation = await _tokenManager.TokenValidation(jwtConfirmationAccountTokenDto);

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

        return client.SendMailAsync(
            new MailMessage(
                _emailOptions.EmailSender ?? throw new NullReferenceException("Email sender missing."),
                userIdAndEmail.Email,
                "subject",
                "message"
                )
        );
    }
}