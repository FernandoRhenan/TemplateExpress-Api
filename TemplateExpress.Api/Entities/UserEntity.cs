
namespace TemplateExpress.Api.Entities;

public class UserEntity
{

    public UserEntity()
    {
        Templates = new List<TemplateEntity>();
    }
    
    public long Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool ConfirmedAccount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public EmailConfirmationTokenEntity EmailConfirmationToken { get; set; }
    public ICollection<TemplateEntity> Templates { get; set; }
}