namespace TemplateExpress.Api.Options;

public class JwtAccountConfirmationOptions
{
    public const string Section = "JwtAccountConfirmationOptions";
    public string? Secret { get; set; }
    public string? Issuer { get; set; }
    
    public string? Authority { get; set; }
    public string? Audience { get; set; }
    
    public bool RequireHttpsMetadata { get; set; }
}