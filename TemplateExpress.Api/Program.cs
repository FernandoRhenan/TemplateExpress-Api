using Microsoft.EntityFrameworkCore;
using TemplateExpress.Api.Data;
using TemplateExpress.Api.Extension;
using TemplateExpress.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");

builder.Services.ConfigureOptions(builder.Configuration);

builder.Services.AddDbContext<DataContext>(opt => opt.UseNpgsql(connectionString));
builder.Services.AddApiRepositoriesExtension();
builder.Services.AddApiServicesExtension();
builder.Services.AddApiSecuritiesExtension();
builder.Services.AddApiUtilsExtension();
builder.Services.AddInputValidatorsExtension();
builder.Services.AddErrorHandlersExtension();
builder.Services.AddAuthenticationExtension(builder.Configuration);
builder.Services.AddAuthorizationExtension();

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();  

app.MapControllers();
app.Run();