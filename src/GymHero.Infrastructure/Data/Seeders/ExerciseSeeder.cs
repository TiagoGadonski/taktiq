using System.Text.Json;
using GymHero.Domain.Entities;
using GymHero.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymHero.Infrastructure.Data.Seeders;

public static class ExerciseSeeder
{
    // Legacy format from old seed files
    private class ExerciseJsonModelLegacy
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

    // New format from exercises folder
    private class ExerciseJsonModelNew
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string>? MusclesWorked { get; set; }
        public List<string>? Equipment { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    public static async Task SeedExercisesAsync(ApplicationDbContext context, ILogger? logger = null)
    {
        try
        {
            // Get existing exercise names to avoid duplicates
            var existingExerciseNames = await context.Exercises
                .Select(e => e.Name.ToLower())
                .ToListAsync();

            var existingExercisesCount = existingExerciseNames.Count;

            logger?.LogInformation("Starting exercise database seeding...");
            logger?.LogInformation("Found {Count} existing exercises in database.", existingExercisesCount);

            var allExercises = new List<Exercise>();

            // Read all JSON files from the exercises folder
            var exercisesFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Seeders", "exercises");

            if (Directory.Exists(exercisesFolderPath))
            {
                var jsonFiles = Directory.GetFiles(exercisesFolderPath, "*.json");
                logger?.LogInformation("Found {Count} JSON files in exercises folder", jsonFiles.Length);

                foreach (var jsonFile in jsonFiles)
                {
                    try
                    {
                        var jsonContent = await File.ReadAllTextAsync(jsonFile);
                        var exercises = JsonSerializer.Deserialize<List<ExerciseJsonModelNew>>(jsonContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (exercises != null && exercises.Any())
                        {
                            foreach (var model in exercises)
                            {
                                // Skip if exercise already exists
                                if (existingExerciseNames.Contains(model.Name.ToLower()))
                                    continue;

                                try
                                {
                                    var exercise = ConvertNewModelToExercise(model);
                                    allExercises.Add(exercise);
                                    existingExerciseNames.Add(model.Name.ToLower()); // Prevent duplicates in same run
                                }
                                catch (Exception ex)
                                {
                                    logger?.LogWarning(ex, "Error converting exercise: {Name} from {File}", model.Name, Path.GetFileName(jsonFile));
                                }
                            }
                            logger?.LogInformation("Loaded {Count} exercises from {File}", exercises.Count, Path.GetFileName(jsonFile));
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "Error reading file: {File}", Path.GetFileName(jsonFile));
                    }
                }
            }
            else
            {
                logger?.LogWarning("Exercises folder not found at: {Path}", exercisesFolderPath);
            }

            // Also read legacy format files from the Seeders root folder
            var seedersRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Seeders");
            var legacyFiles = new[] { "exercises-seed.json", "calisthenics-exercises-seed.json", "calisthenics-exercises-complete.json" };

            foreach (var legacyFileName in legacyFiles)
            {
                var legacyFilePath = Path.Combine(seedersRootPath, legacyFileName);
                if (File.Exists(legacyFilePath))
                {
                    try
                    {
                        var jsonContent = await File.ReadAllTextAsync(legacyFilePath);
                        var exercises = JsonSerializer.Deserialize<List<ExerciseJsonModelLegacy>>(jsonContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (exercises != null && exercises.Any())
                        {
                            var addedCount = 0;
                            foreach (var model in exercises)
                            {
                                // Skip if exercise already exists
                                if (existingExerciseNames.Contains(model.Name.ToLower()))
                                    continue;

                                try
                                {
                                    var exercise = ConvertLegacyModelToExercise(model);
                                    allExercises.Add(exercise);
                                    existingExerciseNames.Add(model.Name.ToLower());
                                    addedCount++;
                                }
                                catch (Exception ex)
                                {
                                    logger?.LogWarning(ex, "Error converting legacy exercise: {Name} from {File}", model.Name, legacyFileName);
                                }
                            }
                            logger?.LogInformation("Loaded {Added}/{Total} exercises from legacy file {File}", addedCount, exercises.Count, legacyFileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "Error reading legacy file: {File}", legacyFileName);
                    }
                }
                else
                {
                    logger?.LogWarning("Legacy file not found: {File}", legacyFilePath);
                }
            }

            // Update video URLs for existing exercises that have real YouTube video URLs
            await UpdateExistingExerciseVideoUrls(context, exercisesFolderPath, logger);

            if (!allExercises.Any())
            {
                logger?.LogInformation("No new exercises found to add.");
                return;
            }

            logger?.LogInformation("Found {Count} NEW exercises to add", allExercises.Count);

            // Bulk insert exercises
            if (allExercises.Any())
            {
                await context.Exercises.AddRangeAsync(allExercises);
                await context.SaveChangesAsync();

                logger?.LogInformation("Exercise seeding completed successfully!");
                logger?.LogInformation("- Seeded: {Count} exercises", allExercises.Count);

                // Log breakdown by muscle group
                var breakdown = allExercises
                    .GroupBy(e => e.MuscleGroup)
                    .OrderBy(g => g.Key)
                    .Select(g => $"{g.Key}: {g.Count()}")
                    .ToList();

                logger?.LogInformation("Exercise breakdown: {Breakdown}", string.Join(", ", breakdown));

                // Log breakdown by category
                var categoryBreakdown = allExercises
                    .GroupBy(e => e.Category)
                    .OrderBy(g => g.Key)
                    .Select(g => $"{g.Key}: {g.Count()}")
                    .ToList();

                logger?.LogInformation("Category breakdown: {Breakdown}", string.Join(", ", categoryBreakdown));
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Fatal error during exercise seeding");
        }
    }

    private static async Task UpdateExistingExerciseVideoUrls(ApplicationDbContext context, string exercisesFolderPath, ILogger? logger)
    {
        try
        {
            logger?.LogInformation("Checking for video URL updates for existing exercises...");

            var videoUrlUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Read all JSON files to collect video URLs
            if (Directory.Exists(exercisesFolderPath))
            {
                var jsonFiles = Directory.GetFiles(exercisesFolderPath, "*.json");

                foreach (var jsonFile in jsonFiles)
                {
                    try
                    {
                        var jsonContent = await File.ReadAllTextAsync(jsonFile);
                        var exercises = JsonSerializer.Deserialize<List<ExerciseJsonModelLegacy>>(jsonContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (exercises != null)
                        {
                            foreach (var model in exercises)
                            {
                                // Only collect real YouTube video URLs (not search URLs)
                                if (!string.IsNullOrEmpty(model.VideoUrl) &&
                                    (model.VideoUrl.Contains("youtu.be/") ||
                                     model.VideoUrl.Contains("youtube.com/watch") ||
                                     model.VideoUrl.Contains("youtube.com/embed/")) &&
                                    !model.VideoUrl.Contains("youtube.com/results"))
                                {
                                    videoUrlUpdates[model.Name] = model.VideoUrl;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Ignore parse errors for individual files
                    }
                }
            }

            if (!videoUrlUpdates.Any())
            {
                logger?.LogInformation("No real YouTube video URLs found in JSON files.");
                return;
            }

            logger?.LogInformation("Found {Count} exercises with real YouTube video URLs to update", videoUrlUpdates.Count);

            // Update existing exercises in database
            var exerciseNames = videoUrlUpdates.Keys.ToList();
            var existingExercises = await context.Exercises
                .Where(e => exerciseNames.Contains(e.Name))
                .ToListAsync();

            var updatedCount = 0;
            foreach (var exercise in existingExercises)
            {
                if (videoUrlUpdates.TryGetValue(exercise.Name, out var newVideoUrl))
                {
                    // Only update if the current URL is different and is a search URL
                    if (exercise.VideoUrl != newVideoUrl &&
                        (string.IsNullOrEmpty(exercise.VideoUrl) ||
                         exercise.VideoUrl.Contains("youtube.com/results")))
                    {
                        exercise.VideoUrl = newVideoUrl;
                        updatedCount++;
                    }
                }
            }

            if (updatedCount > 0)
            {
                await context.SaveChangesAsync();
                logger?.LogInformation("Updated video URLs for {Count} existing exercises", updatedCount);
            }
            else
            {
                logger?.LogInformation("No video URL updates needed for existing exercises");
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Error updating video URLs for existing exercises");
        }
    }

    private static Exercise ConvertNewModelToExercise(ExerciseJsonModelNew model)
    {
        // Get primary muscle group from first item in MusclesWorked list
        var primaryMuscle = model.MusclesWorked?.FirstOrDefault() ?? "Core";
        var secondaryMuscles = model.MusclesWorked?.Skip(1).ToList();

        // Get primary equipment from first item in Equipment list
        var primaryEquipment = model.Equipment?.FirstOrDefault() ?? "Bodyweight";

        // Determine workout location based on equipment
        var workoutLocation = DetermineWorkoutLocation(model.Equipment, model.Category);

        return new Exercise
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Description = model.Description,
            MuscleGroup = ParseMuscleGroup(primaryMuscle),
            SecondaryMuscles = secondaryMuscles?.Select(m => ParseMuscleGroup(m)).Where(m => m != MuscleGroup.Core || secondaryMuscles.Any()).ToList(),
            Equipment = ParseEquipment(primaryEquipment),
            Category = ParseCategory(model.Category),
            Difficulty = ParseEnum<DifficultyLevel>(model.Difficulty),
            ImageUrl = model.ImageUrl,
            WorkoutLocation = workoutLocation,
            IsPublic = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static Exercise ConvertLegacyModelToExercise(ExerciseJsonModelLegacy model)
    {
        return new Exercise
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Description = model.Description,
            MuscleGroup = ParseMuscleGroup(model.MuscleGroup),
            SecondaryMuscles = model.SecondaryMuscles?.Select(m => ParseMuscleGroup(m)).ToList(),
            Equipment = ParseEquipment(model.Equipment),
            Category = ParseCategory(model.Category),
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
            CreatedAt = DateTime.UtcNow
        };
    }

    private static MuscleGroup ParseMuscleGroup(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return MuscleGroup.Core;

        // Map common muscle names to enum values
        var normalized = value.Replace(" ", "").Replace("-", "").ToLowerInvariant();

        return normalized switch
        {
            "chest" or "pectorals" or "pecs" => MuscleGroup.Chest,
            "back" or "lats" or "upperback" or "lowerback" => MuscleGroup.Back,
            "shoulders" or "delts" or "deltoids" => MuscleGroup.Shoulders,
            "biceps" or "bicep" => MuscleGroup.Biceps,
            "triceps" or "tricep" => MuscleGroup.Triceps,
            "forearms" or "forearm" or "wrists" => MuscleGroup.Forearms,
            "quadriceps" or "quads" or "quadricep" => MuscleGroup.Quadriceps,
            "hamstrings" or "hamstring" or "hams" => MuscleGroup.Hamstrings,
            "glutes" or "gluteus" or "butt" or "gluteals" => MuscleGroup.Glutes,
            "calves" or "calf" => MuscleGroup.Calves,
            "abs" or "abdominals" or "core" or "obliques" or "transverseabdominis" => MuscleGroup.Core,
            "traps" or "trapezius" => MuscleGroup.Back,
            "hipflexors" => MuscleGroup.Glutes,
            "adductors" or "abductors" => MuscleGroup.Glutes,
            "neck" => MuscleGroup.Back,
            "serratusanterior" => MuscleGroup.Core,
            "fullbody" => MuscleGroup.Core,
            "tibialis" or "tibialisanterior" => MuscleGroup.Calves,
            "piriformis" => MuscleGroup.Glutes,
            "ankle" or "foot" or "hands" or "diaphragm" => MuscleGroup.Core,
            "arms" => MuscleGroup.Biceps,
            "itband" => MuscleGroup.Glutes,
            _ => Enum.TryParse<MuscleGroup>(value, ignoreCase: true, out var result) ? result : MuscleGroup.Core
        };
    }

    private static Equipment ParseEquipment(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return Equipment.Bodyweight;

        var normalized = value.Replace(" ", "").Replace("-", "").ToLowerInvariant();

        return normalized switch
        {
            "bodyweight" or "none" => Equipment.Bodyweight,
            "barbell" => Equipment.Barbell,
            "dumbbell" or "dumbbells" => Equipment.Dumbbell,
            "kettlebell" or "kettlebells" => Equipment.Kettlebell,
            "cable" or "cablemachine" => Equipment.CableMachine,
            "machine" or "chestpress" or "legpress" or "abmachine" or "hacksquat" or "smithmachine" => Equipment.Machine,
            "resistanceband" or "bands" or "band" => Equipment.ResistanceBand,
            "pullupbar" or "bar" or "lowbar" => Equipment.PullUpBar,
            "parallelbars" or "dips" => Equipment.ParallelBars,
            "medicineball" or "slamball" or "wallball" => Equipment.MedicineBall,
            "stabilityball" or "swissball" => Equipment.SwissBall,
            "foamroller" or "lacrosseball" or "massageball" => Equipment.FoamRoller,
            "trx" or "suspension" => Equipment.TRX,
            "rings" => Equipment.Rings,
            "bench" or "flatbench" or "inclinebench" or "declinebench" => Equipment.Bench,
            "abwheel" => Equipment.AbWheel,
            "box" or "plyobox" or "step" or "stepplatform" => Equipment.Box,
            "sled" or "prowler" => Equipment.SledProwler,
            "battleropes" => Equipment.BattleRopes,
            "jumprope" => Equipment.JumpRope,
            "assaultbike" => Equipment.AssaultBike,
            "stationarybike" or "recumbentbike" or "spinbike" or "bike" => Equipment.Bike,
            "treadmill" => Equipment.Treadmill,
            "elliptical" => Equipment.Elliptical,
            "rowingmachine" => Equipment.RowingMachine,
            "stairclimber" or "skierg" => Equipment.Machine,
            "ez_curl_bar" or "ezcurlbar" or "ezbar" => Equipment.EZBar,
            "landmine" => Equipment.Landmine,
            "sandbag" => Equipment.Bodyweight,
            "trap_bar" or "trapbar" or "hexbar" => Equipment.TrapBar,
            "preacher_bench" or "preacherbench" => Equipment.Bench,
            "dip_station" or "dipstation" => Equipment.DipBars,
            "weight_plate" or "weightplate" or "plate" => Equipment.Plate,
            "ghd" or "ghdmachine" => Equipment.Machine,
            "legcurlmachine" or "legextensionmachine" or "seatedcalfmachine" or "calfmachine" => Equipment.Machine,
            "bosuball" => Equipment.SwissBall,
            "chair" or "sofa" or "couch" => Equipment.Bodyweight,
            "wall" => Equipment.Bodyweight,
            "doorframe" or "door" => Equipment.Bodyweight,
            "towel" => Equipment.Towel,
            "stick" or "rope" or "strap" => Equipment.Rope,
            "backpack" or "waterjug" => Equipment.Bodyweight,
            "stairs" => Equipment.Bodyweight,
            "sliders" => Equipment.Bodyweight,
            "tire" or "sledgehammer" => Equipment.SledProwler,
            "partner" => Equipment.Bodyweight,
            "romanchairpullover" or "romanchair" or "backextensionmachine" => Equipment.Machine,
            "captainschair" => Equipment.Machine,
            "torsorotationmachine" => Equipment.Machine,
            "tibialismachine" => Equipment.Machine,
            "donkeycalfmachine" => Equipment.Machine,
            _ => Enum.TryParse<Equipment>(value, ignoreCase: true, out var result) ? result : Equipment.Bodyweight
        };
    }

    private static ExerciseCategory ParseCategory(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return ExerciseCategory.Strength;

        var normalized = value.Replace(" ", "").Replace("-", "").ToLowerInvariant();

        return normalized switch
        {
            "strength" or "compound" or "isolation" => ExerciseCategory.Strength,
            "cardio" => ExerciseCategory.Cardio,
            "stretching" or "stretch" => ExerciseCategory.Stretching,
            "plyometric" or "plyometrics" => ExerciseCategory.Plyometrics,
            "power" => ExerciseCategory.Power,
            "calisthenics" or "bodyweight" => ExerciseCategory.Calisthenics,
            "functional" => ExerciseCategory.Functional,
            "olympic" or "olympiclifting" => ExerciseCategory.OlympicLifting,
            "mobility" => ExerciseCategory.Mobility,
            "postural" or "posture" or "posturecorrection" => ExerciseCategory.PostureCorrection,
            "corrective" or "rehabilitation" or "recovery" => ExerciseCategory.Rehabilitation,
            "warmup" => ExerciseCategory.WarmUp,
            "cooldown" => ExerciseCategory.CoolDown,
            "balance" => ExerciseCategory.Balance,
            "hiit" => ExerciseCategory.HIIT,
            _ => Enum.TryParse<ExerciseCategory>(value, ignoreCase: true, out var result) ? result : ExerciseCategory.Strength
        };
    }

    private static WorkoutLocation DetermineWorkoutLocation(List<string>? equipment, string? category)
    {
        if (equipment == null || !equipment.Any())
            return WorkoutLocation.Both;

        var equipmentLower = equipment.Select(e => e.ToLowerInvariant()).ToList();

        // Home-friendly equipment
        var homeEquipment = new[] { "bodyweight", "resistance band", "dumbbell", "dumbbells", "kettlebell", "pull-up bar", "jump rope", "foam roller", "stability ball", "yoga mat", "chair", "wall", "door frame", "towel", "backpack", "stairs", "sliders" };

        // Gym-only equipment
        var gymEquipment = new[] { "cable machine", "barbell", "machine", "smith machine", "leg press", "hack squat", "cable", "ez curl bar", "preacher bench", "ghdmachine", "sled", "battle ropes", "assault bike", "rowing machine", "ski erg", "stair climber", "elliptical", "treadmill" };

        bool hasGymEquipment = equipmentLower.Any(e => gymEquipment.Any(g => e.Contains(g)));
        bool hasOnlyHomeEquipment = equipmentLower.All(e => homeEquipment.Any(h => e.Contains(h)) || e == "bodyweight");

        if (hasOnlyHomeEquipment)
            return WorkoutLocation.Home;
        else if (hasGymEquipment)
            return WorkoutLocation.Gym;
        else
            return WorkoutLocation.Both;
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
