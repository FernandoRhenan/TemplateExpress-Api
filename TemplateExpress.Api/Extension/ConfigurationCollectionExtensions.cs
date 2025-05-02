using TemplateExpress.Api.Options;

namespace TemplateExpress.Api.Extension;

public static class ConfigurationCollectionExtensions
{
    public static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtAccountConfirmationOptions>(
            configuration.GetSection(JwtAccountConfirmationOptions.Section));

        services.Configure<JwtAuthOptions>(
            configuration.GetSection(JwtAuthOptions.Section));
        
        services.Configure<EmailConfigurationOptions>(
            configuration.GetSection(EmailConfigurationOptions.Section));
        
        return services;
    }
    
}