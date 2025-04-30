using Microsoft.EntityFrameworkCore;
using TemplateExpress.Api.Data;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Entities;
using TemplateExpress.Api.Interfaces.Repositories;

namespace TemplateExpress.Api.Repositories;

public class UserRepository : RepositoryBase, IUserRepository
{
    public UserRepository(DataContext context) : base(context){} 
    
    public UserEntity InsertUser(UserEntity user)
    {
        Context.Users.Add(user);
        return user;
    }

    public async Task<bool> FindAnEmailAsync(UserEmailDto userEmailDto)
    {
        var thereIsAnEmail = await Context.Users.AnyAsync(u => u.Email == userEmailDto.Email);
        return thereIsAnEmail;
    }

    public async Task<bool> ChangeConfirmedAccountColumnToTrue(long id, bool save = true)
    {
        var user = await Context.Users.FindAsync(id);
        if (user == null) return false;
        user.ConfirmedAccount = true;
        if (save) await SaveChangesAsync();
        return true;
    }

    public async Task<UserEntity?> FindEmailAsync(UserEmailDto userEmailDto)
    {   
        var email = await Context.Users.FirstOrDefaultAsync(u => u.Email == userEmailDto.Email);
        return email ?? null;
    }
    
}