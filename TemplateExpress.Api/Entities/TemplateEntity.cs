namespace TemplateExpress.Api.Entities;

public class TemplateEntity
{
    public long Id { get; set; }
    public string? TemplateName { get; set; }
    public short Width { get; set; }
    public short Height { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public int UserId { get; set; }
}