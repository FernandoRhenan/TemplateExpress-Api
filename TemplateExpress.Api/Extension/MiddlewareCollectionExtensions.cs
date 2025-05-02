using TemplateExpress.Api.Middlewares;

namespace TemplateExpress.Api.Extension;

public static class MiddlewareCollectionExtensions
{
    public static IServiceCollection AddAllMiddlewares(this IServiceCollection services)
    {
        services.AddTransient<GlobalExceptionHandlingMiddleware>();

        return services;
    }
}