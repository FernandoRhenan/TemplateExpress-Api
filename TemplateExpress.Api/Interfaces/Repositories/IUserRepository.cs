using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Entities;

namespace TemplateExpress.Api.Interfaces.Repositories;

public interface IUserRepository : IRepositoryBase
{
    UserEntity InsertUser(UserEntity user);
    Task<bool> FindAnEmailAsync(UserEmailDto userEmailDto);
    Task<UserEntity?> ChangeConfirmedAccountColumnToTrue(long id, bool save = true);
    Task<UserEntity?> FindEmailAsync(UserEmailDto userEmailDto);

}
