using System.Security.Claims;
using System.Text;
using System.Text.Json;
using GymHero.Api.Services;
using GymHero.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Endpoints;

public record AIWorkoutRequest(
    string Prompt,
    string? FitnessLevel,
    int? Duration,
    List<string>? Equipment
);

public record AIWorkoutPlanRequest(
    string Prompt,
    string? FitnessLevel,
    int? DaysPerWeek,
    string? Goal
);

public record WorkoutDay(
    string DayName,
    string Title,
    string Focus,
    List<ExerciseInstruction> Exercises
);

public record AIWorkoutPlanResponse(
    string Title,
    string Description,
    int WeeksCount,
    int DaysPerWeek,
    string Goal,
    List<WorkoutDay> Days
);

public record ExerciseInstruction(
    string Name,
    string BodyPart,
    string Equipment,
    int Sets,
    string Reps,
    string Rest,
    List<string> Instructions,
    string? GifUrl = null,
    string? VideoUrl = null,
    string? ProgressionNotes = null
);

public record AIWorkoutResponse(
    string Title,
    string Description,
    int Duration,
    List<ExerciseInstruction> Exercises
);

public static class AIEndpoints
{
    public static void MapAIEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ai")
                       .WithTags("AI Features")
                       .RequireAuthorization();

        group.MapPost("/generate-workout", async (
            [FromBody] AIWorkoutRequest request,
            ClaimsPrincipal user,
            IConfiguration configuration,
            IApplicationDbContext context,
            ILogger<Program> logger) =>
        {
            try
            {
                // Fetch user profile data
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userProfile = await context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new {
                        u.Name,
                        u.DateOfBirth,
                        u.Gender,
                        u.Height,
                        u.Weight,
                        u.Location,
                        u.Bio,
                        u.GymName
                    })
                    .FirstOrDefaultAsync();

                var geminiApiKey = configuration["Gemini:ApiKey"];
                var openAiApiKey = configuration["OpenAI:ApiKey"];

                AIWorkoutResponse workout;
                var hasGemini = !string.IsNullOrEmpty(geminiApiKey);
                var hasOpenAI = !string.IsNullOrEmpty(openAiApiKey);

                if (!hasGemini && !hasOpenAI)
                {
                    logger.LogWarning("No AI API keys configured. Using enhanced mock generation.");
                    workout = GenerateMockWorkout(request.Prompt, request.FitnessLevel);
                }
                else
                {
                    // Try Gemini first, then OpenAI, then fallback to mock
                    var generated = false;
                    workout = null!;

                    if (hasGemini)
                    {
                        try
                        {
                            logger.LogInformation("Calling Gemini API for workout generation...");
                            workout = await GenerateWorkoutWithGemini(request, geminiApiKey!, userProfile);
                            logger.LogInformation("Successfully generated workout with Gemini");
                            generated = true;
                        }
                        catch (Exception geminiEx)
                        {
                            logger.LogWarning(geminiEx, "Gemini API call failed. Trying OpenAI...");
                        }
                    }

                    if (!generated && hasOpenAI)
                    {
                        try
                        {
                            logger.LogInformation("Calling OpenAI API for workout generation...");
                            workout = await GenerateWorkoutWithAI(request, openAiApiKey!, userProfile);
                            logger.LogInformation("Successfully generated workout with OpenAI");
                            generated = true;
                        }
                        catch (Exception aiEx)
                        {
                            logger.LogWarning(aiEx, "OpenAI API call failed. Falling back to mock generation.");
                        }
                    }

                    if (!generated)
                    {
                        logger.LogWarning("All AI APIs failed. Using enhanced mock generation.");
                        workout = GenerateMockWorkout(request.Prompt, request.FitnessLevel);
                    }
                }

                // Exercises already have YouTube video URLs embedded
                return Results.Ok(workout);
            }
            catch (Exception ex)
            {
                return Results.Json(new { message = ex.Message }, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GenerateAIWorkout")
        .WithSummary("Generate a personalized workout using AI based on user's prompt");

        // Search exercises endpoint
        group.MapGet("/search-exercises", async (
            [FromQuery] string? query,
            [FromQuery] string? muscle,
            [FromQuery] string? equipment,
            [FromQuery] string? level,
            IExerciseMediaService mediaService) =>
        {
            try
            {
                var exercises = await mediaService.SearchExercises(query ?? "");

                // Apply additional filters
                if (!string.IsNullOrEmpty(muscle) && muscle != "all")
                {
                    exercises = exercises.Where(e =>
                        e.PrimaryMuscles.Any(m => m.Equals(muscle, StringComparison.OrdinalIgnoreCase)) ||
                        (e.SecondaryMuscles?.Any(m => m.Equals(muscle, StringComparison.OrdinalIgnoreCase)) ?? false)
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(equipment) && equipment != "all")
                {
                    exercises = exercises.Where(e =>
                        e.Equipment.Equals(equipment, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(level) && level != "all")
                {
                    exercises = exercises.Where(e =>
                        e.Level?.Equals(level, StringComparison.OrdinalIgnoreCase) ?? false
                    ).ToList();
                }

                return Results.Ok(exercises.Take(50).ToList());
            }
            catch (Exception ex)
            {
                // SECURITY: Log errors internally, return generic message to client
                var errorId = Guid.NewGuid();
                Console.WriteLine($"[ERROR {errorId}] Error in search-exercises: {ex.Message}");
                Console.WriteLine($"[ERROR {errorId}] Stack trace: {ex.StackTrace}");

                return Results.Json(new {
                    message = "An error occurred while searching exercises",
                    errorId = errorId.ToString()
                }, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("SearchExercises")
        .WithSummary("Search exercises from the exercise database with optional filters");

        group.MapPost("/generate-plan", async (
            [FromBody] AIWorkoutPlanRequest request,
            ClaimsPrincipal user,
            IConfiguration configuration,
            IApplicationDbContext context,
            IExerciseMediaService mediaService,
            ILogger<Program> logger) =>
        {
            try
            {
                // Fetch user profile data
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userProfile = await context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new {
                        u.Name,
                        u.DateOfBirth,
                        u.Gender,
                        u.Height,
                        u.Weight,
                        u.Location,
                        u.Bio,
                        u.GymName
                    })
                    .FirstOrDefaultAsync();

                var geminiApiKey = configuration["Gemini:ApiKey"];
                var openAiApiKey = configuration["OpenAI:ApiKey"];

                AIWorkoutPlanResponse plan;
                var hasGemini = !string.IsNullOrEmpty(geminiApiKey);
                var hasOpenAI = !string.IsNullOrEmpty(openAiApiKey);

                if (!hasGemini && !hasOpenAI)
                {
                    logger.LogWarning("No AI API keys configured. Using enhanced mock plan generation.");
                    plan = GenerateMockPlan(request.Prompt, request.DaysPerWeek ?? 4, request.FitnessLevel);
                }
                else
                {
                    // Try Gemini first, then OpenAI, then fallback to mock
                    var generated = false;
                    plan = null!;

                    if (hasGemini)
                    {
                        try
                        {
                            logger.LogInformation("Calling Gemini API for plan generation...");
                            plan = await GeneratePlanWithGemini(request, geminiApiKey!, userProfile);
                            logger.LogInformation("Successfully generated plan with Gemini");
                            generated = true;
                        }
                        catch (Exception geminiEx)
                        {
                            logger.LogWarning(geminiEx, "Gemini API call failed. Trying OpenAI...");
                        }
                    }

                    if (!generated && hasOpenAI)
                    {
                        try
                        {
                            logger.LogInformation("Calling OpenAI API for plan generation...");
                            plan = await GeneratePlanWithAI(request, openAiApiKey!, userProfile);
                            logger.LogInformation("Successfully generated plan with OpenAI");
                            generated = true;
                        }
                        catch (Exception aiEx)
                        {
                            logger.LogWarning(aiEx, "OpenAI API call failed. Falling back to mock generation.");
                        }
                    }

                    if (!generated)
                    {
                        logger.LogWarning("All AI APIs failed. Using enhanced mock plan generation.");
                        plan = GenerateMockPlan(request.Prompt, request.DaysPerWeek ?? 4, request.FitnessLevel);
                    }
                }

                // Add GIF URLs to all exercises in all days
                var daysWithMedia = new List<WorkoutDay>();
                foreach (var day in plan.Days)
                {
                    var exercisesWithMedia = new List<ExerciseInstruction>();
                    foreach (var exercise in day.Exercises)
                    {
                        var gifUrl = await mediaService.GetExerciseGifUrl(exercise.Name, exercise.BodyPart, exercise.Equipment);
                        exercisesWithMedia.Add(exercise with { GifUrl = gifUrl });
                    }
                    daysWithMedia.Add(day with { Exercises = exercisesWithMedia });
                }

                var planWithMedia = plan with { Days = daysWithMedia };
                return Results.Ok(planWithMedia);
            }
            catch (Exception ex)
            {
                return Results.Json(new { message = ex.Message }, statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GenerateAIWorkoutPlan")
        .WithSummary("Generate a complete weekly workout plan using AI");
    }

    // Portuguese exercise database organized by muscle group
    // Format: (Name, BodyPart, Equipment, IsCompound)
    private static readonly Dictionary<string, List<(string Name, string BodyPart, string Equipment, bool IsCompound)>> ExerciseDatabase = new()
    {
        ["peito"] = new()
        {
            ("Supino Reto com Barra", "chest", "barbell", true),
            ("Supino Inclinado com Halteres", "chest", "dumbbell", true),
            ("Supino Declinado com Barra", "chest", "barbell", true),
            ("Supino Inclinado com Barra", "chest", "barbell", true),
            ("Supino Reto com Halteres", "chest", "dumbbell", true),
            ("Supino na Máquina", "chest", "machine", true),
            ("Crucifixo com Halteres", "chest", "dumbbell", false),
            ("Crucifixo Inclinado", "chest", "dumbbell", false),
            ("Crucifixo na Polia", "chest", "cable", false),
            ("Cross Over", "chest", "cable", false),
            ("Peck Deck", "chest", "machine", false),
            ("Flexão de Braço", "chest", "body only", true),
            ("Flexão com Elevação", "chest", "body only", true),
            ("Pullover com Halteres", "chest", "dumbbell", false)
        },
        ["costas"] = new()
        {
            ("Levantamento Terra", "back", "barbell", true),
            ("Barra Fixa", "back", "body only", true),
            ("Puxada Frontal", "back", "cable", true),
            ("Remada Curvada com Barra", "back", "barbell", true),
            ("Remada com Halteres", "back", "dumbbell", true),
            ("Remada Baixa", "back", "cable", true),
            ("Remada Cavalinho", "back", "machine", true),
            ("Pulldown", "back", "cable", true),
            ("Remada na Máquina", "back", "machine", true),
            ("Remada Unilateral", "back", "dumbbell", true),
            ("Pullover na Polia", "back", "cable", false),
            ("Serrote", "back", "dumbbell", true)
        },
        ["ombros"] = new()
        {
            ("Desenvolvimento com Barra", "shoulders", "barbell", true),
            ("Desenvolvimento com Halteres", "shoulders", "dumbbell", true),
            ("Desenvolvimento Arnold", "shoulders", "dumbbell", true),
            ("Desenvolvimento na Máquina", "shoulders", "machine", true),
            ("Elevação Lateral", "shoulders", "dumbbell", false),
            ("Elevação Frontal", "shoulders", "dumbbell", false),
            ("Elevação Lateral na Polia", "shoulders", "cable", false),
            ("Elevação Lateral Inclinado", "shoulders", "dumbbell", false),
            ("Remada Alta", "shoulders", "barbell", true),
            ("Face Pull", "shoulders", "cable", false),
            ("Crucifixo Inverso", "shoulders", "dumbbell", false),
            ("Voo Posterior", "shoulders", "cable", false)
        },
        ["bíceps"] = new()
        {
            ("Rosca Direta com Barra", "biceps", "barbell", false),
            ("Rosca Alternada com Halteres", "biceps", "dumbbell", false),
            ("Rosca Martelo", "biceps", "dumbbell", false),
            ("Rosca Scott", "biceps", "barbell", false),
            ("Rosca na Polia", "biceps", "cable", false),
            ("Rosca Concentrada", "biceps", "dumbbell", false),
            ("Rosca 21", "biceps", "barbell", false),
            ("Rosca Inversa", "biceps", "barbell", false),
            ("Rosca Zottman", "biceps", "dumbbell", false)
        },
        ["tríceps"] = new()
        {
            ("Supino Fechado", "triceps", "barbell", true),
            ("Mergulho entre Bancos", "triceps", "body only", true),
            ("Tríceps na Polia", "triceps", "cable", false),
            ("Tríceps Testa com Barra", "triceps", "barbell", false),
            ("Tríceps Francês", "triceps", "dumbbell", false),
            ("Tríceps Coice", "triceps", "dumbbell", false),
            ("Tríceps na Polia com Corda", "triceps", "cable", false),
            ("Tríceps Testa com Halteres", "triceps", "dumbbell", false)
        },
        ["pernas"] = new()
        {
            ("Agachamento Livre", "legs", "barbell", true),
            ("Levantamento Terra", "legs", "barbell", true),
            ("Leg Press 45°", "legs", "machine", true),
            ("Agachamento Sumô", "legs", "barbell", true),
            ("Agachamento Frontal", "legs", "barbell", true),
            ("Afundo com Halteres", "legs", "dumbbell", true),
            ("Afundo Caminhando", "legs", "dumbbell", true),
            ("Stiff", "legs", "barbell", true),
            ("Agachamento no Smith", "legs", "machine", true),
            ("Agachamento Búlgaro", "legs", "dumbbell", true),
            ("Cadeira Extensora", "legs", "machine", false),
            ("Mesa Flexora", "legs", "machine", false),
            ("Cadeira Abdutora", "legs", "machine", false),
            ("Cadeira Adutora", "legs", "machine", false)
        },
        ["glúteos"] = new()
        {
            ("Hip Thrust com Barra", "glutes", "barbell", true),
            ("Agachamento Sumô", "glutes", "barbell", true),
            ("Stiff", "glutes", "barbell", true),
            ("Elevação Pélvica", "glutes", "barbell", true),
            ("Agachamento Búlgaro", "glutes", "dumbbell", true),
            ("Leg Press 45° com Pés Altos", "glutes", "machine", true),
            ("Cadeira Abdutora", "glutes", "machine", false),
            ("Kickback na Polia", "glutes", "cable", false),
            ("Coice no Crossover", "glutes", "cable", false),
            ("Step Up com Halteres", "glutes", "dumbbell", true),
            ("Afundo Reverso", "glutes", "dumbbell", true),
            ("Good Morning", "glutes", "barbell", true),
            ("Cadeira Flexora em Pé", "glutes", "machine", false)
        },
        ["panturrilha"] = new()
        {
            ("Panturrilha em Pé", "calves", "machine", false),
            ("Panturrilha Sentado", "calves", "machine", false),
            ("Panturrilha no Leg Press", "calves", "machine", false),
            ("Elevação de Panturrilha Unilateral", "calves", "dumbbell", false)
        },
        ["abdômen"] = new()
        {
            ("Abdominal Reto", "abs", "body only", false),
            ("Abdominal na Máquina", "abs", "machine", false),
            ("Prancha", "abs", "body only", false),
            ("Prancha Lateral", "abs", "body only", false),
            ("Abdominal Infra", "abs", "body only", false),
            ("Abdominal Bicicleta", "abs", "body only", false),
            ("Elevação de Pernas", "abs", "body only", false),
            ("Abdominal na Polia", "abs", "cable", false),
            ("Abdominal Canivete", "abs", "body only", false),
            ("Mountain Climbers", "abs", "body only", false)
        }
    };

    // Exercise name synonyms for restriction matching
    private static readonly Dictionary<string, HashSet<string>> ExerciseRestrictions = new()
    {
        ["supino"] = new() { "supino reto", "supino inclinado", "supino declinado", "supino fechado", "bench press", "supino com barra", "supino com halteres", "supino na máquina" },
        ["agachamento"] = new() { "agachamento livre", "agachamento sumô", "agachamento no smith", "squat", "agachamento búlgaro" },
        ["levantamento terra"] = new() { "levantamento terra", "deadlift", "terra" },
        ["rosca"] = new() { "rosca direta", "rosca alternada", "rosca martelo", "rosca scott", "rosca concentrada", "curl" },
        ["desenvolvimento"] = new() { "desenvolvimento com barra", "desenvolvimento com halteres", "desenvolvimento arnold", "shoulder press" }
    };

    private static AIWorkoutResponse GenerateMockWorkout(string prompt, string? fitnessLevel = null)
    {
        Console.WriteLine("=== MOCK GENERATION DEBUG ===");
        Console.WriteLine($"Prompt: {prompt}");
        Console.WriteLine($"Fitness Level Received: '{fitnessLevel ?? "NULL"}'");

        var random = new Random();
        var parsedPrompt = ParsePrompt(prompt.ToLower());
        var level = fitnessLevel?.ToLower() ?? "intermediário";

        Console.WriteLine($"Normalized Level: '{level}'");

        // Determine exercise count based on fitness level
        var (minExercises, maxExercises) = level switch
        {
            "iniciante" or "beginner" => (5, 7),
            "avançado" or "advanced" => (8, 10),
            _ => (6, 8) // intermediário
        };

        Console.WriteLine($"Exercise Count Range: {minExercises}-{maxExercises}");

        // Select exercises based on parsed requirements
        var selectedExercises = new List<ExerciseInstruction>();

        // If specific muscle groups were requested, use those
        if (parsedPrompt.MuscleGroups.Any())
        {
            Console.WriteLine($"Path: SPECIFIC MUSCLE GROUPS");
            Console.WriteLine($"Muscle Groups Detected: {string.Join(", ", parsedPrompt.MuscleGroups)}");
            var totalExercises = random.Next(minExercises, maxExercises + 1);
            Console.WriteLine($"Total Exercises to Generate: {totalExercises}");
            var exercisesPerGroup = Math.Max(2, totalExercises / parsedPrompt.MuscleGroups.Count);
            Console.WriteLine($"Exercises Per Group: {exercisesPerGroup}");

            foreach (var muscleGroup in parsedPrompt.MuscleGroups)
            {
                if (ExerciseDatabase.ContainsKey(muscleGroup))
                {
                    var availableExercises = ExerciseDatabase[muscleGroup]
                        .Where(ex => !IsRestricted(ex.Name, parsedPrompt.Restrictions))
                        .ToList();

                    // Prioritize compound exercises, then isolation
                    var compoundExercises = availableExercises.Where(ex => ex.IsCompound).OrderBy(x => random.Next()).ToList();
                    var isolationExercises = availableExercises.Where(ex => !ex.IsCompound).OrderBy(x => random.Next()).ToList();

                    // Select random number of exercises per muscle group
                    var count = Math.Min(
                        muscleGroup == "abdômen" || muscleGroup == "panturrilha" ?
                            random.Next(2, 4) :
                            random.Next(3, exercisesPerGroup + 1),
                        availableExercises.Count
                    );

                    var exercisesToAdd = new List<(string Name, string BodyPart, string Equipment, bool IsCompound)>();

                    // Add compound exercises first (at least 1-2)
                    var compoundCount = Math.Min(Math.Max(1, count / 2), compoundExercises.Count);
                    exercisesToAdd.AddRange(compoundExercises.Take(compoundCount));

                    // Fill remaining with isolation exercises
                    var isolationCount = Math.Min(count - compoundCount, isolationExercises.Count);
                    exercisesToAdd.AddRange(isolationExercises.Take(isolationCount));

                    foreach (var exercise in exercisesToAdd)
                    {
                        selectedExercises.Add(CreateExerciseInstruction(
                            exercise.Name,
                            exercise.BodyPart,
                            exercise.Equipment,
                            selectedExercises.Count == 0, // First exercise is main
                            level,
                            exercise.IsCompound
                        ));
                    }
                }
            }
        }
        else
        {
            Console.WriteLine($"Path: DEFAULT WORKOUT (no specific muscle groups detected)");
            // Intelligent default based on common workout splits
            var workoutType = random.Next(0, 5);
            Console.WriteLine($"Random Workout Type: {workoutType}");
            var muscleGroupsToUse = workoutType switch
            {
                0 => new[] { "peito", "tríceps" },           // Push
                1 => new[] { "costas", "bíceps" },           // Pull
                2 => new[] { "pernas", "panturrilha" },      // Legs
                3 => new[] { "ombros", "abdômen" },          // Shoulders & Core
                _ => new[] { "peito", "costas", "ombros" }   // Upper Body
            };

            Console.WriteLine($"Muscle Groups to Use: {string.Join(", ", muscleGroupsToUse)}");
            var totalExercises = random.Next(minExercises, maxExercises + 1);
            Console.WriteLine($"Total Exercises to Generate: {totalExercises}");
            var exercisesPerGroup = Math.Max(2, totalExercises / muscleGroupsToUse.Length);
            Console.WriteLine($"Exercises Per Group: {exercisesPerGroup}");

            foreach (var muscleGroup in muscleGroupsToUse)
            {
                if (ExerciseDatabase.ContainsKey(muscleGroup))
                {
                    var availableExercises = ExerciseDatabase[muscleGroup]
                        .Where(ex => !IsRestricted(ex.Name, parsedPrompt.Restrictions))
                        .ToList();

                    // Prioritize compound exercises, then isolation
                    var compoundExercises = availableExercises.Where(ex => ex.IsCompound).OrderBy(x => random.Next()).ToList();
                    var isolationExercises = availableExercises.Where(ex => !ex.IsCompound).OrderBy(x => random.Next()).ToList();

                    // Calculate how many exercises to add for this muscle group
                    var countForGroup = Math.Min(exercisesPerGroup, availableExercises.Count);

                    // Add compound exercises first (at least 1, up to half)
                    var compoundCount = Math.Min(Math.Max(1, countForGroup / 2), compoundExercises.Count);
                    var isolationCount = Math.Min(countForGroup - compoundCount, isolationExercises.Count);

                    foreach (var exercise in compoundExercises.Take(compoundCount))
                    {
                        selectedExercises.Add(CreateExerciseInstruction(
                            exercise.Name,
                            exercise.BodyPart,
                            exercise.Equipment,
                            selectedExercises.Count == 0,
                            level,
                            exercise.IsCompound
                        ));
                    }

                    foreach (var exercise in isolationExercises.Take(isolationCount))
                    {
                        selectedExercises.Add(CreateExerciseInstruction(
                            exercise.Name,
                            exercise.BodyPart,
                            exercise.Equipment,
                            false,
                            level,
                            exercise.IsCompound
                        ));
                    }
                }
            }
        }

        // If no exercises selected (all were restricted), provide varied fallback
        if (!selectedExercises.Any())
        {
            var fallbackExercises = new[]
            {
                ("Flexão de Braço", "chest", "body only", true),
                ("Agachamento Livre", "legs", "body only", true),
                ("Prancha", "abs", "body only", false),
                ("Burpee", "cardio", "body only", true),
                ("Afundo", "legs", "body only", true),
                ("Mountain Climbers", "abs", "body only", false)
            }.OrderBy(x => random.Next()).Take(random.Next(5, 7));

            foreach (var (name, bodyPart, equipment, isCompound) in fallbackExercises)
            {
                selectedExercises.Add(CreateExerciseInstruction(name, bodyPart, equipment, selectedExercises.Count == 0, level, isCompound));
            }
        }

        // Generate title based on muscle groups or workout type
        var title = parsedPrompt.MuscleGroups.Any()
            ? GenerateWorkoutTitle(parsedPrompt.MuscleGroups)
            : $"Treino {(level == "avançado" ? "Avançado" : level == "iniciante" ? "Iniciante" : "Intermediário")} Completo";

        var description = parsedPrompt.MuscleGroups.Any()
            ? GenerateWorkoutDescription(parsedPrompt.MuscleGroups, selectedExercises.Count)
            : $"Treino completo com {selectedExercises.Count} exercícios variados para desenvolvimento muscular equilibrado. Nível: {level}.";

        var duration = level switch
        {
            "iniciante" => random.Next(40, 50),
            "avançado" => random.Next(60, 75),
            _ => random.Next(50, 60)
        };

        Console.WriteLine($"FINAL EXERCISE COUNT: {selectedExercises.Count}");
        Console.WriteLine($"Exercise Names: {string.Join(", ", selectedExercises.Select(e => e.Name))}");
        Console.WriteLine("=== END MOCK GENERATION DEBUG ===");

        return new AIWorkoutResponse(
            Title: title,
            Description: description,
            Duration: duration,
            Exercises: selectedExercises
        );
    }

    private static (List<string> MuscleGroups, List<string> Restrictions) ParsePrompt(string prompt)
    {
        var muscleGroups = new List<string>();
        var restrictions = new List<string>();

        // Detect muscle groups in prompt
        var muscleKeywords = new Dictionary<string, string>
        {
            ["peito"] = "peito",
            ["chest"] = "peito",
            ["costas"] = "costas",
            ["back"] = "costas",
            ["ombro"] = "ombros",
            ["shoulder"] = "ombros",
            ["bíceps"] = "bíceps",
            ["bicep"] = "bíceps",
            ["tríceps"] = "tríceps",
            ["tricep"] = "tríceps",
            ["glúteo"] = "glúteos",
            ["gluteo"] = "glúteos",
            ["glute"] = "glúteos",
            ["bumbum"] = "glúteos",
            ["perna"] = "pernas",
            ["leg"] = "pernas",
            ["quadríceps"] = "pernas",
            ["coxa"] = "pernas",
            ["panturrilha"] = "panturrilha",
            ["calf"] = "panturrilha",
            ["abdômen"] = "abdômen",
            ["abdominal"] = "abdômen",
            ["abs"] = "abdômen",
            ["core"] = "abdômen"
        };

        foreach (var (keyword, muscleGroup) in muscleKeywords)
        {
            if (prompt.Contains(keyword) && !muscleGroups.Contains(muscleGroup))
            {
                muscleGroups.Add(muscleGroup);
            }
        }

        // Detect restrictions (exercises to avoid)
        var restrictionPhrases = new[] { "sem ", "sem o ", "não quero ", "evitar ", "excluir ", "without ", "no " };
        foreach (var phrase in restrictionPhrases)
        {
            var index = prompt.IndexOf(phrase);
            if (index >= 0)
            {
                // Extract the exercise name after the restriction phrase
                var afterPhrase = prompt.Substring(index + phrase.Length);
                var words = afterPhrase.Split(new[] { ' ', ',', '.', ';' }, StringSplitOptions.RemoveEmptyEntries);

                // Take first 1-3 words as potential exercise name
                var restriction = string.Join(" ", words.Take(3)).Trim();
                if (!string.IsNullOrWhiteSpace(restriction))
                {
                    restrictions.Add(restriction);
                }
            }
        }

        return (muscleGroups, restrictions);
    }

    private static bool IsRestricted(string exerciseName, List<string> restrictions)
    {
        var lowerName = exerciseName.ToLower();

        foreach (var restriction in restrictions)
        {
            var lowerRestriction = restriction.ToLower();

            // Direct name match
            if (lowerName.Contains(lowerRestriction))
            {
                return true;
            }

            // Check against exercise restriction groups
            foreach (var (key, synonyms) in ExerciseRestrictions)
            {
                if (lowerRestriction.Contains(key) && synonyms.Any(s => lowerName.Contains(s)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static ExerciseInstruction CreateExerciseInstruction(string name, string bodyPart, string equipment, bool isMainExercise, string fitnessLevel = "intermediário", bool isCompound = true)
    {
        // Generate progression notes based on fitness level and exercise type
        var progressionNotes = (fitnessLevel.ToLower(), isCompound, isMainExercise) switch
        {
            ("iniciante", true, true) => "Semana 1-2: 3x10 | Semana 3-4: 3x12 | Foco em técnica e controle",
            ("iniciante", true, false) => "Semana 1-2: 3x12 | Semana 3-4: 3x15 | Aumente amplitude gradualmente",
            ("iniciante", false, _) => "Semana 1-2: 3x12 | Semana 3-4: 3x15 | Conexão mente-músculo",

            ("avançado", true, true) => "Semana 1: 5x6 (pesado) | Semana 2: 4x8 | Semana 3: 5x5 (máximo) | Semana 4: 3x10 (deload)",
            ("avançado", true, false) => "Semana 1: 4x8 | Semana 2: 4x10 | Semana 3: 5x8 (↑ carga) | Semana 4: 3x12 (deload)",
            ("avançado", false, _) => "Semana 1: 4x10 | Semana 2: 4x12 | Semana 3: 4x10 (↑ carga) | Semana 4: 3x15 (deload)",

            (_, true, true) => "Semana 1: 3x10 | Semana 2: 4x10 | Semana 3: 4x8 (↑ carga) | Semana 4: 3x10 (deload)",
            (_, true, false) => "Semana 1: 3x10 | Semana 2: 3x12 | Semana 3: 4x10 (↑ carga) | Semana 4: 3x12",
            _ => "Semana 1: 3x12 | Semana 2: 3x15 | Semana 3: 4x12 (↑ carga) | Semana 4: 3x15"
        };

        var instructionsMap = new Dictionary<string, List<string>>
        {
            ["Supino Reto com Barra"] = new() { "Deite-se no banco com os pés firmes no chão", "Segure a barra com as mãos ligeiramente mais largas que os ombros", "Desça a barra controladamente até o peito", "Empurre a barra de volta à posição inicial" },
            ["Supino Inclinado com Halteres"] = new() { "Ajuste o banco para um ângulo de 30-45 graus", "Segure os halteres acima do peito com os braços estendidos", "Desça os halteres lentamente até o nível do peito", "Empurre os halteres de volta para cima" },
            ["Crucifixo com Halteres"] = new() { "Deite-se no banco com halteres acima do peito", "Com uma ligeira flexão nos cotovelos, abra os braços para os lados", "Desça até sentir um alongamento no peito", "Retorne à posição inicial contraindo o peito" },
            ["Agachamento Livre"] = new() { "Posicione a barra nas costas", "Pés afastados na largura dos ombros", "Desça controladamente até a coxa ficar paralela ao chão", "Suba empurrando pelos calcanhares" },
            ["Leg Press 45°"] = new() { "Sente-se na máquina com as costas apoiadas", "Posicione os pés no centro da plataforma", "Desbloqueie a trava e desça controladamente", "Empurre a plataforma de volta sem travar os joelhos" },
            ["Rosca Direta com Barra"] = new() { "Fique em pé com a barra na frente do corpo", "Mantenha os cotovelos fixos ao lado do corpo", "Flexione os cotovelos levantando a barra", "Desça controladamente até a extensão completa" },
            ["Tríceps na Polia"] = new() { "Segure a barra ou corda anexada à polia alta", "Mantenha os cotovelos colados ao corpo", "Empurre para baixo até a extensão completa", "Retorne controladamente à posição inicial" },
            ["Desenvolvimento com Halteres"] = new() { "Sente-se com os halteres na altura dos ombros", "Empurre os halteres para cima até os braços ficarem estendidos", "Desça controladamente até os halteres ficarem na altura dos ombros", "Mantenha o core contraído durante todo o movimento" },
            ["Barra Fixa"] = new() { "Segure a barra com pegada pronada, mãos afastadas", "Puxe o corpo para cima até o queixo passar a barra", "Desça controladamente até a extensão completa", "Mantenha o core contraído" },
            ["Remada Curvada com Barra"] = new() { "Segure a barra com pegada pronada", "Incline o tronco para frente mantendo as costas retas", "Puxe a barra em direção ao abdômen", "Desça controladamente à posição inicial" },
            ["Elevação Lateral"] = new() { "Segure os halteres ao lado do corpo", "Com cotovelos ligeiramente flexionados, eleve os braços lateralmente", "Suba até a altura dos ombros", "Desça controladamente" },
            ["Prancha"] = new() { "Apoie os antebraços e pontas dos pés no chão", "Mantenha o corpo reto como uma prancha", "Contraia abdômen e glúteos", "Segure a posição pelo tempo determinado" },
            ["Abdominal Reto"] = new() { "Deite-se de costas com joelhos flexionados", "Coloque as mãos atrás da cabeça", "Contraia o abdômen elevando o tronco", "Desça controladamente sem relaxar completamente" },
            ["Flexão de Braço"] = new() { "Posicione as mãos no chão, afastadas na largura dos ombros", "Mantenha o corpo reto dos pés à cabeça", "Desça o corpo flexionando os cotovelos", "Empurre de volta à posição inicial" }
        };

        // YouTube video demonstrations for each exercise
        var videoMap = new Dictionary<string, string>
        {
            ["Supino Reto com Barra"] = "https://www.youtube.com/watch?v=rT7DgCr-3pg",
            ["Supino Inclinado com Halteres"] = "https://www.youtube.com/watch?v=8iPEnn-ltC8",
            ["Supino Declinado com Barra"] = "https://www.youtube.com/watch?v=LfyQBUKR8SE",
            ["Crucifixo com Halteres"] = "https://www.youtube.com/watch?v=eozdVDA78K0",
            ["Crucifixo na Polia"] = "https://www.youtube.com/watch?v=taI4XduLpTk",
            ["Flexão de Braço"] = "https://www.youtube.com/watch?v=IODxDxX7oi4",
            ["Supino na Máquina"] = "https://www.youtube.com/watch?v=xUm0BiZCWlQ",
            ["Cross Over"] = "https://www.youtube.com/watch?v=taI4XduLpTk",
            ["Barra Fixa"] = "https://www.youtube.com/watch?v=eGo4IYlbE5g",
            ["Puxada Frontal"] = "https://www.youtube.com/watch?v=CAwf7n6Luuc",
            ["Remada Curvada com Barra"] = "https://www.youtube.com/watch?v=FWJR5Ve8bnQ",
            ["Remada com Halteres"] = "https://www.youtube.com/watch?v=roCP6wCXPqo",
            ["Levantamento Terra"] = "https://www.youtube.com/watch?v=op9kVnSso6Q",
            ["Pulldown"] = "https://www.youtube.com/watch?v=CAwf7n6Luuc",
            ["Remada na Máquina"] = "https://www.youtube.com/watch?v=UCXxvVItLoM",
            ["Remada Baixa"] = "https://www.youtube.com/watch?v=GZbfZ033f74",
            ["Desenvolvimento com Barra"] = "https://www.youtube.com/watch?v=wol7Hko8RhY",
            ["Desenvolvimento com Halteres"] = "https://www.youtube.com/watch?v=qEwKCR5JCog",
            ["Elevação Lateral"] = "https://www.youtube.com/watch?v=3VcKaXpzqRo",
            ["Elevação Frontal"] = "https://www.youtube.com/watch?v=qzSDdkBYKIo",
            ["Desenvolvimento Arnold"] = "https://www.youtube.com/watch?v=6Z15_WdXmVw",
            ["Elevação Lateral na Polia"] = "https://www.youtube.com/watch?v=PPczBWaVMWk",
            ["Remada Alta"] = "https://www.youtube.com/watch?v=cokS_VlxOec",
            ["Face Pull"] = "https://www.youtube.com/watch?v=rep-qVOkqgk",
            ["Rosca Direta com Barra"] = "https://www.youtube.com/watch?v=kwG2ipFRgfo",
            ["Rosca Alternada com Halteres"] = "https://www.youtube.com/watch?v=sAq_ocpRh_I",
            ["Rosca Martelo"] = "https://www.youtube.com/watch?v=zC3nLlEvin4",
            ["Rosca Scott"] = "https://www.youtube.com/watch?v=fIWP-FRFNU0",
            ["Rosca na Polia"] = "https://www.youtube.com/watch?v=SLPrWz5G-yY",
            ["Rosca Concentrada"] = "https://www.youtube.com/watch?v=0AUGkch3tzc",
            ["Rosca 21"] = "https://www.youtube.com/watch?v=VY9r2iWR8GA",
            ["Tríceps na Polia"] = "https://www.youtube.com/watch?v=2-LAMcpzODU",
            ["Tríceps Testa com Barra"] = "https://www.youtube.com/watch?v=d_KZxkY_0cM",
            ["Tríceps Francês"] = "https://www.youtube.com/watch?v=nRiJVZDpdL0",
            ["Supino Fechado"] = "https://www.youtube.com/watch?v=nEF0bv2FW94",
            ["Mergulho entre Bancos"] = "https://www.youtube.com/watch?v=0326dy_-CzM",
            ["Tríceps Coice"] = "https://www.youtube.com/watch?v=6SS6K3lAwZ8",
            ["Tríceps na Polia com Corda"] = "https://www.youtube.com/watch?v=kiuVA0gs3EI",
            ["Agachamento Livre"] = "https://www.youtube.com/watch?v=ultWZbUMPL8",
            ["Leg Press 45°"] = "https://www.youtube.com/watch?v=IZxyjW7MPJQ",
            ["Cadeira Extensora"] = "https://www.youtube.com/watch?v=YyvSfVjQeL0",
            ["Mesa Flexora"] = "https://www.youtube.com/watch?v=ELOCsoDSmrg",
            ["Agachamento Sumô"] = "https://www.youtube.com/watch?v=aEcW7A9667w",
            ["Afundo com Halteres"] = "https://www.youtube.com/watch?v=D7KaRcUTQeE",
            ["Stiff"] = "https://www.youtube.com/watch?v=CN_7cz3P-1U",
            ["Agachamento no Smith"] = "https://www.youtube.com/watch?v=TAGDQVzS6Bk",
            ["Panturrilha em Pé"] = "https://www.youtube.com/watch?v=JbyjNymZOt0",
            ["Panturrilha Sentado"] = "https://www.youtube.com/watch?v=JJWhJg9AHfE",
            ["Panturrilha no Leg Press"] = "https://www.youtube.com/watch?v=z5sw_5AwTis",
            ["Elevação de Panturrilha Unilateral"] = "https://www.youtube.com/watch?v=3j4TEFO_1Ec",
            ["Abdominal Reto"] = "https://www.youtube.com/watch?v=Ep-Y4KymwFc",
            ["Abdominal na Máquina"] = "https://www.youtube.com/watch?v=sYbm_0tVyUE",
            ["Prancha"] = "https://www.youtube.com/watch?v=ASdvN_XEl_c",
            ["Abdominal Infra"] = "https://www.youtube.com/watch?v=JB2oyawG9KI",
            ["Abdominal Bicicleta"] = "https://www.youtube.com/watch?v=9FGilxCbdz8",
            ["Elevação de Pernas"] = "https://www.youtube.com/watch?v=JB2oyawG9KI",
            ["Abdominal na Polia"] = "https://www.youtube.com/watch?v=LqH5tyDWpik"
        };

        var instructions = instructionsMap.ContainsKey(name)
            ? instructionsMap[name]
            : new List<string> { "Execute o movimento com técnica correta", "Mantenha o controle durante toda a amplitude", "Respire adequadamente", "Foque na contração muscular" };

        var videoUrl = videoMap.ContainsKey(name) ? videoMap[name] : null;

        // Adapt sets, reps, and rest based on fitness level
        var (sets, reps, rest) = (fitnessLevel.ToLower(), isMainExercise) switch
        {
            ("iniciante" or "beginner", true) => (3, "10-12", "120s"),
            ("iniciante" or "beginner", false) => (3, "12-15", "90s"),
            ("avançado" or "advanced", true) => (5, "6-8", "90s"),
            ("avançado" or "advanced", false) => (4, "8-10", "60s"),
            (_, true) => (4, "8-10", "90s"),   // Intermediate main
            _ => (3, "10-12", "60s")             // Intermediate secondary
        };

        return new ExerciseInstruction(
            Name: name,
            BodyPart: bodyPart,
            Equipment: equipment,
            Sets: sets,
            Reps: reps,
            Rest: rest,
            Instructions: instructions,
            GifUrl: null, // Will be populated by media service if available
            VideoUrl: videoUrl,
            ProgressionNotes: progressionNotes
        );
    }

    private static string GenerateWorkoutTitle(List<string> muscleGroups)
    {
        if (!muscleGroups.Any())
        {
            return "Treino de Peito e Tríceps - Hipertrofia";
        }

        var formattedGroups = muscleGroups.Select(g => char.ToUpper(g[0]) + g.Substring(1));
        return $"Treino de {string.Join(" e ", formattedGroups)} - Hipertrofia";
    }

    private static string GenerateWorkoutDescription(List<string> muscleGroups, int exerciseCount)
    {
        if (!muscleGroups.Any())
        {
            return "Treino focado em hipertrofia para peito e tríceps, ideal para nível intermediário";
        }

        var formattedGroups = muscleGroups.Select(g => g);
        return $"Treino completo com {exerciseCount} exercícios para {string.Join(", ", formattedGroups)}, focado em hipertrofia muscular";
    }

    private static async Task<AIWorkoutResponse> GenerateWorkoutWithAI(AIWorkoutRequest request, string apiKey, dynamic? userProfile = null)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        httpClient.Timeout = TimeSpan.FromSeconds(45); // Reasonable timeout for single workout

        // Build user profile context
        var profileContext = BuildUserProfileContext(userProfile);

        var systemPrompt = @"Você é um personal trainer brasileiro altamente qualificado e certificado, especializado em prescrição de treinos personalizados e seguros. Crie treinos DETALHADOS, EFICAZES e CIENTIFICAMENTE EMBASADOS.

REGRAS FUNDAMENTAIS:
1. TODOS os nomes de exercícios DEVEM estar em PORTUGUÊS COMPLETO (ex: ""Supino Reto com Barra"", ""Agachamento Livre com Barra"", ""Rosca Direta com Halteres"")
2. RESPEITE ABSOLUTAMENTE E LITERALMENTE o que o usuário pediu no prompt:
   - Se pedir ""treino focado em glúteos e pernas"", 100% dos exercícios DEVEM ser para glúteos e pernas
   - Se mencionar problema em algum músculo (ex: ""tenho dor no joelho""), EVITE exercícios que sobrecarreguem essa região
   - Se pedir foco em área específica (ex: ""quero focar em glúteos""), priorize exercícios que trabalhem DIRETAMENTE esse músculo
   - NÃO inclua exercícios de outros grupos musculares a menos que o usuário explicitamente mencione
3. RESPEITE ESTRITAMENTE todas as restrições do usuário (ex: se pedir ""sem supino"", NÃO inclua nenhuma variação de supino)
4. ADAPTE o treino ao GÊNERO do usuário:
   - Mulheres: Priorize glúteos, pernas, core quando mencionados; ajuste volume e intensidade considerando diferenças hormonais
   - Homens: Maior ênfase em força e hipertrofia de tronco superior quando apropriado
5. Instruções devem ser claras, detalhadas e profissionais em português, incluindo técnica correta e dicas de segurança
6. Adapte sets, reps, rest e exercícios ao nível do usuário:
   - Iniciante: 2-3 exercícios/grupo, 3-4 sets, 10-12 reps, descanso 90-120s, MÁXIMO 7 exercícios por treino
   - Intermediário: 3-4 exercícios/grupo, 3-5 sets, 8-12 reps, descanso 60-90s, MÁXIMO 9 exercícios por treino
   - Avançado: 4-5 exercícios/grupo, 4-6 sets, 6-12 reps, descanso 45-90s, MÁXIMO 11 exercícios por treino
7. NUNCA crie treinos com mais de 12 exercícios - isso leva a overtraining e baixa qualidade de execução
8. Selecione exercícios apropriados ao equipamento disponível
9. Priorize exercícios compostos primeiro, depois isolados
10. Inclua aquecimento específico quando necessário
11. Seja criativo mas realista com variações de exercícios

ESTRUTURA DE TREINO IDEAL:
- Aquecimento (5-10 min) quando apropriado
- Exercícios compostos principais (1-2)
- Exercícios complementares (2-3)
- Exercícios de isolamento (1-2)
- Alongamento/desaquecimento quando relevante

Retorne APENAS um JSON válido no seguinte formato (sem markdown, sem comentários, sem ```json):
{
  ""title"": ""Título Descritivo do Treino em Português"",
  ""description"": ""Descrição detalhada do treino, objetivo, metodologia e benefícios esperados"",
  ""duration"": 60,
  ""exercises"": [
    {
      ""name"": ""Nome Completo e Específico do Exercício em Português"",
      ""bodyPart"": ""peito|costas|ombros|bíceps|tríceps|pernas|panturrilhas|abdômen|cardio|core"",
      ""equipment"": ""barra|halteres|cabo|peso corporal|máquina|kettlebell|elástico|banco"",
      ""sets"": 4,
      ""reps"": ""8-12"",
      ""rest"": ""60-90s"",
      ""instructions"": [
        ""Setup/Posicionamento: descrição detalhada da posição inicial"",
        ""Execução: descrição completa do movimento concêntrico e excêntrico"",
        ""Respiração: quando inspirar e expirar"",
        ""Segurança/Dicas: pontos de atenção, erros comuns a evitar, ativação muscular""
      ]
    }
  ]
}";

        var fitnessLevel = request.FitnessLevel ?? "intermediário";
        var duration = request.Duration ?? 60;

        var userPrompt = $@"Crie um treino personalizado COMPLETO seguindo EXATAMENTE estas especificações:

REQUISITOS DO USUÁRIO:
{request.Prompt}

{profileContext}

PARÂMETROS OBRIGATÓRIOS:
- NÍVEL DE CONDICIONAMENTO: {fitnessLevel}
- DURAÇÃO DO TREINO: {duration} minutos (ajuste o número de exercícios e sets para caber nesse tempo)
{(request.Equipment != null && request.Equipment.Any() ?
$@"- EQUIPAMENTOS DISPONÍVEIS: {string.Join(", ", request.Equipment)}
  RESTRIÇÃO: Use APENAS os equipamentos listados acima. Não inclua exercícios que requerem outros equipamentos." :
"- EQUIPAMENTOS: Academia completa - todos os equipamentos disponíveis")}

INSTRUÇÕES CRÍTICAS:
1. PERSONALIZE o treino baseado no perfil do usuário acima (idade, peso, altura, etc.)
2. Se o usuário mencionar exercícios para EVITAR ou EXCLUIR, você DEVE respeitar isso COMPLETAMENTE (incluindo variações)
3. Calcule o número adequado de exercícios para caber no tempo especificado
4. Mantenha o treino balanceado e eficiente
5. Se equipamentos limitados, seja criativo com alternativas usando apenas o que está disponível
6. Inclua sempre instruções de segurança e técnica correta
7. Considere possíveis limitações físicas baseadas na idade e condição física do usuário";

        var payload = new
        {
            model = "gpt-4o-mini", // Better model, still affordable
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.7,
            response_format = new { type = "json_object" } // Force JSON response
        };

        var response = await httpClient.PostAsync(
            "https://api.openai.com/v1/chat/completions",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        );

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"OpenAI API error: {responseBody}");
        }

        var jsonResponse = JsonDocument.Parse(responseBody);
        var content = jsonResponse.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrEmpty(content))
        {
            throw new Exception("AI returned empty response");
        }

        // Clean up the response - remove markdown code blocks if present
        content = content.Trim();
        if (content.StartsWith("```json"))
        {
            content = content.Substring(7);
        }
        else if (content.StartsWith("```"))
        {
            content = content.Substring(3);
        }

        if (content.EndsWith("```"))
        {
            content = content.Substring(0, content.Length - 3);
        }

        content = content.Trim();

        try
        {
            var workout = JsonSerializer.Deserialize<AIWorkoutResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (workout == null)
            {
                throw new Exception("Failed to parse AI response - null result");
            }

            // Validate workout structure
            if (string.IsNullOrWhiteSpace(workout.Title))
            {
                throw new Exception("AI returned workout without title");
            }

            if (workout.Exercises == null || !workout.Exercises.Any())
            {
                throw new Exception("AI returned workout without exercises");
            }

            // Enforce maximum exercise count
            const int MAX_EXERCISES = 12;
            if (workout.Exercises.Count > MAX_EXERCISES)
            {
                // Trim to maximum - keep first exercises which are usually compound/main exercises
                workout = workout with {
                    Exercises = workout.Exercises.Take(MAX_EXERCISES).ToList()
                };
            }

            // Validate each exercise
            foreach (var exercise in workout.Exercises)
            {
                if (string.IsNullOrWhiteSpace(exercise.Name))
                {
                    throw new Exception("AI returned exercise without name");
                }

                if (exercise.Instructions == null || !exercise.Instructions.Any())
                {
                    throw new Exception($"Exercise '{exercise.Name}' has no instructions");
                }
            }

            return workout;
        }
        catch (JsonException ex)
        {
            throw new Exception($"Invalid JSON from AI: {ex.Message}. Content preview: {content.Substring(0, Math.Min(300, content.Length))}...");
        }
    }

    private static async Task<AIWorkoutPlanResponse> GeneratePlanWithAI(AIWorkoutPlanRequest request, string apiKey, dynamic? userProfile = null)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        httpClient.Timeout = TimeSpan.FromSeconds(60); // Longer timeout for plan generation

        // Build user profile context
        var profileContext = BuildUserProfileContext(userProfile);

        var daysPerWeek = request.DaysPerWeek ?? 4;
        var fitnessLevel = request.FitnessLevel ?? "intermediário";
        var goal = request.Goal ?? "hipertrofia";

        var systemPrompt = @"Você é um personal trainer brasileiro altamente qualificado e certificado, especializado em periodização e programação de treinos. Sua tarefa é criar planos de treino completos, personalizados e cientificamente embasados.

REGRA CRÍTICA #1 - RESPEITAR O PEDIDO DO USUÁRIO:
⚠️ ATENÇÃO MÁXIMA: O que o usuário pedir no prompt É LEI. Não interprete, não balance, não adicione nada que não foi pedido.
- Se pedir ""treino focado em membros inferiores"" ou ""lower body"" → TODOS os dias devem ser de pernas/glúteos/panturrilhas
- Se pedir ""treino focado em glúteos"" → MAIORIA dos exercícios devem trabalhar glúteos diretamente
- Se pedir ""sem peito"" → ZERO exercícios de peito, nem ""para balancear""
- Se mencionar problema/dor → EVITE completamente exercícios que afetem essa região
- NÃO crie planos ""balanceados"" se o usuário pediu foco específico
- NÃO adicione upper body em plano de lower body ""para completar""

REGRAS FUNDAMENTAIS:
1. TODOS os nomes de exercícios DEVEM estar em PORTUGUÊS (ex: ""Supino Reto com Barra"", ""Agachamento Livre"", ""Remada Curvada"")
2. RESPEITE ESTRITAMENTE todas as restrições do usuário (ex: se pedir ""sem supino"", NÃO inclua nenhuma variação de supino)
3. Implemente PERIODIZAÇÃO adequada - varie volume e intensidade ao longo das semanas
4. Aplique PROGRESSIVE OVERLOAD - aumente gradualmente carga, volume ou densidade
5. Se usuário NÃO especificou balanceamento, NÃO balance - respeite o foco dele
6. Instruções devem ser claras, detalhadas e profissionais em português
7. Adapte sets, reps e rest ao nível do usuário e objetivo específico

PARÂMETROS DE VOLUME POR NÍVEL:
- Iniciante: 2-3 exercícios por grupo muscular, 3-4 sets, descanso 90-120s
- Intermediário: 3-4 exercícios por grupo muscular, 3-5 sets, descanso 60-90s
- Avançado: 4-5 exercícios por grupo muscular, 4-6 sets, descanso 45-90s

PARÂMETROS POR OBJETIVO:
- Força: 3-6 reps, descanso 3-5min, foco em compostos
- Hipertrofia: 6-12 reps, descanso 60-90s, mix de compostos e isolados
- Resistência: 12-20 reps, descanso 30-60s, mais volume
- Condicionamento: Circuitos, supersets, descanso mínimo

PERIODIZAÇÃO (4 semanas):
- Semana 1: Volume moderado, intensidade moderada (base)
- Semana 2: Volume alto, intensidade moderada (acumulação)
- Semana 3: Volume moderado, intensidade alta (intensificação)
- Semana 4: Volume baixo, intensidade moderada (deload/recuperação)

Retorne APENAS um JSON válido no seguinte formato (sem markdown, sem ```json):
{
  ""title"": ""Nome do Plano de Treino"",
  ""description"": ""Descrição detalhada do plano, metodologia e progressão"",
  ""weeksCount"": 4,
  ""daysPerWeek"": {daysPerWeek},
  ""goal"": ""{goal}"",
  ""days"": [
    {
      ""dayName"": ""Treino A"",
      ""title"": ""Peito e Tríceps"",
      ""focus"": ""peito e tríceps"",
      ""exercises"": [
        {
          ""name"": ""Nome Completo do Exercício em Português"",
          ""bodyPart"": ""peito|costas|ombros|bíceps|tríceps|pernas|panturrilhas|abdômen|cardio"",
          ""equipment"": ""barra|halteres|cabo|peso corporal|máquina|kettlebell|elástico"",
          ""sets"": ""3-4 (progressão semanal)"",
          ""reps"": ""8-12 (ajustar por semana)"",
          ""rest"": ""60-90s"",
          ""instructions"": [
            ""Passo 1: Setup e posicionamento detalhado"",
            ""Passo 2: Execução da fase concêntrica"",
            ""Passo 3: Execução da fase excêntrica"",
            ""Passo 4: Dicas de segurança e ativação muscular""
          ],
          ""progressionNotes"": ""Semana 1: 3x10 | Semana 2: 4x10 | Semana 3: 4x8 (↑ carga) | Semana 4: 3x10 (↓ carga)""
        }
      ]
    }
  ]
}";

        var userPrompt = $@"Crie um plano de treino COMPLETO e PERSONALIZADO seguindo EXATAMENTE estas especificações:

⚠️⚠️⚠️ REQUISITO PRINCIPAL DO USUÁRIO (PRIORIDADE MÁXIMA): ⚠️⚠️⚠️
{request.Prompt}

👆 LEIA NOVAMENTE O PEDIDO ACIMA E CRIE O PLANO EXATAMENTE COMO SOLICITADO 👆

{profileContext}

PARÂMETROS OBRIGATÓRIOS:
- DIAS POR SEMANA: {daysPerWeek} dias
- NÍVEL DE CONDICIONAMENTO: {fitnessLevel}
- OBJETIVO PRINCIPAL: {goal}
- EQUIPAMENTOS: Todos disponíveis (academia completa)

INSTRUÇÕES CRÍTICAS:
1. ⚠️ O PEDIDO DO USUÁRIO É ABSOLUTO - Se pedir ""lower body"", ""membros inferiores"", ou ""focado em pernas/glúteos"", TODOS os {daysPerWeek} dias DEVEM ser de lower body
2. ⚠️ NÃO adicione upper body ""para balancear"" se o usuário NÃO pediu
3. ⚠️ NÃO crie plano ""completo"" ou ""balanceado"" se o usuário pediu foco específico
4. PERSONALIZE o plano baseado no perfil do usuário acima (idade, peso, altura, IMC, gênero)
5. Crie EXATAMENTE {daysPerWeek} treinos diferentes respeitando o foco solicitado
6. Se o usuário mencionar exercícios para EVITAR ou EXCLUIR, você DEVE respeitar isso COMPLETAMENTE
7. Inclua notas de progressão semanal para CADA exercício
8. Adapte o volume total ao nível de condicionamento E ao perfil físico do usuário
9. Considere possíveis limitações físicas baseadas na idade e condição física

IMPORTANTE: Este é um plano de 4 semanas com periodização. Inclua instruções claras de como progredir a cada semana.";

        var payload = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.7,
            max_tokens = 4000, // Allow longer responses for complete plans
            response_format = new { type = "json_object" }
        };

        var response = await httpClient.PostAsync(
            "https://api.openai.com/v1/chat/completions",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        );

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"OpenAI API error: {responseBody}");
        }

        var jsonResponse = JsonDocument.Parse(responseBody);
        var content = jsonResponse.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrEmpty(content))
        {
            throw new Exception("AI returned empty response");
        }

        // Clean up the response - remove markdown code blocks if present
        content = content.Trim();
        if (content.StartsWith("```json"))
        {
            content = content.Substring(7);
        }
        else if (content.StartsWith("```"))
        {
            content = content.Substring(3);
        }

        if (content.EndsWith("```"))
        {
            content = content.Substring(0, content.Length - 3);
        }

        content = content.Trim();

        try
        {
            var plan = JsonSerializer.Deserialize<AIWorkoutPlanResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (plan == null || plan.Days == null || !plan.Days.Any())
            {
                throw new Exception("AI returned invalid plan structure");
            }

            // Validate plan has correct number of days
            if (plan.Days.Count != daysPerWeek)
            {
                throw new Exception($"AI returned {plan.Days.Count} days but {daysPerWeek} were requested");
            }

            // Validate and enforce maximum exercises per day
            const int MAX_EXERCISES_PER_DAY = 10;
            var validatedDays = new List<WorkoutDay>();

            foreach (var day in plan.Days)
            {
                if (day.Exercises == null || !day.Exercises.Any())
                {
                    throw new Exception($"Day '{day.DayName}' has no exercises");
                }

                // Enforce maximum exercise count per day
                if (day.Exercises.Count > MAX_EXERCISES_PER_DAY)
                {
                    validatedDays.Add(day with {
                        Exercises = day.Exercises.Take(MAX_EXERCISES_PER_DAY).ToList()
                    });
                }
                else
                {
                    validatedDays.Add(day);
                }
            }

            // Return plan with validated days
            plan = plan with { Days = validatedDays };

            return plan;
        }
        catch (JsonException ex)
        {
            throw new Exception($"Invalid JSON from AI: {ex.Message}. Content preview: {content.Substring(0, Math.Min(300, content.Length))}...");
        }
    }

    private static string BuildUserProfileContext(dynamic? userProfile)
    {
        if (userProfile == null) return "";

        var context = new StringBuilder("PERFIL DO USUÁRIO:\n");

        if (!string.IsNullOrEmpty(userProfile.Name))
            context.AppendLine($"- Nome: {userProfile.Name}");

        if (userProfile.DateOfBirth != null)
        {
            var age = DateTime.Now.Year - ((DateTime)userProfile.DateOfBirth).Year;
            context.AppendLine($"- Idade: {age} anos");
        }

        if (!string.IsNullOrEmpty(userProfile.Gender))
        {
            var genderLabel = userProfile.Gender == "M" ? "Masculino" : userProfile.Gender == "F" ? "Feminino" : userProfile.Gender;
            context.AppendLine($"- Gênero: {genderLabel}");
        }

        if (userProfile.Height != null)
            context.AppendLine($"- Altura: {userProfile.Height} cm");

        if (userProfile.Weight != null)
        {
            context.AppendLine($"- Peso: {userProfile.Weight} kg");

            // Calculate BMI if we have both height and weight
            if (userProfile.Height != null)
            {
                double heightInMeters = (double)userProfile.Height / 100.0;
                double bmi = (double)userProfile.Weight / (heightInMeters * heightInMeters);
                context.AppendLine($"- IMC: {bmi:F1}");
            }
        }

        if (!string.IsNullOrEmpty(userProfile.Location))
            context.AppendLine($"- Localização: {userProfile.Location}");

        if (!string.IsNullOrEmpty(userProfile.GymName))
            context.AppendLine($"- Academia: {userProfile.GymName}");

        if (!string.IsNullOrEmpty(userProfile.Bio))
            context.AppendLine($"- Informações adicionais: {userProfile.Bio}");

        return context.ToString();
    }

    private static async Task<AIWorkoutResponse> GenerateWorkoutWithGemini(AIWorkoutRequest request, string apiKey, dynamic? userProfile = null)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(45);

        // Build user profile context
        var profileContext = BuildUserProfileContext(userProfile);

        var systemPrompt = @"Você é um personal trainer brasileiro altamente qualificado e certificado, especializado em prescrição de treinos personalizados e seguros. Crie treinos DETALHADOS, EFICAZES e CIENTIFICAMENTE EMBASADOS.

REGRAS FUNDAMENTAIS:
1. TODOS os nomes de exercícios DEVEM estar em PORTUGUÊS COMPLETO (ex: ""Supino Reto com Barra"", ""Agachamento Livre com Barra"", ""Rosca Direta com Halteres"")
2. RESPEITE ABSOLUTAMENTE E LITERALMENTE o que o usuário pediu no prompt:
   - Se pedir ""treino focado em glúteos e pernas"", 100% dos exercícios DEVEM ser para glúteos e pernas
   - Se mencionar problema em algum músculo (ex: ""tenho dor no joelho""), EVITE exercícios que sobrecarreguem essa região
   - Se pedir foco em área específica (ex: ""quero focar em glúteos""), priorize exercícios que trabalhem DIRETAMENTE esse músculo
   - NÃO inclua exercícios de outros grupos musculares a menos que o usuário explicitamente mencione
3. RESPEITE ESTRITAMENTE todas as restrições do usuário (ex: se pedir ""sem supino"", NÃO inclua nenhuma variação de supino)
4. ADAPTE o treino ao GÊNERO do usuário:
   - Mulheres: Priorize glúteos, pernas, core quando mencionados; ajuste volume e intensidade considerando diferenças hormonais
   - Homens: Maior ênfase em força e hipertrofia de tronco superior quando apropriado
5. Instruções devem ser claras, detalhadas e profissionais em português, incluindo técnica correta e dicas de segurança
6. Adapte sets, reps, rest e exercícios ao nível do usuário:
   - Iniciante: 2-3 exercícios/grupo, 3-4 sets, 10-12 reps, descanso 90-120s, MÁXIMO 7 exercícios por treino
   - Intermediário: 3-4 exercícios/grupo, 3-5 sets, 8-12 reps, descanso 60-90s, MÁXIMO 9 exercícios por treino
   - Avançado: 4-5 exercícios/grupo, 4-6 sets, 6-12 reps, descanso 45-90s, MÁXIMO 11 exercícios por treino
7. NUNCA crie treinos com mais de 12 exercícios - isso leva a overtraining e baixa qualidade de execução
8. Selecione exercícios apropriados ao equipamento disponível
9. Priorize exercícios compostos primeiro, depois isolados
10. Inclua aquecimento específico quando necessário
11. Seja criativo mas realista com variações de exercícios

Retorne APENAS um JSON válido no seguinte formato:
{
  ""title"": ""Título Descritivo do Treino em Português"",
  ""description"": ""Descrição detalhada do treino, objetivo, metodologia e benefícios esperados"",
  ""duration"": 60,
  ""exercises"": [
    {
      ""name"": ""Nome Completo e Específico do Exercício em Português"",
      ""bodyPart"": ""peito|costas|ombros|bíceps|tríceps|pernas|panturrilhas|abdômen|cardio|core"",
      ""equipment"": ""barra|halteres|cabo|peso corporal|máquina|kettlebell|elástico|banco"",
      ""sets"": 4,
      ""reps"": ""8-12"",
      ""rest"": ""60-90s"",
      ""instructions"": [
        ""Setup/Posicionamento: descrição detalhada da posição inicial"",
        ""Execução: descrição completa do movimento concêntrico e excêntrico"",
        ""Respiração: quando inspirar e expirar"",
        ""Segurança/Dicas: pontos de atenção, erros comuns a evitar, ativação muscular""
      ]
    }
  ]
}";

        var fitnessLevel = request.FitnessLevel ?? "intermediário";
        var duration = request.Duration ?? 60;

        var userPrompt = $@"Crie um treino personalizado COMPLETO seguindo EXATAMENTE estas especificações:

REQUISITOS DO USUÁRIO:
{request.Prompt}

{profileContext}

PARÂMETROS OBRIGATÓRIOS:
- NÍVEL DE CONDICIONAMENTO: {fitnessLevel}
- DURAÇÃO DO TREINO: {duration} minutos (ajuste o número de exercícios e sets para caber nesse tempo)
{(request.Equipment != null && request.Equipment.Any() ?
$@"- EQUIPAMENTOS DISPONÍVEIS: {string.Join(", ", request.Equipment)}
  RESTRIÇÃO: Use APENAS os equipamentos listados acima." :
"- EQUIPAMENTOS: Academia completa - todos os equipamentos disponíveis")}

INSTRUÇÕES CRÍTICAS:
1. PERSONALIZE o treino baseado no perfil do usuário acima (idade, peso, altura, etc.)
2. Se o usuário mencionar exercícios para EVITAR ou EXCLUIR, você DEVE respeitar isso COMPLETAMENTE
3. Calcule o número adequado de exercícios para caber no tempo especificado
4. Mantenha o treino balanceado e eficiente
5. Inclua sempre instruções de segurança e técnica correta
6. Considere possíveis limitações físicas baseadas na idade e condição física do usuário";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = systemPrompt + "\n\n" + userPrompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.7,
                maxOutputTokens = 2048,
                responseMimeType = "application/json"
            }
        };

        var response = await httpClient.PostAsync(
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        );

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Gemini API error: {responseBody}");
        }

        var jsonResponse = JsonDocument.Parse(responseBody);
        var content = jsonResponse.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrEmpty(content))
        {
            throw new Exception("Gemini returned empty response");
        }

        content = content.Trim();
        if (content.StartsWith("```json"))
        {
            content = content.Substring(7);
        }
        else if (content.StartsWith("```"))
        {
            content = content.Substring(3);
        }

        if (content.EndsWith("```"))
        {
            content = content.Substring(0, content.Length - 3);
        }

        content = content.Trim();

        try
        {
            var workout = JsonSerializer.Deserialize<AIWorkoutResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (workout == null || workout.Exercises == null || !workout.Exercises.Any())
            {
                throw new Exception("Gemini returned invalid workout structure");
            }

            // Enforce maximum exercise count
            const int MAX_EXERCISES = 12;
            if (workout.Exercises.Count > MAX_EXERCISES)
            {
                // Trim to maximum - keep first exercises which are usually compound/main exercises
                workout = workout with {
                    Exercises = workout.Exercises.Take(MAX_EXERCISES).ToList()
                };
            }

            return workout;
        }
        catch (JsonException ex)
        {
            throw new Exception($"Invalid JSON from Gemini: {ex.Message}. Content preview: {content.Substring(0, Math.Min(300, content.Length))}...");
        }
    }

    private static async Task<AIWorkoutPlanResponse> GeneratePlanWithGemini(AIWorkoutPlanRequest request, string apiKey, dynamic? userProfile = null)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(60);

        // Build user profile context
        var profileContext = BuildUserProfileContext(userProfile);

        var daysPerWeek = request.DaysPerWeek ?? 4;
        var fitnessLevel = request.FitnessLevel ?? "intermediário";
        var goal = request.Goal ?? "hipertrofia";

        var systemPrompt = @"Você é um personal trainer brasileiro altamente qualificado, especializado em periodização e programação de treinos. Crie planos completos, personalizados e cientificamente embasados.

REGRA CRÍTICA #1 - RESPEITAR O PEDIDO DO USUÁRIO:
⚠️ ATENÇÃO MÁXIMA: O que o usuário pedir no prompt É LEI. Não interprete, não balance, não adicione nada que não foi pedido.
- Se pedir ""treino focado em membros inferiores"" ou ""lower body"" → TODOS os dias devem ser de pernas/glúteos/panturrilhas
- Se pedir ""treino focado em glúteos"" → MAIORIA dos exercícios devem trabalhar glúteos diretamente
- NÃO crie planos ""balanceados"" se o usuário pediu foco específico
- NÃO adicione upper body em plano de lower body ""para completar""

REGRAS FUNDAMENTAIS:
1. TODOS os nomes de exercícios em PORTUGUÊS (ex: ""Supino Reto com Barra"", ""Agachamento Livre"")
2. RESPEITE ESTRITAMENTE todas as restrições do usuário
3. Implemente PERIODIZAÇÃO adequada - varie volume e intensidade
4. Aplique PROGRESSIVE OVERLOAD - aumente gradualmente carga, volume ou densidade
5. Se usuário NÃO especificou balanceamento, NÃO balance - respeite o foco dele
6. Instruções claras e profissionais em português
7. Adapte sets, reps e rest ao nível do usuário

PERIODIZAÇÃO (4 semanas):
- Semana 1: Volume moderado, intensidade moderada (base)
- Semana 2: Volume alto, intensidade moderada (acumulação)
- Semana 3: Volume moderado, intensidade alta (intensificação)
- Semana 4: Volume baixo, intensidade moderada (deload/recuperação)

Retorne APENAS um JSON válido:
{
  ""title"": ""Nome do Plano"",
  ""description"": ""Descrição detalhada do plano"",
  ""weeksCount"": 4,
  ""daysPerWeek"": " + daysPerWeek + @",
  ""goal"": """ + goal + @""",
  ""days"": [
    {
      ""dayName"": ""Treino A"",
      ""title"": ""Peito e Tríceps"",
      ""focus"": ""peito e tríceps"",
      ""exercises"": [
        {
          ""name"": ""Nome Completo em Português"",
          ""bodyPart"": ""peito|costas|ombros|bíceps|tríceps|pernas|panturrilhas|abdômen"",
          ""equipment"": ""barra|halteres|cabo|peso corporal|máquina"",
          ""sets"": ""3-4"",
          ""reps"": ""8-12"",
          ""rest"": ""60-90s"",
          ""instructions"": [
            ""Passo 1: Setup"",
            ""Passo 2: Execução"",
            ""Passo 3: Dicas""
          ],
          ""progressionNotes"": ""Semana 1: 3x10 | Semana 2: 4x10 | Semana 3: 4x8 (↑ carga) | Semana 4: 3x10 (deload)""
        }
      ]
    }
  ]
}";

        var userPrompt = $@"Crie um plano de treino COMPLETO:

⚠️⚠️⚠️ REQUISITO PRINCIPAL DO USUÁRIO (PRIORIDADE MÁXIMA): ⚠️⚠️⚠️
{request.Prompt}

👆 LEIA NOVAMENTE O PEDIDO ACIMA E CRIE O PLANO EXATAMENTE COMO SOLICITADO 👆

{profileContext}

PARÂMETROS:
- DIAS POR SEMANA: {daysPerWeek}
- NÍVEL: {fitnessLevel}
- OBJETIVO: {goal}

INSTRUÇÕES CRÍTICAS:
1. ⚠️ O PEDIDO DO USUÁRIO É ABSOLUTO - Se pedir ""lower body"", TODOS os {daysPerWeek} dias DEVEM ser de lower body
2. ⚠️ NÃO adicione upper body ""para balancear"" se o usuário NÃO pediu
3. ⚠️ NÃO crie plano ""completo"" se o usuário pediu foco específico
4. PERSONALIZE baseado no perfil do usuário (idade, peso, altura, IMC, gênero)
5. Crie EXATAMENTE {daysPerWeek} treinos diferentes respeitando o foco solicitado
6. Inclua notas de progressão para CADA exercício
7. Respeite todas as restrições do usuário
8. Adapte o volume ao perfil físico do usuário";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = systemPrompt + "\n\n" + userPrompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.7,
                maxOutputTokens = 4096,
                responseMimeType = "application/json"
            }
        };

        var response = await httpClient.PostAsync(
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        );

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Gemini API error: {responseBody}");
        }

        var jsonResponse = JsonDocument.Parse(responseBody);
        var content = jsonResponse.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrEmpty(content))
        {
            throw new Exception("Gemini returned empty response");
        }

        content = content.Trim();
        if (content.StartsWith("```json"))
        {
            content = content.Substring(7);
        }
        else if (content.StartsWith("```"))
        {
            content = content.Substring(3);
        }

        if (content.EndsWith("```"))
        {
            content = content.Substring(0, content.Length - 3);
        }

        content = content.Trim();

        try
        {
            var plan = JsonSerializer.Deserialize<AIWorkoutPlanResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (plan == null || plan.Days == null || !plan.Days.Any())
            {
                throw new Exception("Gemini returned invalid plan structure");
            }

            if (plan.Days.Count != daysPerWeek)
            {
                throw new Exception($"Gemini returned {plan.Days.Count} days but {daysPerWeek} were requested");
            }

            // Validate and enforce maximum exercises per day
            const int MAX_EXERCISES_PER_DAY = 10;
            var validatedDays = new List<WorkoutDay>();

            foreach (var day in plan.Days)
            {
                if (day.Exercises == null || !day.Exercises.Any())
                {
                    throw new Exception($"Day '{day.DayName}' has no exercises");
                }

                // Enforce maximum exercise count per day
                if (day.Exercises.Count > MAX_EXERCISES_PER_DAY)
                {
                    validatedDays.Add(day with {
                        Exercises = day.Exercises.Take(MAX_EXERCISES_PER_DAY).ToList()
                    });
                }
                else
                {
                    validatedDays.Add(day);
                }
            }

            // Return plan with validated days
            plan = plan with { Days = validatedDays };

            return plan;
        }
        catch (JsonException ex)
        {
            throw new Exception($"Invalid JSON from Gemini: {ex.Message}. Content preview: {content.Substring(0, Math.Min(300, content.Length))}...");
        }
    }

    private static AIWorkoutPlanResponse GenerateMockPlan(string prompt, int daysPerWeek, string? fitnessLevel = null)
    {
        Console.WriteLine("=== MOCK PLAN GENERATION DEBUG ===");
        Console.WriteLine($"Prompt: {prompt}");
        Console.WriteLine($"Days Per Week: {daysPerWeek}");
        Console.WriteLine($"Fitness Level: '{fitnessLevel ?? "NULL"}'");

        var random = new Random();
        var level = fitnessLevel?.ToLower() ?? "intermediário";
        var days = new List<WorkoutDay>();

        // Determine exercise count per day based on fitness level
        var (minExercisesPerDay, maxExercisesPerDay) = level switch
        {
            "iniciante" or "beginner" => (4, 5),
            "avançado" or "advanced" => (6, 8),
            _ => (5, 7) // intermediário
        };

        Console.WriteLine($"Exercises Per Day Range: {minExercisesPerDay}-{maxExercisesPerDay}");

        // Parse user prompt to detect focus areas
        var parsedPrompt = ParsePrompt(prompt.ToLower());
        var focusMuscleGroups = parsedPrompt.MuscleGroups;

        Console.WriteLine($"Focus Muscle Groups Detected: {string.Join(", ", focusMuscleGroups)}");

        // Define workout splits based on user focus or default to standard splits
        // Format: (DayName, Title, MuscleGroups)
        (string DayName, string Title, string[] MuscleGroups)[] workoutSplits;

        // If user specified specific muscle groups, create a focused plan
        if (focusMuscleGroups.Any())
        {
            Console.WriteLine("Creating FOCUSED plan based on user request");

            // Check if focus is lower body
            var isLowerBodyFocus = focusMuscleGroups.Any(m => m == "pernas" || m == "glúteos" || m == "panturrilha");
            // Check if focus is upper body
            var isUpperBodyFocus = focusMuscleGroups.Any(m => m == "peito" || m == "costas" || m == "ombros" || m == "bíceps" || m == "tríceps");

            if (isLowerBodyFocus && !isUpperBodyFocus)
            {
                // Lower body focused plan
                workoutSplits = daysPerWeek switch
                {
                    2 => new[] {
                        ("Treino A", "Glúteos e Quadríceps", new[] { "glúteos", "pernas" }),
                        ("Treino B", "Posteriores e Panturrilhas", new[] { "pernas", "panturrilha" })
                    },
                    3 => new[] {
                        ("Treino A", "Glúteos e Quadríceps", new[] { "glúteos", "pernas" }),
                        ("Treino B", "Posteriores e Panturrilhas", new[] { "pernas", "panturrilha" }),
                        ("Treino C", "Glúteos e Pernas Completo", new[] { "glúteos", "pernas", "panturrilha" })
                    },
                    4 => new[] {
                        ("Treino A", "Glúteos Foco", new[] { "glúteos" }),
                        ("Treino B", "Quadríceps e Glúteos", new[] { "pernas", "glúteos" }),
                        ("Treino C", "Posteriores e Panturrilhas", new[] { "pernas", "panturrilha" }),
                        ("Treino D", "Lower Body Completo", new[] { "glúteos", "pernas", "panturrilha" })
                    },
                    5 => new[] {
                        ("Treino A", "Glúteos Intenso", new[] { "glúteos" }),
                        ("Treino B", "Quadríceps", new[] { "pernas" }),
                        ("Treino C", "Glúteos e Posteriores", new[] { "glúteos", "pernas" }),
                        ("Treino D", "Panturrilhas e Core", new[] { "panturrilha", "abdômen" }),
                        ("Treino E", "Lower Body Full", new[] { "glúteos", "pernas", "panturrilha" })
                    },
                    _ => new[] {
                        ("Treino A", "Glúteos Foco", new[] { "glúteos" }),
                        ("Treino B", "Quadríceps e Glúteos", new[] { "pernas", "glúteos" }),
                        ("Treino C", "Posteriores", new[] { "pernas" }),
                        ("Treino D", "Panturrilhas e Core", new[] { "panturrilha", "abdômen" })
                    }
                };
            }
            else if (isUpperBodyFocus && !isLowerBodyFocus)
            {
                // Upper body focused plan
                workoutSplits = daysPerWeek switch
                {
                    2 => new[] {
                        ("Treino A", "Peito e Ombros", new[] { "peito", "ombros", "tríceps" }),
                        ("Treino B", "Costas e Braços", new[] { "costas", "bíceps" })
                    },
                    3 => new[] {
                        ("Treino A", "Peito e Tríceps", new[] { "peito", "tríceps" }),
                        ("Treino B", "Costas e Bíceps", new[] { "costas", "bíceps" }),
                        ("Treino C", "Ombros e Braços", new[] { "ombros", "bíceps", "tríceps" })
                    },
                    4 => new[] {
                        ("Treino A", "Peito", new[] { "peito", "tríceps" }),
                        ("Treino B", "Costas", new[] { "costas", "bíceps" }),
                        ("Treino C", "Ombros", new[] { "ombros" }),
                        ("Treino D", "Braços Completo", new[] { "bíceps", "tríceps" })
                    },
                    _ => new[] {
                        ("Treino A", "Peito e Tríceps", new[] { "peito", "tríceps" }),
                        ("Treino B", "Costas e Bíceps", new[] { "costas", "bíceps" }),
                        ("Treino C", "Ombros", new[] { "ombros" }),
                        ("Treino D", "Braços", new[] { "bíceps", "tríceps" })
                    }
                };
            }
            else
            {
                // Specific muscle group focus (e.g., just glutes, just chest, etc.)
                var primaryMuscle = focusMuscleGroups.First();
                var secondaryMuscles = focusMuscleGroups.Skip(1).ToArray();

                workoutSplits = Enumerable.Range(0, daysPerWeek)
                    .Select(i =>
                    {
                        var dayLetter = (char)('A' + i);
                        if (i % 2 == 0)
                        {
                            return ($"Treino {dayLetter}", $"{char.ToUpper(primaryMuscle[0])}{primaryMuscle.Substring(1)} Foco", new[] { primaryMuscle });
                        }
                        else if (secondaryMuscles.Any())
                        {
                            var muscle = secondaryMuscles[i % secondaryMuscles.Length];
                            return ($"Treino {dayLetter}", $"{char.ToUpper(muscle[0])}{muscle.Substring(1)}", new[] { muscle });
                        }
                        else
                        {
                            return ($"Treino {dayLetter}", $"{char.ToUpper(primaryMuscle[0])}{primaryMuscle.Substring(1)} Variação", new[] { primaryMuscle });
                        }
                    })
                    .ToArray();
            }
        }
        else
        {
            // Default balanced plan if no specific focus detected
            Console.WriteLine("Creating DEFAULT balanced plan");
            workoutSplits = daysPerWeek switch
        {
            2 => new[] {
                ("Treino A", "Corpo Superior", new[] { "peito", "costas", "ombros" }),
                ("Treino B", "Corpo Inferior", new[] { "pernas", "panturrilha", "abdômen" })
            },
            3 => new[] {
                ("Treino A", "Peito e Tríceps", new[] { "peito", "tríceps" }),
                ("Treino B", "Costas e Bíceps", new[] { "costas", "bíceps" }),
                ("Treino C", "Pernas e Panturrilhas", new[] { "pernas", "panturrilha" })
            },
            4 => new[] {
                ("Treino A", "Peito e Tríceps", new[] { "peito", "tríceps" }),
                ("Treino B", "Costas e Bíceps", new[] { "costas", "bíceps" }),
                ("Treino C", "Pernas e Panturrilhas", new[] { "pernas", "panturrilha" }),
                ("Treino D", "Ombros e Abdômen", new[] { "ombros", "abdômen" })
            },
            5 => new[] {
                ("Treino A", "Peito", new[] { "peito" }),
                ("Treino B", "Costas", new[] { "costas" }),
                ("Treino C", "Pernas", new[] { "pernas" }),
                ("Treino D", "Ombros", new[] { "ombros" }),
                ("Treino E", "Braços e Abdômen", new[] { "bíceps", "tríceps", "abdômen" })
            },
            6 => new[] {
                ("Treino A", "Peito e Bíceps", new[] { "peito", "bíceps" }),
                ("Treino B", "Costas e Tríceps", new[] { "costas", "tríceps" }),
                ("Treino C", "Pernas (Quadríceps)", new[] { "pernas" }),
                ("Treino D", "Ombros e Abdômen", new[] { "ombros", "abdômen" }),
                ("Treino E", "Pernas (Posteriores)", new[] { "pernas" }),
                ("Treino F", "Braços", new[] { "bíceps", "tríceps" })
            },
            _ => new[] {
                ("Treino A", "Peito e Tríceps", new[] { "peito", "tríceps" }),
                ("Treino B", "Costas e Bíceps", new[] { "costas", "bíceps" }),
                ("Treino C", "Pernas e Panturrilhas", new[] { "pernas", "panturrilha" }),
                ("Treino D", "Ombros e Abdômen", new[] { "ombros", "abdômen" })
            }
        };
        }

        Console.WriteLine($"Using {workoutSplits.Length}-day split");

        foreach (var (dayName, dayTitle, muscleGroups) in workoutSplits)
        {
            var exercisesForDay = new List<ExerciseInstruction>();
            var totalExercisesForDay = random.Next(minExercisesPerDay, maxExercisesPerDay + 1);
            var exercisesPerGroup = Math.Max(1, totalExercisesForDay / muscleGroups.Length);

            Console.WriteLine($"Day: {dayName} - {dayTitle}, Target Exercises: {totalExercisesForDay}");

            foreach (var muscleGroup in muscleGroups)
            {
                if (ExerciseDatabase.ContainsKey(muscleGroup))
                {
                    var availableExercises = ExerciseDatabase[muscleGroup];

                    // Separate compound and isolation exercises
                    var compoundExercises = availableExercises.Where(ex => ex.IsCompound).OrderBy(x => random.Next()).ToList();
                    var isolationExercises = availableExercises.Where(ex => !ex.IsCompound).OrderBy(x => random.Next()).ToList();

                    var countForThisGroup = Math.Min(
                        muscleGroup == "abdômen" || muscleGroup == "panturrilha" ?
                            random.Next(1, 3) : // Smaller muscles get 1-2 exercises
                            random.Next(2, exercisesPerGroup + 2), // Main muscles get more
                        availableExercises.Count
                    );

                    Console.WriteLine($"  Muscle: {muscleGroup}, Exercises: {countForThisGroup}");

                    // Prioritize compound exercises (at least 1, up to half of count)
                    var compoundCount = Math.Min(Math.Max(1, countForThisGroup / 2), compoundExercises.Count);
                    var isolationCount = Math.Min(countForThisGroup - compoundCount, isolationExercises.Count);

                    // Add compound exercises first
                    for (int i = 0; i < compoundCount && exercisesForDay.Count < totalExercisesForDay; i++)
                    {
                        var exercise = compoundExercises[i];
                        exercisesForDay.Add(CreateExerciseInstruction(
                            exercise.Name,
                            exercise.BodyPart,
                            exercise.Equipment,
                            exercisesForDay.Count == 0, // First exercise is main
                            level,
                            exercise.IsCompound
                        ));
                    }

                    // Add isolation exercises
                    for (int i = 0; i < isolationCount && exercisesForDay.Count < totalExercisesForDay; i++)
                    {
                        var exercise = isolationExercises[i];
                        exercisesForDay.Add(CreateExerciseInstruction(
                            exercise.Name,
                            exercise.BodyPart,
                            exercise.Equipment,
                            false,
                            level,
                            exercise.IsCompound
                        ));
                    }
                }
            }

            // If we didn't get enough exercises, fill with random ones from the muscle groups
            while (exercisesForDay.Count < minExercisesPerDay)
            {
                var randomMuscleGroup = muscleGroups[random.Next(muscleGroups.Length)];
                if (ExerciseDatabase.ContainsKey(randomMuscleGroup))
                {
                    var availableExercises = ExerciseDatabase[randomMuscleGroup];
                    var randomExercise = availableExercises[random.Next(availableExercises.Count)];

                    // Avoid duplicates
                    if (!exercisesForDay.Any(e => e.Name == randomExercise.Name))
                    {
                        exercisesForDay.Add(CreateExerciseInstruction(
                            randomExercise.Name,
                            randomExercise.BodyPart,
                            randomExercise.Equipment,
                            false,
                            level,
                            randomExercise.IsCompound
                        ));
                    }
                }
            }

            Console.WriteLine($"  Total exercises for this day: {exercisesForDay.Count}");

            days.Add(new WorkoutDay(
                DayName: dayName,
                Title: dayTitle,
                Focus: $"Desenvolvimento de {string.Join(", ", muscleGroups)}",
                Exercises: exercisesForDay
            ));
        }

        // Generate title and goal from user prompt
        var title = string.IsNullOrWhiteSpace(prompt)
            ? "Plano de Treino Personalizado"
            : $"Plano: {prompt.Substring(0, Math.Min(50, prompt.Length))}";

        var goal = string.IsNullOrWhiteSpace(prompt)
            ? "Treino personalizado"
            : prompt.Length > 100 ? prompt.Substring(0, 100) + "..." : prompt;

        Console.WriteLine($"FINAL PLAN: {days.Count} days total");
        foreach (var day in days)
        {
            Console.WriteLine($"  {day.Title}: {day.Exercises.Count} exercises ({string.Join(", ", day.Exercises.Select(e => e.Name))})");
        }
        Console.WriteLine("=== END MOCK PLAN GENERATION DEBUG ===");

        return new AIWorkoutPlanResponse(
            Title: title,
            Description: "Plano completo de treino dividido por grupos musculares para máximo ganho de massa muscular",
            WeeksCount: 4,
            DaysPerWeek: days.Count,
            Goal: goal,
            Days: days
        );
    }
}
