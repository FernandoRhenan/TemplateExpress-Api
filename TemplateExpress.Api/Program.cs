using Microsoft.EntityFrameworkCore;
using TemplateExpress.Api.Data;
using TemplateExpress.Api.Extension;
using TemplateExpress.Api.Middlewares;
using TemplateExpress.Api.Options;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");

builder.Services.ConfigureOptions(builder.Configuration);

builder.Services.AddDbContext<DataContext>(opt => opt.UseNpgsql(connectionString));
builder.Services.AddApiRepositories();
builder.Services.AddApiServices();
builder.Services.AddApiSecurities();
builder.Services.AddApiUtils();
builder.Services.AddInputValidators();

builder.Services.AddTransient<GlobalExceptionHandlingMiddleware>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.MapControllers();
app.Run();