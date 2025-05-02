using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Entities;
using TemplateExpress.Api.Results;

namespace TemplateExpress.Tests;

internal static class DefaultObjects
{
    
    private static readonly Random Random = new();
    private static readonly DateTime Now = DateTime.Now;
    
    internal static (CreateUserDto createUserDto,
        UserIdAndEmailDto userIdAndEmailDto,
        JwtTokenDto jwtConfirmationAccountTokenDto,
        List<IErrorMessage> errorMessages,
        UserEntity userEntity,
        EmailAndPasswordDto emailAndPasswordDto
        ) GenerateDefaultObjects()
    {
        var userId = (long)Random.Next(1, 50_000);
        
        var createUserDto = new CreateUserDto("test@test.com", "comusertest1", "=d#OdcA)53?p7$$$Sv_0 ");
        var userIdAndEmailDto = new UserIdAndEmailDto(userId, createUserDto.Email);
        var jwtConfirmationAccountTokenDto = new JwtTokenDto("token");
        var errorMessages = new List<IErrorMessage>
        {
            new ErrorMessage("Invalid input", "Check the fields.")
        };
        var userEntity = new UserEntity
        {
            Id = userId,
            Email = createUserDto.Email,
            Password = createUserDto.Password,
            Username = createUserDto.Username,
            CreatedAt = Now,
            UpdatedAt = Now
        };
        var emailAndPasswordDto = new EmailAndPasswordDto(createUserDto.Email, createUserDto.Password);
        
        return (createUserDto, userIdAndEmailDto, jwtConfirmationAccountTokenDto, errorMessages, userEntity, emailAndPasswordDto);
    }
    
}