using GymHero.Application.Common.Interfaces;
using GymHero.Application.Services;
using GymHero.Infrastructure.Authentication;
using GymHero.Infrastructure.Data;
using GymHero.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SendGrid;

namespace GymHero.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // --- Configuração de Autenticação ---
        var jwtSettings = new JwtSettings();
        configuration.Bind(JwtSettings.SectionName, jwtSettings);

        // Registra as configurações para serem injetáveis via IOptions<JwtSettings>
        services.AddSingleton(Options.Create(jwtSettings));

        // --- Configuração de SendGrid ---
        var sendGridApiKey = configuration["SendGrid:ApiKey"];
        if (!string.IsNullOrEmpty(sendGridApiKey) && sendGridApiKey != "your_sendgrid_api_key_here")
        {
            services.AddScoped<ISendGridClient>(sp => new SendGridClient(sendGridApiKey));
        }
        else
        {
            // Fallback: register a null implementation for development
            services.AddScoped<ISendGridClient>(sp => new SendGridClient("SG.invalid-key-for-development"));
        }

        // Registra as implementações concretas dos serviços
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddScoped<IPaymentService, StripePaymentService>();
        services.AddScoped<IPayPalPaymentService, PayPalPaymentService>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<IVideoProcessingService, VideoProcessingService>();
        services.AddScoped<IGooglePlacesService, GooglePlacesService>();

        // --- Background Services ---
        services.AddHostedService<PlanExpirationCheckService>();

        // --- Configuração do Banco de Dados ---
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Increased timeout for cold starts in Azure
                npgsqlOptions.CommandTimeout(120); // 120 seconds for first connection
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5, // More retries for Azure cold starts
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });

            // Enable sensitive data logging only in development
            if (configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") != "Production")
            {
                options.EnableSensitiveDataLogging();
            }
        });

        // Registra o ApplicationDbContext como a implementação de IApplicationDbContext
        // Usamos Scoped para que a instância do DbContext dure por uma requisição HTTP.
        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ExerciseSeederService>();
        services.AddScoped<ComprehensiveExerciseSeederService>();
        services.AddScoped<DevelopmentSeederService>();
        services.AddScoped<ExerciseEnhancementService>();

        // Password hasher for User entity (used by DevelopmentSeederService)
        services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<GymHero.Domain.Entities.User>,
            Microsoft.AspNetCore.Identity.PasswordHasher<GymHero.Domain.Entities.User>>();

        return services;
    }
}