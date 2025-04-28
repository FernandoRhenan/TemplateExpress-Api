namespace TemplateExpress.Api.Options;

public class EmailConfigurationOptions
{
    public const string Section = "EmailConfiguration";
    
    public string? Host { get; set; }
    public int Port { get; set; }
    public string? EmailSender { get; set; }
    
    public string? Username { get; set; }
    
    public string? Password { get; set; }
    
}
