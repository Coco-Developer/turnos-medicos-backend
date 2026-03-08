using ApiGestionTurnosMedicos.Config;
using ApiGestionTurnosMedicos.Middlewares;
using ApiGestionTurnosMedicos.Validations;
using BusinessLogic.AppLogic;
using BusinessLogic.AppLogic.Services;
using DataAccess.Context;
using DataAccess.Data;
using DataAccess.Repository;
using DataAccess.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// 1. Logging
// ---------------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ---------------------------------------------------------
// 2. DB
// ---------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddSingleton<EfDbCommandInterceptor>();
builder.Services.AddSingleton<SaveChangesTimingInterceptor>();

builder.Services.AddDbContext<GestionTurnosContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }

    // Registrar interceptores
    options.AddInterceptors(builder.Services.BuildServiceProvider().GetRequiredService<EfDbCommandInterceptor>());
    options.AddInterceptors(builder.Services.BuildServiceProvider().GetRequiredService<SaveChangesTimingInterceptor>());
});

// ---------------------------------------------------------
// 3. JWT + Settings
// ---------------------------------------------------------
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var keyString = jwtSettings["Key"] ?? throw new ArgumentNullException("JWT Key missing in configuration.");
var key = Encoding.UTF8.GetBytes(keyString);

builder.Services.Configure<JwtSettings>(jwtSettings);

// IMPORTANTE: usa el nombre real de tu sección en appsettings.
// Si tu sección es GmailSettings, dejalo así:
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("GmailSettings"));
// Si prefieres "EmailSettings", cambia esta línea y tu appsettings.

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),

            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier,

            ClockSkew = TimeSpan.Zero
        };
    });

// ---------------------------------------------------------
// 4. DI
// ---------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(ConfigureSwagger);

// Repos
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<PacienteRepository>();
builder.Services.AddScoped<MedicoRepository>();
builder.Services.AddScoped<TurnoRepository>();
builder.Services.AddScoped<EspecialidadRepository>();
builder.Services.AddScoped<EstadoRepository>();

// Logic
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AdminSetupService>();
builder.Services.AddScoped<PacienteLogic>();
builder.Services.AddScoped<MedicoLogic>();
builder.Services.AddScoped<TurnoLogic>();
builder.Services.AddScoped<EspecialidadLogic>();
builder.Services.AddScoped<EstadoLogic>();

// Validations
builder.Services.AddScoped<ValidationsMethodPost>();
builder.Services.AddScoped<ValidationsMethodPut>();

// Email (alineado a DI)
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("GmailSettings"));
builder.Services.AddScoped<IMessage, Message>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// API Keys
var allowedApiKeys = builder.Configuration.GetSection("AllowedApiKeys").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddSingleton<IList<string>>(allowedApiKeys);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontCors", policy =>
       policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "https://agreeable-wave-058616a0f.4.azurestaticapps.net"
              )
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ---------------------------------------------------------
// 5. Pipeline
// ---------------------------------------------------------
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChronoMed API v1.6");
    });

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<GestionTurnosContext>();
        try
        {
            var server = db.Database.GetDbConnection().DataSource;
            Console.WriteLine($"🔍 Intentando conectar a: {server}");
            if (db.Database.CanConnect())
            {
                Console.WriteLine("✅ Conexión a Azure SQL exitosa.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error crítico de conexión: {ex.Message}");
        }
    }
}

app.UseCors("FrontCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

void ConfigureSwagger(SwaggerGenOptions c)
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChronoMed API", Version = "v1.6" });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "X-API-KEY",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Clave API necesaria para acceder a ciertos recursos"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT: Bearer {su_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" } },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
}
