using ApiGestionTurnosMedicos.Config;
using ApiGestionTurnosMedicos.Middlewares;
using ApiGestionTurnosMedicos.Services;
using BusinessLogic.AppLogic;
using BusinessLogic.AppLogic.Services;
using DataAccess.Data;
using DataAccess.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using DotNetEnv;
using System.IO;

// Cargar .env
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath)) Env.Load(envPath);

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environment = builder.Environment;

// Sobrescribir configuración desde .env
configuration["ConnectionStrings:DefaultConnection"] = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
configuration["JwtSettings:Key"] = Environment.GetEnvironmentVariable("JWT_KEY");
configuration["JwtSettings:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER");
configuration["JwtSettings:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
configuration["EmailSettings:Username"] = Environment.GetEnvironmentVariable("EMAIL_USERNAME");
configuration["EmailSettings:Password"] = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
configuration["AllowedApiKeys"] = Environment.GetEnvironmentVariable("API_KEYS");

// Validar secretos críticos
if (string.IsNullOrEmpty(configuration["JwtSettings:Key"])) throw new InvalidOperationException("JWT_KEY no configurado");
if (string.IsNullOrEmpty(configuration["ConnectionStrings:DefaultConnection"])) throw new InvalidOperationException("DB_CONNECTION_STRING no configurado");

// POCOs
builder.Services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
builder.Services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

// API Keys
var allowedApiKeysArray = configuration["AllowedApiKeys"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? Array.Empty<string>();
builder.Services.AddSingleton(allowedApiKeysArray);

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontCors", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChronoMed API", Version = "v1.6" });
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "X-API-KEY",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Ingresa tu API Key válida"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" } }, Array.Empty<string>() },
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

// DbContext
builder.Services.AddDbContext<GestionTurnosContext>(opt =>
    opt.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
);

// DI Servicios
builder.Services.AddTransient<IMessage, Message>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddScoped<AdminSetupService>();
builder.Services.AddScoped<TurnoRepository>();
builder.Services.AddScoped<PacienteRepository>();
builder.Services.AddScoped<PacienteLogic>();
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<EmailService>();

// JWT Authentication
var jwtCfg = configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtCfg["Key"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtCfg["Issuer"],
            ValidAudience = jwtCfg["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChronoMed API v1.6"));
}

app.UseCors("FrontCors");

app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
    }
    else
    {
        await next();
    }
});

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.MapPost("/sendEmail", (SendEmailRequest req, IMessage svc) =>
{
    svc.SendEmail(req.Subject, req.Body, req.To);
});

app.Run();