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

// Add services to the container.

builder.Services.AddControllers();

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
        ValidIssuer = "http://localhost:8000",
        ValidAudience = "http://localhost:8000",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret:jwt"))
    };
});


builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "development",
        builder =>
        {
            builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
    options.AddPolicy(
      "prod",
      builder =>
      {
          builder.WithOrigins("https://frontend.com").AllowAnyMethod().AllowAnyHeader();
      }
  );
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fraud Detection API", Version = "v1" });
    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer {token}\"",
        }
    );

    

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                new string[] { }
            },
        }
    );
});

// Configure Entity Framework Core with PostgreSQL
builder.Services.AddDbContext<DatabaseContext>(options =>
   options.UseNpgsql(builder.Configuration["db:connection"]));

builder.Services.AddIdentity<AppUser, AppUserRoles>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddEntityFrameworkStores<DatabaseContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401; // return 401 instead of redirect
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403; // return 403 instead of redirect
        return Task.CompletedTask;
    };
});

builder.Services.AddScoped<UserManager<AppUser>>();
builder.Services.AddScoped<RoleManager<AppUserRoles>>();
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

if(app.Environment.IsProduction())
{
    app.UseCors("prod");
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("development");
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fraud Detection API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication(); // 🔥 Add this line before UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();