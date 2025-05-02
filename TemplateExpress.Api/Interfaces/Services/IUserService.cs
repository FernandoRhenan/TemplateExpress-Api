using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Results;

namespace TemplateExpress.Api.Interfaces.Services;

public interface IUserService
{
    
    Task<Result<JwtTokenDto>> CreateUserAndTokenAsync(CreateUserDto createUserDto, IValidator<CreateUserDto> validator);
    Task<Result<JwtTokenDto>> ConfirmAccountAsync(JwtTokenDto jwtTokenDto);
    Task<Result<JwtTokenDto>> GenerateConfirmationAccountTokenAsync(EmailAndPasswordDto emailAndPasswordDto, IValidator<EmailAndPasswordDto> validator);
    Task<Result<JwtTokenDto>> LoginAsync(EmailAndPasswordDto emailAndPasswordDto, IValidator<EmailAndPasswordDto> validator);
}