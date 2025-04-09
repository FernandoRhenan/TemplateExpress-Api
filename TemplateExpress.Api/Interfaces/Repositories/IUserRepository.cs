using TemplateExpress.Api.Entities;

namespace TemplateExpress.Api.Interfaces.Repositories;

public interface IUserRepository
{
    Task<UserEntity> InsertUserAsync(UserEntity user);
    Task<bool> FindAnEmailAsync(string email);

    Task<EmailConfirmationTokenEntity> InsertEmailConfirmationTokenAsync(
        EmailConfirmationTokenEntity emailConfirmationToken);
}