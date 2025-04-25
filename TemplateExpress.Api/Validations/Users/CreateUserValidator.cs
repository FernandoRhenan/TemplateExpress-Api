using FluentValidation;
using TemplateExpress.Api.Dto.UserDto;

namespace TemplateExpress.Api.Validations.Users;

public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).ApplyEmailRules();
        RuleFor(x => x.Username).ApplyUsernameRules();
        RuleFor(x => x.Password).ApplyPasswordRules();
    }
}