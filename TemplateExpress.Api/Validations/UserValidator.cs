using FluentValidation;
using TemplateExpress.Api.Dto.UserDtos;

namespace TemplateExpress.Api.Validations;

public class UserValidator : AbstractValidator<CreateUserDto>
{
    public UserValidator()
    {
        RuleFor(user => user.Email).EmailAddress();
        
        RuleFor(user => user.Username)
            .NotEmpty()
            .WithMessage("Username cannot be empty.")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters long.")
            .MaximumLength(30)
            .WithMessage("Username must be a maximum of 30 characters.");
        
        RuleFor(user => user.Password)
            .NotEmpty()
            .WithMessage("Password cannot be empty.")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long.")
            .MaximumLength(100)
            .WithMessage("Password must be a maximum of 100 characters.");
    }
}