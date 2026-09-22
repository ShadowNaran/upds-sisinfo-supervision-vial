using System.Text;
using System.Text.Json.Serialization;
using Arooaf.Api.Data;
using Arooaf.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// configuracion jwt hu-01 tarea 7
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Falta la sección 'Jwt' en appsettings.json.");

if (jwt.SecretKey.Length < 32)
{
    throw new InvalidOperationException("La clave secreta JWT debe tener al menos 32 caracteres.");
}

builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// base de datos ep-04 hu-17 inmemory para desarrollo sin conexion local
// npgsql postgresql para produccion cambiar en appsettings json
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var provider = builder.Configuration.GetValue<string>("Database:Provider");

    if (string.Equals(provider, "Postgres", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
    }
    else
    {
        options.UseInMemoryDatabase("ArooafInMemoryDev");
    }
});

// scalar para documentacion api (clase readme)
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevWeb", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// seed de usuarios de inicio hu-01 con inmemory se recrean en cada arranque
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var provider = builder.Configuration.GetValue<string>("Database:Provider");

    if (!string.Equals(provider, "Postgres", StringComparison.OrdinalIgnoreCase))
    {
        db.Database.EnsureCreated();
    }

    AppDbSeeder.Seed(db);
}

// scalar en desarrollo (clase readme)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("DevWeb");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}