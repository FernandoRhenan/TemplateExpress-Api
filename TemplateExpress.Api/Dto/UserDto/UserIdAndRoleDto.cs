using TemplateExpress.Api.EnumTypes;

namespace TemplateExpress.Api.Dto.UserDto;

public record UserIdAndRoleDto(
    long Id,
    UserRoles Role
    );