using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Bank_Slip_Scanner_App.Data;
using Bank_Slip_Scanner_App.Services;

var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
// ═══════════════════════════════════════════════════════════
//  1. DATABASE
// ═══════════════════════════════════════════════════════════
                    (Microsoft.EntityFrameworkCore.ServerVersion)builder.Services.AddDbContext<ApplicationDbContext>(options =>
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// ═══════════════════════════════════════════════════════════
//  2. SERVICES - SPRINT 1 + SPRINT 2
// ═══════════════════════════════════════════════════════════

// Sprint 1 - Authentication
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Sprint 2 - PDF Processing
//builder.Services.AddScoped<IPdfService, PdfService>();
//builder.Services.AddScoped<IParsingService, ParsingService>();

// ═══════════════════════════════════════════════════════════
//  3. JWT AUTHENTICATION
// ═══════════════════════════════════════════════════════════

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT SecretKey is not configured in appsettings.json");
}

builder.Services
    .AddAuthentication(options =>
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
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ═══════════════════════════════════════════════════════════
//  4. CORS
// ═══════════════════════════════════════════════════════════

builder.Services.AddCors(options =>
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()));

// ═══════════════════════════════════════════════════════════
//  5. CONTROLLERS + SWAGGER
// ═══════════════════════════════════════════════════════════

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Bank Slip Scanner API",
        Version = "v1"
    });
});

// ═══════════════════════════════════════════════════════════
//  BUILD
// ═══════════════════════════════════════════════════════════
        builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();

// ═══════════════════════════════════════════════════════════
//  MIDDLEWARE
// ═══════════════════════════════════════════════════════════

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();

// IMPORTANT: L'ordre est critique!
app.UseCors("AllowAngular");      // 1. CORS d'abord
app.UseAuthentication();          // 2. Authentication
app.UseAuthorization();           // 3. Authorization
app.MapControllers();             // 4. Controllers
        app.UseAuthorization();
app.Run();        app.Run();
    }
}