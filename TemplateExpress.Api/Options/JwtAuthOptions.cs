namespace TemplateExpress.Api.Options;

public class JwtAuthOptions
{
    public const string Section = "JwtAuthOptions";
    public string? Secret { get; set; }
}