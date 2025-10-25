using System.Text.Json;

namespace GymHero.Api.Services;

public interface IExerciseMediaService
{
    Task<string?> GetExerciseGifUrl(string exerciseName, string? bodyPart = null, string? equipment = null);
    Task<List<ExerciseMedia>> SearchExercises(string query);
}

public record ExerciseMedia(
    string Id,
    string Name,
    string Equipment,
    List<string> PrimaryMuscles,
    List<string>? SecondaryMuscles,
    List<string> Images,
    List<string>? Instructions,
    string? Category,
    string? Level
)
{
    public string? GifUrl => Images?.FirstOrDefault() != null
        ? $"https://raw.githubusercontent.com/yuhonas/free-exercise-db/main/exercises/{Images[0]}"
        : null;

    public string NamePt { get; init; } = string.Empty;
    public string EquipmentPt { get; init; } = string.Empty;
    public List<string> PrimaryMusclesPt { get; init; } = new();
};

public class ExerciseMediaService : IExerciseMediaService
{
    private static List<ExerciseMedia>? _cachedExercises;
    private static DateTime _cacheTime = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private const string ExerciseDbUrl = "https://raw.githubusercontent.com/yuhonas/free-exercise-db/main/dist/exercises.json";

    private readonly HttpClient _httpClient;
    private readonly ILogger<ExerciseMediaService> _logger;

