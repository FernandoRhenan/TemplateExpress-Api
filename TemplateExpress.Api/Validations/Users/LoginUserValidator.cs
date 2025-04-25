using FluentValidation;
using TemplateExpress.Api.Dto.UserDto;

namespace TemplateExpress.Api.Validations.Users;

public class LoginUserValidator : AbstractValidator<EmailAndPasswordDto>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Email).ApplyEmailRules();
        RuleFor(x => x.Password).ApplyPasswordRules();
    }
}