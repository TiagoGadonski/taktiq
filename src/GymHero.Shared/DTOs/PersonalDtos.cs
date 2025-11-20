using System.ComponentModel.DataAnnotations;

namespace GymHero.Shared.DTOs;
public class AddClientRequest
{
    [Required(ErrorMessage = "O email do aluno é obrigatório.")]
    [EmailAddress(ErrorMessage = "Por favor, insira um email válido.")]
    public string ClientEmail { get; set; } = "";
}
public record ClientResponse(Guid Id, string Name, string Email);
public record ClientProgressDashboardResponse(
    IEnumerable<ProgressMetricResponse> BodyMetrics,
    IEnumerable<PersonalRecordResponse> PersonalRecords
);
public record GenerateWorkoutPlanRequest(
    string Goal, // Ex: "Hipertrofia", "Força", "Resistência"
    string Level, // Ex: "Iniciante", "Intermediário", "Avançado"
    int DaysPerWeek // Ex: 3, 4, 5
);

public record ProgressMetricResponse(
    Guid Id,
    string Type,
    double Value,
    string Unit,
    DateTime Date
);

// DTO para exibir um Recorde Pessoal
public record PersonalRecordResponse(
    Guid ExerciseId,
    string ExerciseName,
    int Reps,
    double MaxLoad,
    DateTime DateAchieved
);

/// <summary>
/// DTO para adicionar notas sobre um cliente.
/// </summary>
public record AddClientNotesRequest(
    string Notes
);

/// <summary>
/// DTO para criar um convite de aluno.
/// </summary>
public record CreateStudentInvitationRequest(
    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    string Email,

    string? Name,

    Guid? WorkoutPlanId
);

/// <summary>
/// DTO para atualizar perfil público do Personal Trainer.
/// </summary>
public record UpdatePersonalProfileRequest
{
    public string? ProfileSlug { get; set; }
    public string? Specialization { get; set; }
    public string? Education { get; set; }
    public string? Experience { get; set; }
    public string? PricingInfo { get; set; }
    public bool? IsPublicProfile { get; set; }
    public string? InstagramUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? WebsiteUrl { get; set; }
}

/// <summary>
/// DTO para resposta do perfil público do Personal Trainer.
/// </summary>
public record PublicPersonalProfileResponse(
    Guid Id,
    string Name,
    string? ProfileSlug,
    string? ProfilePictureUrl,
    string? Bio,
    string? Location,
    string? Specialization,
    string? Education,
    string? Experience,
    string? PricingInfo,
    string? InstagramUrl,
    string? FacebookUrl,
    string? WebsiteUrl,
    int StudentCount
);