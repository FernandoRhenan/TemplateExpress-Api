using Microsoft.EntityFrameworkCore;
using TemplateExpress.Api.Data;
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

    public async Task<bool> FindAnEmailAsync(string email)
    {
        var thereIsAnEmail = await Context.Users.AnyAsync(u => u.Email == email);
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
    
}