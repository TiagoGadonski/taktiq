using System.ComponentModel.DataAnnotations;

namespace GymHero.Shared.DTOs;

// Enum para definir quem pode participar do desafio
public enum ChallengeTargetType
{
    AllUsers = 0,
    AllTrainers = 1,
    SpecificUsers = 2
}

// DTO para criar um novo desafio
public record CreateChallengeRequest(
    string Title,
    string Type, // Ex: "TOTAL_VOLUME", "SESSION_FREQUENCY"
    double TargetValue,
    DateTime StartDate,
    DateTime EndDate,
    ChallengeTargetType TargetType = ChallengeTargetType.SpecificUsers,
    bool IsDefault = false,
    string? IconName = null
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

    public ChallengeTargetType TargetType { get; set; } = ChallengeTargetType.SpecificUsers;

    public bool IsDefault { get; set; } = false;

    public string? IconName { get; set; }
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
    DateTime EndDate,
    ChallengeTargetType TargetType,
    bool IsDefault,
    string? IconName = null
);

// DTO para progresso de desafio
public class ChallengeProgressDto
{
    public Guid ParticipantId { get; set; }
    public double CurrentValue { get; set; }
    public DateTime LastUpdate { get; set; }
}

// DTO para desafio com informação de participação
public class ChallengeWithParticipationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double TargetValue { get; set; }
    public double CurrentValue { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ChallengeTargetType TargetType { get; set; }
    public bool IsDefault { get; set; }
    public bool IsParticipating { get; set; }
    public string? IconName { get; set; }
    public List<ChallengeProgressDto> Progresses { get; set; } = new();
}