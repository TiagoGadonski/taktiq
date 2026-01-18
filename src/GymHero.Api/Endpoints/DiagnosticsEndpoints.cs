using GymHero.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace GymHero.Api.Endpoints;

public static class DiagnosticsEndpoints
{
    public static void MapDiagnosticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/diagnostics").WithTags("Diagnostics");

        // Test database connection
        group.MapGet("/db-test", async (
            GymHero.Infrastructure.Data.ApplicationDbContext context,
            ILogger<GymHero.Infrastructure.Data.ApplicationDbContext> logger) =>
        {
            try
            {
                logger.LogInformation("Testing database connection...");

                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    logger.LogError("Database connection failed - cannot connect");
                    return Results.Problem("Cannot connect to database", statusCode: 500);
                }

                logger.LogInformation("Database connection successful, testing exercise count...");
                var exerciseCount = await context.Exercises.CountAsync();

                logger.LogInformation("Successfully counted {Count} exercises", exerciseCount);

                return Results.Ok(new
                {
                    databaseConnected = true,
                    exerciseCount = exerciseCount,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database test failed: {Message}", ex.Message);
                return Results.Problem($"Database test failed: {ex.Message}", statusCode: 500);
            }
        })
        .AllowAnonymous();

        // Test simple response (no database)
        group.MapGet("/ping", () => Results.Ok(new
        {
            status = "healthy",
            message = "API is responding",
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        }))
        .AllowAnonymous();
    }
}
