namespace TemplateExpress.Api.Entities;

public class EmailConfirmationTokenEntity
{
    public int Id { get; set; }
    public Guid Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public int UserId { get; set; }
}