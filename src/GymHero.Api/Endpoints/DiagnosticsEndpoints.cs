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
            version = "2.0.0-exercises",
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        }))
        .AllowAnonymous();

        // Check seeder files
        group.MapGet("/seeder-files", () =>
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var exercisesPath = Path.Combine(baseDir, "Data", "Seeders", "exercises");
            var seedersPath = Path.Combine(baseDir, "Data", "Seeders");

            var exerciseFiles = new List<string>();
            var seederFiles = new List<string>();

            if (Directory.Exists(exercisesPath))
            {
                exerciseFiles = Directory.GetFiles(exercisesPath, "*.json").Select(Path.GetFileName).ToList()!;
            }

            if (Directory.Exists(seedersPath))
            {
                seederFiles = Directory.GetFiles(seedersPath, "*.json").Select(Path.GetFileName).ToList()!;
            }

            return Results.Ok(new
            {
                baseDirectory = baseDir,
                exercisesFolderPath = exercisesPath,
                exercisesFolderExists = Directory.Exists(exercisesPath),
                exerciseFilesCount = exerciseFiles.Count,
                exerciseFiles = exerciseFiles.Take(10).ToList(),
                seedersFolderPath = seedersPath,
                seedersFolderExists = Directory.Exists(seedersPath),
                seederFilesCount = seederFiles.Count,
                seederFiles = seederFiles,
                timestamp = DateTime.UtcNow
            });
        })
        .AllowAnonymous();
    }
}
