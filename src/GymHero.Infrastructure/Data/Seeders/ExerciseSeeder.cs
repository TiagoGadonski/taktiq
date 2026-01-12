using System.Text.Json;
using GymHero.Domain.Entities;
using GymHero.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymHero.Infrastructure.Data.Seeders;

public static class ExerciseSeeder
{
    private class ExerciseJsonModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string MuscleGroup { get; set; } = string.Empty;
        public List<string>? SecondaryMuscles { get; set; }
        public string Equipment { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public List<string>? Instructions { get; set; }
        public List<string>? Tips { get; set; }
        public List<string>? CommonMistakes { get; set; }
        public string? Notes { get; set; }
        public string? VideoUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string WorkoutLocation { get; set; } = "Both";
        public bool IsPublic { get; set; } = true;
        public string? SpaceRequired { get; set; }
        public List<string>? Progressions { get; set; }
        public List<string>? Regressions { get; set; }
        public string? NoEquipmentAlternative { get; set; }
    }

    public static async Task SeedExercisesAsync(ApplicationDbContext context, ILogger? logger = null)
    {
        try
        {
            // Check if exercises already exist
            var existingExercisesCount = await context.Exercises.CountAsync();

            if (existingExercisesCount > 0)
            {
                logger?.LogInformation("Exercises already seeded. Found {Count} exercises in database.", existingExercisesCount);
                return;
            }

            logger?.LogInformation("Starting exercise database seeding...");

            // Read both JSON files: gym exercises and calisthenics exercises
            var gymExercisesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Seeders", "exercises-seed.json");
            var calisthenicsExercisesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Seeders", "calisthenics-exercises-complete.json");

            var allExerciseModels = new List<ExerciseJsonModel>();

            // Read gym exercises
            if (File.Exists(gymExercisesPath))
            {
                var gymJsonContent = await File.ReadAllTextAsync(gymExercisesPath);
                var gymExercises = JsonSerializer.Deserialize<List<ExerciseJsonModel>>(gymJsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (gymExercises != null && gymExercises.Any())
                {
                    allExerciseModels.AddRange(gymExercises);
                    logger?.LogInformation("Loaded {Count} gym exercises", gymExercises.Count);
                }
            }
            else
            {
                logger?.LogWarning("Gym exercise seed file not found at: {Path}", gymExercisesPath);
            }

            // Read calisthenics exercises
            if (File.Exists(calisthenicsExercisesPath))
            {
                var calisthenicsJsonContent = await File.ReadAllTextAsync(calisthenicsExercisesPath);
                var calisthenicsExercises = JsonSerializer.Deserialize<List<ExerciseJsonModel>>(calisthenicsJsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (calisthenicsExercises != null && calisthenicsExercises.Any())
                {
                    allExerciseModels.AddRange(calisthenicsExercises);
                    logger?.LogInformation("Loaded {Count} calisthenics/home exercises", calisthenicsExercises.Count);
                }
            }
            else
            {
                logger?.LogWarning("Calisthenics exercise seed file not found at: {Path}", calisthenicsExercisesPath);
            }

            if (!allExerciseModels.Any())
            {
                logger?.LogWarning("No exercises found in seed files.");
                return;
            }

            logger?.LogInformation("Found {Count} total exercises to seed", allExerciseModels.Count);

            var exerciseModels = allExerciseModels;

            // Convert and add exercises
            var exercises = new List<Exercise>();
            var successCount = 0;
            var errorCount = 0;

            foreach (var model in exerciseModels)
            {
                try
                {
                    var exercise = new Exercise
                    {
                        Id = Guid.NewGuid(),
                        Name = model.Name,
                        Description = model.Description,
                        MuscleGroup = ParseEnum<MuscleGroup>(model.MuscleGroup),
                        SecondaryMuscles = model.SecondaryMuscles?
                            .Select(m => ParseEnum<MuscleGroup>(m))
                            .ToList(),
                        Equipment = ParseEnum<Equipment>(model.Equipment),
                        Category = ParseEnum<ExerciseCategory>(model.Category),
                        Difficulty = ParseEnum<DifficultyLevel>(model.Difficulty),
                        Instructions = model.Instructions,
                        Tips = model.Tips,
                        CommonMistakes = model.CommonMistakes,
                        Notes = model.Notes,
                        VideoUrl = model.VideoUrl,
                        ImageUrl = model.ImageUrl,
                        ThumbnailUrl = model.ThumbnailUrl,
                        WorkoutLocation = ParseEnum<WorkoutLocation>(model.WorkoutLocation),
                        IsPublic = model.IsPublic,
                        SpaceRequired = model.SpaceRequired,
                        Progressions = model.Progressions,
                        Regressions = model.Regressions,
                        NoEquipmentAlternative = model.NoEquipmentAlternative,
                        CreatedAt = DateTime.UtcNow
                    };

                    exercises.Add(exercise);
                    successCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    logger?.LogWarning(ex, "Error parsing exercise: {Name}", model.Name);
                }
            }

            // Bulk insert exercises
            if (exercises.Any())
            {
                await context.Exercises.AddRangeAsync(exercises);
                await context.SaveChangesAsync();

                logger?.LogInformation("Exercise seeding completed successfully!");
                logger?.LogInformation("- Seeded: {SuccessCount} exercises", successCount);

                if (errorCount > 0)
                {
                    logger?.LogWarning("- Failed: {ErrorCount} exercises", errorCount);
                }

                // Log breakdown by muscle group
                var breakdown = exercises
                    .GroupBy(e => e.MuscleGroup)
                    .OrderBy(g => g.Key)
                    .Select(g => $"{g.Key}: {g.Count()}")
                    .ToList();

                logger?.LogInformation("Exercise breakdown: {Breakdown}", string.Join(", ", breakdown));
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Fatal error during exercise seeding");
        }
    }

    private static T ParseEnum<T>(string value) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return default;
        }

        // Try exact match first
        if (Enum.TryParse<T>(value, ignoreCase: true, out var result))
        {
            return result;
        }

        // If not found, return default value
        return default;
    }
}
