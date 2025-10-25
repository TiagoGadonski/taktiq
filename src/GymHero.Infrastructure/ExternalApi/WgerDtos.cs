using Newtonsoft.Json;

namespace GymHero.Infrastructure.ExternalApi;

// Estrutura genérica para as respostas da API da Wger
public class WgerApiResult<T>
{
    [JsonProperty("results")]
    public List<T> Results { get; set; } = new();
    
    [JsonProperty("next")]
    public string? NextPageUrl { get; set; }
}

// Detalhes de um exercício da Wger
public class ExerciseInfo
{
    [JsonProperty("id")]
    public int WgerId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = "";

    [JsonProperty("description")]
    public string Description { get; set; } = "";

    [JsonProperty("muscles")]
    public List<IdNamePair> PrimaryMuscles { get; set; } = new();

    [JsonProperty("equipment")]
    public List<IdNamePair> Equipment { get; set; } = new();
}

// Detalhes de uma imagem da Wger
public class ImageInfo
{
    [JsonProperty("image")]
    public string ImageUrl { get; set; } = "";
}

// Classe genérica para objetos que têm "id" e "name" na API da Wger
public class IdNamePair
{
    [JsonProperty("name")]
    public string Name { get; set; } = "";
}