using TemplateExpress.Api.Entities;

namespace TemplateExpress.Api.Interfaces.Repositories;

public interface IUserRepository : IRepositoryBase
{
    UserEntity InsertUser(UserEntity user);
    Task<bool> FindAnEmailAsync(string email);
}
