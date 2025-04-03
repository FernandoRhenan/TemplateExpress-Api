using Microsoft.EntityFrameworkCore;
using TemplateExpress.Api.Data;
using TemplateExpress.Api.Entities;
using TemplateExpress.Api.Interfaces.Repositories;

namespace TemplateExpress.Api.Repositories;

public class UserRepository : IUserRepository
{

    private readonly DataContext _context;

    public UserRepository(DataContext context)
    {
        _context = context;
    }
    
    public async Task<UserEntity> InsertUserAsync(UserEntity user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> FindAnEmailAsync(string email)
    {
        var thereIsAnEmail = await _context.Users.AnyAsync(u => u.Email == email);
        return thereIsAnEmail;
    }

    public async Task<bool> FindAnUsernameAsync(string name)
    {
        var thereIsAnUsername = await _context.Users.AnyAsync(u => u.Username == name);
        return thereIsAnUsername;
    }
}