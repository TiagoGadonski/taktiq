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
        string? nextUrl = "https://wger.de/api/v2/exerciseinfo/?language=pt&limit=100";

        while (!string.IsNullOrEmpty(nextUrl))
        {
            var response = await client.GetAsync(nextUrl, cancellationToken);
            if (!response.IsSuccessStatusCode) break;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResult = JsonConvert.DeserializeObject<WgerApiResult<ExerciseInfo>>(json);

            if (apiResult is null || !apiResult.Results.Any()) break;

            foreach (var exerciseInfo in apiResult.Results)
            {
                if (await _context.Exercises.AnyAsync(e => e.Name == exerciseInfo.Name, cancellationToken)) continue;

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

                var newExercise = new Exercise
                {
                    Name = exerciseInfo.Name,
                    Notes = SanitizeHtml(exerciseInfo.Description), // Guardamos o HTML semi-limpo
                    MuscleGroup = exerciseInfo.PrimaryMuscles.FirstOrDefault()?.Name ?? "Geral",
                    Category = exerciseInfo.PrimaryMuscles.FirstOrDefault()?.Name ?? "Geral",
                    Equipment = string.Join(", ", exerciseInfo.Equipment.Select(e => e.Name)),
                    ImageUrl = imageUrl // Guardamos o URL da imagem
                };
                
                await _context.Exercises.AddAsync(newExercise, cancellationToken);
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            nextUrl = apiResult.NextPageUrl;
        }
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