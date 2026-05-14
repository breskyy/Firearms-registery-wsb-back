using System.Text;
using EWeaponRegistry.Api.Middleware;
using EWeaponRegistry.Infrastructure;
using EWeaponRegistry.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Infrastructure services (DbContext, Services, Gateways)
builder.Services.AddInfrastructure(builder.Configuration);

// Add HttpContextAccessor for audit service
builder.Services.AddHttpContextAccessor();

// Add controllers
builder.Services.AddControllers();

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EWeaponRegistry API",
        Version = "v1",
        Description = """
            Cyfrowa Rejestracja i Obsługa Broni Palnej - REST API

            System centralnego rejestru broni palnej na rynku cywilnym.

            **UWAGA:** Ten system jest projektem studenckim/demonstracyjnym.
            Integracje zewnętrzne (mObywatel, login.gov.pl, płatności, WPA) są MOCKOWANE.
            System NIE łączy się z prawdziwymi usługami zewnętrznymi.

            **Bezpieczeństwo:**
            - Dane wrażliwe (PESEL, dane osobowe) są szyfrowane AES-256
            - Wszystkie krytyczne operacje są logowane w audit log
            - Produkcyjnie wymagane HTTPS/TLS 1.3
            """,
        Contact = new OpenApiContact
        {
            Name = "Projekt Studencki WSB"
        }
    });

    // JWT Bearer authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token. Example: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// CORS configuration for frontend integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed database on startup. SeedData skips if users already exist,
// so this is safe to run on every startup (including Production/Railway).
await SeedData.InitializeAsync(app.Services);

// Configure the HTTP request pipeline
app.UseGlobalExceptionHandler();

// Swagger UI (Development only in production, but enabled here for demo)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "EWeaponRegistry API v1");
    options.RoutePrefix = string.Empty; // Swagger at root
});

// HTTPS redirection - commented for Docker/local development
// In production, configure HTTPS/TLS 1.3 at reverse proxy level
// app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Startup logging
app.Logger.LogInformation("EWeaponRegistry API started");
app.Logger.LogInformation("Swagger UI available at: http://localhost:5000/");
app.Logger.LogInformation("Health check: http://localhost:5000/api/v1/health");

app.Run();
