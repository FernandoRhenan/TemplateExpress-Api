
using TemplateExpress.Api.EnumTypes;

namespace TemplateExpress.Api.Entities;

public class UserEntity
{

    public UserEntity()
    {
        Templates = new List<TemplateEntity>();
    }
    
    public long Id { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public UserRoles Role { get; set; } = UserRoles.User;
    public bool ConfirmedAccount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<TemplateEntity> Templates { get; set; }
}