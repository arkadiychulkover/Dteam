using System.Text;
using DteamBackend.Configuration;
using DteamBackend.Data;
using DteamBackend.Hubs;
using DteamBackend.Interfaces;
using DteamBackend.Middlewares;
using DteamBackend.Services;
using DteamBackend.BackgroundServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;

namespace DteamBackend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Support dynamic PORT assigned by Railway / Cloud providers
            var port = Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrEmpty(port))
            {
                builder.WebHost.UseUrls($"http://+:{port}");
            }

            builder.Services.AddDbContextFactory<AppDbContext>(options =>
            {
                var connStr = ResolveConnectionString(builder.Configuration);
                if (connStr.Contains("Host=", StringComparison.OrdinalIgnoreCase) || connStr.Contains("Server=", StringComparison.OrdinalIgnoreCase))
                {
                    options.UseNpgsql(connStr);
                }
                else
                {
                    options.UseSqlite(connStr);
                }
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            });
            builder.Services.AddScoped(p => p.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

            builder.Services.AddHttpClient();

            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddScoped<IInitDataService, InitDataService>();
            builder.Services.AddScoped<IRewardSettingsService, RewardSettingsService>();
            builder.Services.AddScoped<IActivityService, ActivityService>();
            builder.Services.AddScoped<TonService>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.Configure<EthereumOptions>(builder.Configuration.GetSection(EthereumOptions.SectionName));
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IHardhatTokenService, HardhatTokenService>();
            builder.Services.AddScoped<INftService, NftService>();
            builder.Services.AddHostedService<NftTransferListenerService>();
            builder.Services.AddHostedService<OnlineTimeRewardService>();
            builder.Services.AddHostedService<HardhatNodeManagerService>();

            builder.Services.Configure<ChatOptions>(builder.Configuration.GetSection(ChatOptions.SectionName));
            builder.Services.AddSingleton<IChatFileStorage, LocalChatFileStorage>();
            builder.Services.AddScoped<IChatRealtimeNotifier, SignalRChatRealtimeNotifier>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();

            builder.Services.AddSignalR();
            builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Smtp"));
            builder.Services.AddTransient<IEmailService, SmtpEmailService>();
            builder.Services.AddTransient<RecommendationService>();

            builder.Services.AddSingleton<ITelegramAlertService, TelegramAlertService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("DteamCorsPolicy", policy =>
                {
                    var frontendUrlEnv = builder.Configuration["FRONTEND_URL"] ?? builder.Configuration["FrontendUrl"];
                    var defaultOrigins = new List<string>
                    {
                        "https://dteam-app.vercel.app",
                        "http://localhost:5173",
                        "https://localhost:5173",
                        "http://127.0.0.1:5173",
                        "https://127.0.0.1:5173",
                        "http://localhost:5174",
                        "http://localhost:3000",
                        "http://localhost:5117",
                        "https://localhost:7264"
                    };

                    if (!string.IsNullOrWhiteSpace(frontendUrlEnv))
                    {
                        var customOrigins = frontendUrlEnv.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        defaultOrigins.AddRange(customOrigins);
                    }

                    policy.SetIsOriginAllowed(origin =>
                          {
                              if (string.IsNullOrWhiteSpace(origin)) return false;
                              try
                              {
                                  var uri = new Uri(origin);
                                  var host = uri.Host;
                                  return host == "localhost"
                                      || host == "127.0.0.1"
                                      || host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase)
                                      || defaultOrigins.Any(o => Uri.TryCreate(o, UriKind.Absolute, out var u) && u.Host.Equals(host, StringComparison.OrdinalIgnoreCase));
                              }
                              catch
                              {
                                  return false;
                              }
                          })
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddFixedWindowLimiter("AuthLimiter", opt =>
                {
                    opt.PermitLimit = 10;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter("PaymentLimiter", opt =>
                {
                    opt.PermitLimit = 15;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                });

                options.AddSlidingWindowLimiter("GeneralLimiter", opt =>
                {
                    opt.PermitLimit = 120;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.SegmentsPerWindow = 4;
                    opt.QueueLimit = 0;
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
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"].ToString();
                        if (string.IsNullOrEmpty(accessToken))
                        {
                            accessToken = context.Request.Query["token"].ToString();
                        }

                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/hubs") ||
                             path.StartsWithSegments("/hub") ||
                             path.StartsWithSegments("/api/chat/media") ||
                             path.StartsWithSegments("/api/chat/uploads")))
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
                    await context.Database.EnsureCreatedAsync();

                    var initDataService = services.GetRequiredService<IInitDataService>();
                    await initDataService.EnsureAllSchemasAsync(context);
                    await initDataService.InitializeAsync(context);

                    var nftService = services.GetRequiredService<INftService>();
                    await nftService.EnsureNftCollectionInitializedAsync();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Ошибка при инициализации начальных данных в базе данных.");
                }
            }

            var forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardedHeadersOptions.KnownIPNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeadersOptions);

            // Глобальный перехватчик исключений и отправка алертов в Telegram
            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            app.UseRouting();

            app.UseCors("DteamCorsPolicy");

            app.UseRateLimiter();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dteam API v1");
                c.RoutePrefix = "swagger";
            });

            // Ensure upload and static asset directories exist
            try
            {
                var webRoot = app.Environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
                if (!Directory.Exists(webRoot))
                {
                    Directory.CreateDirectory(webRoot);
                }
                foreach (var subDir in new[] { "uploads", "game_images", "community", "comunity", "storage" })
                {
                    var dirPath = Path.Combine(webRoot, subDir);
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogWarning(ex, "Failed to ensure static upload folders exist.");
            }

            var contentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings[".mp4"] = "video/mp4";
            contentTypeProvider.Mappings[".webm"] = "video/webm";
            contentTypeProvider.Mappings[".mov"] = "video/quicktime";
            contentTypeProvider.Mappings[".m4v"] = "video/x-m4v";
            contentTypeProvider.Mappings[".webp"] = "image/webp";
            contentTypeProvider.Mappings[".zip"] = "application/zip";

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = contentTypeProvider
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<CheckBannedUserMiddleware>();

            var isContainer = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"))
                           || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT"));
            if (!app.Environment.IsDevelopment() && !isContainer)
            {
                app.UseHttpsRedirection();
            }

            app.MapControllers();

            app.MapHub<FriendsHub>("/hubs/friends");
            app.MapHub<FriendsHub>("/hub/friends");
            app.MapHub<OnlineHub>("/hubs/online");
            app.MapHub<OnlineHub>("/hub/online");
            app.MapHub<ChatHub>("/hubs/chat");
            app.MapHub<ChatHub>("/hub/chat");
            app.MapHub<NotificationHub>("/hubs/notifications");
            app.MapHub<NotificationHub>("/hub/notifications");

            // Initialize default reward settings and ensure table exists
            try
            {
                using var scope = app.Services.CreateScope();
                var rewardService = scope.ServiceProvider.GetRequiredService<IRewardSettingsService>();
                await rewardService.GetSettingsAsync();
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogWarning(ex, "Failed to initialize reward settings on startup.");
            }

            await app.RunAsync();
        }

        private static string ResolveConnectionString(IConfiguration configuration)
        {
            // 1. Высший приоритет: Environment переменные (Railway, Docker, Production)

            // 1.1 Явный ConnectionStrings__DefaultConnection в Environment
            var envDefaultConn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                              ?? Environment.GetEnvironmentVariable("ConnectionStrings:DefaultConnection");
            if (!string.IsNullOrWhiteSpace(envDefaultConn))
            {
                Console.WriteLine("[Database] Using connection string from ConnectionStrings__DefaultConnection environment variable.");
                return ParseIfNeeded(envDefaultConn);
            }

            // 1.2 Дефолтный ключ Railway / Cloud - DATABASE_URL или DATABASE_PUBLIC_URL
            var envDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
                              ?? Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL");
            if (!string.IsNullOrWhiteSpace(envDatabaseUrl))
            {
                Console.WriteLine("[Database] Using connection string from DATABASE_URL environment variable.");
                return ParseIfNeeded(envDatabaseUrl);
            }

            // 1.3 Дефолтные ключи Railway PostgreSQL (PGHOST, PGDATABASE, PGUSER, PGPASSWORD, PGPORT)
            var pgHost = Environment.GetEnvironmentVariable("PGHOST");
            var pgDb = Environment.GetEnvironmentVariable("PGDATABASE");
            if (!string.IsNullOrWhiteSpace(pgHost) || !string.IsNullOrWhiteSpace(pgDb))
            {
                var host = string.IsNullOrWhiteSpace(pgHost) ? "localhost" : pgHost;
                var port = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
                var user = Environment.GetEnvironmentVariable("PGUSER") ?? "postgres";
                var pass = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "";
                var db = string.IsNullOrWhiteSpace(pgDb) ? "railway" : pgDb;

                Console.WriteLine($"[Database] Configured from PG* environment variables (Host={host}, Port={port}, Db={db}, User={user})");
                return $"Host={host};Port={port};Database={db};Username={user};Password={pass};SSL Mode=Prefer;Trust Server Certificate=true;";
            }

            // 2. Если в Environment ничего нет, берем из Configuration (appsettings.json)
            var connStr = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(connStr))
            {
                return ParseIfNeeded(connStr);
            }

            // 3. Fallback по умолчанию
            Console.WriteLine("[Database] Fallback to local SQLite database (dteam.db).");
            return "Data Source=dteam.db";
        }

        private static string ParseIfNeeded(string connStr)
        {
            if (connStr.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                connStr.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var uri = new Uri(connStr);
                    var userInfo = uri.UserInfo.Split(':');
                    var user = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "postgres";
                    var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
                    var db = uri.AbsolutePath.TrimStart('/');
                    if (string.IsNullOrWhiteSpace(db)) db = "railway";
                    var port = uri.Port > 0 ? uri.Port : 5432;

                    Console.WriteLine($"[Database] Parsed postgres URI connection string (Host={uri.Host}, Port={port}, Db={db})");
                    return $"Host={uri.Host};Port={port};Database={db};Username={user};Password={pass};SSL Mode=Prefer;Trust Server Certificate=true;";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Database] Failed to parse DATABASE_URL URI: {ex.Message}. Using raw string.");
                }
            }

            if (connStr.Contains("Host=", StringComparison.OrdinalIgnoreCase) || connStr.Contains("Server=", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("[Database] Using PostgreSQL connection string.");
            }
            else
            {
                Console.WriteLine("[Database] Using SQLite connection string.");
            }

            return connStr;
        }
    }
}
