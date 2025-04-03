namespace TemplateExpress.Api.Entities;

public class TemplateEntity
{
    public TemplateEntity()
    {
        TemplateObjects = new List<TemplateObjectEntity>();
    }
    public long Id { get; set; }
    public string? TemplateName { get; set; }
    public short Width { get; set; }
    public short Height { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public long UserId { get; set; }
    public required UserEntity User { get; set; }
    public ICollection<TemplateObjectEntity> TemplateObjects { get; set; }
}
