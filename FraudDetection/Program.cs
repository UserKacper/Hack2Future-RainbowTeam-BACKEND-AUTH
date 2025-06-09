using FraudDetection.Database.Models;
using FraudDetection.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using FraudDetection.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Stałe dla nazw konfiguracji i CORS
const string CorsPolicyDevelopment = "Development";
const string CorsPolicyProduction = "Production";

const string ConfigJwtIssuer = "JWT_ISSUER";
const string ConfigJwtAudience = "JWT_AUDIENCE";
const string ConfigJwtSecret = "SECRET_JWT";
const string ConfigDbConnection = "DB_CONNECTION_STRING";

// Pobieramy i weryfikujemy konfigurację
var connectionString = builder.Configuration[ConfigDbConnection];
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Database connection string is not configured.");
}

var jwtIssuer = builder.Configuration[ConfigJwtIssuer]
    ?? (builder.Environment.IsDevelopment() ? "http://localhost:5000" : "https://0.0.0.0:443");

var jwtAudience = builder.Configuration[ConfigJwtAudience]
    ?? (builder.Environment.IsDevelopment() ? "http://localhost:5000" : "https://0.0.0.0:443");

var jwtSecret = builder.Configuration[ConfigJwtSecret];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("JWT secret key is not configured.");
}

// Dodajemy serwisy
builder.Services.AddControllers();

builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<AppUser, AppUserRoles>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<DatabaseContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyDevelopment, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    var prodOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
                      ?? new[] { "https://claimguardai.com", "https://www.claimguardai.com" };

    options.AddPolicy(CorsPolicyProduction, policy =>
    {
        policy.WithOrigins(prodOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fraud Detection API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT in format: 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

var app = builder.Build();

// Po zbudowaniu aplikacji możemy już logować
var logger = app.Logger;
logger.LogInformation("Starting Fraud Detection API...");



using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    db.Database.Migrate();
}


// Używamy CORS wg środowiska
app.UseCors(builder.Environment.IsDevelopment() ? CorsPolicyDevelopment : CorsPolicyProduction);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fraud Detection API v1"));
}

// HTTPS tylko poza developmentem
if (!builder.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
