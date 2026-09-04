using System.Text;
using DteamBackend.Data;
using DteamBackend.Hubs;
using DteamBackend.Interfaces;
using DteamBackend.Middlewares;
using DteamBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace DteamBackend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContextFactory<AppDbContext>(options =>
            {
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            });
            builder.Services.AddScoped(p => p.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

            builder.Services.AddHttpClient();

            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddScoped<IInitDataService, InitDataService>();
            builder.Services.AddScoped<TonService>();

            builder.Services.AddSignalR();
            builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Smtp"));
            builder.Services.AddTransient<IEmailService, SmtpEmailService>();
            builder.Services.AddTransient<RecommendationService>();

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
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/hubs") || path.StartsWithSegments("/hub")))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"[JWT Bearer] Auth failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Dteam API",
                    Version = "v1",
                    Description = "API documentation for Dteam Backend"
                });

                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Введите JWT токен (авторизация Bearer)"
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    await context.Database.MigrateAsync();

                    var gamesNeedingVector = await context.Games.ToListAsync();
                    var changed = false;
                    foreach (var game in gamesNeedingVector)
                    {
                        if (game.TasteVector.All(v => Math.Abs(v) < 1e-6f))
                        {
                            game.RecalculateTasteVector();
                            changed = true;
                        }
                    }
                    if (changed) await context.SaveChangesAsync();

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

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dteam API v1");
                c.RoutePrefix = "swagger";
            });

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<CheckBannedUserMiddleware>();

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.MapControllers();
            app.MapHub<FriendsHub>("/hubs/friends");
            app.MapHub<FriendsHub>("/hub/friends");

            await app.RunAsync();
        }
    }
}

