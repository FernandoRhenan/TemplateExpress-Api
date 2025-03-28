namespace TemplateExpress.Api.Dto.UserDtos;

public record CreateUserDto(
    string Email,
    string Username,
    string Password
    );