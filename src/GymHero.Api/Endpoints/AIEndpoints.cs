using System.Security.Claims;
using System.Text;
using System.Text.Json;
using GymHero.Api.Services;
using GymHero.Application.Common.Interfaces;
using GymHero.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Endpoints;

public record AIWorkoutRequest(
    string Prompt,
    string? FitnessLevel,
    int? Duration,
    List<string>? Equipment,
    string? WorkoutLocation,  // ✅ "gym", "home", or "both" - overrides user profile preference
    bool IncludeWarmup = false,        // ✅ Include warmup exercises (5-10 min)
    bool IncludeCooldown = false,      // ✅ Include cooldown/stretching (5-10 min)
    bool IncludeMobility = false       // ✅ Include mobility exercises
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
    string? ProgressionNotes = null,
    string? RPE = null,              // Rate of Perceived Exertion guidance
    string? Tempo = null,             // Tempo prescription (e.g., "3-0-1-0")
    string? WarmupSets = null,        // Warm-up guidance for beginners
    string? ExerciseType = null       // ✅ "warmup", "main", "mobility", "cooldown"
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
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
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
                        u.Injuries,
                        u.HealthConditions,
                        u.ExerciseGoal,
                        u.Height,
                        u.Weight,
                        u.Location,
                        u.Bio,
                        u.GymName,
                        u.PreferredWorkoutLocation,
                        u.PracticesBoxing
                    })
                    .FirstOrDefaultAsync();

                // ✅ NEW: Fetch exercises from database for AI to prioritize
                logger.LogInformation("Fetching exercises from database...");
                var workoutLocationParam = request.WorkoutLocation ?? (userProfile?.PreferredWorkoutLocation == WorkoutLocation.Home ? "home" :
                    userProfile?.PreferredWorkoutLocation == WorkoutLocation.Gym ? "gym" : null);

                var exercisesFromDb = await GetExercisesFromDatabase(
                    context,
                    workoutLocationParam,
                    null, // Will get all muscle groups
                    userProfile?.Injuries
                );
                logger.LogInformation($"Found {exercisesFromDb.Count} exercises in database suitable for this workout");

                var geminiApiKey = configuration["Gemini:ApiKey"];
                var openAiApiKey = configuration["OpenAI:ApiKey"];

                AIWorkoutResponse workout;
                var hasGemini = !string.IsNullOrEmpty(geminiApiKey);
                var hasOpenAI = !string.IsNullOrEmpty(openAiApiKey);

                if (!hasGemini && !hasOpenAI)
                {
                    logger.LogWarning("No AI API keys configured. Using enhanced mock generation.");
                    workout = GenerateMockWorkout(request.Prompt, request.FitnessLevel, userProfile?.ExerciseGoal, userProfile, request.WorkoutLocation, request.Duration);
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
                            workout = await GenerateWorkoutWithGemini(request, geminiApiKey!, userProfile, exercisesFromDb, context, cancellationToken);
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
                            workout = await GenerateWorkoutWithAI(request, openAiApiKey!, userProfile, exercisesFromDb, context, cancellationToken);
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
                        workout = GenerateMockWorkout(request.Prompt, request.FitnessLevel, userProfile?.ExerciseGoal, userProfile, request.WorkoutLocation, request.Duration);
                    }
                }

                // ✅ NEW: Auto-save new exercises to database
                logger.LogInformation("Checking for new exercises to save to database...");
                var savedCount = await AutoSaveNewExercises(context, workout, logger);
                if (savedCount > 0)
                {
                    logger.LogInformation($"✅ Saved {savedCount} new exercise(s) to database");
                }

                // ✅ SERVER-SIDE VALIDATION: Filter out gym equipment for home workouts
                // Check request location first, then fall back to profile preference
                var isHomeWorkoutRequest = !string.IsNullOrEmpty(request.WorkoutLocation)
                    ? request.WorkoutLocation.ToLower() == "home"
                    : userProfile?.PreferredWorkoutLocation == WorkoutLocation.Home;

                if (isHomeWorkoutRequest)
                {
                    // Comprehensive list of gym equipment keywords in Portuguese and English
                    var gymEquipment = new[] {
                        // Portuguese equipment
                        "halter", "halteres", "barra", "barras", "anilha", "anilhas",
                        "máquina", "maquina", "cabo", "cabos", "kettlebell",
                        "smith", "leg press", "supino", "banco", "polia", "polias",
                        "elástico", "elastico", "corda naval", "medicine ball",
                        "TRX", "aparelho", "equipamento", "peso", "pesos",
                        "esteira", "bicicleta ergométrica", "elíptico",

                        // English equipment
                        "dumbbell", "dumbbells", "barbell", "barbells", "cable", "cables",
                        "machine", "bench", "pulley", "kettlebell", "resistance band",
                        "medicine ball", "weight plate", "weights", "equipment",
                        "treadmill", "elliptical", "bike",

                        // Exercise names that require equipment
                        "rosca", "desenvolvimento", "crucifixo", "remada", "pulldown",
                        "extensão", "flexão de braço na barra", "pull-up", "chin-up",
                        "levantamento terra", "deadlift", "agachamento com", "squat with"
                    };
                    var originalCount = workout.Exercises.Count;

                    var filteredExercises = workout.Exercises.Where(e =>
                    {
                        var equipmentLower = e.Equipment?.ToLower() ?? "";
                        var nameLower = e.Name?.ToLower() ?? "";

                        // Check if exercise contains any gym equipment keywords
                        var hasGymEquipment = gymEquipment.Any(eq =>
                            equipmentLower.Contains(eq) || nameLower.Contains(eq));

                        return !hasGymEquipment; // Keep only bodyweight exercises
                    }).ToList();

                    var filteredCount = originalCount - filteredExercises.Count;
                    if (filteredCount > 0)
                    {
                        logger.LogWarning($"⚠️ FILTERED {filteredCount} gym exercises from home workout! AI not following instructions.");

                        var updatedDescription = workout.Description;
                        // If we filtered out too many exercises, add a warning to the description
                        if (filteredCount > 2)
                        {
                            updatedDescription += $"\n\n⚠️ Nota: Alguns exercícios com equipamento de academia foram removidos automaticamente pois você configurou treino em casa.";
                        }

                        // Create new workout with filtered exercises
                        workout = new AIWorkoutResponse(
                            workout.Title,
                            updatedDescription,
                            workout.Duration,
                            filteredExercises
                        );
                    }

                    // If ALL exercises were filtered out, return an error
                    if (workout.Exercises.Count == 0)
                    {
                        logger.LogError("❌ ALL exercises were gym-based for a home workout request!");
                        return Results.Json(new
                        {
                            message = "Erro: O AI gerou apenas exercícios de academia. Por favor, tente novamente ou ajuste sua preferência de local de treino.",
                            error = "NO_HOME_EXERCISES_GENERATED"
                        }, statusCode: StatusCodes.Status500InternalServerError);
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
            IExerciseMediaService mediaService,
            ILogger<Program> logger) =>
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
                logger.LogError(ex, "[ERROR {ErrorId}] Error in search-exercises", errorId);

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
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Fetch user profile data
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userProfile = await context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new {
                        u.Id,  // ✅ Added for postural assessment lookup
                        u.Name,
                        u.DateOfBirth,
                        u.Gender,
                        u.Injuries,
                        u.HealthConditions,
                        u.ExerciseGoal,
                        u.ExcludedExercises,  // ✅ Added for excluded exercises
                        u.Height,
                        u.Weight,
                        u.Location,
                        u.Bio,
                        u.GymName,
                        u.PreferredWorkoutLocation,
                        u.PracticesBoxing
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
                    plan = GenerateMockPlan(request.Prompt, request.DaysPerWeek ?? 4, request.FitnessLevel, userProfile);
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
                            plan = await GeneratePlanWithGemini(request, geminiApiKey!, userProfile, context, cancellationToken);
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
                            plan = await GeneratePlanWithAI(request, openAiApiKey!, userProfile, context, cancellationToken);
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
                        plan = GenerateMockPlan(request.Prompt, request.DaysPerWeek ?? 4, request.FitnessLevel, userProfile);
                    }
                }

                // ✅ NEW: Auto-save new exercises from all days of the plan
                logger.LogInformation("Checking for new exercises in workout plan to save to database...");
                var totalSavedInPlan = 0;
                foreach (var day in plan.Days)
                {
                    var savedInDay = await AutoSaveNewExercisesFromDay(context, day, logger);
                    totalSavedInPlan += savedInDay;
                }
                if (totalSavedInPlan > 0)
                {
                    logger.LogInformation($"✅ Saved {totalSavedInPlan} new exercise(s) from workout plan to database");
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

        // 🎯 NEW: Analyze postural photos with GPT-4 Vision
        group.MapPost("/analyze-posture", async (
            [FromForm] IFormFile frontPhoto,
            [FromForm] IFormFile sidePhoto,
            [FromForm] IFormFile backPhoto,
            ClaimsPrincipal user,
            IConfiguration configuration,
            ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Starting postural analysis with AI Vision...");

                // Validate files
                if (frontPhoto == null || sidePhoto == null || backPhoto == null)
                {
                    return Results.BadRequest(new { message = "Todas as 3 fotos são obrigatórias: frontal, lateral e costas" });
                }

                // Validate file sizes (max 5MB each)
                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (frontPhoto.Length > maxFileSize || sidePhoto.Length > maxFileSize || backPhoto.Length > maxFileSize)
                {
                    return Results.BadRequest(new { message = "Cada foto deve ter no máximo 5MB" });
                }

                // Validate file types
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
                if (!allowedTypes.Contains(frontPhoto.ContentType) ||
                    !allowedTypes.Contains(sidePhoto.ContentType) ||
                    !allowedTypes.Contains(backPhoto.ContentType))
                {
                    return Results.BadRequest(new { message = "Apenas arquivos JPEG e PNG são permitidos" });
                }

                var openAiKey = configuration["OpenAI:ApiKey"];
                if (string.IsNullOrEmpty(openAiKey))
                {
                    logger.LogError("OpenAI API key not configured");
                    return Results.Json(new { message = "OpenAI não configurado. Análise postural indisponível." },
                        statusCode: StatusCodes.Status503ServiceUnavailable);
                }

                // Convert photos to base64
                logger.LogInformation("Converting photos to base64...");
                var frontBase64 = await ConvertToBase64(frontPhoto);
                var sideBase64 = await ConvertToBase64(sidePhoto);
                var backBase64 = await ConvertToBase64(backPhoto);

                // Analyze with GPT-4 Vision
                logger.LogInformation("Calling GPT-4 Vision API...");
                var analysis = await AnalyzePostureWithVision(
                    frontBase64,
                    sideBase64,
                    backBase64,
                    openAiKey,
                    logger
                );

                logger.LogInformation("✅ Postural analysis completed successfully");
                return Results.Ok(analysis);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error analyzing posture");
                return Results.Json(new { message = $"Erro na análise: {ex.Message}" },
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("AnalyzePosture")
        .WithSummary("Analyze postural photos using AI vision")
        .RequireAuthorization("RequirePersonalRole")
        .DisableAntiforgery();
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
            ("Pullover com Halteres", "chest", "dumbbell", false),
            // Bodyweight exercises for home - EXPANDED
            ("Flexão de Braço", "chest", "body only", true),
            ("Flexão Diamante", "chest", "body only", true),
            ("Flexão Declinada", "chest", "body only", true),
            ("Flexão Inclinada", "chest", "body only", true),
            ("Flexão Archer", "chest", "body only", true),
            ("Flexão Hindu", "chest", "body only", true),
            ("Flexão com Elevação", "chest", "body only", true),
            ("Flexão Espartana", "chest", "body only", true),
            ("Flexão com Rotação", "chest", "body only", true),
            ("Flexão Explosiva", "chest", "body only", true),
            ("Flexão de Borboleta", "chest", "body only", true),
            ("Flexão com Apoio Unilateral", "chest", "body only", true),
            ("Dips entre Cadeiras", "chest", "body only", true),
            ("Flexão Spiderman", "chest", "body only", true),
            ("Flexão Typewriter", "chest", "body only", true)
        },
        ["costas"] = new()
        {
            ("Levantamento Terra", "back", "barbell", true),
            ("Puxada Frontal", "back", "cable", true),
            ("Remada Curvada com Barra", "back", "barbell", true),
            ("Remada com Halteres", "back", "dumbbell", true),
            ("Remada Baixa", "back", "cable", true),
            ("Remada Cavalinho", "back", "machine", true),
            ("Pulldown", "back", "cable", true),
            ("Remada na Máquina", "back", "machine", true),
            ("Remada Unilateral", "back", "dumbbell", true),
            ("Pullover na Polia", "back", "cable", false),
            ("Serrote", "back", "dumbbell", true),
            // Bodyweight exercises for home - EXPANDED
            ("Barra Fixa", "back", "body only", true),
            ("Barra Fixa Supinada", "back", "body only", true),
            ("Barra Fixa Neutra", "back", "body only", true),
            ("Remada Invertida", "back", "body only", true),
            ("Remada Invertida em Cadeira", "back", "body only", true),
            ("Superman", "back", "body only", false),
            ("Prancha Reversa", "back", "body only", false),
            ("Extensão Lombar", "back", "body only", false),
            ("Bird Dog", "back", "body only", false),
            ("Ponte de Glúteos", "back", "body only", true),
            ("Nadador", "back", "body only", false),
            ("Snow Angels", "back", "body only", false),
            ("Y-T-W Raises", "back", "body only", false),
            ("Cobra Stretch Dinâmico", "back", "body only", false),
            ("Aquaman", "back", "body only", false)
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
            ("Voo Posterior", "shoulders", "cable", false),
            // Bodyweight exercises for home
            ("Flexão Pike", "shoulders", "body only", true),
            ("Prancha Lateral", "shoulders", "body only", false),
            ("Handstand Push-up", "shoulders", "body only", true)
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
            ("Rosca Zottman", "biceps", "dumbbell", false),
            // Bodyweight exercises for home (limited options)
            ("Chin-up Supinado", "biceps", "body only", true),
            ("Rosca Isométrica com Toalha", "biceps", "body only", false)
        },
        ["tríceps"] = new()
        {
            ("Supino Fechado", "triceps", "barbell", true),
            ("Tríceps na Polia", "triceps", "cable", false),
            ("Tríceps Testa com Barra", "triceps", "barbell", false),
            ("Tríceps Francês", "triceps", "dumbbell", false),
            ("Tríceps Coice", "triceps", "dumbbell", false),
            ("Tríceps na Polia com Corda", "triceps", "cable", false),
            ("Tríceps Testa com Halteres", "triceps", "dumbbell", false),
            // Bodyweight exercises for home
            ("Mergulho entre Bancos", "triceps", "body only", true),
            ("Flexão Diamante", "triceps", "body only", true),
            ("Flexão Fechada", "triceps", "body only", true),
            ("Tríceps no Chão", "triceps", "body only", false)
        },
        ["pernas"] = new()
        {
            ("Agachamento Livre com Barra", "legs", "barbell", true),
            ("Levantamento Terra", "legs", "barbell", true),
            ("Leg Press 45°", "legs", "machine", true),
            ("Agachamento Sumô com Peso", "legs", "barbell", true),
            ("Agachamento Frontal", "legs", "barbell", true),
            ("Afundo com Halteres", "legs", "dumbbell", true),
            ("Afundo Caminhando", "legs", "dumbbell", true),
            ("Stiff", "legs", "barbell", true),
            ("Agachamento no Smith", "legs", "machine", true),
            ("Agachamento Búlgaro com Peso", "legs", "dumbbell", true),
            ("Cadeira Extensora", "legs", "machine", false),
            ("Mesa Flexora", "legs", "machine", false),
            ("Cadeira Abdutora", "legs", "machine", false),
            ("Cadeira Adutora", "legs", "machine", false),
            // Bodyweight exercises for home
            ("Agachamento Livre", "legs", "body only", true),
            ("Agachamento Pistol", "legs", "body only", true),
            ("Agachamento Búlgaro", "legs", "body only", true),
            ("Afundo", "legs", "body only", true),
            ("Afundo Reverso", "legs", "body only", true),
            ("Afundo Lateral", "legs", "body only", true),
            ("Agachamento Sumô", "legs", "body only", true),
            ("Step Up", "legs", "body only", true),
            ("Wall Sit", "legs", "body only", false),
            ("Salto Agachamento", "legs", "body only", true)
        },
        ["glúteos"] = new()
        {
            ("Hip Thrust com Barra", "glutes", "barbell", true),
            ("Agachamento Sumô com Peso", "glutes", "barbell", true),
            ("Stiff", "glutes", "barbell", true),
            ("Elevação Pélvica com Barra", "glutes", "barbell", true),
            ("Agachamento Búlgaro com Peso", "glutes", "dumbbell", true),
            ("Leg Press 45° com Pés Altos", "glutes", "machine", true),
            ("Cadeira Abdutora", "glutes", "machine", false),
            ("Kickback na Polia", "glutes", "cable", false),
            ("Coice no Crossover", "glutes", "cable", false),
            ("Step Up com Halteres", "glutes", "dumbbell", true),
            ("Afundo Reverso com Peso", "glutes", "dumbbell", true),
            ("Good Morning", "glutes", "barbell", true),
            ("Cadeira Flexora em Pé", "glutes", "machine", false),
            // Bodyweight exercises for home
            ("Ponte de Glúteos", "glutes", "body only", true),
            ("Ponte de Glúteos Uma Perna", "glutes", "body only", true),
            ("Agachamento Sumô", "glutes", "body only", true),
            ("Agachamento Búlgaro", "glutes", "body only", true),
            ("Afundo Reverso", "glutes", "body only", true),
            ("Step Up", "glutes", "body only", true),
            ("Coice de Glúteo", "glutes", "body only", false),
            ("Fire Hydrant", "glutes", "body only", false)
        },
        ["panturrilha"] = new()
        {
            ("Panturrilha em Pé na Máquina", "calves", "machine", false),
            ("Panturrilha Sentado", "calves", "machine", false),
            ("Panturrilha no Leg Press", "calves", "machine", false),
            ("Elevação de Panturrilha com Halteres", "calves", "dumbbell", false),
            // Bodyweight exercises for home
            ("Elevação de Panturrilha", "calves", "body only", false),
            ("Elevação de Panturrilha Unilateral", "calves", "body only", false),
            ("Salto na Ponta dos Pés", "calves", "body only", false)
        },
        ["abdômen"] = new()
        {
            ("Abdominal na Máquina", "abs", "machine", false),
            ("Abdominal na Polia", "abs", "cable", false),
            // Bodyweight exercises for home
            ("Abdominal Reto", "abs", "body only", false),
            ("Prancha", "abs", "body only", false),
            ("Prancha Lateral", "abs", "body only", false),
            ("Abdominal Infra", "abs", "body only", false),
            ("Abdominal Bicicleta", "abs", "body only", false),
            ("Elevação de Pernas", "abs", "body only", false),
            ("Abdominal Canivete", "abs", "body only", false),
            ("Mountain Climbers", "abs", "body only", false),
            ("Prancha com Toque no Ombro", "abs", "body only", false),
            ("Russian Twist", "abs", "body only", false),
            ("V-Up", "abs", "body only", false),
            ("Dead Bug", "abs", "body only", false),
            ("Hollow Body Hold", "abs", "body only", false)
        },
        ["cardio"] = new()
        {
            ("Corrida na Esteira", "cardio", "machine", true),
            ("Bicicleta Ergométrica", "cardio", "machine", true),
            ("Elíptico", "cardio", "machine", true),
            ("Remador", "cardio", "machine", true),
            ("Bike Sprint", "cardio", "machine", true),
            ("Escada Rolante", "cardio", "machine", true),
            ("Corrida com Elevação", "cardio", "machine", true),
            ("Assault Bike", "cardio", "machine", true),
            // Bodyweight exercises for home
            ("Corrida ao Ar Livre", "cardio", "body only", true),
            ("Pular Corda", "cardio", "body only", true),
            ("Burpee", "cardio", "body only", true),
            ("Polichinelo", "cardio", "body only", true),
            ("High Knees", "cardio", "body only", true),
            ("Caminhada Rápida", "cardio", "body only", true),
            ("Box Jump", "cardio", "body only", true),
            ("Sprint Intervalado", "cardio", "body only", true),
            ("Step (Subir e Descer)", "cardio", "body only", true),
            ("Corrida Estacionária", "cardio", "body only", true),
            ("Skaters", "cardio", "body only", true),
            ("Bear Crawl", "cardio", "body only", true),
            ("Crab Walk", "cardio", "body only", true),
            ("Inchworms", "cardio", "body only", true)
        },
        // ✅ NEW: Functional Calisthenics - Advanced bodyweight movements
        ["calistenia"] = new()
        {
            // Pull Progressions
            ("Australian Pull-up", "back", "body only", true),
            ("Archer Pull-up", "back", "body only", true),
            ("Typewriter Pull-up", "back", "body only", true),
            ("L-Sit Pull-up", "back", "body only", true),
            ("Muscle-up", "back", "body only", true),
            // Push Progressions
            ("Pseudo Planche Push-up", "chest", "body only", true),
            ("Pike Push-up Elevado", "shoulders", "body only", true),
            ("Ring Push-up", "chest", "body only", true),
            ("Korean Dips", "triceps", "body only", true),
            // Core & Skills
            ("L-Sit", "abs", "body only", false),
            ("Dragon Flag", "abs", "body only", false),
            ("Human Flag Progressão", "abs", "body only", false),
            ("Tuck Front Lever", "back", "body only", true),
            ("Tuck Back Lever", "back", "body only", true),
            ("Handstand Hold", "shoulders", "body only", false),
            ("Crow Pose", "shoulders", "body only", false),
            // Leg Progressions
            ("Pistol Squat Assistido", "legs", "body only", true),
            ("Shrimp Squat", "legs", "body only", true),
            ("Nordic Curl Assistido", "legs", "body only", true),
            ("Sissy Squat", "legs", "body only", true),
            // Dynamic Movements
            ("Explosive Pull-up", "back", "body only", true),
            ("Clapping Push-up", "chest", "body only", true),
            ("Jump Squat", "legs", "body only", true),
            ("Tuck Planche Hold", "shoulders", "body only", false)
        },
        // ✅ NEW: Boxing & Combat Training
        ["boxe"] = new()
        {
            // Shadowboxing & Technique
            ("Shadowboxing", "cardio", "body only", true),
            ("Soco Jab", "shoulders", "body only", false),
            ("Soco Direto (Cross)", "chest", "body only", false),
            ("Gancho (Hook)", "shoulders", "body only", false),
            ("Uppercut", "shoulders", "body only", false),
            ("Combinações de Socos", "cardio", "body only", true),
            // Footwork & Movement
            ("Footwork Básico", "legs", "body only", true),
            ("Esquivas Laterais (Slip)", "abs", "body only", true),
            ("Giro de Cintura (Roll)", "abs", "body only", true),
            ("Pivot e Movimentação", "legs", "body only", true),
            // Conditioning
            ("Soco no Ar (Air Punching)", "shoulders", "body only", false),
            ("Burpee com Soco", "cardio", "body only", true),
            ("Mountain Climber para Boxeador", "cardio", "body only", true),
            ("Abdominais de Boxeador", "abs", "body only", false),
            ("Prancha com Socos Alternados", "abs", "body only", false),
            // Power & Explosiveness
            ("Medicine Ball Slam", "abs", "body only", true),
            ("Rotação de Tronco Explosiva", "abs", "body only", true),
            ("Sprint com Mudança de Direção", "legs", "body only", true)
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

    // Helper method: Determine exercise difficulty level
    // Beginner: Machines, simple bodyweight, easier movements
    // Intermediate: Dumbbells, cables, moderate complexity
    // Advanced: Barbells, complex movements, unilateral, high skill
    private static string GetExerciseDifficulty(string exerciseName, string equipment, bool isCompound)
    {
        var nameLower = exerciseName.ToLower();

        // Advanced exercises (complex, high skill, or dangerous if done wrong)
        if (nameLower.Contains("levantamento terra") || nameLower.Contains("deadlift") ||
            nameLower.Contains("agachamento livre com barra") ||
            nameLower.Contains("power clean") || nameLower.Contains("clean") ||
            nameLower.Contains("snatch") || nameLower.Contains("arranco") ||
            nameLower.Contains("pistol") || nameLower.Contains("handstand") ||
            nameLower.Contains("muscle-up") || nameLower.Contains("muscle up") ||
            nameLower.Contains("dragon flag") || nameLower.Contains("front lever") ||
            nameLower.Contains("back lever") || nameLower.Contains("planche") ||
            nameLower.Contains("human flag") || nameLower.Contains("good morning") ||
            nameLower.Contains("archer") || nameLower.Contains("typewriter") ||
            nameLower.Contains("pseudo planche") || nameLower.Contains("l-sit pull") ||
            nameLower.Contains("korean dips") || nameLower.Contains("nordic curl") ||
            nameLower.Contains("shrimp squat") || nameLower.Contains("sissy squat") ||
            nameLower.Contains("explosiv") || nameLower.Contains("clapping") ||
            nameLower.Contains("unilateral"))
        {
            return "advanced";
        }

        // Beginner-friendly (machines, simple movements)
        if (equipment == "machine" ||
            nameLower.Contains("flexão de braço") || nameLower.Contains("flexão inclinada") ||
            nameLower.Contains("wall sit") || nameLower.Contains("prancha") ||
            nameLower.Contains("elevação de panturrilha") ||
            nameLower.Contains("abdominal reto") || nameLower.Contains("ponte"))
        {
            return "beginner";
        }

        // Equipment-based difficulty
        if (equipment == "barbell" && isCompound) return "advanced";
        if (equipment == "barbell" && !isCompound) return "intermediate";
        if (equipment == "dumbbell" && isCompound) return "intermediate";
        if (equipment == "cable") return "intermediate";

        // Bodyweight compound exercises
        if (equipment == "body only" && isCompound)
        {
            if (nameLower.Contains("barra fixa") || nameLower.Contains("pull-up") ||
                nameLower.Contains("mergulho") || nameLower.Contains("dips"))
                return "intermediate";
            return "beginner";
        }

        return "beginner"; // Default for isolation and simple movements
    }

    // Helper method: Get movement pattern for balance tracking
    private static string GetMovementPattern(string exerciseName, string bodyPart)
    {
        var nameLower = exerciseName.ToLower();

        // Horizontal Push
        if (nameLower.Contains("supino") || nameLower.Contains("bench press") ||
            nameLower.Contains("flexão") || nameLower.Contains("push-up"))
            return "horizontal_push";

        // Vertical Push
        if (nameLower.Contains("desenvolvimento") || nameLower.Contains("press") && bodyPart == "shoulders" ||
            nameLower.Contains("pike"))
            return "vertical_push";

        // Horizontal Pull
        if (nameLower.Contains("remada") || nameLower.Contains("row"))
            return "horizontal_pull";

        // Vertical Pull
        if (nameLower.Contains("barra fixa") || nameLower.Contains("pull-up") ||
            nameLower.Contains("puxada") || nameLower.Contains("pulldown"))
            return "vertical_pull";

        // Hip Hinge
        if (nameLower.Contains("levantamento terra") || nameLower.Contains("deadlift") ||
            nameLower.Contains("stiff") || nameLower.Contains("good morning") ||
            nameLower.Contains("hip thrust") || nameLower.Contains("ponte"))
            return "hip_hinge";

        // Knee Dominant
        if (nameLower.Contains("agachamento") || nameLower.Contains("squat") ||
            nameLower.Contains("leg press") || nameLower.Contains("afundo") || nameLower.Contains("lunge") ||
            nameLower.Contains("extensora"))
            return "knee_dominant";

        // Isolation patterns
        if (bodyPart == "biceps" || bodyPart == "triceps") return "arm_isolation";
        if (bodyPart == "shoulders" && (nameLower.Contains("elevação") || nameLower.Contains("lateral")))
            return "shoulder_isolation";

        return "other";
    }

    // Helper method: Filter exercises by fitness level
    private static List<(string Name, string BodyPart, string Equipment, bool IsCompound)> FilterExercisesByLevel(
        List<(string Name, string BodyPart, string Equipment, bool IsCompound)> exercises,
        string fitnessLevel)
    {
        var level = fitnessLevel?.ToLower() ?? "intermediário";

        return exercises.Where(ex =>
        {
            var difficulty = GetExerciseDifficulty(ex.Name, ex.Equipment, ex.IsCompound);

            return level switch
            {
                "iniciante" or "beginner" => difficulty == "beginner" || difficulty == "intermediate",
                "intermediário" or "intermediate" => true, // Can do all exercises
                "avançado" or "advanced" => true, // Can do all exercises
                _ => true
            };
        }).ToList();
    }

    // Helper method: Order exercises optimally (compound first, isolation last, abs/cardio at end)
    private static List<ExerciseInstruction> OrderExercisesOptimally(List<ExerciseInstruction> exercises)
    {
        return exercises.OrderBy(ex =>
        {
            var name = ex.Name.ToLower();

            // Priority 1: Olympic lifts (advanced only) - highest priority
            if (name.Contains("power clean") || name.Contains("clean and jerk") ||
                name.Contains("snatch") || name.Contains("arranco"))
                return 1;

            // Priority 2: Heavy compound barbell lifts
            if (ex.Equipment == "barbell" && ex.BodyPart != "cardio" && ex.BodyPart != "abs" &&
                (name.Contains("agachamento livre com barra") || name.Contains("levantamento terra") ||
                 name.Contains("supino reto com barra") || name.Contains("deadlift")))
                return 2;

            // Priority 3: Other compound exercises
            if ((ex.Equipment == "barbell" || ex.Equipment == "dumbbell") &&
                ex.BodyPart != "cardio" && ex.BodyPart != "abs" &&
                (name.Contains("agachamento") || name.Contains("remada") || name.Contains("supino") ||
                 name.Contains("desenvolvimento") || name.Contains("leg press") || name.Contains("barra fixa")))
                return 3;

            // Priority 4: Compound bodyweight & machines
            if (ex.BodyPart != "cardio" && ex.BodyPart != "abs" &&
                (name.Contains("flexão") || name.Contains("mergulho") ||
                 ex.Equipment == "machine" && name.Contains("press")))
                return 4;

            // Priority 5: Isolation exercises
            if (ex.BodyPart != "cardio" && ex.BodyPart != "abs")
                return 5;

            // Priority 6: Core/abs exercises
            if (ex.BodyPart == "abs")
                return 6;

            // Priority 7: Cardio at the end
            if (ex.BodyPart == "cardio")
                return 7;

            return 5; // Default to isolation priority
        }).ToList();
    }

    // Helper method: Validate volume landmarks (total sets per muscle per week)
    private static void ValidateVolumeLandmarks(List<WorkoutDay> days, string fitnessLevel)
    {

        // Map body parts to muscle groups
        var muscleSets = new Dictionary<string, int>();

        foreach (var day in days)
        {
            foreach (var exercise in day.Exercises)
            {
                var muscle = exercise.BodyPart.ToLower();
                if (muscle != "cardio") // Don't count cardio
                {
                    muscleSets[muscle] = muscleSets.GetValueOrDefault(muscle) + exercise.Sets;
                }
            }
        }

        // Define volume guidelines per level
        var (minLarge, maxLarge, minMedium, maxMedium, minSmall, maxSmall) = fitnessLevel?.ToLower() switch
        {
            "iniciante" or "beginner" => (8, 12, 6, 10, 4, 8),
            "avançado" or "advanced" => (16, 24, 12, 18, 10, 16),
            _ => (12, 18, 10, 14, 8, 12) // Intermediate
        };

        // Categorize muscles
        var largeMuscles = new[] { "chest", "back", "legs", "glutes" };
        var mediumMuscles = new[] { "shoulders" };
        var smallMuscles = new[] { "biceps", "triceps", "calves", "abs" };

        foreach (var (muscle, sets) in muscleSets)
        {
            var (min, max) = muscle switch
            {
                var m when largeMuscles.Contains(m) => (minLarge, maxLarge),
                var m when mediumMuscles.Contains(m) => (minMedium, maxMedium),
                var m when smallMuscles.Contains(m) => (minSmall, maxSmall),
                _ => (minSmall, maxSmall)
            };

            var status = sets < min ? "⚠️ TOO LOW" : sets > max ? "⚠️ TOO HIGH" : "✅ OK";
        }
    }

    // Helper method: Check recovery time between muscle groups
    private static void ValidateRecoveryTime(List<WorkoutDay> days)
    {
        for (int i = 0; i < days.Count - 1; i++)
        {
            var today = days[i];
            var tomorrow = days[i + 1];

            var todayMuscles = today.Exercises.Select(e => e.BodyPart.ToLower()).Distinct().ToList();
            var tomorrowMuscles = tomorrow.Exercises.Select(e => e.BodyPart.ToLower()).Distinct().ToList();

            var overlap = todayMuscles.Intersect(tomorrowMuscles).Where(m => m != "abs" && m != "cardio").ToList();
        }
    }

    // Helper method: Check movement pattern balance (push/pull ratio)
    private static void ValidateMovementPatternBalance(List<WorkoutDay> days)
    {
        var patternCounts = new Dictionary<string, int>();

        foreach (var day in days)
        {
            foreach (var exercise in day.Exercises)
            {
                var pattern = GetMovementPattern(exercise.Name, exercise.BodyPart);
                if (pattern != "other" && pattern != "arm_isolation" && pattern != "shoulder_isolation")
                {
                    patternCounts[pattern] = patternCounts.GetValueOrDefault(pattern) + 1;
                }
            }
        }

        // Check ratios
        var horizontalPush = patternCounts.GetValueOrDefault("horizontal_push");
        var horizontalPull = patternCounts.GetValueOrDefault("horizontal_pull");
        var verticalPush = patternCounts.GetValueOrDefault("vertical_push");
        var verticalPull = patternCounts.GetValueOrDefault("vertical_pull");
        var hipHinge = patternCounts.GetValueOrDefault("hip_hinge");
        var kneeDominant = patternCounts.GetValueOrDefault("knee_dominant");

        if (horizontalPush > 0 && horizontalPull > 0)
        {
            var ratio = (double)horizontalPush / horizontalPull;
        }

        if (verticalPush > 0 && verticalPull > 0)
        {
            var ratio = (double)verticalPush / verticalPull;
        }

        if (hipHinge > 0 && kneeDominant > 0)
        {
            var ratio = (double)hipHinge / kneeDominant;
        }
    }

    private static AIWorkoutResponse GenerateMockWorkout(string prompt, string? fitnessLevel = null, string? exerciseGoal = null, dynamic? userProfile = null, string? workoutLocation = null, int? requestedDuration = null)
    {
        // ✅ Check workout location: prioritize request parameter, then fall back to user profile preference
        var isHomeWorkout = !string.IsNullOrEmpty(workoutLocation)
            ? workoutLocation.ToLower() == "home"
            : userProfile?.PreferredWorkoutLocation == WorkoutLocation.Home;

        var random = new Random();
        var parsedPrompt = ParsePrompt(prompt.ToLower());
        var level = fitnessLevel?.ToLower() ?? "intermediário";

        // Check if user's goal mentions abs/core/six-pack
        var absRelatedGoals = new[] { "six-pack", "six pack", "tanquinho", "definir abdômen", "abdômen definido", "abdominal", "abs", "core", "perder barriga" };
        var goalMentionsAbs = !string.IsNullOrEmpty(exerciseGoal) && absRelatedGoals.Any(keyword => exerciseGoal.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        if (goalMentionsAbs && !parsedPrompt.MuscleGroups.Contains("abdômen"))
        {
            parsedPrompt.MuscleGroups.Add("abdômen");
        }

        // Determine exercise count based on fitness level
        // Beginners need fewer exercises to focus on form, advanced can handle more volume
        var (minExercises, maxExercises) = level switch
        {
            "iniciante" or "beginner" => (4, 6),    // Fewer exercises, focus on basics
            "avançado" or "advanced" => (8, 12),    // More exercises, higher volume
            _ => (6, 8)                              // Intermediate: moderate volume
        };

        // Select exercises based on parsed requirements
        var selectedExercises = new List<ExerciseInstruction>();

        // If specific muscle groups were requested, use those
        if (parsedPrompt.MuscleGroups.Any())
        {
            var totalExercises = random.Next(minExercises, maxExercises + 1);
            var exercisesPerGroup = Math.Max(2, totalExercises / parsedPrompt.MuscleGroups.Count);

            foreach (var muscleGroup in parsedPrompt.MuscleGroups)
            {
                if (ExerciseDatabase.ContainsKey(muscleGroup))
                {
                    var availableExercises = ExerciseDatabase[muscleGroup]
                        .Where(ex => !IsRestricted(ex.Name, parsedPrompt.Restrictions))
                        .Where(ex => !isHomeWorkout || ex.Equipment == "body only") // Filter for home workouts
                        .ToList();

                    // ✅ NEW: Filter by fitness level (beginner-friendly vs advanced exercises)
                    availableExercises = FilterExercisesByLevel(availableExercises, level);

                    // ✅ NEW: Further differentiate GYM workouts by equipment preference based on level
                    if (!isHomeWorkout)
                    {
                        if (level == "iniciante" || level == "beginner")
                        {
                            // Beginners: HEAVILY prioritize machines, then dumbbells, avoid barbells
                            availableExercises = availableExercises
                                .OrderBy(ex => ex.Equipment == "machine" ? 0 :
                                              ex.Equipment == "dumbbell" ? 1 :
                                              ex.Equipment == "cable" ? 2 : 3)
                                .ThenBy(x => random.Next())
                                .ToList();
                        }
                        else if (level == "avançado" || level == "advanced")
                        {
                            // Advanced: HEAVILY prioritize free weights (barbells, dumbbells), then machines
                            availableExercises = availableExercises
                                .OrderBy(ex => ex.Equipment == "barbell" ? 0 :
                                              ex.Equipment == "dumbbell" ? 1 :
                                              ex.Equipment == "cable" ? 2 : 3)
                                .ThenBy(x => random.Next())
                                .ToList();
                        }
                    }

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
            // Intelligent default based on common workout splits
            var workoutType = random.Next(0, 5);
            var muscleGroupsToUse = workoutType switch
            {
                0 => new[] { "peito", "tríceps" },           // Push
                1 => new[] { "costas", "bíceps" },           // Pull
                2 => new[] { "pernas", "panturrilha" },      // Legs
                3 => new[] { "ombros", "abdômen" },          // Shoulders & Core
                _ => new[] { "peito", "costas", "ombros" }   // Upper Body
            };

            var totalExercises = random.Next(minExercises, maxExercises + 1);
            var exercisesPerGroup = Math.Max(2, totalExercises / muscleGroupsToUse.Length);

            foreach (var muscleGroup in muscleGroupsToUse)
            {
                if (ExerciseDatabase.ContainsKey(muscleGroup))
                {
                    var availableExercises = ExerciseDatabase[muscleGroup]
                        .Where(ex => !IsRestricted(ex.Name, parsedPrompt.Restrictions))
                        .Where(ex => !isHomeWorkout || ex.Equipment == "body only") // Filter for home workouts
                        .ToList();

                    // ✅ NEW: Filter by fitness level (beginner-friendly vs advanced exercises)
                    availableExercises = FilterExercisesByLevel(availableExercises, level);

                    // ✅ NEW: Further differentiate GYM workouts by equipment preference based on level
                    if (!isHomeWorkout)
                    {
                        if (level == "iniciante" || level == "beginner")
                        {
                            // Beginners: HEAVILY prioritize machines, then dumbbells, avoid barbells
                            availableExercises = availableExercises
                                .OrderBy(ex => ex.Equipment == "machine" ? 0 :
                                              ex.Equipment == "dumbbell" ? 1 :
                                              ex.Equipment == "cable" ? 2 : 3)
                                .ThenBy(x => random.Next())
                                .ToList();
                        }
                        else if (level == "avançado" || level == "advanced")
                        {
                            // Advanced: HEAVILY prioritize free weights (barbells, dumbbells), then machines
                            availableExercises = availableExercises
                                .OrderBy(ex => ex.Equipment == "barbell" ? 0 :
                                              ex.Equipment == "dumbbell" ? 1 :
                                              ex.Equipment == "cable" ? 2 : 3)
                                .ThenBy(x => random.Next())
                                .ToList();
                        }
                    }

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

        // Enforce maximum exercise count (5-7 exercises per workout)
        // This prevents workouts with too many muscle groups from having excessive exercises
        const int MAX_EXERCISES = 7;
        if (selectedExercises.Count > MAX_EXERCISES)
        {
            // The exercises are already ordered with compound exercises first,
            // so we can simply take the first MAX_EXERCISES
            selectedExercises = selectedExercises.Take(MAX_EXERCISES).ToList();
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

        // Add finishers: abs exercises (always) and cardio (occasionally)
        // Abs should be added to most workouts
        if (!parsedPrompt.MuscleGroups.Contains("abdômen") && ExerciseDatabase.ContainsKey("abdômen"))
        {
            var absExercises = ExerciseDatabase["abdômen"]
                .Where(ex => !IsRestricted(ex.Name, parsedPrompt.Restrictions))
                .Where(ex => !isHomeWorkout || ex.Equipment == "body only") // Filter for home workouts
                .OrderBy(x => random.Next())
                .Take(random.Next(1, 3)) // 1-2 abs exercises
                .ToList();

            foreach (var absEx in absExercises)
            {
                selectedExercises.Add(CreateExerciseInstruction(
                    absEx.Name,
                    absEx.BodyPart,
                    absEx.Equipment,
                    false,
                    level,
                    absEx.IsCompound
                ));
            }
        }

        // Add cardio occasionally (40% chance, or if user goal involves cardio)
        var shouldAddCardio = random.Next(0, 10) < 4 ||
                             (exerciseGoal?.Contains("emagrecer", StringComparison.OrdinalIgnoreCase) ?? false) ||
                             (exerciseGoal?.Contains("perder peso", StringComparison.OrdinalIgnoreCase) ?? false) ||
                             (exerciseGoal?.Contains("definir", StringComparison.OrdinalIgnoreCase) ?? false);

        // Check if user explicitly restricted cardio - check both parsed restrictions AND original prompt
        var hasCardioRestriction = parsedPrompt.Restrictions.Any(r =>
            r.Contains("cardio", StringComparison.OrdinalIgnoreCase) ||
            r.Contains("cardiovascular", StringComparison.OrdinalIgnoreCase) ||
            r.Contains("aeróbico", StringComparison.OrdinalIgnoreCase) ||
            r.Contains("aerobico", StringComparison.OrdinalIgnoreCase) ||
            r.Contains("corrida", StringComparison.OrdinalIgnoreCase) ||
            r.Contains("esteira", StringComparison.OrdinalIgnoreCase));

        // ADDITIONAL CHECK: Also check the original prompt directly for common "no cardio" patterns
        if (!hasCardioRestriction)
        {
            var lowerPrompt = prompt.ToLower();
            var noCardioPatterns = new[] {
                "sem cardio", "sem o cardio", "no cardio", "não quero cardio",
                "evitar cardio", "excluir cardio", "nada de cardio", "nao quero cardio",
                "without cardio", "don't want cardio", "dont want cardio"
            };
            hasCardioRestriction = noCardioPatterns.Any(pattern => lowerPrompt.Contains(pattern));
        }

        if (shouldAddCardio && !parsedPrompt.MuscleGroups.Contains("cardio") && !hasCardioRestriction && ExerciseDatabase.ContainsKey("cardio"))
        {
            // Filter out simulated exercises like "Natação (Simulada)"
            var cardioExercises = ExerciseDatabase["cardio"]
                .Where(ex => !IsRestricted(ex.Name, parsedPrompt.Restrictions))
                .Where(ex => !isHomeWorkout || ex.Equipment == "body only") // Filter for home workouts
                .Where(ex => !ex.Name.Contains("Simulada", StringComparison.OrdinalIgnoreCase) &&
                            !ex.Name.Contains("Simulated", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => random.Next())
                .Take(1)
                .ToList();

            foreach (var cardioEx in cardioExercises)
            {
                selectedExercises.Add(CreateExerciseInstruction(
                    cardioEx.Name,
                    cardioEx.BodyPart,
                    cardioEx.Equipment,
                    false,
                    level,
                    cardioEx.IsCompound
                ));
            }
        }

        // ✅ NEW: Add boxing exercises if user practices boxing
        var practicesBoxing = userProfile?.PracticesBoxing ?? false;
        if (practicesBoxing && ExerciseDatabase.ContainsKey("boxe"))
        {
            var boxingExercises = ExerciseDatabase["boxe"]
                .Where(ex => !IsRestricted(ex.Name, parsedPrompt.Restrictions))
                .OrderBy(x => random.Next())
                .Take(random.Next(2, 4)) // 2-3 boxing exercises
                .ToList();

            foreach (var boxingEx in boxingExercises)
            {
                selectedExercises.Add(CreateExerciseInstruction(
                    boxingEx.Name,
                    boxingEx.BodyPart,
                    boxingEx.Equipment,
                    false,
                    level,
                    boxingEx.IsCompound
                ));
            }
        }

        // ✅ NEW: Apply strict exercise ordering (compound first, isolation last, abs/cardio/boxing at end)
        selectedExercises = OrderExercisesOptimally(selectedExercises);

        // Generate title based on muscle groups or workout type
        var title = parsedPrompt.MuscleGroups.Any()
            ? GenerateWorkoutTitle(parsedPrompt.MuscleGroups)
            : $"Treino {(level == "avançado" ? "Avançado" : level == "iniciante" ? "Iniciante" : "Intermediário")} Completo";

        var description = parsedPrompt.MuscleGroups.Any()
            ? GenerateWorkoutDescription(parsedPrompt.MuscleGroups, selectedExercises.Count)
            : $"Treino completo com {selectedExercises.Count} exercícios variados para desenvolvimento muscular equilibrado. Nível: {level}.";

        // Use requested duration if provided, otherwise calculate based on level
        var duration = requestedDuration ?? level switch
        {
            "iniciante" or "beginner" => random.Next(30, 45),   // Shorter workouts, more rest
            "avançado" or "advanced" => random.Next(70, 90),    // Longer workouts, more volume
            _ => random.Next(50, 65)                             // Intermediate duration
        };

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

        // PRIORITY: Detect compound phrases first (lower body, upper body, etc.)
        // These override individual muscle detection
        var lowerBodyPhrases = new[] { "lower body", "lower-body", "lowerbody", "membros inferiores", "inferior", "lower limb", "lower member" };
        var upperBodyPhrases = new[] { "upper body", "upper-body", "upperbody", "membros superiores", "superior", "upper limb", "upper member" };

        var isLowerBodyFocus = lowerBodyPhrases.Any(phrase => prompt.Contains(phrase, StringComparison.OrdinalIgnoreCase));
        var isUpperBodyFocus = upperBodyPhrases.Any(phrase => prompt.Contains(phrase, StringComparison.OrdinalIgnoreCase));

        if (isLowerBodyFocus)
        {
            // Add all lower body muscle groups
            muscleGroups.Add("glúteos");
            muscleGroups.Add("pernas");
            muscleGroups.Add("panturrilha");
        }
        else if (isUpperBodyFocus)
        {
            // Add all upper body muscle groups
            muscleGroups.Add("peito");
            muscleGroups.Add("costas");
            muscleGroups.Add("ombros");
            muscleGroups.Add("bíceps");
            muscleGroups.Add("tríceps");
        }
        else
        {
            // Only check individual keywords if no compound phrase was detected
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
                ["core"] = "abdômen",
                ["cardio"] = "cardio",
                ["cardiovascular"] = "cardio",
                ["aeróbico"] = "cardio",
                ["aerobico"] = "cardio",
                ["corrida"] = "cardio",
                ["running"] = "cardio",
                ["bike"] = "cardio",
                ["bicicleta"] = "cardio",
                ["esteira"] = "cardio",
                ["treadmill"] = "cardio"
            };

            foreach (var (keyword, muscleGroup) in muscleKeywords)
            {
                if (prompt.Contains(keyword, StringComparison.OrdinalIgnoreCase) && !muscleGroups.Contains(muscleGroup))
                {
                    muscleGroups.Add(muscleGroup);
                }
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
        var progressionNotes = bodyPart == "cardio"
            ? fitnessLevel.ToLower() switch
            {
                "iniciante" or "beginner" => "Semana 1-2: 15 min ritmo leve | Semana 3-4: 20 min ritmo moderado | Foco na consistência",
                "avançado" or "advanced" => "Semana 1: 30 min moderado | Semana 2: 35 min com intervalos | Semana 3: 40 min | Semana 4: 30 min (recuperação)",
                _ => "Semana 1: 20 min ritmo moderado | Semana 2: 25 min | Semana 3: 30 min com intervalos | Semana 4: 20 min (recuperação)"
            }
            : (fitnessLevel.ToLower(), isCompound) switch
            {
                // BEGINNER: Focus on form, gradual increases, lighter loads
                ("iniciante" or "beginner", true) => "INICIANTE: Semana 1-2: 3x12-15 (leve) | Semana 3-4: 3x15 (mesma carga) | Priorize TÉCNICA sobre carga",
                ("iniciante" or "beginner", false) => "INICIANTE: Semana 1-2: 2x15-20 | Semana 3-4: 2x20 | Foco em sentir o músculo trabalhando",

                // ADVANCED: Heavy loads, progressive overload, periodization with deload
                ("avançado" or "advanced", true) => "AVANÇADO: Sem 1: 5x5-8 (80-85% 1RM) | Sem 2: 5x6 (85% 1RM) | Sem 3: 5x5 (87-90% 1RM) | Sem 4: 3x8 (deload 70%)",
                ("avançado" or "advanced", false) => "AVANÇADO: Sem 1: 4x10-12 | Sem 2: 4x12 (↑carga) | Sem 3: 5x10 (↑volume) | Sem 4: 3x12 (deload)",

                // INTERMEDIATE: Balance between volume and intensity
                (_, true) => "INTERMEDIÁRIO: Sem 1: 4x8-10 | Sem 2: 4x10 (mesma carga) | Sem 3: 4x8 (↑carga 5%) | Sem 4: 3x10 (deload)",
                _ => "INTERMEDIÁRIO: Sem 1: 3x12-15 | Sem 2: 3x15 | Sem 3: 4x12 (↑volume) | Sem 4: 3x15"
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
            ["Flexão de Braço"] = new() { "Posicione as mãos no chão, afastadas na largura dos ombros", "Mantenha o corpo reto dos pés à cabeça", "Desça o corpo flexionando os cotovelos", "Empurre de volta à posição inicial" },
            ["Corrida na Esteira"] = new() { "Ajuste a velocidade e inclinação conforme seu nível", "Mantenha a postura ereta e olhar à frente", "Pise com a parte média do pé", "Mantenha os braços relaxados e balançando naturalmente" },
            ["Corrida ao Ar Livre"] = new() { "Escolha um ritmo sustentável", "Mantenha a postura ereta durante toda a corrida", "Respire de forma rítmica e controlada", "Aumente a intensidade progressivamente" },
            ["Bicicleta Ergométrica"] = new() { "Ajuste o selim na altura do quadril", "Mantenha as costas retas e core contraído", "Pedale com cadência constante", "Ajuste a resistência conforme necessário" },
            ["Elíptico"] = new() { "Posicione os pés firmemente nas plataformas", "Segure as barras móveis para trabalho de braços", "Mantenha o movimento fluido e contínuo", "Varie a resistência e inclinação" },
            ["Remador"] = new() { "Prenda os pés nas alças", "Puxe o cabo até o abdômen mantendo as costas retas", "Estenda as pernas primeiro, depois puxe com os braços", "Retorne controladamente à posição inicial" },
            ["Pular Corda"] = new() { "Segure a corda com as mãos na altura do quadril", "Pule com as pontas dos pés", "Mantenha os cotovelos próximos ao corpo", "Gire a corda usando os pulsos" },
            ["Burpee"] = new() { "Comece em pé, depois agache e apoie as mãos no chão", "Jogue as pernas para trás em posição de flexão", "Faça uma flexão de braço", "Pule de volta e salte com os braços para cima" },
            ["Polichinelo"] = new() { "Fique em pé com os pés juntos e braços ao lado", "Salte abrindo as pernas e elevando os braços acima da cabeça", "Retorne à posição inicial saltando", "Mantenha o ritmo constante" },
            ["High Knees"] = new() { "Corra no lugar elevando os joelhos até a altura do quadril", "Alterne as pernas rapidamente", "Balance os braços acompanhando o movimento", "Mantenha o core contraído" },
            ["Bike Sprint"] = new() { "Ajuste a resistência da bike para sprints", "Pedale na máxima velocidade por intervalos curtos", "Mantenha o core estável", "Alterne entre sprints e recuperação ativa" },
            ["Escada Rolante"] = new() { "Suba os degraus com postura ereta", "Use os corrimãos apenas para equilíbrio", "Pise com o pé inteiro em cada degrau", "Mantenha um ritmo consistente" },
            ["Caminhada Rápida"] = new() { "Caminhe em ritmo acelerado", "Balance os braços naturalmente", "Mantenha os passos longos e firmes", "Respire profundamente" },
            ["Sprint Intervalado"] = new() { "Aqueça por 5 minutos", "Corra na máxima velocidade por 20-30 segundos", "Recupere caminhando ou trotando por 60-90 segundos", "Repita o ciclo conforme planejado" },
            ["Box Jump"] = new() { "Posicione-se na frente de uma caixa estável", "Agache ligeiramente e salte explosivamente", "Aterrisse suavemente com ambos os pés na caixa", "Desça controladamente e repita" }
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
            ["Abdominal na Polia"] = "https://www.youtube.com/watch?v=LqH5tyDWpik",
            ["Corrida na Esteira"] = "https://www.youtube.com/watch?v=wCVSv7UxB2E",
            ["Corrida ao Ar Livre"] = "https://www.youtube.com/watch?v=brFHyOtTwH4",
            ["Bicicleta Ergométrica"] = "https://www.youtube.com/watch?v=8-d1W8U_6kI",
            ["Elíptico"] = "https://www.youtube.com/watch?v=4x5tP-LhfiY",
            ["Remador"] = "https://www.youtube.com/watch?v=GiAVqkCT0RA",
            ["Pular Corda"] = "https://www.youtube.com/watch?v=1BZM2Vre5oc",
            ["Burpee"] = "https://www.youtube.com/watch?v=TU8QYVW0gDU",
            ["Polichinelo"] = "https://www.youtube.com/watch?v=iSSAk4XCsRA",
            ["High Knees"] = "https://www.youtube.com/watch?v=8opcQdC-V-U",
            ["Bike Sprint"] = "https://www.youtube.com/watch?v=8-d1W8U_6kI",
            ["Escada Rolante"] = "https://www.youtube.com/watch?v=mRx-3LQbfvk",
            ["Caminhada Rápida"] = "https://www.youtube.com/watch?v=gXvlz0JgBZ0",
            ["Sprint Intervalado"] = "https://www.youtube.com/watch?v=M5gC6B5WXX8",
            ["Box Jump"] = "https://www.youtube.com/watch?v=NBY9-kTuHEk"
        };

        var instructions = instructionsMap.ContainsKey(name)
            ? instructionsMap[name]
            : new List<string> { "Execute o movimento com técnica correta", "Mantenha o controle durante toda a amplitude", "Respire adequadamente", "Foque na contração muscular" };

        var videoUrl = videoMap.ContainsKey(name) ? videoMap[name] : null;

        // Adapt sets, reps, and rest based on fitness level and exercise type
        // Use isCompound to differentiate ALL compound exercises, not just the first one
        var (sets, reps, rest) = bodyPart == "cardio"
            ? fitnessLevel.ToLower() switch
            {
                "iniciante" or "beginner" => (1, "15-20 min", "60s"),
                "avançado" or "advanced" => (1, "30-40 min", "60s"),
                _ => (1, "20-30 min", "60s")  // Intermediate
            }
            : (fitnessLevel.ToLower(), isCompound) switch
            {
                // BEGINNER: Lower volume, higher reps, more rest
                ("iniciante" or "beginner", true) => (3, "12-15", "120s"),      // Compound: 3x12-15
                ("iniciante" or "beginner", false) => (2, "15-20", "90s"),      // Isolation: 2x15-20

                // ADVANCED: Higher volume, lower reps, less rest
                ("avançado" or "advanced", true) => (5, "5-8", "90s"),          // Compound: 5x5-8
                ("avançado" or "advanced", false) => (4, "10-12", "60s"),       // Isolation: 4x10-12

                // INTERMEDIATE: Moderate volume and intensity
                (_, true) => (4, "8-10", "90s"),                                 // Compound: 4x8-10
                _ => (3, "12-15", "60s")                                         // Isolation: 3x12-15
            };

        // ✅ NEW: Calculate RPE (Rate of Perceived Exertion) / RIR (Reps in Reserve)
        var rpe = bodyPart == "cardio" ? null : fitnessLevel.ToLower() switch
        {
            "iniciante" or "beginner" => "RPE 6-7 (poderia fazer 3-4 reps a mais) - Foco em TÉCNICA, não em carga máxima",
            "intermediário" or "intermediate" => "RPE 7-8 (poderia fazer 2-3 reps a mais) - Busque progressão gradual",
            "avançado" or "advanced" => isCompound
                ? "RPE 8-9 (poderia fazer 1-2 reps a mais) - Treine próximo à falha muscular"
                : "RPE 8-9 (poderia fazer 1-2 reps a mais) - Última série até a falha",
            _ => "RPE 7-8 (poderia fazer 2-3 reps a mais)"
        };

        // ✅ NEW: Calculate Tempo (Eccentric-Pause-Concentric-Pause)
        var tempo = bodyPart == "cardio" ? null : fitnessLevel.ToLower() switch
        {
            "iniciante" or "beginner" => "3-0-1-0 (3 seg descendo, 1 seg subindo) - Controle TOTAL do movimento",
            "intermediário" or "intermediate" => "2-0-1-0 (2 seg descendo, 1 seg subindo) - Ritmo controlado",
            "avançado" or "advanced" => isCompound
                ? "2-0-X-0 (2 seg descendo, explosivo subindo) - Força e potência"
                : "3-0-1-1 (3 seg descendo, pausa de 1 seg) - Máxima contração",
            _ => "2-0-1-0"
        };

        // ✅ NEW: Add warm-up sets for beginners on compound exercises
        var warmupSets = bodyPart == "cardio" ? null :
            (fitnessLevel.ToLower() == "iniciante" || fitnessLevel.ToLower() == "beginner") && isCompound
            ? "AQUECIMENTO: 1x10 @ 50% da carga | 1x5 @ 70% da carga | Depois: séries de trabalho"
            : null;

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
            ProgressionNotes: progressionNotes,
            RPE: rpe,
            Tempo: tempo,
            WarmupSets: warmupSets
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

    private static async Task<AIWorkoutResponse> GenerateWorkoutWithAI(
        AIWorkoutRequest request,
        string apiKey,
        dynamic? userProfile = null,
        List<(string Name, string MuscleGroup, string? Equipment, string? Description, string? ImageUrl, string? VideoUrl)>? exercisesFromDb = null,
        IApplicationDbContext? context = null,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        httpClient.Timeout = TimeSpan.FromSeconds(45); // Reasonable timeout for single workout

        // Build user profile context (use async version if context available)
        var profileContext = context != null
            ? await BuildUserProfileContextAsync(userProfile, context, cancellationToken)
            : BuildUserProfileContext(userProfile);

        // ✅ NEW: Build exercise list context
        var exerciseListContext = exercisesFromDb != null && exercisesFromDb.Any()
            ? BuildExerciseListContext(exercisesFromDb)
            : "";

        var systemPrompt = @"Você é um personal trainer brasileiro altamente qualificado e certificado, especializado em prescrição de treinos personalizados e seguros. Crie treinos DETALHADOS, EFICAZES e CIENTIFICAMENTE EMBASADOS.

REGRAS FUNDAMENTAIS:
1. TODOS os nomes de exercícios DEVEM estar em PORTUGUÊS COMPLETO (ex: ""Supino Reto com Barra"", ""Agachamento Livre com Barra"", ""Rosca Direta com Halteres"")
2. ⚠️⚠️⚠️ RESPEITE OBRIGATORIAMENTE O LOCAL DE TREINO PREFERIDO DO USUÁRIO:
   - Se o usuário preferir treinar em CASA: Use EXCLUSIVAMENTE exercícios de PESO CORPORAL (flexões, agachamentos livres, prancha, elevação de pernas, burpees, etc.). É ABSOLUTAMENTE PROIBIDO incluir QUALQUER exercício com halteres, barras, máquinas, cabos, kettlebells, anilhas ou peso externo
   - Se o usuário preferir ACADEMIA: Use exercícios com equipamentos de academia (barras, halteres, máquinas, cabos)
   - Esta preferência tem PRIORIDADE MÁXIMA E ABSOLUTA sobre qualquer outra consideração
   - IMPORTANTE: Para treino em casa, o campo ""equipment"" de TODOS os exercícios DEVE ser ""peso corporal"". Se aparecer ""halteres"", ""barra"" ou qualquer equipamento, você FALHOU
3. RESPEITE ABSOLUTAMENTE E LITERALMENTE o que o usuário pediu no prompt:
   - Se pedir ""treino focado em glúteos e pernas"", 100% dos exercícios DEVEM ser para glúteos e pernas
   - Se mencionar problema em algum músculo (ex: ""tenho dor no joelho""), EVITE exercícios que sobrecarreguem essa região
   - Se pedir foco em área específica (ex: ""quero focar em glúteos""), priorize exercícios que trabalhem DIRETAMENTE esse músculo
   - NÃO inclua exercícios de outros grupos musculares a menos que o usuário explicitamente mencione
4. RESPEITE ESTRITAMENTE todas as restrições do usuário (ex: se pedir ""sem supino"", NÃO inclua nenhuma variação de supino)
5. ADAPTE o treino ao GÊNERO do usuário:
   - Mulheres: Priorize glúteos, pernas, core quando mencionados; ajuste volume e intensidade considerando diferenças hormonais
   - Homens: Maior ênfase em força e hipertrofia de tronco superior quando apropriado
5. ATENÇÃO ESPECIAL AO OBJETIVO DO USUÁRIO:
   - Se o objetivo mencionar ""six-pack"", ""tanquinho"", ""abdômen definido"", ""abs"", ""core"", ou ""perder barriga"", você DEVE SEMPRE incluir 2-3 exercícios abdominais eficazes no treino
   - Exemplos de exercícios abdominais: Abdominal Reto, Prancha, Abdominal Bicicleta, Elevação de Pernas, Abdominal na Polia, Prancha Lateral, etc.
5. Instruções devem ser claras, detalhadas e profissionais em português, incluindo técnica correta e dicas de segurança
6. ⚠️ DIFERENCIE CLARAMENTE por nível de condicionamento - use parâmetros MUITO DIFERENTES:

   INICIANTE (Beginner):
   - Exercícios compostos: 3 sets x 12-15 reps, descanso 120s
   - Exercícios isolados: 2 sets x 15-20 reps, descanso 90s
   - Total de exercícios: 4-6 exercícios (foco em QUALIDADE e TÉCNICA)
   - Duração: 30-45 minutos
   - Foco: aprender movimentos, construir base de força, evitar lesões

   INTERMEDIÁRIO (Intermediate):
   - Exercícios compostos: 4 sets x 8-10 reps, descanso 90s
   - Exercícios isolados: 3 sets x 12-15 reps, descanso 60s
   - Total de exercícios: 6-8 exercícios (equilíbrio volume/intensidade)
   - Duração: 50-65 minutos
   - Foco: progressão de carga, volume moderado-alto

   AVANÇADO (Advanced):
   - Exercícios compostos: 5 sets x 5-8 reps, descanso 90s (cargas pesadas)
   - Exercícios isolados: 4 sets x 10-12 reps, descanso 60s
   - Total de exercícios: 8-12 exercícios (alto volume total)
   - Duração: 70-90 minutos
   - Foco: máxima hipertrofia/força, periodização, técnicas avançadas

7. NUNCA crie treinos com mais de 12 exercícios - isso leva a overtraining e baixa qualidade de execução
8. Selecione exercícios apropriados ao equipamento disponível
9. Priorize exercícios compostos primeiro, depois isolados
10. Inclua aquecimento específico quando necessário
11. Seja criativo mas realista com variações de exercícios

ESTRUTURA DO TREINO (IMPORTANTE - Siga conforme as opções selecionadas pelo usuário):

1. AQUECIMENTO (Se IncludeWarmup = true):
   - Iniciar com 5-10 minutos de aquecimento dinâmico
   - Exemplos: Corrida leve, polichinelos (jumping jacks), rotação de braços, balanço de pernas, burpees leves
   - Use exerciseType: ""warmup"" para todos os exercícios de aquecimento
   - Objetivo: preparar o corpo, elevar frequência cardíaca, aumentar temperatura muscular

2. EXERCÍCIOS PRINCIPAIS:
   - Compostos primeiro (1-2 exercícios), depois isolamento (1-2 exercícios)
   - Use exerciseType: ""main"" para todos os exercícios principais
   - Aqui vai a maior parte do treino

3. MOBILIDADE ARTICULAR (Se IncludeMobility = true):
   - Adicionar 5-8 exercícios de mobilidade articular
   - Focar nas articulações que serão/foram usadas no treino (ombros, quadril, tornozelos, etc.)
   - Exemplos: Círculos de braço, rotação de quadril, alongamento de quadríceps em pé, cat-cow, world's greatest stretch
   - Use exerciseType: ""mobility"" para exercícios de mobilidade
   - Objetivo: melhorar amplitude de movimento, prevenir lesões

4. ALONGAMENTO FINAL (Se IncludeCooldown = true):
   - Finalizar com 5-10 minutos de alongamento estático
   - Alongar TODOS os músculos trabalhados no treino
   - Exemplos: Alongamento de peitoral, isquiotibiais, quadríceps, panturrilha, lombar
   - Use exerciseType: ""cooldown"" para alongamentos finais
   - Objetivo: reduzir tensão muscular, melhorar flexibilidade, auxiliar recuperação

⚠️ IMPORTANTE SOBRE DURAÇÃO:
- Ajuste a duração total do treino para incluir aquecimento/mobilidade/alongamento!
- Se o usuário pedir 60 minutos E incluir aquecimento (10 min) + alongamento (10 min), os exercícios principais devem caber em ~40 minutos
- Exemplo: 60 min total = 10 min aquecimento + 40 min exercícios principais + 10 min alongamento

🔴🔴🔴 CRÍTICO - CAMPO exerciseType É OBRIGATÓRIO:
- NUNCA omita o campo ""exerciseType"" em nenhum exercício
- TODO exercício DEVE ter ""exerciseType"": ""warmup"" OU ""main"" OU ""mobility"" OU ""cooldown""
- Use ""main"" se não for aquecimento, mobilidade ou alongamento
- Verifique que TODOS os exercícios do JSON têm o campo exerciseType antes de retornar

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
      ],
      ""exerciseType"": ""main""
    }
  ]
}

⚠️⚠️⚠️ IMPORTANTE - Campo exerciseType É OBRIGATÓRIO:
- SEMPRE inclua o campo ""exerciseType"" em TODOS os exercícios
- Valores possíveis: ""warmup"", ""main"", ""mobility"", ""cooldown""
- Exemplos:
  * Exercício de aquecimento: ""exerciseType"": ""warmup""
  * Exercício principal: ""exerciseType"": ""main""
  * Exercício de mobilidade: ""exerciseType"": ""mobility""
  * Exercício de alongamento: ""exerciseType"": ""cooldown""
- Se IncludeWarmup=false, IncludeMobility=false, IncludeCooldown=false, use ""main"" para todos";

        var fitnessLevel = request.FitnessLevel ?? "intermediário";
        var duration = request.Duration ?? 60;

        var userPrompt = $@"Crie um treino personalizado COMPLETO seguindo EXATAMENTE estas especificações:

REQUISITOS DO USUÁRIO:
{request.Prompt}

{profileContext}

{exerciseListContext}

PARÂMETROS OBRIGATÓRIOS:
- NÍVEL DE CONDICIONAMENTO: {fitnessLevel}
- DURAÇÃO DO TREINO: {duration} minutos (ajuste o número de exercícios e sets para caber nesse tempo)
{(request.Equipment != null && request.Equipment.Any() ?
$@"- EQUIPAMENTOS DISPONÍVEIS: {string.Join(", ", request.Equipment)}
  RESTRIÇÃO: Use APENAS os equipamentos listados acima. Não inclua exercícios que requerem outros equipamentos." :
"")}

OPÇÕES DE ESTRUTURA DO TREINO SELECIONADAS PELO USUÁRIO:
- Incluir Aquecimento: {request.IncludeWarmup} {(request.IncludeWarmup ? "✅ (OBRIGATÓRIO - incluir aquecimento dinâmico de 5-10 min)" : "❌ (NÃO incluir)")}
- Incluir Mobilidade Articular: {request.IncludeMobility} {(request.IncludeMobility ? "✅ (OBRIGATÓRIO - incluir 5-8 exercícios de mobilidade)" : "❌ (NÃO incluir)")}
- Incluir Alongamento Final: {request.IncludeCooldown} {(request.IncludeCooldown ? "✅ (OBRIGATÓRIO - incluir alongamento estático de 5-10 min)" : "❌ (NÃO incluir)")}

INSTRUÇÕES CRÍTICAS:
1. ⚠️⚠️⚠️ PRIORIDADE ABSOLUTA: Verifique o LOCAL DE TREINO PREFERIDO no perfil do usuário acima e RESPEITE 100%
2. PERSONALIZE o treino baseado no perfil do usuário acima (idade, peso, altura, etc.)
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

            // ✅ FALLBACK INTELIGENTE: Se AI não retornou exerciseType, classificar automaticamente
            if (workout.Exercises != null && workout.Exercises.Any())
            {
                var fixedExercises = new List<ExerciseInstruction>();
                var totalExercises = workout.Exercises.Count;

                for (int i = 0; i < totalExercises; i++)
                {
                    var exercise = workout.Exercises[i];

                    if (string.IsNullOrEmpty(exercise.ExerciseType))
                    {
                        // Classificar automaticamente baseado em lógica inteligente
                        var detectedType = DetectExerciseType(
                            exercise.Name,
                            i,
                            totalExercises,
                            request.IncludeWarmup,
                            request.IncludeMobility,
                            request.IncludeCooldown
                        );

                        fixedExercises.Add(exercise with { ExerciseType = detectedType });
                    }
                    else
                    {
                        fixedExercises.Add(exercise);
                    }
                }
                workout = workout with { Exercises = fixedExercises };
            }

            // 🔥 SUPER FALLBACK: Se AI ignorou as instruções e não gerou warmup/mobility/cooldown, ADICIONAR automaticamente
            var hasWarmup = workout.Exercises.Any(e => e.ExerciseType == "warmup");
            var hasMobility = workout.Exercises.Any(e => e.ExerciseType == "mobility");
            var hasCooldown = workout.Exercises.Any(e => e.ExerciseType == "cooldown");

            var finalExercises = new List<ExerciseInstruction>();

            // Adicionar warmup padrão se solicitado mas não gerado
            if (request.IncludeWarmup && !hasWarmup)
            {
                finalExercises.AddRange(GetDefaultWarmupExercises());
            }

            // Adicionar exercícios principais
            finalExercises.AddRange(workout.Exercises.Where(e => e.ExerciseType == "warmup" || e.ExerciseType == "main" || string.IsNullOrEmpty(e.ExerciseType)));

            // Adicionar mobility padrão se solicitado mas não gerado
            if (request.IncludeMobility && !hasMobility)
            {
                finalExercises.AddRange(GetDefaultMobilityExercises());
            }
            else
            {
                finalExercises.AddRange(workout.Exercises.Where(e => e.ExerciseType == "mobility"));
            }

            // Adicionar cooldown padrão se solicitado mas não gerado
            if (request.IncludeCooldown && !hasCooldown)
            {
                finalExercises.AddRange(GetDefaultCooldownExercises());
            }
            else
            {
                finalExercises.AddRange(workout.Exercises.Where(e => e.ExerciseType == "cooldown"));
            }

            workout = workout with { Exercises = finalExercises };

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

    private static async Task<AIWorkoutPlanResponse> GeneratePlanWithAI(
        AIWorkoutPlanRequest request,
        string apiKey,
        dynamic? userProfile = null,
        IApplicationDbContext? context = null,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        httpClient.Timeout = TimeSpan.FromSeconds(60); // Longer timeout for plan generation

        // Build user profile context (use async version if context available)
        var profileContext = context != null
            ? await BuildUserProfileContextAsync(userProfile, context, cancellationToken)
            : BuildUserProfileContext(userProfile);

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

REGRA CRÍTICA #2 - LOCAL DE TREINO:
⚠️ RESPEITE OBRIGATORIAMENTE O LOCAL DE TREINO PREFERIDO DO USUÁRIO:
- Se o usuário preferir treinar em CASA: Use APENAS exercícios de peso corporal ou equipamento mínimo (flexões, agachamentos livres, prancha, elevação de pernas, etc.). NÃO inclua exercícios com barras, halteres, máquinas ou cabos
- Se o usuário preferir ACADEMIA: Use exercícios com equipamentos de academia (barras, halteres, máquinas, cabos)
- Esta preferência tem PRIORIDADE MÁXIMA sobre qualquer outra consideração

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

INSTRUÇÕES CRÍTICAS:
1. ⚠️⚠️⚠️ PRIORIDADE ABSOLUTA: Verifique o LOCAL DE TREINO PREFERIDO no perfil do usuário acima e RESPEITE 100%
2. ⚠️ O PEDIDO DO USUÁRIO É ABSOLUTO - Se pedir ""lower body"", ""membros inferiores"", ou ""focado em pernas/glúteos"", TODOS os {daysPerWeek} dias DEVEM ser de lower body
3. ⚠️ NÃO adicione upper body ""para balancear"" se o usuário NÃO pediu
4. ⚠️ NÃO crie plano ""completo"" ou ""balanceado"" se o usuário pediu foco específico
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

        // Add workout location preference - CRITICAL
        if (userProfile.PreferredWorkoutLocation != null)
        {
            string locationText = userProfile.PreferredWorkoutLocation switch
            {
                0 => @"ACADEMIA (Gym)
   ✅ PODE: Barras, halteres, máquinas, cabos, leg press, supino, smith machine, etc.
   ❌ NÃO precisa limitar a peso corporal",
                1 => @"🏠🏠🏠 TREINO EM CASA - ZERO EQUIPAMENTO DE ACADEMIA 🏠🏠🏠

   ✅✅✅ PERMITIDO (PESO CORPORAL APENAS):
   • Flexões (todas as variações)
   • Agachamentos livres (sem peso)
   • Afundos (sem peso)
   • Prancha e variações
   • Abdominais (todos os tipos sem peso)
   • Burpees
   • Polichinelos
   • Mountain Climbers
   • Elevação de pernas
   • Pontes de glúteos
   • Step-ups (usando escada/cadeira)
   • Wall sits

   ❌❌❌ ABSOLUTAMENTE PROIBIDO:
   • Halteres
   • Barras
   • Anilhas
   • Máquinas
   • Cabos
   • Kettlebells
   • Elásticos
   • Qualquer peso externo

   ⚠️⚠️⚠️ ATENÇÃO MÁXIMA:
   Se você incluir QUALQUER exercício que mencione ""halter"", ""barra"", ""peso"", ""máquina"", ""cabo"", ""kettlebell"", ""anilha"" ou similar, você FALHOU COMPLETAMENTE.

   EXEMPLOS DE EXERCÍCIOS PROIBIDOS PARA CASA:
   • ""Agachamento com halteres"" ❌
   • ""Supino com barra"" ❌
   • ""Rosca com halteres"" ❌
   • ""Desenvolvimento com halteres"" ❌
   • ""Stiff com barra"" ❌
   • QUALQUER exercício que não seja 100% peso corporal ❌",
                2 => @"AMBOS (Both)
   ✅ PODE: Tanto exercícios de academia quanto de casa",
                _ => "Academia (padrão)"
            };
            context.AppendLine($"\n🏠🏠🏠 LOCAL DE TREINO PREFERIDO - RESPEITE ISSO ACIMA DE TUDO 🏠🏠🏠");
            context.AppendLine(locationText);
            context.AppendLine("⚠️⚠️⚠️ ESSA É A REGRA #1 - RESPEITE O LOCAL ACIMA ⚠️⚠️⚠️\n");
        }

        // Add exercise goal
        if (!string.IsNullOrEmpty(userProfile.ExerciseGoal))
            context.AppendLine($"\n🎯 OBJETIVO DE TREINO: {userProfile.ExerciseGoal}");

        // Add health conditions
        if (!string.IsNullOrEmpty(userProfile.HealthConditions))
            context.AppendLine($"\n🏥 CONDIÇÕES DE SAÚDE: {userProfile.HealthConditions}");

        // Add injuries/limitations with contraindications
        if (!string.IsNullOrEmpty(userProfile.Injuries))
        {
            context.AppendLine($"\n⚠️ LESÕES/LIMITAÇÕES REPORTADAS: {userProfile.Injuries}");

            var contraindications = GetContraindicatedExercises(userProfile.Injuries);
            if (contraindications.Any())
            {
                context.AppendLine($"⚠️ EXERCÍCIOS A EVITAR COMPLETAMENTE:");
                foreach (var exercise in contraindications)
                {
                    context.AppendLine($"   • {exercise}");
                }
            }

            var safeAlternatives = GetSafeAlternatives(userProfile.Injuries);
            if (safeAlternatives.Any())
            {
                context.AppendLine($"✅ ALTERNATIVAS SEGURAS RECOMENDADAS:");
                foreach (var exercise in safeAlternatives)
                {
                    context.AppendLine($"   • {exercise}");
                }
            }
        }

        // Add permanently excluded exercises
        if (!string.IsNullOrEmpty(userProfile.ExcludedExercises))
        {
            context.AppendLine($"\n❌❌❌ EXERCÍCIOS PERMANENTEMENTE EXCLUÍDOS PELO USUÁRIO:");
            var excludedList = ((string)userProfile.ExcludedExercises).Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(e => e.Trim())
                                    .ToList();
            foreach (var exercise in excludedList)
            {
                context.AppendLine($"   • {exercise}");
            }
            context.AppendLine("⚠️⚠️⚠️ NUNCA inclua estes exercícios, mesmo que sejam ideais para o objetivo!");
            context.AppendLine("⚠️⚠️⚠️ Isso é uma restrição ABSOLUTA do usuário!");
        }

        return context.ToString();
    }

    // ✅ NEW ASYNC VERSION: Considers postural assessments when building user context
    private static async Task<string> BuildUserProfileContextAsync(
        dynamic? userProfile,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Start with the sync version's output
        var baseContext = BuildUserProfileContext(userProfile);
        var context = new StringBuilder(baseContext);

        // ✅ Check for active postural assessment
        if (userProfile?.Id != null)
        {
            var userId = (Guid)userProfile.Id;
            var activeAssessment = await dbContext.StudentAssessments
                .Where(a => a.StudentId == userId && a.IsActive)
                .OrderByDescending(a => a.AssessmentType == "Postural" ? 1 : 0)
                .ThenByDescending(a => a.AssessmentDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (activeAssessment != null && activeAssessment.AssessmentType == "Postural")
            {
                context.AppendLine("\n🏥 AVALIAÇÃO POSTURAL RECENTE:");
                context.AppendLine($"Data: {activeAssessment.AssessmentDate:dd/MM/yyyy}");

                var issues = new List<string>();
                var correctiveExercises = new List<string>();
                var contraindicatedExercises = new List<string>();

                // Cabeça anteriorizada (Forward Head)
                if (activeAssessment.ForwardHead is "Moderate" or "Severe")
                {
                    issues.Add($"Cabeça anteriorizada ({activeAssessment.ForwardHead})");
                    correctiveExercises.Add("✅ INCLUIR: Chin Tucks, Face Pulls, Remada Alta, Reverse Flyes, Retração Escapular");
                    contraindicatedExercises.Add("❌ EVITAR: Bench Press com carga excessiva, Behind-Neck Press, Encolhimento pesado");
                }

                // Ombros protusos (Rounded Shoulders)
                if (activeAssessment.RoundedShoulders is "Moderate" or "Severe")
                {
                    issues.Add($"Ombros protusos ({activeAssessment.RoundedShoulders})");
                    correctiveExercises.Add("✅ INCLUIR: Remadas (todas variações), Rotação Externa, Band Pull-Aparts, Face Pulls");
                    contraindicatedExercises.Add("❌ EVITAR: Dips profundos, Bench Press pegada larga, Flexões cotovelos abertos");
                }

                // Inclinação pélvica anterior (Anterior Pelvic Tilt)
                if (activeAssessment.AnteriorPelvicTilt is "Moderate" or "Severe")
                {
                    issues.Add($"Inclinação pélvica anterior ({activeAssessment.AnteriorPelvicTilt})");
                    correctiveExercises.Add("✅ INCLUIR: Prancha, Dead Bug, Glute Bridges, Hip Thrust, Alongamento iliopsoas");
                    contraindicatedExercises.Add("❌ EVITAR: Sit-ups sem controle, Leg Raises sem retroversão, Hiperextensões");
                }

                // Inclinação pélvica posterior (Posterior Pelvic Tilt)
                if (activeAssessment.PosteriorPelvicTilt is "Moderate" or "Severe")
                {
                    issues.Add($"Inclinação pélvica posterior ({activeAssessment.PosteriorPelvicTilt})");
                    correctiveExercises.Add("✅ INCLUIR: Alongamento isquiotibiais, Hip Thrusts, Ponte glúteos, Stiff");
                }

                // Joelhos valgos (Knee Valgus)
                if (activeAssessment.KneeValgus is "Moderate" or "Severe")
                {
                    issues.Add($"Joelhos valgos ({activeAssessment.KneeValgus})");
                    correctiveExercises.Add("✅ INCLUIR: Clamshells, Abdução lateral, Monster Walks, Agachamento com foco abdução");
                    contraindicatedExercises.Add("❌ EVITAR: Agachamento profundo sem correção, Leg Press pés juntos");
                }

                // Joelhos varos (Knee Varus)
                if (activeAssessment.KneeVarus is "Moderate" or "Severe")
                {
                    issues.Add($"Joelhos varos ({activeAssessment.KneeVarus})");
                    correctiveExercises.Add("✅ INCLUIR: Fortalecimento adutores, Controle medial joelho");
                }

                // Pés planos (Flat Feet)
                if (activeAssessment.FlatFeet is "Moderate" or "Severe")
                {
                    issues.Add($"Pés planos ({activeAssessment.FlatFeet})");
                    correctiveExercises.Add("✅ INCLUIR: Toe Curls, Calf Raises, Treino descalço quando possível");
                    contraindicatedExercises.Add("❌ EVITAR: Corrida alto impacto sem calçado adequado");
                }

                // Escoliose (Scoliosis)
                if (activeAssessment.Scoliosis is "Moderate" or "Severe")
                {
                    issues.Add($"Escoliose ({activeAssessment.Scoliosis})");
                    correctiveExercises.Add("✅ INCLUIR: Exercícios unilaterais, Prancha lateral, Core anti-rotacional");
                    contraindicatedExercises.Add("❌ EVITAR: Cargas axiais pesadas, Levantamento terra máximo");
                }

                if (issues.Any())
                {
                    context.AppendLine("\n⚠️⚠️⚠️ DESVIOS POSTURAIS IDENTIFICADOS:");
                    foreach (var issue in issues)
                        context.AppendLine($"   • {issue}");
                }

                if (correctiveExercises.Any())
                {
                    context.AppendLine("\n💪 EXERCÍCIOS CORRETIVOS OBRIGATÓRIOS (INCLUIR NO TREINO):");
                    foreach (var exercise in correctiveExercises)
                        context.AppendLine($"   {exercise}");
                }

                if (contraindicatedExercises.Any())
                {
                    context.AppendLine("\n⛔ EXERCÍCIOS CONTRAINDICADOS (NUNCA INCLUIR):");
                    foreach (var exercise in contraindicatedExercises)
                        context.AppendLine($"   {exercise}");
                }

                if (correctiveExercises.Any() || contraindicatedExercises.Any())
                {
                    context.AppendLine("\n🎯 ESTRATÉGIA BASEADA NA AVALIAÇÃO POSTURAL:");
                    context.AppendLine("   1. INCLUIR pelo menos 2-3 exercícios corretivos listados acima");
                    context.AppendLine("   2. EVITAR completamente os exercícios contraindicados");
                    context.AppendLine("   3. PRIORIZAR fortalecimento de músculos fracos identificados");
                    context.AppendLine("   4. FOCAR em corrigir os desvios posturais ao longo do tempo");
                }

                if (!string.IsNullOrEmpty(activeAssessment.TrainerNotes))
                {
                    context.AppendLine($"\n📝 OBSERVAÇÕES DO PERSONAL TRAINER:");
                    context.AppendLine($"   {activeAssessment.TrainerNotes}");
                }
            }
        }

        return context.ToString();
    }

    private static List<string> GetContraindicatedExercises(string injuries)
    {
        var contraindicatedExercises = new List<string>();
        var injuryList = injuries.ToLower().Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(i => i.Trim())
                                .ToList();

        // Comprehensive contraindications mapping
        var contraindicationsMap = new Dictionary<string, string[]>
        {
            // Knee issues
            ["knee"] = new[] { "Leg Extension", "Extensão de Perna", "Deep Squat", "Agachamento Profundo", "Lunges com Salto", "Jump Lunges", "Box Jumps", "Pistol Squat" },
            ["joelho"] = new[] { "Leg Extension", "Extensão de Perna", "Deep Squat", "Agachamento Profundo", "Lunges com Salto", "Jump Lunges", "Box Jumps", "Pistol Squat" },

            // Herniated disc - most restrictive (requires avoiding spinal compression and twisting)
            ["herniated"] = new[] { "Good Morning", "Straight-Leg Deadlift", "Levantamento Terra Rígido", "T-Bar Row", "Bent-Over Barbell Row", "Hyperextension", "Deadlift", "Levantamento Terra", "Heavy Squat", "Agachamento Pesado", "Seated Overhead Press", "Desenvolvimento Sentado", "Sit-ups", "Abdominais Tradicionais", "Leg Raises", "Elevação de Pernas", "Russian Twist", "Box Jumps", "Burpees" },
            ["hernia"] = new[] { "Good Morning", "Straight-Leg Deadlift", "Levantamento Terra Rígido", "T-Bar Row", "Bent-Over Barbell Row", "Hyperextension", "Deadlift", "Levantamento Terra", "Heavy Squat", "Agachamento Pesado", "Seated Overhead Press", "Desenvolvimento Sentado", "Sit-ups", "Abdominais Tradicionais", "Leg Raises", "Elevação de Pernas", "Russian Twist", "Box Jumps", "Burpees" },
            ["hérnia"] = new[] { "Good Morning", "Straight-Leg Deadlift", "Levantamento Terra Rígido", "T-Bar Row", "Bent-Over Barbell Row", "Hyperextension", "Deadlift", "Levantamento Terra", "Heavy Squat", "Agachamento Pesado", "Seated Overhead Press", "Desenvolvimento Sentado", "Sit-ups", "Abdominais Tradicionais", "Leg Raises", "Elevação de Pernas", "Russian Twist", "Box Jumps", "Burpees" },
            ["disc"] = new[] { "Good Morning", "Straight-Leg Deadlift", "Levantamento Terra Rígido", "T-Bar Row", "Bent-Over Barbell Row", "Hyperextension", "Deadlift", "Levantamento Terra", "Heavy Squat", "Agachamento Pesado", "Seated Overhead Press", "Desenvolvimento Sentado", "Sit-ups", "Abdominais Tradicionais", "Leg Raises", "Elevação de Pernas", "Russian Twist", "Box Jumps", "Burpees" },
            ["disco"] = new[] { "Good Morning", "Straight-Leg Deadlift", "Levantamento Terra Rígido", "T-Bar Row", "Bent-Over Barbell Row", "Hyperextension", "Deadlift", "Levantamento Terra", "Heavy Squat", "Agachamento Pesado", "Seated Overhead Press", "Desenvolvimento Sentado", "Sit-ups", "Abdominais Tradicionais", "Leg Raises", "Elevação de Pernas", "Russian Twist", "Box Jumps", "Burpees" },
            ["ciática"] = new[] { "Good Morning", "Straight-Leg Deadlift", "Levantamento Terra Rígido", "T-Bar Row", "Bent-Over Barbell Row", "Hyperextension", "Deadlift", "Levantamento Terra", "Heavy Squat", "Agachamento Pesado", "Seated Overhead Press", "Desenvolvimento Sentado", "Sit-ups", "Abdominais Tradicionais", "Leg Raises", "Elevação de Pernas", "Russian Twist", "Box Jumps", "Burpees" },
            ["sciatica"] = new[] { "Good Morning", "Straight-Leg Deadlift", "Levantamento Terra Rígido", "T-Bar Row", "Bent-Over Barbell Row", "Hyperextension", "Deadlift", "Levantamento Terra", "Heavy Squat", "Agachamento Pesado", "Seated Overhead Press", "Desenvolvimento Sentado", "Sit-ups", "Abdominais Tradicionais", "Leg Raises", "Elevação de Pernas", "Russian Twist", "Box Jumps", "Burpees" },

            // Lower back issues (general back pain - less restrictive than herniated disc)
            ["lower back"] = new[] { "Good Morning", "Straight-Leg Deadlift", "Levantamento Terra Rígido", "T-Bar Row", "Bent-Over Barbell Row com Carga Pesada", "Hyperextension com Peso Excessivo" },
            ["lombar"] = new[] { "Good Morning", "Straight-Leg Deadlift", "Levantamento Terra Rígido", "T-Bar Row", "Bent-Over Barbell Row com Carga Pesada", "Hyperextension com Peso Excessivo" },
            ["back pain"] = new[] { "Good Morning", "Straight-Leg Deadlift", "Levantamento Terra Rígido", "T-Bar Row", "Bent-Over Barbell Row com Carga Pesada" },
            ["costas"] = new[] { "Good Morning", "Straight-Leg Deadlift", "Levantamento Terra Rígido", "T-Bar Row", "Bent-Over Barbell Row com Carga Pesada" },

            // Shoulder issues - comprehensive list for rotator cuff and impingement protection
            ["shoulder"] = new[] { "Behind-Neck Press", "Desenvolvimento por Trás", "Behind-Neck Lat Pulldown", "Pulldown por Trás da Nuca", "Upright Row", "Remada Alta", "Dips (deep)", "Mergulho Profundo", "Bench Press with Wide Grip", "Supino com Pegada Muito Aberta", "Lateral Raises com Carga Pesada", "Heavy Lateral Raises", "Overhead Press com Carga Máxima", "Military Press Pesado", "Pec Deck com Amplitude Excessiva", "Flyes com Halteres Pesados e Amplitude Completa" },
            ["ombro"] = new[] { "Behind-Neck Press", "Desenvolvimento por Trás", "Behind-Neck Lat Pulldown", "Pulldown por Trás da Nuca", "Upright Row", "Remada Alta", "Dips (deep)", "Mergulho Profundo", "Bench Press with Wide Grip", "Supino com Pegada Muito Aberta", "Elevação Lateral com Carga Pesada", "Heavy Lateral Raises", "Desenvolvimento Militar Pesado", "Overhead Press com Carga Máxima", "Crucifixo com Halteres Pesados", "Pec Deck com Amplitude Excessiva" },

            // Specific shoulder injuries - rotator cuff
            ["rotator cuff"] = new[] { "Behind-Neck Press", "Desenvolvimento por Trás", "Behind-Neck Lat Pulldown", "Pulldown por Trás da Nuca", "Upright Row", "Remada Alta", "Dips (deep)", "Mergulho Profundo", "Bench Press with Wide Grip", "Supino com Pegada Muito Aberta", "Lateral Raises com Carga Pesada", "Heavy Lateral Raises", "Overhead Press com Carga Máxima", "Military Press Pesado", "Pec Deck com Amplitude Excessiva", "Flyes com Halteres Pesados e Amplitude Completa", "Muscle-ups", "Handstand Push-ups" },
            ["manguito rotador"] = new[] { "Behind-Neck Press", "Desenvolvimento por Trás", "Behind-Neck Lat Pulldown", "Pulldown por Trás da Nuca", "Upright Row", "Remada Alta", "Dips (deep)", "Mergulho Profundo", "Bench Press with Wide Grip", "Supino com Pegada Muito Aberta", "Elevação Lateral com Carga Pesada", "Desenvolvimento Militar Pesado", "Overhead Press com Carga Máxima", "Crucifixo com Halteres Pesados", "Pec Deck com Amplitude Excessiva", "Muscle-ups", "Parada de Mão" },

            // Shoulder impingement - similar but may allow some lighter movements
            ["impingement"] = new[] { "Behind-Neck Press", "Desenvolvimento por Trás", "Behind-Neck Lat Pulldown", "Pulldown por Trás da Nuca", "Upright Row", "Remada Alta", "Dips (deep)", "Overhead Press com Carga Máxima", "Military Press Pesado", "Lateral Raises acima de 90 graus", "Pec Deck com Amplitude Excessiva" },
            ["impacto"] = new[] { "Behind-Neck Press", "Desenvolvimento por Trás", "Behind-Neck Lat Pulldown", "Pulldown por Trás da Nuca", "Upright Row", "Remada Alta", "Dips (deep)", "Overhead Press com Carga Máxima", "Desenvolvimento Militar Pesado", "Elevação Lateral acima de 90 graus", "Pec Deck com Amplitude Excessiva" },

            // Wrist issues
            ["wrist"] = new[] { "Heavy Barbell Curls", "Rosca Barra Pesada", "Front Squat", "Agachamento Frontal", "Overhead Press com Barra Pesada", "Push-ups com Rotação" },
            ["punho"] = new[] { "Heavy Barbell Curls", "Rosca Barra Pesada", "Front Squat", "Agachamento Frontal", "Overhead Press com Barra Pesada", "Push-ups com Rotação" },

            // Elbow issues
            ["elbow"] = new[] { "Skull Crushers", "Tríceps Testa", "Close-Grip Bench Press", "Supino Pegada Fechada", "Overhead Tricep Extension", "Pull-ups com Pegada Supinada" },
            ["cotovelo"] = new[] { "Skull Crushers", "Tríceps Testa", "Close-Grip Bench Press", "Supino Pegada Fechada", "Overhead Tricep Extension", "Pull-ups com Pegada Supinada" },

            // Hip issues
            ["hip"] = new[] { "Deep Squats", "Agachamento Profundo", "Sumo Deadlift", "Levantamento Terra Sumô", "High Step-Ups", "Bulgarian Split Squat Profundo" },
            ["quadril"] = new[] { "Deep Squats", "Agachamento Profundo", "Sumo Deadlift", "Levantamento Terra Sumô", "High Step-Ups", "Bulgarian Split Squat Profundo" },

            // Neck issues
            ["neck"] = new[] { "Heavy Shrugs", "Encolhimento Pesado", "Behind-Neck Press", "Desenvolvimento por Trás", "Upright Row", "Remada Alta" },
            ["pescoço"] = new[] { "Heavy Shrugs", "Encolhimento Pesado", "Behind-Neck Press", "Desenvolvimento por Trás", "Upright Row", "Remada Alta" },

            // Ankle issues
            ["ankle"] = new[] { "Jump Squats", "Agachamento com Salto", "Box Jumps", "Calf Raises com Carga Muito Pesada", "Lunges com Salto" },
            ["tornozelo"] = new[] { "Jump Squats", "Agachamento com Salto", "Box Jumps", "Calf Raises com Carga Muito Pesada", "Lunges com Salto" }
        };

        foreach (var injury in injuryList)
        {
            foreach (var (key, exercises) in contraindicationsMap)
            {
                if (injury.Contains(key))
                {
                    contraindicatedExercises.AddRange(exercises);
                }
            }
        }

        return contraindicatedExercises.Distinct().ToList();
    }

    private static List<string> GetSafeAlternatives(string injuries)
    {
        var safeAlternatives = new List<string>();
        var injuryList = injuries.ToLower().Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(i => i.Trim())
                                .ToList();

        // Safe alternatives mapping
        var alternativesMap = new Dictionary<string, string[]>
        {
            ["knee"] = new[] { "Leg Press (ROM parcial)", "Cadeira Flexora", "Agachamento no Smith (ROM controlado)", "Bicicleta Ergométrica", "Step-Ups baixos" },
            ["joelho"] = new[] { "Leg Press (ROM parcial)", "Cadeira Flexora", "Agachamento no Smith (ROM controlado)", "Bicicleta Ergométrica", "Step-Ups baixos" },

            // Herniated disc - focus on no spinal compression, neutral spine exercises
            ["herniated"] = new[] { "Leg Press (45 graus, ROM moderado)", "Cadeira Flexora", "Cadeira Extensora (leve)", "Remada com Apoio no Peito", "Pulldown (neutro)", "Hip Thrust", "Ponte de Glúteo", "Caminhada", "Bicicleta Reclinada", "Prancha Isométrica (curta duração)", "Bird Dog", "Dead Bug" },
            ["hernia"] = new[] { "Leg Press (45 graus, ROM moderado)", "Cadeira Flexora", "Cadeira Extensora (leve)", "Remada com Apoio no Peito", "Pulldown (neutro)", "Hip Thrust", "Ponte de Glúteo", "Caminhada", "Bicicleta Reclinada", "Prancha Isométrica (curta duração)", "Bird Dog", "Dead Bug" },
            ["hérnia"] = new[] { "Leg Press (45 graus, ROM moderado)", "Cadeira Flexora", "Cadeira Extensora (leve)", "Remada com Apoio no Peito", "Pulldown (neutro)", "Elevação Pélvica", "Ponte de Glúteo", "Caminhada", "Bicicleta Reclinada", "Prancha Isométrica (curta duração)", "Bird Dog", "Dead Bug" },
            ["disc"] = new[] { "Leg Press (45 graus, ROM moderado)", "Cadeira Flexora", "Cadeira Extensora (leve)", "Remada com Apoio no Peito", "Pulldown (neutro)", "Hip Thrust", "Ponte de Glúteo", "Caminhada", "Bicicleta Reclinada", "Prancha Isométrica (curta duração)", "Bird Dog", "Dead Bug" },
            ["disco"] = new[] { "Leg Press (45 graus, ROM moderado)", "Cadeira Flexora", "Cadeira Extensora (leve)", "Remada com Apoio no Peito", "Pulldown (neutro)", "Elevação Pélvica", "Ponte de Glúteo", "Caminhada", "Bicicleta Reclinada", "Prancha Isométrica (curta duração)", "Bird Dog", "Dead Bug" },
            ["ciática"] = new[] { "Leg Press (45 graus, ROM moderado)", "Cadeira Flexora", "Cadeira Extensora (leve)", "Remada com Apoio no Peito", "Pulldown (neutro)", "Elevação Pélvica", "Ponte de Glúteo", "Caminhada", "Bicicleta Reclinada", "Prancha Isométrica (curta duração)", "Bird Dog", "Dead Bug" },
            ["sciatica"] = new[] { "Leg Press (45 graus, ROM moderado)", "Cadeira Flexora", "Cadeira Extensora (leve)", "Remada com Apoio no Peito", "Pulldown (neutro)", "Hip Thrust", "Ponte de Glúteo", "Caminhada", "Bicicleta Reclinada", "Prancha Isométrica (curta duração)", "Bird Dog", "Dead Bug" },

            // Lower back issues (general back pain - less restrictive than herniated disc)
            ["lower back"] = new[] { "Leg Press", "Cadeira Flexora", "Remada com Apoio no Peito", "Pulldown", "Hip Thrust", "Ponte de Glúteo" },
            ["lombar"] = new[] { "Leg Press", "Cadeira Flexora", "Remada com Apoio no Peito", "Pulldown", "Hip Thrust", "Ponte de Glúteo" },
            ["back pain"] = new[] { "Leg Press", "Cadeira Flexora", "Remada com Apoio no Peito", "Pulldown", "Hip Thrust" },
            ["costas"] = new[] { "Leg Press", "Cadeira Flexora", "Remada com Apoio no Peito", "Pulldown", "Hip Thrust" },

            ["shoulder"] = new[] { "Desenvolvimento com Halteres (neutro)", "Elevação Frontal Moderada", "Face Pulls", "Crucifixo no Cabo (altura moderada)", "Push-ups (ROM controlado)", "Remada com Apoio no Peito", "Pulldown Neutro", "Lateral Raises com Cabo (leve)", "Rotação Externa com Banda", "Scapular Retraction", "Arnold Press (leve)" },
            ["ombro"] = new[] { "Desenvolvimento com Halteres (pegada neutra)", "Elevação Frontal Moderada", "Face Pulls", "Crucifixo no Cabo (altura moderada)", "Flexões (ROM controlado)", "Remada com Apoio no Peito", "Pulldown Neutro", "Elevação Lateral no Cabo (leve)", "Rotação Externa com Elástico", "Retração Escapular", "Arnold Press (leve)" },

            // Rotator cuff specific - focus on rehabilitation and strengthening
            ["rotator cuff"] = new[] { "Rotação Externa com Banda Leve", "Rotação Interna com Banda Leve", "Face Pulls (leve)", "Scapular Wall Slides", "Prone Y-T-W", "Retração Escapular", "Isométricos de Ombro", "Pulldown Neutro (leve)", "Remada com Apoio no Peito (leve)", "Band Pull-Aparts" },
            ["manguito rotador"] = new[] { "Rotação Externa com Elástico Leve", "Rotação Interna com Elástico Leve", "Face Pulls (leve)", "Wall Slides", "Prone Y-T-W", "Retração Escapular", "Isométricos de Ombro", "Pulldown Neutro (leve)", "Remada com Apoio no Peito (leve)", "Separação de Banda" },

            // Impingement - focus on exercises that avoid overhead positions
            ["impingement"] = new[] { "Face Pulls", "Elevação Frontal até 90 graus", "Lateral Raises até 90 graus (leve)", "Remada com Apoio no Peito", "Pulldown Neutro", "Push-ups em Superfície Elevada", "Rotação Externa com Banda", "Scapular Retraction", "Cable Rows (neutro)" },
            ["impacto"] = new[] { "Face Pulls", "Elevação Frontal até 90 graus", "Elevação Lateral até 90 graus (leve)", "Remada com Apoio no Peito", "Pulldown Neutro", "Flexões em Superfície Elevada", "Rotação Externa com Elástico", "Retração Escapular", "Remada no Cabo (neutro)" },

            ["wrist"] = new[] { "Rosca Martelo", "Rosca com Halteres", "Agachamento no Hack", "Desenvolvimento com Halteres", "Cabos para Tríceps" },
            ["punho"] = new[] { "Rosca Martelo", "Rosca com Halteres", "Agachamento no Hack", "Desenvolvimento com Halteres", "Cabos para Tríceps" },

            ["elbow"] = new[] { "Tríceps na Polia (corda)", "Extensão de Tríceps Unilateral", "Pulldown (neutro)", "Rosca Martelo" },
            ["cotovelo"] = new[] { "Tríceps na Polia (corda)", "Extensão de Tríceps Unilateral", "Pulldown (neutro)", "Rosca Martelo" },

            ["hip"] = new[] { "Leg Press (ROM confortável)", "Hip Thrust", "Cadeira Abdutora", "Cadeira Adutora", "Step-Ups moderados" },
            ["quadril"] = new[] { "Leg Press (ROM confortável)", "Elevação Pélvica", "Cadeira Abdutora", "Cadeira Adutora", "Step-Ups moderados" },

            ["neck"] = new[] { "Desenvolvimento com Halteres", "Elevação Lateral", "Remada com Apoio", "Face Pulls (leve)" },
            ["pescoço"] = new[] { "Desenvolvimento com Halteres", "Elevação Lateral", "Remada com Apoio", "Face Pulls (leve)" },

            ["ankle"] = new[] { "Leg Press", "Agachamento no Smith", "Cadeira Extensora", "Bicicleta Ergométrica" },
            ["tornozelo"] = new[] { "Leg Press", "Agachamento no Smith", "Cadeira Extensora", "Bicicleta Ergométrica" }
        };

        foreach (var injury in injuryList)
        {
            foreach (var (key, exercises) in alternativesMap)
            {
                if (injury.Contains(key))
                {
                    safeAlternatives.AddRange(exercises);
                }
            }
        }

        return safeAlternatives.Distinct().ToList();
    }

    // ✅ NEW: Fetch exercises from database with filters
    private static async Task<List<(string Name, string MuscleGroup, string? Equipment, string? Description, string? ImageUrl, string? VideoUrl)>> GetExercisesFromDatabase(
        IApplicationDbContext context,
        string? workoutLocation = null,
        string? muscleGroupFilter = null,
        string? injuries = null)
    {
        var query = context.Exercises.AsQueryable();

        // Filter by workout location if specified
        if (!string.IsNullOrEmpty(workoutLocation))
        {
            var locationEnum = workoutLocation.ToLower() switch
            {
                "home" => WorkoutLocation.Home,
                "gym" => WorkoutLocation.Gym,
                _ => WorkoutLocation.Both
            };

            query = query.Where(e =>
                e.WorkoutLocation == locationEnum ||
                e.WorkoutLocation == WorkoutLocation.Both);
        }

        // Filter by muscle group if specified
        if (!string.IsNullOrEmpty(muscleGroupFilter))
        {
            var muscleGroupEnum = ParseMuscleGroup(muscleGroupFilter);
            query = query.Where(e => e.MuscleGroup == muscleGroupEnum);
        }

        var exercises = await query
            .Select(e => new
            {
                e.Name,
                MuscleGroup = e.MuscleGroup.ToString(),
                Equipment = e.Equipment.ToString(),
                e.Description,
                e.ImageUrl,
                e.VideoUrl
            })
            .ToListAsync();

        // Filter out contraindicated exercises if injuries exist
        if (!string.IsNullOrEmpty(injuries))
        {
            var contraindicatedExercises = GetContraindicatedExercises(injuries);
            exercises = exercises.Where(e =>
                !contraindicatedExercises.Any(c =>
                    e.Name.Contains(c, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        return exercises
            .Select(e => (e.Name, e.MuscleGroup, e.Equipment, e.Description, e.ImageUrl, e.VideoUrl))
            .ToList();
    }

    // ✅ NEW: Save new exercise to database automatically
    private static async Task<Guid> SaveNewExerciseToDatabase(
        IApplicationDbContext context,
        string name,
        string muscleGroup,
        string? equipment = null,
        string? description = null,
        string? category = null)
    {
        // Check if exercise already exists
        var existing = await context.Exercises
            .FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower());

        if (existing != null)
        {
            return existing.Id;
        }

        // Parse string values to enums
        var muscleGroupEnum = ParseMuscleGroup(muscleGroup);
        var equipmentEnum = ParseEquipment(equipment);
        var categoryEnum = ParseCategory(category ?? muscleGroup);

        // Determine workout location based on equipment
        var workoutLocation = equipmentEnum switch
        {
            Equipment.None => WorkoutLocation.Home,
            Equipment.Barbell or Equipment.CableMachine or Equipment.Machine or Equipment.SmithMachine => WorkoutLocation.Gym,
            Equipment.Dumbbell or Equipment.Kettlebell or Equipment.ResistanceBand or Equipment.Bench => WorkoutLocation.Both,
            _ => WorkoutLocation.Both
        };

        var newExercise = new Domain.Entities.Exercise
        {
            Name = name,
            Description = description,
            MuscleGroup = muscleGroupEnum,
            Equipment = equipmentEnum,
            Category = categoryEnum,
            Difficulty = DifficultyLevel.Intermediate, // Default to intermediate for AI-generated exercises
            WorkoutLocation = workoutLocation,
            ImageUrl = null, // Will be populated later by seeder or manual upload
            VideoUrl = null  // Will be populated later by seeder or manual upload
        };

        await context.Exercises.AddAsync(newExercise);
        await context.SaveChangesAsync(CancellationToken.None);

        return newExercise.Id;
    }

    // ✅ NEW: Auto-save new exercises from AI-generated workout
    private static async Task<int> AutoSaveNewExercises(
        IApplicationDbContext context,
        AIWorkoutResponse workout,
        ILogger<Program> logger)
    {
        var savedCount = 0;

        if (workout?.Exercises == null || !workout.Exercises.Any())
        {
            return savedCount;
        }

        foreach (var exercise in workout.Exercises)
        {
            try
            {
                // Check if exercise already exists in database
                var exists = await context.Exercises
                    .AnyAsync(e => e.Name.ToLower() == exercise.Name.ToLower());

                if (!exists)
                {
                    logger.LogInformation($"New exercise found: '{exercise.Name}' - saving to database...");

                    // Extract muscle group from bodyPart
                    var muscleGroup = MapBodyPartToMuscleGroup(exercise.BodyPart);

                    // Save to database
                    await SaveNewExerciseToDatabase(
                        context,
                        name: exercise.Name,
                        muscleGroup: muscleGroup,
                        equipment: exercise.Equipment,
                        description: null, // AI doesn't provide description in response
                        category: muscleGroup
                    );

                    savedCount++;
                    logger.LogInformation($"✅ Saved '{exercise.Name}' to database");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"Failed to save exercise '{exercise.Name}' to database. Continuing...");
                // Continue with next exercise - don't fail the entire request
            }
        }

        return savedCount;
    }

    // ✅ NEW: Auto-save new exercises from a workout day (for workout plans)
    private static async Task<int> AutoSaveNewExercisesFromDay(
        IApplicationDbContext context,
        WorkoutDay day,
        ILogger<Program> logger)
    {
        var savedCount = 0;

        if (day?.Exercises == null || !day.Exercises.Any())
        {
            return savedCount;
        }

        foreach (var exercise in day.Exercises)
        {
            try
            {
                // Check if exercise already exists in database
                var exists = await context.Exercises
                    .AnyAsync(e => e.Name.ToLower() == exercise.Name.ToLower());

                if (!exists)
                {
                    logger.LogInformation($"New exercise found in '{day.DayName}': '{exercise.Name}' - saving to database...");

                    // Extract muscle group from bodyPart
                    var muscleGroup = MapBodyPartToMuscleGroup(exercise.BodyPart);

                    // Save to database
                    await SaveNewExerciseToDatabase(
                        context,
                        name: exercise.Name,
                        muscleGroup: muscleGroup,
                        equipment: exercise.Equipment,
                        description: null, // AI doesn't provide description in response
                        category: muscleGroup
                    );

                    savedCount++;
                    logger.LogInformation($"✅ Saved '{exercise.Name}' to database");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"Failed to save exercise '{exercise.Name}' to database. Continuing...");
                // Continue with next exercise - don't fail the entire request
            }
        }

        return savedCount;
    }

    // ✅ NEW: Map AI bodyPart to our MuscleGroup
    private static string MapBodyPartToMuscleGroup(string bodyPart)
    {
        return bodyPart?.ToLower() switch
        {
            "chest" => "Peito",
            "back" => "Costas",
            "shoulders" => "Ombros",
            "biceps" => "Bíceps",
            "triceps" => "Tríceps",
            "legs" => "Pernas",
            "quadriceps" => "Pernas",
            "hamstrings" => "Pernas",
            "glutes" => "Glúteos",
            "calves" => "Panturrilha",
            "abs" => "Abdômen",
            "core" => "Abdômen",
            "cardio" => "Cardio",
            "full body" => "Corpo Todo",
            _ => "Geral"
        };
    }

    // ✅ MODIFIED: Build exercise list context for AI
    private static string BuildExerciseListContext(List<(string Name, string MuscleGroup, string? Equipment, string? Description, string? ImageUrl, string? VideoUrl)> exercises)
    {
        if (!exercises.Any()) return "";

        var context = new StringBuilder("\n\n📚 BANCO DE EXERCÍCIOS DISPONÍVEIS (PRIORIZE ESTES):\n");
        context.AppendLine("Use PREFERENCIALMENTE exercícios desta lista. Apenas crie novos se absolutamente necessário.\n");

        var groupedExercises = exercises
            .GroupBy(e => e.MuscleGroup)
            .OrderBy(g => g.Key);

        foreach (var group in groupedExercises)
        {
            context.AppendLine($"\n🎯 {group.Key.ToUpper()}:");
            foreach (var exercise in group.Take(15)) // Limit to 15 per group to avoid token overflow
            {
                var equipmentInfo = !string.IsNullOrEmpty(exercise.Equipment) ? $" ({exercise.Equipment})" : "";
                context.AppendLine($"   • {exercise.Name}{equipmentInfo}");
            }
        }

        context.AppendLine("\n⚠️ IMPORTANTE: Prefira sempre usar exercícios desta lista. Eles têm fotos e vídeos disponíveis para melhor experiência do usuário.");

        return context.ToString();
    }

    private static async Task<AIWorkoutResponse> GenerateWorkoutWithGemini(
        AIWorkoutRequest request,
        string apiKey,
        dynamic? userProfile = null,
        List<(string Name, string MuscleGroup, string? Equipment, string? Description, string? ImageUrl, string? VideoUrl)>? exercisesFromDb = null,
        IApplicationDbContext? context = null,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(45);

        // Build user profile context (use async version if context available)
        var profileContext = context != null
            ? await BuildUserProfileContextAsync(userProfile, context, cancellationToken)
            : BuildUserProfileContext(userProfile);

        // ✅ NEW: Build exercise list context
        var exerciseListContext = exercisesFromDb != null && exercisesFromDb.Any()
            ? BuildExerciseListContext(exercisesFromDb)
            : "";

        var systemPrompt = @"Você é um personal trainer brasileiro altamente qualificado e certificado, especializado em prescrição de treinos personalizados e seguros. Crie treinos DETALHADOS, EFICAZES e CIENTIFICAMENTE EMBASADOS.

REGRAS FUNDAMENTAIS:
1. TODOS os nomes de exercícios DEVEM estar em PORTUGUÊS COMPLETO (ex: ""Supino Reto com Barra"", ""Agachamento Livre com Barra"", ""Rosca Direta com Halteres"")
2. ⚠️⚠️⚠️ RESPEITE OBRIGATORIAMENTE O LOCAL DE TREINO PREFERIDO DO USUÁRIO:
   - Se o usuário preferir treinar em CASA: Use EXCLUSIVAMENTE exercícios de PESO CORPORAL (flexões, agachamentos livres, prancha, elevação de pernas, burpees, etc.). É ABSOLUTAMENTE PROIBIDO incluir QUALQUER exercício com halteres, barras, máquinas, cabos, kettlebells, anilhas ou peso externo
   - Se o usuário preferir ACADEMIA: Use exercícios com equipamentos de academia (barras, halteres, máquinas, cabos)
   - Esta preferência tem PRIORIDADE MÁXIMA E ABSOLUTA sobre qualquer outra consideração
   - IMPORTANTE: Para treino em casa, o campo ""equipment"" de TODOS os exercícios DEVE ser ""peso corporal"". Se aparecer ""halteres"", ""barra"" ou qualquer equipamento, você FALHOU
3. RESPEITE ABSOLUTAMENTE E LITERALMENTE o que o usuário pediu no prompt:
   - Se pedir ""treino focado em glúteos e pernas"", 100% dos exercícios DEVEM ser para glúteos e pernas
   - Se mencionar problema em algum músculo (ex: ""tenho dor no joelho""), EVITE exercícios que sobrecarreguem essa região
   - Se pedir foco em área específica (ex: ""quero focar em glúteos""), priorize exercícios que trabalhem DIRETAMENTE esse músculo
   - NÃO inclua exercícios de outros grupos musculares a menos que o usuário explicitamente mencione
4. RESPEITE ESTRITAMENTE todas as restrições do usuário (ex: se pedir ""sem supino"", NÃO inclua nenhuma variação de supino)
5. ADAPTE o treino ao GÊNERO do usuário:
   - Mulheres: Priorize glúteos, pernas, core quando mencionados; ajuste volume e intensidade considerando diferenças hormonais
   - Homens: Maior ênfase em força e hipertrofia de tronco superior quando apropriado
5. ATENÇÃO ESPECIAL AO OBJETIVO DO USUÁRIO:
   - Se o objetivo mencionar ""six-pack"", ""tanquinho"", ""abdômen definido"", ""abs"", ""core"", ou ""perder barriga"", você DEVE SEMPRE incluir 2-3 exercícios abdominais eficazes no treino
   - Exemplos de exercícios abdominais: Abdominal Reto, Prancha, Abdominal Bicicleta, Elevação de Pernas, Abdominal na Polia, Prancha Lateral, etc.
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

ESTRUTURA DO TREINO (IMPORTANTE - Siga conforme as opções selecionadas pelo usuário):

1. AQUECIMENTO (Se IncludeWarmup = true):
   - Iniciar com 5-10 minutos de aquecimento dinâmico
   - Exemplos: Corrida leve, polichinelos (jumping jacks), rotação de braços, balanço de pernas, burpees leves
   - Use exerciseType: ""warmup"" para todos os exercícios de aquecimento
   - Objetivo: preparar o corpo, elevar frequência cardíaca, aumentar temperatura muscular

2. EXERCÍCIOS PRINCIPAIS:
   - Compostos primeiro (1-2 exercícios), depois isolamento (1-2 exercícios)
   - Use exerciseType: ""main"" para todos os exercícios principais
   - Aqui vai a maior parte do treino

3. MOBILIDADE ARTICULAR (Se IncludeMobility = true):
   - Adicionar 5-8 exercícios de mobilidade articular
   - Focar nas articulações que serão/foram usadas no treino (ombros, quadril, tornozelos, etc.)
   - Exemplos: Círculos de braço, rotação de quadril, alongamento de quadríceps em pé, cat-cow, world's greatest stretch
   - Use exerciseType: ""mobility"" para exercícios de mobilidade
   - Objetivo: melhorar amplitude de movimento, prevenir lesões

4. ALONGAMENTO FINAL (Se IncludeCooldown = true):
   - Finalizar com 5-10 minutos de alongamento estático
   - Alongar TODOS os músculos trabalhados no treino
   - Exemplos: Alongamento de peitoral, isquiotibiais, quadríceps, panturrilha, lombar
   - Use exerciseType: ""cooldown"" para alongamentos finais
   - Objetivo: reduzir tensão muscular, melhorar flexibilidade, auxiliar recuperação

⚠️ IMPORTANTE SOBRE DURAÇÃO:
- Ajuste a duração total do treino para incluir aquecimento/mobilidade/alongamento!
- Se o usuário pedir 60 minutos E incluir aquecimento (10 min) + alongamento (10 min), os exercícios principais devem caber em ~40 minutos
- Exemplo: 60 min total = 10 min aquecimento + 40 min exercícios principais + 10 min alongamento

🔴🔴🔴 CRÍTICO - CAMPO exerciseType É OBRIGATÓRIO:
- NUNCA omita o campo ""exerciseType"" em nenhum exercício
- TODO exercício DEVE ter ""exerciseType"": ""warmup"" OU ""main"" OU ""mobility"" OU ""cooldown""
- Use ""main"" se não for aquecimento, mobilidade ou alongamento
- Verifique que TODOS os exercícios do JSON têm o campo exerciseType antes de retornar

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
      ],
      ""exerciseType"": ""main""
    }
  ]
}

⚠️⚠️⚠️ IMPORTANTE - Campo exerciseType É OBRIGATÓRIO:
- SEMPRE inclua o campo ""exerciseType"" em TODOS os exercícios
- Valores possíveis: ""warmup"", ""main"", ""mobility"", ""cooldown""
- Exemplos:
  * Exercício de aquecimento: ""exerciseType"": ""warmup""
  * Exercício principal: ""exerciseType"": ""main""
  * Exercício de mobilidade: ""exerciseType"": ""mobility""
  * Exercício de alongamento: ""exerciseType"": ""cooldown""
- Se IncludeWarmup=false, IncludeMobility=false, IncludeCooldown=false, use ""main"" para todos";

        var fitnessLevel = request.FitnessLevel ?? "intermediário";
        var duration = request.Duration ?? 60;

        var userPrompt = $@"Crie um treino personalizado COMPLETO seguindo EXATAMENTE estas especificações:

REQUISITOS DO USUÁRIO:
{request.Prompt}

{profileContext}

{exerciseListContext}

PARÂMETROS OBRIGATÓRIOS:
- NÍVEL DE CONDICIONAMENTO: {fitnessLevel}
- DURAÇÃO DO TREINO: {duration} minutos (ajuste o número de exercícios e sets para caber nesse tempo)
{(request.Equipment != null && request.Equipment.Any() ?
$@"- EQUIPAMENTOS DISPONÍVEIS: {string.Join(", ", request.Equipment)}
  RESTRIÇÃO: Use APENAS os equipamentos listados acima." :
"")}

OPÇÕES DE ESTRUTURA DO TREINO SELECIONADAS PELO USUÁRIO:
- Incluir Aquecimento: {request.IncludeWarmup} {(request.IncludeWarmup ? "✅ (OBRIGATÓRIO - incluir aquecimento dinâmico de 5-10 min)" : "❌ (NÃO incluir)")}
- Incluir Mobilidade Articular: {request.IncludeMobility} {(request.IncludeMobility ? "✅ (OBRIGATÓRIO - incluir 5-8 exercícios de mobilidade)" : "❌ (NÃO incluir)")}
- Incluir Alongamento Final: {request.IncludeCooldown} {(request.IncludeCooldown ? "✅ (OBRIGATÓRIO - incluir alongamento estático de 5-10 min)" : "❌ (NÃO incluir)")}

INSTRUÇÕES CRÍTICAS:
1. ⚠️⚠️⚠️ PRIORIDADE ABSOLUTA: Verifique o LOCAL DE TREINO PREFERIDO no perfil do usuário acima e RESPEITE 100%
2. PERSONALIZE o treino baseado no perfil do usuário acima (idade, peso, altura, etc.)
3. Se o usuário mencionar exercícios para EVITAR ou EXCLUIR, você DEVE respeitar isso COMPLETAMENTE
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

            // ✅ FALLBACK INTELIGENTE: Se AI não retornou exerciseType, classificar automaticamente
            var fixedExercises = new List<ExerciseInstruction>();
            var totalExercises = workout.Exercises.Count;

            for (int i = 0; i < totalExercises; i++)
            {
                var exercise = workout.Exercises[i];

                if (string.IsNullOrEmpty(exercise.ExerciseType))
                {
                    // Classificar automaticamente baseado em lógica inteligente
                    var detectedType = DetectExerciseType(
                        exercise.Name,
                        i,
                        totalExercises,
                        request.IncludeWarmup,
                        request.IncludeMobility,
                        request.IncludeCooldown
                    );

                    fixedExercises.Add(exercise with { ExerciseType = detectedType });
                }
                else
                {
                    fixedExercises.Add(exercise);
                }
            }
            workout = workout with { Exercises = fixedExercises };

            // 🔥 SUPER FALLBACK: Se AI ignorou as instruções e não gerou warmup/mobility/cooldown, ADICIONAR automaticamente
            var hasWarmup = workout.Exercises.Any(e => e.ExerciseType == "warmup");
            var hasMobility = workout.Exercises.Any(e => e.ExerciseType == "mobility");
            var hasCooldown = workout.Exercises.Any(e => e.ExerciseType == "cooldown");

            var finalExercises = new List<ExerciseInstruction>();

            // Adicionar warmup padrão se solicitado mas não gerado
            if (request.IncludeWarmup && !hasWarmup)
            {
                finalExercises.AddRange(GetDefaultWarmupExercises());
            }

            // Adicionar exercícios principais
            finalExercises.AddRange(workout.Exercises.Where(e => e.ExerciseType == "warmup" || e.ExerciseType == "main" || string.IsNullOrEmpty(e.ExerciseType)));

            // Adicionar mobility padrão se solicitado mas não gerado
            if (request.IncludeMobility && !hasMobility)
            {
                finalExercises.AddRange(GetDefaultMobilityExercises());
            }
            else
            {
                finalExercises.AddRange(workout.Exercises.Where(e => e.ExerciseType == "mobility"));
            }

            // Adicionar cooldown padrão se solicitado mas não gerado
            if (request.IncludeCooldown && !hasCooldown)
            {
                finalExercises.AddRange(GetDefaultCooldownExercises());
            }
            else
            {
                finalExercises.AddRange(workout.Exercises.Where(e => e.ExerciseType == "cooldown"));
            }

            workout = workout with { Exercises = finalExercises };

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

    private static async Task<AIWorkoutPlanResponse> GeneratePlanWithGemini(
        AIWorkoutPlanRequest request,
        string apiKey,
        dynamic? userProfile = null,
        IApplicationDbContext? context = null,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(60);

        // Build user profile context (use async version if context available)
        var profileContext = context != null
            ? await BuildUserProfileContextAsync(userProfile, context, cancellationToken)
            : BuildUserProfileContext(userProfile);

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

REGRA CRÍTICA #2 - LOCAL DE TREINO:
⚠️ RESPEITE OBRIGATORIAMENTE O LOCAL DE TREINO PREFERIDO DO USUÁRIO:
- Se o usuário preferir treinar em CASA: Use APENAS exercícios de peso corporal ou equipamento mínimo (flexões, agachamentos livres, prancha, elevação de pernas, etc.). NÃO inclua exercícios com barras, halteres, máquinas ou cabos
- Se o usuário preferir ACADEMIA: Use exercícios com equipamentos de academia (barras, halteres, máquinas, cabos)
- Esta preferência tem PRIORIDADE MÁXIMA sobre qualquer outra consideração

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
1. ⚠️⚠️⚠️ PRIORIDADE ABSOLUTA: Verifique o LOCAL DE TREINO PREFERIDO no perfil do usuário acima e RESPEITE 100%
2. ⚠️ O PEDIDO DO USUÁRIO É ABSOLUTO - Se pedir ""lower body"", TODOS os {daysPerWeek} dias DEVEM ser de lower body
3. ⚠️ NÃO adicione upper body ""para balancear"" se o usuário NÃO pediu
4. ⚠️ NÃO crie plano ""completo"" se o usuário pediu foco específico
5. PERSONALIZE baseado no perfil do usuário (idade, peso, altura, IMC, gênero)
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

    private static AIWorkoutPlanResponse GenerateMockPlan(string prompt, int daysPerWeek, string? fitnessLevel = null, dynamic? userProfile = null)
    {
        // Check if user prefers home workouts
        var isHomeWorkout = userProfile?.PreferredWorkoutLocation == WorkoutLocation.Home;

        var random = new Random();
        var level = fitnessLevel?.ToLower() ?? "intermediário";
        var days = new List<WorkoutDay>();

        // Determine exercise count per day based on fitness level
        // Beginners need fewer exercises per session to avoid overtraining
        var (minExercisesPerDay, maxExercisesPerDay) = level switch
        {
            "iniciante" or "beginner" => (3, 5),    // Fewer exercises, focus on recovery
            "avançado" or "advanced" => (7, 10),    // More exercises, higher work capacity
            _ => (5, 7)                              // Intermediate: moderate volume
        };

        // Parse user prompt to detect focus areas
        var parsedPrompt = ParsePrompt(prompt.ToLower());
        var focusMuscleGroups = parsedPrompt.MuscleGroups;

        // Define workout splits based on user focus or default to standard splits
        // Format: (DayName, Title, MuscleGroups)
        (string DayName, string Title, string[] MuscleGroups)[] workoutSplits;

        // If user specified specific muscle groups, create a focused plan
        if (focusMuscleGroups.Any())
        {

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

        foreach (var (dayName, dayTitle, muscleGroups) in workoutSplits)
        {
            var exercisesForDay = new List<ExerciseInstruction>();
            var totalExercisesForDay = random.Next(minExercisesPerDay, maxExercisesPerDay + 1);
            var exercisesPerGroup = Math.Max(1, totalExercisesForDay / muscleGroups.Length);

            foreach (var muscleGroup in muscleGroups)
            {
                if (ExerciseDatabase.ContainsKey(muscleGroup))
                {
                    var availableExercises = ExerciseDatabase[muscleGroup]
                        .Where(ex => !isHomeWorkout || ex.Equipment == "body only") // Filter for home workouts
                        .ToList();

                    // Separate compound and isolation exercises
                    var compoundExercises = availableExercises.Where(ex => ex.IsCompound).OrderBy(x => random.Next()).ToList();
                    var isolationExercises = availableExercises.Where(ex => !ex.IsCompound).OrderBy(x => random.Next()).ToList();

                    var countForThisGroup = Math.Min(
                        muscleGroup == "abdômen" || muscleGroup == "panturrilha" ?
                            random.Next(1, 3) : // Smaller muscles get 1-2 exercises
                            random.Next(2, exercisesPerGroup + 2), // Main muscles get more
                        availableExercises.Count
                    );

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
            var fillAttempts = 0;
            var maxFillAttempts = 50; // Prevent infinite loop
            while (exercisesForDay.Count < minExercisesPerDay && fillAttempts < maxFillAttempts)
            {
                fillAttempts++;
                var randomMuscleGroup = muscleGroups[random.Next(muscleGroups.Length)];
                if (ExerciseDatabase.ContainsKey(randomMuscleGroup))
                {
                    var availableExercises = ExerciseDatabase[randomMuscleGroup]
                        .Where(ex => !isHomeWorkout || ex.Equipment == "body only") // Filter for home workouts
                        .ToList();

                    if (availableExercises.Count == 0) continue; // Skip if no exercises available

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

            if (fillAttempts >= maxFillAttempts)
            {
                // Could not fill to minimum exercises - limited exercises available
            }

            // Add finishers: abs (most days) and cardio (2-3x per week)
            var shouldAddAbs = !muscleGroups.Contains("abdômen") && ExerciseDatabase.ContainsKey("abdômen");
            if (shouldAddAbs)
            {
                var absExercises = ExerciseDatabase["abdômen"]
                    .Where(ex => !isHomeWorkout || ex.Equipment == "body only") // Filter for home workouts
                    .OrderBy(x => random.Next())
                    .Take(random.Next(1, 3))
                    .ToList();

                foreach (var absEx in absExercises)
                {
                    exercisesForDay.Add(CreateExerciseInstruction(
                        absEx.Name,
                        absEx.BodyPart,
                        absEx.Equipment,
                        false,
                        level,
                        absEx.IsCompound
                    ));
                }
            }

            // Add cardio 2-3 times per week (randomly distributed across days)
            // For a 5-day plan: 40-60% chance per day gives ~2-3 cardio sessions
            // For fewer days: adjust probability accordingly
            var cardioChance = daysPerWeek switch
            {
                2 => 0.5,  // 50% chance = ~1 day with cardio
                3 => 0.4,  // 40% chance = ~1-2 days
                4 => 0.4,  // 40% chance = ~1-2 days
                5 => 0.5,  // 50% chance = ~2-3 days
                _ => 0.4   // 40% chance for 6+ days
            };

            var shouldAddCardio = random.NextDouble() < cardioChance &&
                                 !muscleGroups.Contains("cardio") &&
                                 ExerciseDatabase.ContainsKey("cardio");

            if (shouldAddCardio)
            {
                // Filter out simulated exercises
                var cardioExercises = ExerciseDatabase["cardio"]
                    .Where(ex => !isHomeWorkout || ex.Equipment == "body only") // Filter for home workouts
                    .Where(ex => !ex.Name.Contains("Simulada", StringComparison.OrdinalIgnoreCase) &&
                                !ex.Name.Contains("Simulated", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(x => random.Next())
                    .Take(1)
                    .ToList();

                foreach (var cardioEx in cardioExercises)
                {
                    exercisesForDay.Add(CreateExerciseInstruction(
                        cardioEx.Name,
                        cardioEx.BodyPart,
                        cardioEx.Equipment,
                        false,
                        level,
                        cardioEx.IsCompound
                    ));
                }
            }

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

        // ✅ NEW: Apply exercise ordering to each day
        for (int i = 0; i < days.Count; i++)
        {
            days[i] = days[i] with { Exercises = OrderExercisesOptimally(days[i].Exercises) };
        }

        // ✅ NEW: Validate workout plan quality
        ValidateVolumeLandmarks(days, level);
        ValidateRecoveryTime(days);
        ValidateMovementPatternBalance(days);

        return new AIWorkoutPlanResponse(
            Title: title,
            Description: "Plano completo de treino dividido por grupos musculares para máximo ganho de massa muscular",
            WeeksCount: 4,
            DaysPerWeek: days.Count,
            Goal: goal,
            Days: days
        );
    }

    /// <summary>
    /// Retorna exercícios padrão de aquecimento quando a AI ignora a instrução
    /// </summary>
    private static List<ExerciseInstruction> GetDefaultWarmupExercises()
    {
        return new List<ExerciseInstruction>
        {
            new("Polichinelos (Jumping Jacks)", "cardio", "peso corporal", 1, "30 segundos", "15s",
                new List<string> { "Fique em pé com pés juntos", "Salte abrindo pernas e elevando braços acima da cabeça", "Retorne à posição inicial", "Mantenha ritmo constante" },
                null, null, null, null, null, null, "warmup"),
            new("Corrida Estacionária", "cardio", "peso corporal", 1, "45 segundos", "15s",
                new List<string> { "Corra no lugar elevando joelhos até altura do quadril", "Mantenha ritmo moderado", "Use braços para auxiliar o movimento" },
                null, null, null, null, null, null, "warmup"),
            new("Rotação de Braços", "ombros", "peso corporal", 1, "20 repetições", "10s",
                new List<string> { "Estenda braços lateralmente", "Faça círculos amplos com os braços", "10 para frente, 10 para trás" },
                null, null, null, null, null, null, "warmup")
        };
    }

    /// <summary>
    /// Retorna exercícios padrão de mobilidade quando a AI ignora a instrução
    /// </summary>
    private static List<ExerciseInstruction> GetDefaultMobilityExercises()
    {
        return new List<ExerciseInstruction>
        {
            new("Círculos de Quadril", "core", "peso corporal", 2, "10 por lado", "20s",
                new List<string> { "Fique em pé com mãos na cintura", "Faça círculos amplos com o quadril", "10 no sentido horário, 10 anti-horário" },
                null, null, null, null, null, null, "mobility"),
            new("Gato-Vaca (Cat-Cow)", "core", "peso corporal", 2, "15 repetições", "20s",
                new List<string> { "Posição de quatro apoios", "Arqueie as costas olhando para cima (vaca)", "Arredonde as costas olhando para baixo (gato)", "Movimento lento e controlado" },
                null, null, null, null, null, null, "mobility"),
            new("World's Greatest Stretch", "corpo todo", "peso corporal", 2, "8 por lado", "30s",
                new List<string> { "Posição de afundo baixo", "Cotovelo toca o chão interno do pé da frente", "Rotação de tronco com braço estendido para cima", "Alterne os lados" },
                null, null, null, null, null, null, "mobility")
        };
    }

    /// <summary>
    /// Retorna exercícios padrão de alongamento quando a AI ignora a instrução
    /// </summary>
    private static List<ExerciseInstruction> GetDefaultCooldownExercises()
    {
        return new List<ExerciseInstruction>
        {
            new("Alongamento de Peitorais", "peito", "peso corporal", 1, "30 segundos", "10s",
                new List<string> { "Entrelace os dedos atrás das costas", "Estique os braços e eleve-os gentilmente", "Mantenha o peito aberto e ombros para trás" },
                null, null, null, null, null, null, "cooldown"),
            new("Alongamento de Isquiotibiais", "pernas", "peso corporal", 1, "30 segundos por perna", "10s",
                new List<string> { "Sentado com uma perna estendida", "Incline-se para frente tentando tocar os dedos do pé", "Mantenha as costas retas", "Segure a posição sem balançar" },
                null, null, null, null, null, null, "cooldown"),
            new("Alongamento de Quadríceps", "pernas", "peso corporal", 1, "30 segundos por perna", "10s",
                new List<string> { "Em pé, segure um pé atrás de você", "Puxe o calcanhar em direção ao glúteo", "Mantenha joelhos juntos", "Use parede para equilíbrio se necessário" },
                null, null, null, null, null, null, "cooldown"),
            new("Alongamento de Lombar (Child's Pose)", "core", "peso corporal", 1, "45 segundos", "0s",
                new List<string> { "Ajoelhe-se e sente sobre os calcanhares", "Estenda braços à frente no chão", "Abaixe o tronco entre as coxas", "Respire profundamente e relaxe" },
                null, null, null, null, null, null, "cooldown")
        };
    }

    /// <summary>
    /// Detecta automaticamente o tipo de exercício baseado em palavras-chave e contexto
    /// </summary>
    private static string DetectExerciseType(
        string exerciseName,
        int position,
        int totalExercises,
        bool includeWarmup,
        bool includeMobility,
        bool includeCooldown)
    {
        var nameLower = exerciseName.ToLowerInvariant();

        // 🔥 WARMUP - Palavras-chave de aquecimento
        var warmupKeywords = new[]
        {
            "jumping jack", "polichinelo", "burpee", "mountain climber", "high knee",
            "butt kick", "corrida", "caminhada", "jogging", "skip", "rope jump",
            "arm circle", "leg swing", "hip circle", "torso rotation", "rotação",
            "balanço", "aquecimento", "warm up", "warmup", "cardio leve"
        };

        // 🤸 MOBILITY - Palavras-chave de mobilidade
        var mobilityKeywords = new[]
        {
            "mobilidade", "mobility", "rotação", "rotation", "círculo", "circle",
            "world's greatest stretch", "cat cow", "cat-cow", "thread the needle",
            "90/90", "hip opener", "shoulder dislocate", "band pull apart",
            "scapular", "escapular", "articular", "dinâmico", "dynamic"
        };

        // 🧘 COOLDOWN - Palavras-chave de alongamento
        var cooldownKeywords = new[]
        {
            "alongamento", "stretch", "stretching", "estático", "static",
            "relaxamento", "cooldown", "cool down", "foam roll", "liberação miofascial",
            "child's pose", "pigeon pose", "cobra stretch", "downward dog",
            "quadriceps stretch", "hamstring stretch", "calf stretch"
        };

        // 1️⃣ Verificar palavras-chave de COOLDOWN (prioridade se no final)
        if (includeCooldown && cooldownKeywords.Any(k => nameLower.Contains(k)))
        {
            return "cooldown";
        }

        // 2️⃣ Verificar palavras-chave de WARMUP (prioridade se no início)
        if (includeWarmup && warmupKeywords.Any(k => nameLower.Contains(k)))
        {
            return "warmup";
        }

        // 3️⃣ Verificar palavras-chave de MOBILITY
        if (includeMobility && mobilityKeywords.Any(k => nameLower.Contains(k)))
        {
            return "mobility";
        }

        // 4️⃣ Classificação por POSIÇÃO (fallback)
        // Se incluir warmup: primeiros 2-3 exercícios
        if (includeWarmup && position < Math.Min(3, totalExercises / 3))
        {
            return "warmup";
        }

        // Se incluir cooldown: últimos 2-3 exercícios
        if (includeCooldown && position >= totalExercises - Math.Min(3, totalExercises / 3))
        {
            return "cooldown";
        }

        // Se incluir mobility: exercícios do meio (se não encaixaram em warmup/cooldown)
        if (includeMobility && position >= 2 && position < totalExercises - 2)
        {
            // Verificar se é um exercício de baixa intensidade (poucos sets)
            // Exercícios de mobilidade geralmente têm 1-2 sets
            // (Não temos acesso ao sets aqui, então usar apenas posição)
            // Marcar alguns do meio como mobility se a opção estiver ativa
            if (position == 2 || position == 3)
            {
                return "mobility";
            }
        }

        // 5️⃣ Padrão: exercício principal
        return "main";
    }

    /// <summary>
    /// Converte IFormFile para base64 string
    /// </summary>
    private static async Task<string> ConvertToBase64(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Analisa postura usando GPT-4 Vision
    /// </summary>
    private static async Task<GymHero.Shared.DTOs.PosturalAnalysisResponse> AnalyzePostureWithVision(
        string frontBase64,
        string sideBase64,
        string backBase64,
        string apiKey,
        ILogger logger)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        httpClient.Timeout = TimeSpan.FromSeconds(60);

        var systemPrompt = @"Você é um fisioterapeuta especializado em avaliação postural.

Analise as 3 fotos fornecidas (frente, lateral, costas) e classifique os seguintes desvios posturais.

IMPORTANTE: Retorne APENAS um JSON válido com esta estrutura EXATA (sem markdown, sem ```json, apenas o JSON puro):

{
  ""forwardHead"": ""None"",
  ""roundedShoulders"": ""None"",
  ""anteriorPelvicTilt"": ""None"",
  ""posteriorPelvicTilt"": ""None"",
  ""kneeValgus"": ""None"",
  ""kneeVarus"": ""None"",
  ""scoliosis"": ""None"",
  ""flatFeet"": ""None"",
  ""observations"": ""Descreva aqui os principais achados posturais observados nas fotos. Seja específico sobre o que você viu em cada vista (frontal, lateral, posterior)."",
  ""recommendations"": ""Liste 3-5 exercícios corretivos específicos baseados nos desvios encontrados. Inclua o nome do exercício e seu objetivo.""
}

VALORES PERMITIDOS para cada desvio:
- ""None"": Alinhamento normal, sem desvios significativos
- ""Mild"": Desvio leve, observável mas não crítico
- ""Moderate"": Desvio moderado, requer atenção e correção
- ""Severe"": Desvio severo, necessita intervenção imediata

CRITÉRIOS DE AVALIAÇÃO:

1. FORWARD HEAD (Cabeça Anteriorizada) - Vista Lateral:
   - None: Orelha alinhada com ombro
   - Mild: Orelha 1-2cm à frente do ombro
   - Moderate: Orelha 2-4cm à frente do ombro
   - Severe: Orelha >4cm à frente do ombro

2. ROUNDED SHOULDERS (Ombros Protusos) - Vista Lateral:
   - None: Ombros alinhados sobre quadril
   - Mild: Ombros levemente à frente do alinhamento
   - Moderate: Ombros visivelmente projetados para frente
   - Severe: Ombros muito anteriorizados com escápulas aladas

3. ANTERIOR/POSTERIOR PELVIC TILT (Inclinação Pélvica) - Vista Lateral:
   - Anterior: Lordose lombar aumentada, bumbum empinado
   - Posterior: Lombar retificada, bumbum ""escondido""
   - None: Curvatura lombar natural

4. KNEE VALGUS (Joelhos em X) - Vista Frontal:
   - None: Joelhos alinhados com tornozelos e quadril
   - Mild: Joelhos levemente para dentro
   - Moderate: Joelhos visivelmente em X
   - Severe: Joelhos muito aproximados, tornozelos afastados

5. KNEE VARUS (Joelhos em Parênteses) - Vista Frontal:
   - None: Joelhos alinhados
   - Mild: Pequeno arqueamento
   - Moderate: Arqueamento visível
   - Severe: Pernas muito arqueadas

6. SCOLIOSIS (Escoliose) - Vista Posterior:
   - None: Coluna reta, ombros e quadril nivelados
   - Mild: Leve curvatura lateral, ombros levemente desnivelados
   - Moderate: Curvatura visível, assimetria clara
   - Severe: Curvatura pronunciada, rotação vertebral

7. FLAT FEET (Pés Planos) - Vista Posterior/Lateral:
   - None: Arco plantar visível
   - Mild: Arco reduzido
   - Moderate: Arco muito reduzido
   - Severe: Sem arco, pé completamente plano

SEJA CONSERVADOR: Em caso de dúvida entre duas classificações, escolha a menos severa.
SEJA ESPECÍFICO: Nas observações, mencione exatamente o que você viu e em qual vista (frontal/lateral/posterior).";

        var requestBody = new
        {
            model = "gpt-4o",
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = (object)systemPrompt
                },
                new
                {
                    role = "user",
                    content = (object)new object[]
                    {
                        new { type = "text", text = "Analise as 3 fotos posturais abaixo e retorne o JSON com a avaliação completa:" },
                        new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{frontBase64}", detail = "high" } },
                        new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{sideBase64}", detail = "high" } },
                        new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{backBase64}", detail = "high" } }
                    }
                }
            },
            max_tokens = 1500,
            temperature = 0.3,
            response_format = new { type = "json_object" }
        };

        logger.LogInformation("Sending request to OpenAI Vision API...");
        var response = await httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogError($"OpenAI API error: {response.StatusCode} - {errorContent}");
            throw new Exception($"OpenAI API error: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<GymHero.Shared.DTOs.OpenAIVisionResponse>();
        if (result?.Choices == null || !result.Choices.Any())
        {
            throw new Exception("OpenAI returned empty response");
        }

        var content = result.Choices[0].Message.Content;
        logger.LogInformation($"Received response from OpenAI: {content.Substring(0, Math.Min(200, content.Length))}...");

        // Parse JSON response
        var analysis = JsonSerializer.Deserialize<GymHero.Shared.DTOs.PosturalAnalysisResponse>(
            content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (analysis == null)
        {
            throw new Exception("Failed to parse AI response");
        }

        return analysis;
    }

    // ✅ Helper methods to map strings to enums
    private static MuscleGroup ParseMuscleGroup(string? muscleGroup)
    {
        if (string.IsNullOrWhiteSpace(muscleGroup)) return MuscleGroup.FullBody;

        return muscleGroup.ToLower().Trim() switch
        {
            "chest" or "peito" or "peitoral" => MuscleGroup.Chest,
            "back" or "costas" or "dorsais" => MuscleGroup.Back,
            "shoulders" or "ombros" or "deltoides" => MuscleGroup.Shoulders,
            "biceps" or "bíceps" => MuscleGroup.Biceps,
            "triceps" or "tríceps" => MuscleGroup.Triceps,
            "forearms" or "antebraços" => MuscleGroup.Forearms,
            "core" or "abdomen" or "abdômen" or "abs" => MuscleGroup.Core,
            "quadriceps" or "quadríceps" or "coxa frontal" => MuscleGroup.Quadriceps,
            "hamstrings" or "isquiotibiais" or "posterior de coxa" => MuscleGroup.Hamstrings,
            "glutes" or "glúteos" or "gluteos" => MuscleGroup.Glutes,
            "calves" or "panturrilhas" => MuscleGroup.Calves,
            "adductors" or "adutores" => MuscleGroup.Adductors,
            "abductors" or "abdutores" => MuscleGroup.Abductors,
            "full body" or "corpo inteiro" or "full-body" => MuscleGroup.FullBody,
            "neck" or "pescoço" => MuscleGroup.Neck,
            "lower back" or "lombar" or "lower-back" => MuscleGroup.LowerBack,
            "cardio" or "cardiovascular" => MuscleGroup.Cardio,
            _ => MuscleGroup.FullBody
        };
    }

    private static Equipment ParseEquipment(string? equipment)
    {
        if (string.IsNullOrWhiteSpace(equipment)) return Equipment.None;

        return equipment.ToLower().Trim() switch
        {
            "none" or "nenhum" or "body only" or "bodyweight" or "peso corporal" => Equipment.None,
            "barbell" or "barra" or "barra livre" => Equipment.Barbell,
            "dumbbell" or "dumbbells" or "halter" or "halteres" => Equipment.Dumbbell,
            "kettlebell" or "kettlebells" => Equipment.Kettlebell,
            "cable" or "cable machine" or "cabo" or "polia" => Equipment.CableMachine,
            "machine" or "máquina" or "maquina" => Equipment.Machine,
            "resistance band" or "resistance bands" or "faixa elástica" or "faixa" => Equipment.ResistanceBand,
            "pull-up bar" or "barra fixa" => Equipment.PullUpBar,
            "bench" or "banco" => Equipment.Bench,
            "medicine ball" or "bola medicinal" => Equipment.MedicineBall,
            "stability ball" or "swiss ball" or "bola suíça" => Equipment.SwissBall,
            "foam roller" => Equipment.FoamRoller,
            "jump rope" or "corda" or "corda de pular" => Equipment.JumpRope,
            "box" or "caixa" => Equipment.Box,
            "trx" or "suspension trainer" => Equipment.TRX,
            "battle rope" or "battle ropes" or "corda naval" => Equipment.BattleRopes,
            "sled" or "trenó" => Equipment.SledProwler,
            "rowing machine" or "remo" => Equipment.RowingMachine,
            "treadmill" or "esteira" => Equipment.Treadmill,
            "bike" or "bicicleta" or "bike ergométrica" => Equipment.Bike,
            "elliptical" or "elíptico" => Equipment.Elliptical,
            "assault bike" => Equipment.AssaultBike,
            _ => Equipment.None
        };
    }

    private static ExerciseCategory ParseCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category)) return ExerciseCategory.Strength;

        return category.ToLower().Trim() switch
        {
            "strength" or "força" or "forca" => ExerciseCategory.Strength,
            "hypertrophy" or "hipertrofia" => ExerciseCategory.Hypertrophy,
            "power" or "potência" or "potencia" => ExerciseCategory.Power,
            "endurance" or "resistência" or "resistencia" => ExerciseCategory.Endurance,
            "cardio" or "cardiovascular" => ExerciseCategory.Cardio,
            "hiit" => ExerciseCategory.HIIT,
            "functional" or "funcional" => ExerciseCategory.Functional,
            "olympic" or "olímpico" or "olimpico" or "olympic lifting" => ExerciseCategory.OlympicLifting,
            "powerlifting" => ExerciseCategory.Powerlifting,
            "calisthenics" or "calistenia" => ExerciseCategory.Calisthenics,
            "plyometric" or "plyometrics" or "pliométrico" or "pliometrico" or "pliometria" => ExerciseCategory.Plyometrics,
            "isolation" or "isolamento" => ExerciseCategory.Isolation,
            "compound" or "composto" => ExerciseCategory.Compound,
            "stretching" or "alongamento" => ExerciseCategory.Stretching,
            "warmup" or "warm-up" or "aquecimento" => ExerciseCategory.WarmUp,
            "cooldown" or "cool-down" or "desaquecimento" => ExerciseCategory.CoolDown,
            "mobility" or "mobilidade" => ExerciseCategory.Mobility,
            "flexibility" or "flexibilidade" => ExerciseCategory.Flexibility,
            "balance" or "equilíbrio" or "equilibrio" => ExerciseCategory.Balance,
            "stability" or "estabilidade" or "estabilização" => ExerciseCategory.Stability,
            "rehabilitation" or "reabilitação" or "reabilitacao" => ExerciseCategory.Rehabilitation,
            "posture" or "posture correction" or "postura" or "correção postural" => ExerciseCategory.PostureCorrection,
            "isometric" or "isométrico" => ExerciseCategory.Isometric,
            _ => ExerciseCategory.Strength
        };
    }
}
