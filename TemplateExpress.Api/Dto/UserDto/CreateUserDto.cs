namespace TemplateExpress.Api.Dto.UserDto;

public record CreateUserDto(
    string Email,
    string Username,
    string Password
    );