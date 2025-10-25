using System.ComponentModel.DataAnnotations;

namespace GymHero.Shared.DTOs;

// Alteramos para uma 'class' para garantir que as propriedades são mutáveis (get; set;)
public class UpdateProfileRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Por favor, insira um email válido")]
    public string Email { get; set; } = "";

    public DateTime? DateOfBirth { get; set; }
    public string? Location { get; set; }
    public string? Bio { get; set; }
    public double? Height { get; set; }
    public double? Weight { get; set; }
    public string? GymName { get; set; }
    public string? PhoneNumber { get; set; }
}

// O DTO de resposta pode continuar a ser um record, pois não o modificamos.
public record UserProfileResponse(
    string Name,
    string Email,
    string Role,
    DateTime? DateOfBirth,
    string? Location,
    string? Bio,
    double? Height,
    double? Weight,
    string? ProfilePictureUrl,
    string? GymName,
    string? PhoneNumber
);

public record PublicProfileResponse(
    Guid Id,
    string Name,
    string? Location,
    string? Bio,
    string? Email,
    string? ProfilePictureUrl,
    string? GymName,
    string? PhoneNumber,
    List<WorkoutSummary>? RecentWorkouts,
    List<CompletedChallengeDto>? CompletedChallenges
);

public record WorkoutSummary(
    Guid Id,
    string PlanName,
    DateTime CompletedAt
);

public record CompletedChallengeDto(
    Guid ChallengeId,
    string Title,
    string Type,
    double TargetValue,
    double CurrentValue,
    DateTime CompletedAt
);