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
/// DTO para atualizar notas sobre um cliente.
/// </summary>
public record UpdateClientNotesRequest(
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
/// DTO para resumo de aluno (para exibição no perfil público do PT).
/// </summary>
public record StudentSummaryDto(
    string? ProfilePictureUrl
);

/// <summary>
/// DTO para enviar solicitação de Personal Trainer.
/// </summary>
public record SendPTRequestRequest(
    string? Message
);

/// <summary>
/// DTO para responder solicitação de Personal Trainer.
/// </summary>
public record RespondToPTRequestRequest(
    bool Accepted
);

/// <summary>
/// DTO para resposta de solicitação de Personal Trainer.
/// </summary>
public record PTRequestResponse(
    Guid Id,
    Guid TrainerId,
    string TrainerName,
    string? TrainerProfilePicture,
    string? Message,
    string Status,
    DateTime CreatedAt
);

/// <summary>
/// DTO para atribuir plano a múltiplos alunos.
/// </summary>
public record BulkAssignPlanRequest(
    IEnumerable<Guid> StudentIds,
    string PlanName,
    string? Goal,
    Guid? TemplatePlanId,
    DateTime? ExpirationDate
);

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
    int StudentCount,
    IEnumerable<StudentSummaryDto> RecentStudents
);

// ===== Student Groups DTOs =====

/// <summary>
/// DTO para criar um novo grupo de alunos.
/// </summary>
public record CreateStudentGroupRequest(
    [Required(ErrorMessage = "O nome do grupo é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    string Name,

    string? Description,

    string? Tags
);

/// <summary>
/// DTO para atualizar um grupo de alunos.
/// </summary>
public record UpdateStudentGroupRequest(
    [Required(ErrorMessage = "O nome do grupo é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    string Name,

    string? Description,

    string? Tags
);

/// <summary>
/// DTO para adicionar alunos a um grupo.
/// </summary>
public record AddStudentsToGroupRequest(
    [Required(ErrorMessage = "IDs dos alunos são obrigatórios.")]
    [MinLength(1, ErrorMessage = "Pelo menos um aluno deve ser selecionado.")]
    IEnumerable<Guid> StudentIds
);

/// <summary>
/// DTO para resumo de membro do grupo.
/// </summary>
public record GroupMemberSummary(
    Guid Id,
    string Name,
    string Email,
    string? ProfilePictureUrl,
    DateTime AddedAt
);

/// <summary>
/// DTO para resposta de grupo de alunos.
/// </summary>
public record StudentGroupResponse(
    Guid Id,
    string Name,
    string? Description,
    string? Tags,
    int MemberCount,
    DateTime CreatedAt
);

/// <summary>
/// DTO para resposta detalhada de grupo de alunos (com membros).
/// </summary>
public record StudentGroupDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    string? Tags,
    int MemberCount,
    DateTime CreatedAt,
    IEnumerable<GroupMemberSummary> Members
);

/// <summary>
/// DTO para atribuir plano a um grupo.
/// </summary>
public record AssignPlanToGroupRequest(
    [Required(ErrorMessage = "Nome do plano é obrigatório.")]
    string PlanName,

    string? Goal,

    Guid? TemplatePlanId,

    DateTime? ExpirationDate
);