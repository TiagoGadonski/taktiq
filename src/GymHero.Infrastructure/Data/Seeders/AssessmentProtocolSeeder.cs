using System.Text.Json;
using GymHero.Domain.Entities;
using GymHero.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymHero.Infrastructure.Data.Seeders;

public static class AssessmentProtocolSeeder
{
    private class ProtocolJsonModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ProtocolType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Instructions { get; set; }
        public string? Equipment { get; set; }
        public int? DurationMinutes { get; set; }
        public string MeasurementFields { get; set; } = "[]";
        public string? NormativeData { get; set; }
        public string? CalculationFormula { get; set; }
        public bool IsPublic { get; set; } = true;
    }

    public static async Task SeedProtocolsAsync(ApplicationDbContext context, ILogger? logger = null)
    {
        try
        {
            // Check if protocols already exist
            var existingProtocolsCount = await context.AssessmentProtocols.CountAsync();

            if (existingProtocolsCount > 0)
            {
                logger?.LogInformation("Assessment protocols already seeded. Found {Count} protocols in database.", existingProtocolsCount);
                return;
            }

            logger?.LogInformation("Starting assessment protocol seeding...");

            // Read JSON file
            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Seeders", "assessment-protocols-seed.json");

            if (!File.Exists(jsonPath))
            {
                logger?.LogWarning("Assessment protocol seed file not found at: {Path}", jsonPath);
                return;
            }

            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            var protocolModels = JsonSerializer.Deserialize<List<ProtocolJsonModel>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (protocolModels == null || !protocolModels.Any())
            {
                logger?.LogWarning("No protocols found in seed file.");
                return;
            }

            logger?.LogInformation("Found {Count} protocols to seed", protocolModels.Count);

            // Convert and add protocols
            var protocols = new List<AssessmentProtocol>();
            var successCount = 0;
            var errorCount = 0;

            foreach (var model in protocolModels)
            {
                try
                {
                    var protocol = new AssessmentProtocol
                    {
                        Id = Guid.NewGuid(),
                        Name = model.Name,
                        Description = model.Description,
                        ProtocolType = ParseEnum<AssessmentProtocolType>(model.ProtocolType),
                        Category = model.Category,
                        Instructions = model.Instructions,
                        Equipment = model.Equipment,
                        DurationMinutes = model.DurationMinutes,
                        MeasurementFields = model.MeasurementFields,
                        NormativeData = model.NormativeData,
                        CalculationFormula = model.CalculationFormula,
                        IsPublic = model.IsPublic,
                        CreatedAt = DateTime.UtcNow
                    };

                    protocols.Add(protocol);
                    successCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    logger?.LogWarning(ex, "Error parsing protocol: {Name}", model.Name);
                }
            }

            // Bulk insert protocols
            if (protocols.Any())
            {
                await context.AssessmentProtocols.AddRangeAsync(protocols);
                await context.SaveChangesAsync();

                logger?.LogInformation("Protocol seeding completed successfully!");
                logger?.LogInformation("- Seeded: {SuccessCount} protocols", successCount);

                if (errorCount > 0)
                {
                    logger?.LogWarning("- Failed: {ErrorCount} protocols", errorCount);
                }

                // Log breakdown by category
                var breakdown = protocols
                    .GroupBy(p => p.Category)
                    .OrderBy(g => g.Key)
                    .Select(g => $"{g.Key}: {g.Count()}")
                    .ToList();

                logger?.LogInformation("Protocol breakdown: {Breakdown}", string.Join(", ", breakdown));
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Fatal error during protocol seeding");
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
