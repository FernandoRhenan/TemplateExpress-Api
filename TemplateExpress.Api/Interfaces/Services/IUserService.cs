using FluentValidation;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Results;

namespace TemplateExpress.Api.Interfaces.Services;

public interface IUserService
{
    
    Task<Result<UserEmailDto>> CreateUserAsync(IValidator<CreateUserDto> userValidator, CreateUserDto createUserDto);
}