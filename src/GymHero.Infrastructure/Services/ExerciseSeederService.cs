using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Infrastructure.ExternalApi;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace GymHero.Infrastructure.Services;

public class ExerciseSeederService
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public ExerciseSeederService(IApplicationDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    public async Task SeedExercisesAsync(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();

        // Buscar exercícios em português e inglês para ter mais variedade
        var languages = new[] { "pt", "en" };
        int totalAdded = 0;
        int totalSkipped = 0;

        foreach (var language in languages)
        {
            string? nextUrl = $"https://wger.de/api/v2/exerciseinfo/?language={language}&limit=200";

            while (!string.IsNullOrEmpty(nextUrl))
            {
                var response = await client.GetAsync(nextUrl, cancellationToken);
                if (!response.IsSuccessStatusCode) break;

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var apiResult = JsonConvert.DeserializeObject<WgerApiResult<ExerciseInfo>>(json);

                if (apiResult is null || !apiResult.Results.Any()) break;

                foreach (var exerciseInfo in apiResult.Results)
                {
                    if (await _context.Exercises.AnyAsync(e => e.Name == exerciseInfo.Name, cancellationToken))
                    {
                        totalSkipped++;
                        continue;
                    }

                    // --- INÍCIO DA NOVA LÓGICA DE BUSCA DE IMAGEM ---
                    string? imageUrl = null;
                    var imageApiUrl = $"https://wger.de/api/v2/exerciseimage/?exercise_base={exerciseInfo.WgerId}&is_main=True";
                    var imageResponse = await client.GetAsync(imageApiUrl, cancellationToken);
                    if (imageResponse.IsSuccessStatusCode)
                    {
                        var imageJson = await imageResponse.Content.ReadAsStringAsync(cancellationToken);
                        var imageApiResult = JsonConvert.DeserializeObject<WgerApiResult<ImageInfo>>(imageJson);
                        imageUrl = imageApiResult?.Results.FirstOrDefault()?.ImageUrl;
                    }
                    // --- FIM DA NOVA LÓGICA ---

                    // Determinar WorkoutLocation baseado no equipamento
                    var workoutLocation = DetermineWorkoutLocation(exerciseInfo.Equipment.Select(e => e.Name).ToList());

                    var newExercise = new Exercise
                    {
                        Name = exerciseInfo.Name,
                        Description = SanitizeHtml(exerciseInfo.Description),
                        Notes = string.Empty,
                        MuscleGroup = exerciseInfo.PrimaryMuscles.FirstOrDefault()?.Name ?? "Geral",
                        Category = exerciseInfo.PrimaryMuscles.FirstOrDefault()?.Name ?? "Geral",
                        Equipment = string.Join(", ", exerciseInfo.Equipment.Select(e => e.Name)),
                        ImageUrl = imageUrl,
                        VideoUrl = null,
                        WorkoutLocation = workoutLocation
                    };

                    await _context.Exercises.AddAsync(newExercise, cancellationToken);
                    totalAdded++;
                }

                await _context.SaveChangesAsync(cancellationToken);
                nextUrl = apiResult.NextPageUrl;
            }
        }
    }

    private Domain.Enums.WorkoutLocation DetermineWorkoutLocation(List<string> equipmentList)
    {
        // Se não tem equipamento ou só tem "body weight", é Home
        if (!equipmentList.Any() || equipmentList.All(e =>
            e.ToLower().Contains("body weight") ||
            e.ToLower().Contains("bodyweight") ||
            e == "none"))
        {
            return Domain.Enums.WorkoutLocation.Home;
        }

        // Equipamentos que sugerem apenas academia
        var gymOnlyEquipment = new[]
        {
            "barbell", "ez barbell", "barra", "cable", "machine", "smith machine",
            "leg press", "hack squat", "lat pulldown", "cable machine", "pec deck",
            "leg curl", "leg extension", "chest press"
        };

        if (equipmentList.Any(e => gymOnlyEquipment.Any(g => e.ToLower().Contains(g))))
        {
            return Domain.Enums.WorkoutLocation.Gym;
        }

        // Equipamentos que podem ser usados em casa ou academia
        var bothEquipment = new[]
        {
            "dumbbell", "halter", "kettlebell", "bench", "pull-up bar", "barra fixa",
            "mat", "resistance band", "faixa", "jump rope", "corda"
        };

        if (equipmentList.Any(e => bothEquipment.Any(b => e.ToLower().Contains(b))))
        {
            return Domain.Enums.WorkoutLocation.Both;
        }

        // Por padrão, assume que pode ser feito em ambos
        return Domain.Enums.WorkoutLocation.Both;
    }
    
    private string SanitizeHtml(string html)
    {
        // Remove scripts e estilos, mas mantém tags de formatação básicas
        var sanitized = Regex.Replace(html, "<script.*?</script>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, "<style.*?</style>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        // Pode adicionar mais regras se necessário, mas por agora isto é suficiente
        return sanitized;
    }
}