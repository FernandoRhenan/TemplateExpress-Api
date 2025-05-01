using FluentValidation;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Repositories;
using TemplateExpress.Api.Interfaces.Security;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Interfaces.Utils;
using TemplateExpress.Api.Repositories;
using TemplateExpress.Api.Security;
using TemplateExpress.Api.Services;
using TemplateExpress.Api.Utils;
using TemplateExpress.Api.Validations.Users;

namespace TemplateExpress.Api.Extension;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInputValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateUserDto>, CreateUserValidator>();
        services.AddScoped<IValidator<EmailAndPasswordDto>, LoginUserValidator>();
        
        return services;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddTransient<IEmailService, EmailService>();
        
        return services;
    }
    
    public static IServiceCollection AddApiRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    } 
    
    public static IServiceCollection AddApiUtils(this IServiceCollection services)
    {
        services.AddScoped<IBCryptUtil, BCryptUtil>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        
        return services;
    }
    public static IServiceCollection AddApiSecurities(this IServiceCollection services)
    {
        services.AddScoped<ITokenManager, TokenManager>();
        
        return services;
    }
}