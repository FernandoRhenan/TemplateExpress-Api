namespace TemplateExpress.Api.Entities;
 
public class EmailConfirmationTokenEntity
{
    public long Id { get; set; }
    public string Token { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long UserId { get; set; }
    public UserEntity User { get; set; }
}