using GymHero.Shared.Enums;

namespace GymHero.Shared.DTOs;

// Response for listing protocols
public record ProtocolListResponse(
    Guid Id,
    string Name,
    string Description,
    AssessmentProtocolType ProtocolType,
    string Category,
    int? DurationMinutes,
    bool IsPublic
);

// Detailed protocol response with all information
public record ProtocolDetailResponse(
    Guid Id,
    string Name,
    string Description,
    AssessmentProtocolType ProtocolType,
    string Category,
    string? Instructions,
    string? Equipment,
    int? DurationMinutes,
    string MeasurementFields,  // JSON string
    string? NormativeData,     // JSON string
    string? CalculationFormula,
    bool IsPublic,
    Guid? CreatedByUserId,
    DateTime CreatedAt
);

// Request to record a protocol result
public record RecordProtocolResultRequest(
    Guid ProtocolId,
    Guid StudentId,
    DateTime TestDate,
    string Measurements,  // JSON with actual measurements
    string? TrainerNotes,
    Guid? StudentAssessmentId  // Optional: link to a student assessment
);

// Response for protocol result
public record ProtocolResultResponse(
    Guid Id,
    Guid ProtocolId,
    string ProtocolName,
    Guid StudentId,
    string StudentName,
    DateTime TestDate,
    string Measurements,
    double? CalculatedScore,
    string? ResultUnit,
    string? Classification,
    string? TrainerNotes,
    string? Recommendations,
    DateTime CreatedAt
);

// Response for listing multiple results (e.g., progress tracking)
public record ProtocolResultSummaryResponse(
    Guid Id,
    Guid ProtocolId,
    string ProtocolName,
    DateTime TestDate,
    double? CalculatedScore,
    string? ResultUnit,
    string? Classification
);

// Request to update a protocol result
public record UpdateProtocolResultRequest(
    string Measurements,
    string? TrainerNotes,
    string? Recommendations
);
