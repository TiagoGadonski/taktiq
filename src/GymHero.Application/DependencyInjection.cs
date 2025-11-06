using System.Reflection;
using FluentValidation;
using GymHero.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GymHero.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registra MediatR e todos os Handlers no assembly da Application
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Add caching behavior to MediatR pipeline
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        // Registra FluentValidation e todos os Validators no assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Mapster - opcional, podemos configurar mais tarde
        // services.AddMapster();

        return services;
    }
}