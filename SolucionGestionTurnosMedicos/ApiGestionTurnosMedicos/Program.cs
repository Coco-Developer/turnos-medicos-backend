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

// -----------------------
// 0. Cargar variables de entorno
// -----------------------
Env.Load(); // Lee .env en la raíz del proyecto

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environment = builder.Environment;

// -----------------------
// 1. Sobrescribir configuración sensible desde .env
// -----------------------
builder.Configuration["ConnectionStrings:DefaultConnection"] =
    Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

builder.Configuration["GmailSettings:Username"] =
    Environment.GetEnvironmentVariable("GMAIL_USERNAME");

builder.Configuration["GmailSettings:Password"] =
    Environment.GetEnvironmentVariable("GMAIL_PASSWORD");

builder.Configuration["JwtSettings:Key"] =
    Environment.GetEnvironmentVariable("JWT_KEY");

// Guardamos API_KEYS como string y convertimos a array solo cuando lo usamos
builder.Configuration["AllowedApiKeys"] = Environment.GetEnvironmentVariable("API_KEYS");

// -----------------------
// 2. Controllers
// -----------------------
builder.Services.AddControllers();

// -----------------------
// 3. CORS
// -----------------------
var origenLocalHost = "_origenLocalHost";
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
              .AllowAnyHeader()
              .AllowAnyMethod()
    );

    options.AddPolicy(origenLocalHost, policy =>
        policy.SetIsOriginAllowed(origin =>
        {
            try
            {
                return origin.StartsWith("http://192.168.") || origin.StartsWith("http://localhost");
            }
            catch { return false; }
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
    );
});

// -----------------------
// 4. Swagger / OpenAPI
// -----------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChronoMed API", Version = "v1.6" });

    // ApiKey
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "X-API-KEY",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Ingresa tu API Key válida"
    });

    // JWT Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Authorization: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" } }, Array.Empty<string>() },
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

// -----------------------
// 5. DbContext
// -----------------------
builder.Services.AddDbContext<GestionTurnosContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// -----------------------
// 6. DI: Servicios y repositorios
// -----------------------
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

// -----------------------
// 7. JWT
// -----------------------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtCfg = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtCfg["Key"]!);

// -----------------------
// 8. Autenticación JWT
// -----------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
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

// -----------------------
// 9. Pipeline
// -----------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChronoMed API v1.6"));
}

app.UseCors(origenLocalHost);

// Middleware OPTIONS preflight
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

// -----------------------
// 10. Endpoint email
// -----------------------
app.MapPost("/sendEmail", (SendEmailRequest req, IMessage svc) =>
{
    svc.SendEmail(req.Subject, req.Body, req.To);
});

// -----------------------
// 11. Uso de AllowedApiKeys en runtime
// -----------------------
var allowedApiKeys = builder.Configuration["AllowedApiKeys"]?.Split(',') ?? Array.Empty<string>();

app.Run();