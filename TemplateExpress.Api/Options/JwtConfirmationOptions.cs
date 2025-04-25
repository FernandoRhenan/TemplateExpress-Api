namespace TemplateExpress.Api.Options;

public class JwtConfirmationOptions
{
    public const string Section = "JwtConfirmationOptions";
    public string? Secret { get; set; }
}