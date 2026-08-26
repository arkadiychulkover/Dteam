using System.Text;
using DteamBackend.Data;
using DteamBackend.Interfaces;
using DteamBackend.Middlewares;
using DteamBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;

namespace DteamBackend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Database Context (PostgreSQL)
            builder.Services.AddDbContextFactory<AppDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            });
            builder.Services.AddScoped(p => p.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

            // HTTP Client
            builder.Services.AddHttpClient();

            // Application Services
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddScoped<IInitDataService, InitDataService>();
            builder.Services.AddScoped<TonService>();

            // SMTP Email Service Configuration
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Smtp"));
            builder.Services.AddTransient<IEmailService, SmtpEmailService>();

            // CORS Policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("DteamCorsPolicy", policy =>
                {
                    policy.WithOrigins(
                              "http://localhost:5173",
                              "https://localhost:5173",
                              "http://127.0.0.1:5173",
                              "https://127.0.0.1:5173",
                              "http://localhost:5174",
                              "http://localhost:3000",
                              "http://localhost:5117",
                              "https://localhost:7264")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // JWT Authentication Setup
            var secretKey = builder.Configuration["Jwt:Secret"] 
                ?? "DteamSuperSecretJwtKey2026_dteam_io_security_token_key_spec_32bytes_long";
            var issuer = builder.Configuration["Jwt:Issuer"] ?? "DteamBackend";
            var audience = builder.Configuration["Jwt:Audience"] ?? "DteamApp";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"[JWT Bearer] Auth failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddControllers();

            var app = builder.Build();

            // Automatic Database Migration and Seed
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    await context.Database.MigrateAsync();

                    var initDataService = services.GetRequiredService<IInitDataService>();
                    await initDataService.InitializeAsync(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Ошибка при инициализации начальных данных в базе данных.");
                }
            }

            app.UseCors("DteamCorsPolicy");

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<CheckBannedUserMiddleware>();

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
