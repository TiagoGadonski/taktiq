namespace GymHero.Shared.DTOs;

// DTO para campos customizados
public record CustomFieldDto(
    string FieldName,
    string FieldValue,
    string? FieldType  // "text", "number", "select"
);

// Request para criar nova avaliação
public record CreateAssessmentRequest(
    Guid StudentId,
    string AssessmentType,  // "Postural", "Physical", "Neuromotor", "Custom"

    // Avaliação Postural (opcional)
    string? ForwardHead,
    string? RoundedShoulders,
    string? AnteriorPelvicTilt,
    string? PosteriorPelvicTilt,
    string? KneeValgus,
    string? KneeVarus,
    string? FlatFeet,
    string? Scoliosis,

    // Avaliação Física (opcional)
    double? BodyFatPercentage,
    double? MuscleMass,
    double? FlexibilityScore,
    double? StrengthScore,
    double? CardioScore,

    // Campos customizados
    List<CustomFieldDto>? CustomFields,

    // Notas do PT
    string? TrainerNotes
);

// Request para atualizar avaliação existente
public record UpdateAssessmentRequest(
    string AssessmentType,

    // Avaliação Postural
    string? ForwardHead,
    string? RoundedShoulders,
    string? AnteriorPelvicTilt,
    string? PosteriorPelvicTilt,
    string? KneeValgus,
    string? KneeVarus,
    string? FlatFeet,
    string? Scoliosis,

    // Avaliação Física
    double? BodyFatPercentage,
    double? MuscleMass,
    double? FlexibilityScore,
    double? StrengthScore,
    double? CardioScore,

    // Campos customizados
    List<CustomFieldDto>? CustomFields,

    // Notas do PT
    string? TrainerNotes
);

// Response resumido para lista de avaliações
public record AssessmentResponse(
    Guid Id,
    Guid StudentId,
    string StudentName,
    string StudentEmail,
    string AssessmentType,
    DateTime AssessmentDate,
    bool IsActive,
    string? Summary  // Resumo dos principais achados
);

// Response detalhado com todos os campos
public record AssessmentDetailResponse(
    Guid Id,
    Guid StudentId,
    string StudentName,
    string StudentEmail,
    Guid TrainerId,
    string TrainerName,
    string AssessmentType,
    DateTime AssessmentDate,
    bool IsActive,

    // Avaliação Postural
    string? ForwardHead,
    string? RoundedShoulders,
    string? AnteriorPelvicTilt,
    string? PosteriorPelvicTilt,
    string? KneeValgus,
    string? KneeVarus,
    string? FlatFeet,
    string? Scoliosis,

    // Avaliação Física
    double? BodyFatPercentage,
    double? MuscleMass,
    double? FlexibilityScore,
    double? StrengthScore,
    double? CardioScore,

    // Campos customizados
    List<CustomFieldDto>? CustomFields,

    // Observações e recomendações
    string? TrainerNotes,
    string? Recommendations
);