    public ExerciseMediaService(HttpClient httpClient, ILogger<ExerciseMediaService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ExerciseMedia>> SearchExercises(string query)
    {
        var exercises = await GetAllExercisesAsync();

        if (string.IsNullOrWhiteSpace(query))
        {
            return exercises.Select(e => AddPortugueseTranslations(e)).ToList();
        }

        query = query.ToLower();

        // Translate common Portuguese terms to English
        var translatedQueries = TranslatePortugueseToEnglish(query);

        var results = exercises
            .Where(e =>
                translatedQueries.Any(q =>
                    (e.Name?.ToLower().Contains(q) ?? false) ||
                    (e.Equipment?.ToLower().Contains(q) ?? false) ||
                    (e.PrimaryMuscles?.Any(m => m?.ToLower().Contains(q) ?? false) ?? false) ||
                    (e.Category?.ToLower().Contains(q) ?? false)
                ))
            .ToList();

        // Add Portuguese translations to results
        return results.Select(e => AddPortugueseTranslations(e)).ToList();
    }

    private ExerciseMedia AddPortugueseTranslations(ExerciseMedia exercise)
    {
        return exercise with
        {
            NamePt = TranslateExerciseNameToPortuguese(exercise.Name),
            EquipmentPt = TranslateEquipmentToPortuguese(exercise.Equipment),
            PrimaryMusclesPt = exercise.PrimaryMuscles?.Select(TranslateMuscleToPortuguese).ToList() ?? new List<string>()
        };
    }

    private List<string> TranslatePortugueseToEnglish(string query)
    {
        var queries = new List<string> { query }; // Always include original query

        // Exercise name translations (PT-BR -> EN)
        var exerciseTranslations = new Dictionary<string, string>
        {
            // Common exercises
            { "supino", "bench press" },
            { "agachamento", "squat" },
            { "levantamento terra", "deadlift" },
            { "terra", "deadlift" },
            { "remada", "row" },
            { "puxada", "pull" },
            { "barra fixa", "pull up" },
            { "rosca", "curl" },
            { "desenvolvimento", "press" },
            { "crucifixo", "fly" },
            { "elevação", "raise" },
            { "extensão", "extension" },
            { "flexão", "curl" },
            { "tríceps", "triceps" },
            { "bíceps", "biceps" },
            { "mergulho", "dip" },
            { "prancha", "plank" },
            { "passada", "lunge" },
            { "afundo", "lunge" },
            { "stiff", "romanian deadlift" },
            { "panturrilha", "calf" },

            // Equipment translations
            { "barra", "barbell" },
            { "halter", "dumbbell" },
            { "halteres", "dumbbell" },
            { "cabo", "cable" },
            { "polia", "cable" },
            { "máquina", "machine" },
            { "peso livre", "free weight" },
            { "corpo", "body" },
            { "smith", "smith" },

            // Muscle group translations
            { "peito", "chest" },
            { "peitoral", "chest" },
            { "costas", "back" },
            { "dorsal", "back" },
            { "ombro", "shoulder" },
            { "ombros", "shoulder" },
            { "deltoide", "shoulder" },
            { "braço", "arm" },
            { "braços", "arm" },
            { "perna", "leg" },
            { "pernas", "leg" },
            { "coxa", "leg" },
            { "quadríceps", "quadriceps" },
            { "posterior", "hamstring" },
            { "glúteo", "glute" },
            { "abdômen", "abs" },
            { "abdominal", "abs" },  // Works for both exercise and muscle group
            { "core", "abs" },
            { "trapézio", "traps" },
            { "antebraço", "forearm" },
            { "lombar", "lower back" },
        };

        // Check for translations
        foreach (var translation in exerciseTranslations)
        {
            if (query.Contains(translation.Key))
            {
                queries.Add(translation.Value);
                // Also add the query with the Portuguese word replaced
                queries.Add(query.Replace(translation.Key, translation.Value));
            }
        }

        return queries.Distinct().ToList();
    }

    private string TranslateExerciseNameToPortuguese(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        var lowerName = name.ToLower();

        // Direct exercise name translations
        var translations = new Dictionary<string, string>
        {
            { "bench press", "Supino" },
            { "incline bench press", "Supino Inclinado" },
            { "decline bench press", "Supino Declinado" },
            { "dumbbell press", "Supino com Halteres" },
            { "squat", "Agachamento" },
            { "deadlift", "Levantamento Terra" },
            { "romanian deadlift", "Stiff" },
            { "barbell row", "Remada com Barra" },
            { "dumbbell row", "Remada com Halter" },
            { "pull up", "Barra Fixa" },
            { "pull-up", "Barra Fixa" },
            { "lat pulldown", "Puxada" },
            { "curl", "Rosca" },
            { "bicep curl", "Rosca Bíceps" },
            { "hammer curl", "Rosca Martelo" },
            { "triceps extension", "Extensão de Tríceps" },
            { "triceps pushdown", "Tríceps na Polia" },
            { "shoulder press", "Desenvolvimento" },
            { "overhead press", "Desenvolvimento" },
            { "military press", "Desenvolvimento Militar" },
            { "lateral raise", "Elevação Lateral" },
            { "front raise", "Elevação Frontal" },
            { "fly", "Crucifixo" },
            { "flyes", "Crucifixo" },
            { "dumbbell fly", "Crucifixo com Halteres" },
            { "cable fly", "Crucifixo no Cabo" },
            { "lunge", "Afundo" },
            { "leg press", "Leg Press" },
            { "leg extension", "Cadeira Extensora" },
            { "leg curl", "Mesa Flexora" },
            { "calf raise", "Elevação de Panturrilha" },
            { "dip", "Mergulho" },
            { "crunch", "Abdominal" },
            { "plank", "Prancha" },
            { "shrug", "Encolhimento" },
        };

        foreach (var translation in translations)
        {
            if (lowerName.Contains(translation.Key))
            {
                return name.Replace(translation.Key, translation.Value, StringComparison.OrdinalIgnoreCase);
            }
        }

        return name; // Return original if no translation found
    }

    private string TranslateEquipmentToPortuguese(string equipment)
    {
        if (string.IsNullOrEmpty(equipment)) return equipment;

        return equipment.ToLower() switch
        {
            "barbell" => "Barra",
            "dumbbell" => "Halteres",
            "cable" => "Cabo",
            "machine" => "Máquina",
            "body only" => "Peso Corporal",
            "body weight" => "Peso Corporal",
            "kettlebell" => "Kettlebell",
            "bands" => "Elásticos",
            "medicine ball" => "Bola Medicinal",
            "exercise ball" => "Bola Suíça",
            "foam roll" => "Rolo de Espuma",
            "e-z curl bar" => "Barra W",
            _ => equipment
        };
    }

    private string TranslateMuscleToPortuguese(string muscle)
    {
        if (string.IsNullOrEmpty(muscle)) return muscle;

        return muscle.ToLower() switch
        {
            "chest" => "Peito",
            "back" => "Costas",
            "shoulders" => "Ombros",
            "biceps" => "Bíceps",
            "triceps" => "Tríceps",
            "forearms" => "Antebraços",
            "abs" => "Abdômen",
            "abdominals" => "Abdominais",
            "quadriceps" => "Quadríceps",
            "hamstrings" => "Posterior",
            "calves" => "Panturrilhas",
            "glutes" => "Glúteos",
            "traps" => "Trapézio",
            "lats" => "Dorsais",
            "middle back" => "Costas (Meio)",
            "lower back" => "Lombar",
            "neck" => "Pescoço",
            "adductors" => "Adutores",
            "abductors" => "Abdutores",
            _ => muscle
        };
    }

    public async Task<string?> GetExerciseGifUrl(string exerciseName, string? bodyPart = null, string? equipment = null)
    {
        try
        {
            var exercises = await GetAllExercisesAsync();

            // Try exact name match first
            var exercise = exercises.FirstOrDefault(e =>
                e.Name.Equals(exerciseName, StringComparison.OrdinalIgnoreCase));

            // If not found, try fuzzy match with equipment and muscles
            if (exercise == null && equipment != null)
            {
                exercise = exercises.FirstOrDefault(e =>
                    e.Name.Contains(exerciseName, StringComparison.OrdinalIgnoreCase) &&
                    e.Equipment.Equals(equipment, StringComparison.OrdinalIgnoreCase));
            }

            // If not found, try fuzzy match with primary muscles
            if (exercise == null && bodyPart != null)
            {
                exercise = exercises.FirstOrDefault(e =>
                    e.Name.Contains(exerciseName, StringComparison.OrdinalIgnoreCase) &&
                    e.PrimaryMuscles.Any(m => m.Equals(bodyPart, StringComparison.OrdinalIgnoreCase)));
            }

            // If still not found, try just name similarity
            if (exercise == null)
            {
                exercise = exercises.FirstOrDefault(e =>
                    e.Name.Contains(exerciseName, StringComparison.OrdinalIgnoreCase));
            }

            _logger.LogInformation("Exercise lookup: '{Name}' -> Found: {Found}, GIF: {Gif}",
                exerciseName, exercise != null, exercise?.GifUrl ?? "null");

            return exercise?.GifUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching exercise GIF for {ExerciseName}", exerciseName);
            return null;
        }
    }

    private async Task<List<ExerciseMedia>> GetAllExercisesAsync()
    {
        // Return cached data if still valid
        if (_cachedExercises != null && DateTime.UtcNow - _cacheTime < CacheDuration)
        {
            return _cachedExercises;
        }

        try
        {
            _logger.LogInformation("Fetching exercise database from GitHub...");
            var response = await _httpClient.GetStringAsync(ExerciseDbUrl);
            var exercises = JsonSerializer.Deserialize<List<ExerciseMedia>>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (exercises != null && exercises.Count > 0)
            {
                _cachedExercises = exercises;
                _cacheTime = DateTime.UtcNow;
                _logger.LogInformation("Successfully loaded {Count} exercises", exercises.Count);
                return exercises;
            }

            _logger.LogWarning("No exercises loaded from database");
            return _cachedExercises ?? new List<ExerciseMedia>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading exercise database");
            return _cachedExercises ?? new List<ExerciseMedia>();
        }
    }
}
