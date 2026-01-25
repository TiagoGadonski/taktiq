// Deployment Version: 2025-12-31-v4-groups-system - Student Groups Feature
// Build timestamp: 2025-12-31T05:30:00Z
// This version includes full student groups management system
using System.Text;
using GymHero.Api.Endpoints; // Vamos criar isso a seguir
using GymHero.Api.Middleware;
using GymHero.Application;
using GymHero.Infrastructure;
using GymHero.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// 1. REGISTRAR SERVIÇOS (Injeção de Dependência)
// ===============================================

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Log startup to help diagnose deployment issues
Log.Information("=== Application Starting ===");
Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);
Log.Information("ProcessId: {ProcessId}", Environment.ProcessId);
Log.Information("MachineName: {MachineName}", Environment.MachineName);

// Configure Kestrel for Azure App Service
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
    serverOptions.Limits.MaxRequestBodySize = 104857600; // 100 MB
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2);
});

// Adiciona os serviços das nossas camadas com os métodos de extensão que criamos
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// Configure DataProtection to persist keys properly in Azure
if (builder.Environment.IsProduction())
{
    // Use /home/site which persists across container restarts in Azure App Service
    var dataProtectionPath = "/home/site/DataProtection-Keys";

    try
    {
        Directory.CreateDirectory(dataProtectionPath);

        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
            .SetApplicationName("GymHero");

        Log.Information("DataProtection keys will be persisted to: {Path}", dataProtectionPath);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to configure persistent DataProtection storage. Falling back to default (ephemeral) storage.");
    }
}

// Configure Redis distributed cache
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "GymHero_";
    });
    Log.Information("Using Redis cache");
}
else
{
    // Fallback to in-memory cache if Redis is not configured
    builder.Services.AddDistributedMemoryCache();
    Log.Information("Using in-memory cache (Redis not configured)");
}

builder.Services.AddHttpClient();
builder.Services.AddHttpClient<GymHero.Api.Services.ExerciseMediaService>();
builder.Services.AddSingleton<GymHero.Api.Services.IExerciseMediaService, GymHero.Api.Services.ExerciseMediaService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    var logger = sp.GetRequiredService<ILogger<GymHero.Api.Services.ExerciseMediaService>>();
    return new GymHero.Api.Services.ExerciseMediaService(httpClient, logger);
});

// Adiciona e configura a autenticação JWT
builder.Services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };

        // Configure JWT authentication for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our SignalR hub
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequirePersonalRole", policy => policy.RequireRole("PersonalTrainer"));
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AdminOrPersonalPolicy", policy => policy.RequireRole("Admin", "PersonalTrainer"));
});

// Add rate limiting to prevent brute force and DOS attacks
// TODO: Rate limiting temporarily disabled due to compatibility issues
// builder.Services.AddRateLimiter(options =>
// {
//     // Implementation will be added with correct .NET 8 API
// });

// Adiciona e configura o Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TaktIQ API", Version = "v1" });
    // Configuração para que o Swagger possa enviar o token JWT nas requisições
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(); // Serviço necessário para o handler

// Add SignalR for real-time communication
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Development: Allow localhost origins for testing
        options.AddPolicy("AllowDevelopment", policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:3002", "http://127.0.0.1:3000")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .SetIsOriginAllowedToAllowWildcardSubdomains();
        });
    }
    else
    {
        // Production: Restrict to specific domains
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] {
                "https://taktiq.app",
                "https://www.taktiq.app",
                "https://taktiq-web-frontend.azurewebsites.net",
                "https://taktiq-web-frontend-fzetgjhvhqbpdtc4.brazilsouth-01.azurewebsites.net"
            };

        Log.Information("CORS Configuration - Allowed Origins: {Origins}", string.Join(", ", allowedOrigins));

        options.AddPolicy("Production", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .WithExposedHeaders("*")
                  .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
        });
    }
});

var app = builder.Build();

// Warm up database connection and apply migrations on startup
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<GymHero.Infrastructure.Data.ApplicationDbContext>();

    Log.Information("Warming up database connection...");
    var canConnect = await dbContext.Database.CanConnectAsync();

    if (canConnect)
    {
        Log.Information("Database connection successful");

        // Apply pending migrations automatically
        Log.Information("Checking for pending database migrations...");
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

        if (pendingMigrations.Any())
        {
            Log.Information("Found {Count} pending migrations. Applying...", pendingMigrations.Count());
            await dbContext.Database.MigrateAsync();
            Log.Information("Database migrations applied successfully");
        }
        else
        {
            Log.Information("No pending migrations found");
        }

        // Seed comprehensive exercise database
        Log.Information("Checking exercise database...");
        var seederLogger = scope.ServiceProvider.GetRequiredService<ILogger<GymHero.Infrastructure.Data.ApplicationDbContext>>();
        await GymHero.Infrastructure.Data.Seeders.ExerciseSeeder.SeedExercisesAsync(dbContext, seederLogger);

        // Seed assessment protocols
        Log.Information("Checking assessment protocols database...");
        await GymHero.Infrastructure.Data.Seeders.AssessmentProtocolSeeder.SeedProtocolsAsync(dbContext, seederLogger);
    }
    else
    {
        Log.Warning("Database connection warmup failed - database may be unavailable");
    }
}
catch (Exception ex)
{
    Log.Error(ex, "Error during database initialization");
    // Don't crash the app - let it start anyway for diagnostics
}

