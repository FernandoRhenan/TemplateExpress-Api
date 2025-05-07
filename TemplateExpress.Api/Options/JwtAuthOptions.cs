namespace TemplateExpress.Api.Options;

public class JwtAuthOptions
{
    public const string Section = "JwtAuthOptions";
    public string? Secret { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    
    public bool RequireHttpsMetadata { get; set; }
    
}