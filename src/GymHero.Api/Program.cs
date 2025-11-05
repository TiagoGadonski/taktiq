using System.Text;
using GymHero.Api.Endpoints; // Vamos criar isso a seguir
using GymHero.Api.Middleware;
using GymHero.Application;
using GymHero.Infrastructure;
using GymHero.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

// Adiciona os serviços das nossas camadas com os métodos de extensão que criamos
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

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
                  .AllowCredentials();
        });
    }
    else
    {
        // Production: Restrict to specific domains
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "https://yourdomain.com" };

        options.AddPolicy("Production", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                  .WithHeaders("Content-Type", "Authorization")
                  .AllowCredentials();
        });
    }
});

var app = builder.Build();

// 2. CONFIGURAR O PIPELINE DE MIDDLEWARE HTTP
// ============================================
// Em ambiente de desenvolvimento, usamos a UI do Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

// Configurar o servidor de arquivos estáticos para servir as imagens de perfil
app.UseStaticFiles();

// Use appropriate CORS policy based on environment
app.UseCors(app.Environment.IsDevelopment() ? "AllowDevelopment" : "Production");

// Apply rate limiting
// TODO: Rate limiting temporarily disabled
// app.UseRateLimiter();

// IMPORTANTE: A ordem aqui é crucial.
// Primeiro autentica, depois autoriza.
app.UseAuthentication();
app.UseAuthorization();

// 3. MAPEAR OS ENDPOINTS
// ======================
app.MapAuthEndpoints(); // Nosso método de extensão para os endpoints de autenticação
app.MapWorkoutPlanEndpoints();
app.MapExerciseEndpoints();
app.MapSessionEndpoints();
app.MapProgressEndpoints();
app.MapAdminEndpoints();
app.MapMeEndpoints();
app.MapChallengeEndpoints();
app.MapRankingEndpoints();
app.MapPersonalEndpoints();
app.MapFriendsEndpoints();
app.MapUsersEndpoints();
app.MapPublicEndpoints();
app.MapAIEndpoints(); // AI-powered workout generation
app.Run();