using ApiGestionTurnosMedicos.Config;
using ApiGestionTurnosMedicos.Middlewares;
using ApiGestionTurnosMedicos.Services;
using ApiGestionTurnosMedicos.Validations;
using BusinessLogic.AppLogic;
using BusinessLogic.AppLogic.Services;
using DataAccess.Context;
using DataAccess.Data;
using DataAccess.Repository;
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
// 1. Configuración de Logging
// ---------------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ---------------------------------------------------------
// 2. Configuración de Base de Datos (Optimizado para Azure)
// ---------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<GestionTurnosContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // Resiliencia: Reintentos automáticos para fallos transitorios en la nube
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });

    if (builder.Environment.IsDevelopment())
    {
        // Permite ver datos de parámetros en los logs de error (solo desarrollo)
        options.EnableSensitiveDataLogging();
    }
});

// ---------------------------------------------------------
// 3. Seguridad, JWT y Configuración (POCOs)
// ---------------------------------------------------------
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var keyString = jwtSettings["Key"] ?? throw new ArgumentNullException("JWT Key missing in configuration.");
var key = Encoding.UTF8.GetBytes(keyString);

builder.Services.Configure<JwtSettings>(jwtSettings);
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("GmailSettings"));

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

            // 🔥 CLAVE PARA QUE [Authorize(Roles="Admin")] FUNCIONE
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier,

            ClockSkew = TimeSpan.Zero
        };
    });

// ---------------------------------------------------------
// 4. Inyección de Dependencias (DI) - Repositorios y Lógica
// ---------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(ConfigureSwagger);

// --- REPOSITORIOS (Capa DataAccess) ---
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<PacienteRepository>();
builder.Services.AddScoped<MedicoRepository>();
builder.Services.AddScoped<TurnoRepository>();
builder.Services.AddScoped<EspecialidadRepository>();
builder.Services.AddScoped<EstadoRepository>();

// --- LÓGICA DE NEGOCIO (Capa BusinessLogic) ---
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AdminSetupService>(); // Resuelve error 500
builder.Services.AddScoped<PacienteLogic>();
builder.Services.AddScoped<MedicoLogic>();
builder.Services.AddScoped<TurnoLogic>();
builder.Services.AddScoped<EspecialidadLogic>();
builder.Services.AddScoped<EstadoLogic>();

// --- VALIDACIONES ---
builder.Services.AddScoped<ValidationsMethodPost>();
builder.Services.AddScoped<ValidationsMethodPut>();


// --- SERVICIOS TRANSVERSALES ---
builder.Services.AddScoped<EmailService>();
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddTransient<IMessage, Message>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// --- CONFIGURACIÓN API KEYS ---
var allowedApiKeys = builder.Configuration.GetSection("AllowedApiKeys").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddSingleton<IList<string>>(allowedApiKeys);

// --- CORS ---
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
// 5. Pipeline de Middlewares (Orden Crítico)
// ---------------------------------------------------------

// Manejo global de excepciones (siempre primero)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChronoMed API v1.6");
    });

    // TEST DE CONEXIÓN REAL AL INICIAR
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

// ---------------------------------------------------------
// Helper: Configuración de Swagger (Seguridad)
// ---------------------------------------------------------
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