using FluentValidation;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Results;

namespace TemplateExpress.Api.Interfaces.Services;

public interface IUserService
{
    
    Task<Result<JwtConfirmationAccountTokenDto>> CreateUserAndTokenAsync(CreateUserDto createUserDto);
    Task<Result<string>> ConfirmAccountAsync(JwtConfirmationAccountTokenDto jwtConfirmationAccountTokenDto);
}