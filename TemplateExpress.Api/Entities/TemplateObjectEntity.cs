namespace TemplateExpress.Api.Entities;

public class TemplateObjectEntity
{
    public long Id { get; set; }
    public string? FieldName { get; set; } 
    public bool Italic { get; set; }
    public bool Bold { get; set; }
    public short FontSize { get; set; }
    public string FontFamily { get; set; }
    public string FillStyle { get; set; }
    public short XPoint { get; set; }
    public short YPoint { get; set; }
    public short BaseBoxHeight { get; set; }
    public short FontBoundingBoxDescent { get; set; }
    public short MaxRows { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public long TemplateId { get; set; }
    public TemplateEntity Template { get; set; }
    }