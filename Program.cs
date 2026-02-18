using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Bank_Slip_Scanner_App.Data;
using Bank_Slip_Scanner_App.Services;
using static Bank_Slip_Scanner_App.Services.IJwtTokenService;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // base de données sql
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(

                    (Microsoft.EntityFrameworkCore.ServerVersion)builder.Services.AddDbContext<ApplicationDbContext>(options =>
                       options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))

              ));
        // services 
        builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        //JWT Authentification
        var jwt = builder.Configuration.GetSection("JwtSettings");
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(Opt =>
            {
                Opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt["Issuer"],
                    ValidAudience = jwt["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!)),
                    ClockSkew = TimeSpan.Zero
                };
            });
        builder.Services.AddAuthentication();
        // CORS -> Angular sur localhost:4200
        builder.Services.AddCors(opt =>
         opt.AddPolicy("AllowAngular", p =>
          p.WithOrigins("http://localhost:4200")
          .AllowAnyMethod()
          .AllowAnyHeader()));
        // controlles + swagger
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // build
        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseCors("AllowAngular");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}