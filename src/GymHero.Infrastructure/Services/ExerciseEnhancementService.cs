using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Infrastructure.ExternalApi;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace GymHero.Infrastructure.Services;

public class ExerciseEnhancementService
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string? _geminiApiKey;

    public ExerciseEnhancementService(
        IApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _geminiApiKey = configuration["GoogleAI:ApiKey"];
    }

    public async Task<EnhancementResult> EnhanceAllExercisesAsync(CancellationToken cancellationToken = default)
    {
        var result = new EnhancementResult();
        var exercises = await _context.Exercises.ToListAsync(cancellationToken);
        var client = _httpClientFactory.CreateClient();

        foreach (var exercise in exercises)
        {
            try
            {
                var wasEnhanced = false;

                // 1. Traduzir nome se estiver em inglês
                if (NeedsTranslation(exercise.Name))
                {
                    var translatedName = await TranslateExerciseName(exercise.Name, client, cancellationToken);
                    if (!string.IsNullOrEmpty(translatedName))
                    {
                        exercise.Name = translatedName;
                        wasEnhanced = true;
                    }
                }

                // 2. Adicionar imagem se não tiver
                if (string.IsNullOrEmpty(exercise.ImageUrl))
                {
                    var imageUrl = await FindImageForExercise(exercise.Name, client, cancellationToken);
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        exercise.ImageUrl = imageUrl;
                        wasEnhanced = true;
                    }
                }

                // 3. Adicionar vídeo se não tiver
                if (string.IsNullOrEmpty(exercise.VideoUrl))
                {
                    var videoUrl = GetYouTubeVideoForExercise(exercise.Name);
                    if (!string.IsNullOrEmpty(videoUrl))
                    {
                        exercise.VideoUrl = videoUrl;
                        wasEnhanced = true;
                    }
                }

                // 4. Melhorar descrição se for genérica
                if (IsGenericDescription(exercise.Description))
                {
                    var enhancedDescription = await GenerateDetailedDescription(exercise, client, cancellationToken);
                    if (!string.IsNullOrEmpty(enhancedDescription))
                    {
                        exercise.Description = enhancedDescription;
                        exercise.Notes = enhancedDescription; // Também atualiza Notes
                        wasEnhanced = true;
                    }
                }

                if (wasEnhanced)
                {
                    result.EnhancedCount++;
                }
                else
                {
                    result.SkippedCount++;
                }

                // Salvar a cada 10 exercícios para evitar perda em caso de erro
                if ((result.EnhancedCount + result.SkippedCount) % 10 == 0)
                {
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"{exercise.Name}: {ex.Message}");
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return result;
    }

    private bool NeedsTranslation(string name)
    {
        // Verifica se o nome contém palavras em inglês comuns
        var englishKeywords = new[] {
            "push", "pull", "squat", "press", "curl", "row", "raise",
            "fly", "dip", "crunch", "plank", "lunge", "deadlift", "bench"
        };

        return englishKeywords.Any(keyword => name.ToLower().Contains(keyword));
    }

    private bool IsGenericDescription(string? description)
    {
        if (string.IsNullOrEmpty(description)) return true;

        // Verifica se é a descrição genérica comum
        var genericPhrases = new[] {
            "Execute o movimento com técnica correta",
            "Mantenha o controle durante toda a amplitude",
            "Respire adequadamente"
        };

        return genericPhrases.Any(phrase => description.Contains(phrase));
    }

    private async Task<string?> TranslateExerciseName(string englishName, HttpClient client, CancellationToken ct)
    {
        // Primeiro, tenta um mapeamento manual para exercícios comuns
        var translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Push-up", "Flexão de Braço" },
            { "Push up", "Flexão de Braço" },
            { "Pushup", "Flexão de Braço" },
            { "Pull-up", "Barra Fixa" },
            { "Pull up", "Barra Fixa" },
            { "Squat", "Agachamento" },
            { "Bench Press", "Supino Reto" },
            { "Deadlift", "Levantamento Terra" },
            { "Plank", "Prancha" },
            { "Crunch", "Abdominal" },
            { "Lunge", "Afundo" },
            { "Dip", "Mergulho" },
            { "Bicep Curl", "Rosca Bíceps" },
            { "Shoulder Press", "Desenvolvimento de Ombros" },
            { "Lat Pulldown", "Puxada Alta" },
            { "Leg Press", "Leg Press" },
            { "Calf Raise", "Elevação de Panturrilha" },
            { "Mountain Climbers", "Escalador" },
            { "Burpee", "Burpee" },
            { "Jump Rope", "Pular Corda" },
            { "Face Pull", "Puxada para o Rosto" }
        };

        if (translations.TryGetValue(englishName, out var translation))
        {
            return translation;
        }

        // Se não encontrou no mapeamento, usa AI para traduzir
        if (!string.IsNullOrEmpty(_geminiApiKey))
        {
            try
            {
                return await TranslateWithGemini(englishName, client, ct);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private async Task<string?> TranslateWithGemini(string text, HttpClient client, CancellationToken ct)
    {
        var prompt = $"Traduza o nome deste exercício para português brasileiro (apenas o nome, sem explicações): {text}";
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_geminiApiKey}",
            content,
            ct
        );

        if (!response.IsSuccessStatusCode) return null;

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        dynamic? result = JsonConvert.DeserializeObject(responseJson);

        return result?.candidates?[0]?.content?.parts?[0]?.text?.ToString()?.Trim();
    }

    private async Task<string?> FindImageForExercise(string exerciseName, HttpClient client, CancellationToken ct)
    {
        try
        {
            // Tenta buscar imagem da API Wger baseado no nome
            var searchUrl = $"https://wger.de/api/v2/exercise/?name={Uri.EscapeDataString(exerciseName)}&limit=1";
            var response = await client.GetAsync(searchUrl, ct);

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            var apiResult = JsonConvert.DeserializeObject<WgerApiResult<dynamic>>(json);

            if (apiResult?.Results?.Any() == true)
            {
                var exerciseId = apiResult.Results[0].id;
                var imageUrl = $"https://wger.de/api/v2/exerciseimage/?exercise={exerciseId}&is_main=True";
                var imageResponse = await client.GetAsync(imageUrl, ct);

                if (imageResponse.IsSuccessStatusCode)
                {
                    var imageJson = await imageResponse.Content.ReadAsStringAsync(ct);
                    var imageResult = JsonConvert.DeserializeObject<WgerApiResult<ImageInfo>>(imageJson);
                    return imageResult?.Results?.FirstOrDefault()?.ImageUrl;
                }
            }
        }
        catch
        {
            // Se falhar, retorna null
        }

        return null;
    }

    private string? GetYouTubeVideoForExercise(string exerciseName)
    {
        // Mapeamento manual de vídeos do YouTube para exercícios comuns
        var videoMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Flexão de Braço", "https://www.youtube.com/watch?v=IODxDxX7oi4" },
            { "Barra Fixa", "https://www.youtube.com/watch?v=eGo4IYlbE5g" },
            { "Agachamento", "https://www.youtube.com/watch?v=aclHkVaku9U" },
            { "Supino Reto", "https://www.youtube.com/watch?v=rT7DgCr-3pg" },
            { "Levantamento Terra", "https://www.youtube.com/watch?v=op9kVnSso6Q" },
            { "Prancha", "https://www.youtube.com/watch?v=ASdvN_XEl_c" },
            { "Abdominal", "https://www.youtube.com/watch?v=Xyd_fa5zoEU" },
            { "Afundo", "https://www.youtube.com/watch?v=QOVaHwm-Q6U" },
            { "Mergulho", "https://www.youtube.com/watch?v=2z8JmcrW-As" },
            { "Rosca Bíceps", "https://www.youtube.com/watch?v=ykJmrZ5v0Oo" },
            { "Desenvolvimento de Ombros", "https://www.youtube.com/watch?v=qEwKCR5JCog" },
            { "Burpee", "https://www.youtube.com/watch?v=auBLPXO8Fww" },
            { "Escalador", "https://www.youtube.com/watch?v=nmwgirgXLYM" },
            { "Pular Corda", "https://www.youtube.com/watch?v=FJmRQ5iTXKE" }
        };

        if (videoMap.TryGetValue(exerciseName, out var videoUrl))
        {
            return videoUrl;
        }

        return null;
    }

    private async Task<string?> GenerateDetailedDescription(Exercise exercise, HttpClient client, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(_geminiApiKey)) return null;

        try
        {
            var prompt = $@"Gere uma descrição detalhada em português brasileiro para o exercício '{exercise.Name}'.
Grupo muscular: {exercise.MuscleGroup}
Equipamento: {exercise.Equipment}

A descrição deve incluir:
1. Como executar o exercício (passo a passo)
2. Músculos trabalhados
3. Dicas de forma/técnica
4. Benefícios do exercício

Seja específico e detalhado (máximo 300 palavras).";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_geminiApiKey}",
                content,
                ct
            );

            if (!response.IsSuccessStatusCode) return null;

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            dynamic? result = JsonConvert.DeserializeObject(responseJson);

            return result?.candidates?[0]?.content?.parts?[0]?.text?.ToString()?.Trim();
        }
        catch
        {
            return null;
        }
    }
}

public class EnhancementResult
{
    public int EnhancedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
