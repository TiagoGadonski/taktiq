using GymHero.Application.Common.Interfaces;
using GymHero.Infrastructure.Authentication;
using GymHero.Infrastructure.Data;
using GymHero.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

        // Registra as implementações concretas dos serviços
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // --- Configuração do Banco de Dados ---
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Registra o ApplicationDbContext como a implementação de IApplicationDbContext
        // Usamos Scoped para que a instância do DbContext dure por uma requisição HTTP.
        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ExerciseSeederService>();

        return services;
    }
}