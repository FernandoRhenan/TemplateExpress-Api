using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TemplateExpress.Api.Data;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Interfaces.Repositories;
using TemplateExpress.Api.Interfaces.Security;
using TemplateExpress.Api.Interfaces.Utils;
using TemplateExpress.Api.Middlewares;
using TemplateExpress.Api.Options;
using TemplateExpress.Api.Services;
using TemplateExpress.Api.Repositories;
using TemplateExpress.Api.Security;
using TemplateExpress.Api.Utils;
using TemplateExpress.Api.Validations.Users;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");

builder.Services.AddDbContext<DataContext>(opt => opt.UseNpgsql(connectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddScoped<ITokenManager, TokenManager>();
builder.Services.AddScoped<IBCryptUtil, BCryptUtil>();

builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();

// TODO Add it in a extension method.
builder.Services.AddScoped<IValidator<CreateUserDto>, CreateUserValidator>();
builder.Services.AddScoped<IValidator<EmailAndPasswordDto>, LoginUserValidator>();

builder.Services.Configure<JwtConfirmationOptions>(
    builder.Configuration.GetSection(JwtConfirmationOptions.Section));

builder.Services.Configure<EmailConfiguration>(
    builder.Configuration.GetSection(EmailConfiguration.Section));


builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.MapControllers();
app.Run();