// Configure graceful shutdown for Azure App Service
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStopping.Register(() =>
{
    Log.Information("Application is stopping - waiting for active connections to complete...");
    Thread.Sleep(5000); // Give 5 seconds for requests to complete
});

lifetime.ApplicationStopped.Register(() =>
{
    Log.Information("Application stopped gracefully");
    Log.CloseAndFlush();
});

// 2. CONFIGURAR O PIPELINE DE MIDDLEWARE HTTP
// ============================================

// Configure forwarded headers for Azure App Service (behind load balancer)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

// Em ambiente de desenvolvimento, usamos a UI do Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Skip HTTPS redirection in production (Azure handles this at load balancer level)
// if (!app.Environment.IsDevelopment())
// {
//     app.UseHttpsRedirection();
//     app.UseHsts();
// }

// Configurar o servidor de arquivos estáticos para servir as imagens de perfil
app.UseStaticFiles();

// IMPORTANT: CORS must come early, before other middleware that might block requests
app.UseCors(app.Environment.IsDevelopment() ? "AllowDevelopment" : "Production");

// Additional CORS headers middleware - ensures headers are present even in error responses
app.UseMiddleware<CorsHeadersMiddleware>();

// Security headers middleware - adds protective HTTP headers
app.UseMiddleware<SecurityHeadersMiddleware>();

// Input validation middleware - prevents SQL injection and XSS attacks
app.UseMiddleware<InputValidationMiddleware>();

// Activity logging middleware - logs all HTTP requests
app.UseMiddleware<ActivityLoggingMiddleware>();

// Chat rate limiting middleware - prevents spam and abuse
app.UseMiddleware<ChatRateLimitingMiddleware>();

// Apply rate limiting
// TODO: Rate limiting temporarily disabled
// app.UseRateLimiter();

// IMPORTANTE: A ordem aqui é crucial.
// Primeiro autentica, depois autoriza.
app.UseAuthentication();
app.UseAuthorization();

// 3. MAPEAR OS ENDPOINTS
// ======================

// Health check endpoint for Azure monitoring (no authentication required)
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .AllowAnonymous();

// DEPLOY TEST - Added 2026-01-25 01:55
app.MapGet("/deploy-test-v4", () => Results.Ok(new {
    message = "Deploy v4 funcionando!",
    deployedAt = "2026-01-25T01:55:00Z",
    codeVersion = "4.0.0"
}))
   .AllowAnonymous();

// Diagnostics endpoints for troubleshooting
app.MapDiagnosticsEndpoints();

// EMERGENCY ENDPOINT - Restart App Service
app.MapPost("/api/admin/force-restart", () =>
{
    Environment.Exit(0); // Force app restart
    return Results.Ok(new { message = "Restarting..." });
})
   .RequireAuthorization("RequireAdminRole");

app.MapAuthEndpoints(); // Nosso método de extensão para os endpoints de autenticação
app.MapWorkoutPlanEndpoints();
app.MapExerciseEndpoints();
app.MapSessionEndpoints();
app.MapSetEndpoints();
app.MapProgressEndpoints();
app.MapAdminEndpoints();
app.MapMeEndpoints();
app.MapChallengeEndpoints();
app.MapRankingEndpoints();
app.MapPersonalEndpoints();
app.MapAssessmentEndpoints();
app.MapAssessmentProtocolEndpoints(); // Physical assessment protocols
app.MapProgressPhotoEndpoints(); // Before/after progress photos
app.MapAnalyticsEndpoints(); // Comprehensive trainer analytics
app.MapWhatsAppEndpoints(); // WhatsApp messaging via Twilio
app.MapPdfEndpoints(); // PDF report generation
app.MapPeriodizationEndpoints(); // Automatic periodization
app.MapPublicPersonalEndpoints();
app.MapFriendsEndpoints();
app.MapUsersEndpoints();
app.MapPublicEndpoints();
app.MapAIEndpoints(); // AI-powered workout generation
app.MapSimplifiedAIEndpoints(); // AI v2 - Simplified workout generation (quick workout & weekly plan)
app.MapNotificationEndpoints();
app.MapPostEndpoints(); // Personal Trainer blog posts
app.MapCertificationEndpoints(); // Trainer certifications
app.MapTestimonialEndpoints(); // Testimonials and reviews
app.MapAnnouncementEndpoints(); // Platform announcements and popups
app.MapMediaEndpoints(); // Media upload (images and videos)
app.MapWorkoutPlanCommentEndpoints(); // Workout plan comments
app.MapPaymentEndpoints(); // Payment processing (Stripe)
app.MapStripeConnectEndpoints(); // Stripe Connect for trainer payouts
app.MapWithdrawalEndpoints(); // Trainer withdrawal requests
app.MapPlacesEndpoints(); // Google Places API (nearby gyms, geocoding)
app.MapChatEndpoints(); // Real-time chat

// Map SignalR hub
app.MapHub<GymHero.Api.Hubs.ChatHub>("/hubs/chat");

Log.Information("=== Application Configuration Complete ===");
Log.Information("Starting application on port 8080...");

app.Run();

Log.Information("Application stopped");