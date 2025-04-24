using FluentValidation;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Results;

namespace TemplateExpress.Api.Interfaces.Services;

public interface IUserService
{
    
    Task<Result<JwtConfirmationAccountTokenDto>> CreateUserAndTokenAsync(CreateUserDto createUserDto, IValidator<CreateUserDto> validator);
    Task<Result<string>> ConfirmAccountAsync(JwtConfirmationAccountTokenDto jwtConfirmationAccountTokenDto);
    Task<Result<JwtConfirmationAccountTokenDto>> GenerateConfirmationAccountTokenAsync(EmailAndPasswordDto emailAndPasswordDto, IValidator<EmailAndPasswordDto> validator);

}