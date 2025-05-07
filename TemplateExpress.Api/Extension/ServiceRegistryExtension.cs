using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Repositories;
using TemplateExpress.Api.Interfaces.Security;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Interfaces.Utils;
using TemplateExpress.Api.Middlewares;
using TemplateExpress.Api.Options;
using TemplateExpress.Api.Repositories;
using TemplateExpress.Api.Security;
using TemplateExpress.Api.Services;
using TemplateExpress.Api.Utils;
using TemplateExpress.Api.Validations.Users;

namespace TemplateExpress.Api.Extension;

public static class ServiceRegistryExtension
{
    public static IServiceCollection AddInputValidatorsExtension(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateUserDto>, CreateUserValidator>();
        services.AddTransient<IValidator<EmailAndPasswordDto>, LoginUserValidator>();
        
        return services;
    }

    public static IServiceCollection AddApiServicesExtension(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmailService, EmailService>();
        
        return services;
    }
    
    public static IServiceCollection AddApiRepositoriesExtension(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    } 
    
    public static IServiceCollection AddApiUtilsExtension(this IServiceCollection services)
    {
        services.AddScoped<IBCryptUtil, BCryptUtil>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        
        return services;
    }
    public static IServiceCollection AddApiSecuritiesExtension(this IServiceCollection services)
    {
        services.AddScoped<ITokenManager, TokenManager>();
        
        return services;
    }

    public static IServiceCollection AddErrorHandlersExtension(this IServiceCollection services)
    {
        services.AddTransient<GlobalExceptionHandlingMiddleware>();

        return services;
    }

    public static IServiceCollection AddAuthenticationExtension(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtAuthOptions = configuration
            .GetSection(JwtAuthOptions.Section)
            .Get<JwtAuthOptions>();
        
        if(string.IsNullOrWhiteSpace(jwtAuthOptions?.Secret)) throw new InvalidOperationException("Missing JWT Secret."); 

        var key = Encoding.UTF8.GetBytes(jwtAuthOptions.Secret);
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = jwtAuthOptions.RequireHttpsMetadata;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtAuthOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAuthOptions.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.FromMinutes(2)
                };
            });

        return services;
    }
    
    public static IServiceCollection AddAuthorizationExtension(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"))
            .AddPolicy("User", policy =>
                policy.RequireRole("User"));

        return services;
    }
}