using System.ComponentModel.DataAnnotations;

namespace GymHero.Shared.DTOs;

// DTO para criar um novo desafio
public record CreateChallengeRequest(
    string Title,
    string Type, // Ex: "TOTAL_VOLUME", "SESSION_FREQUENCY"
    double TargetValue,
    DateTime StartDate,
    DateTime EndDate
);

// DTO para exibir um desafio e o seu progresso
public class CreateCustomChallengeRequest
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    public string Title { get; set; } = "";
    
    public string Type { get; set; } = "TOTAL_VOLUME"; // Valor padrão

    [Range(1, double.MaxValue, ErrorMessage = "A meta deve ser maior que zero.")]
    public double TargetValue { get; set; } = 1000;

    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(30);

    public List<Guid> FriendIds { get; set; } = new();
}

// O DTO de resposta pode continuar a ser um record
public record ChallengeResponse(
    Guid Id,
    string Title,
    string Type,
    double TargetValue,
    double CurrentValue,
    string Status,
    DateTime StartDate,
    DateTime EndDate
);