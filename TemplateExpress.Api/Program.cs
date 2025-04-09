using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TemplateExpress.Api.Data;
using TemplateExpress.Api.Dto.UserDto;
using TemplateExpress.Api.Interfaces.Services;
using TemplateExpress.Api.Interfaces.Repositories;
using TemplateExpress.Api.Interfaces.Security;
using TemplateExpress.Api.Options;
using TemplateExpress.Api.Services;
using TemplateExpress.Api.Repositories;
using TemplateExpress.Api.Security;
using TemplateExpress.Api.Validations;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");

builder.Services.AddDbContext<DataContext>(opt => opt.UseNpgsql(connectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenManager, TokenManager>();
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.Section));


builder.Services.AddScoped<IValidator<CreateUserDto>, UserValidator>();

builder.Services.AddControllers();

var app = builder.Build();
// app.UseMiddleware<GlobalErrorHandlingMiddleware>();
app.MapControllers();
app.Run();