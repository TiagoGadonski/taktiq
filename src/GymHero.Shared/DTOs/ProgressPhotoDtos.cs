using GymHero.Shared.Enums;

namespace GymHero.Shared.DTOs;

// Request to create a progress photo
public record CreateProgressPhotoRequest(
    Guid StudentId,
    Guid MediaId,
    ProgressPhotoType PhotoType,
    BodyAngle BodyAngle,
    DateTime PhotoDate,

    double? WeightKg,
    double? BodyFatPercentage,
    double? MuscleMassKg,

    double? ChestCm,
    double? WaistCm,
    double? HipsCm,
    double? LeftArmCm,
    double? RightArmCm,
    double? LeftThighCm,
    double? RightThighCm,
    double? LeftCalfCm,
    double? RightCalfCm,

    // Notes
    string? TrainerNotes,
    string? StudentNotes,
    string? Caption,

    // Visibility
    bool IsVisibleToStudent,
    bool IsPublic,

    Guid? StudentAssessmentId
);

public record ProgressPhotoResponse(
    Guid Id,
    Guid StudentId,
    string StudentName,
    Guid MediaId,
    string MediaUrl,
    string? ThumbnailUrl,
    Guid UploadedBy,
    string UploaderName,
    ProgressPhotoType PhotoType,
    BodyAngle BodyAngle,
    DateTime PhotoDate,

    // Measurements
    double? WeightKg,
    double? BodyFatPercentage,
    double? MuscleMassKg,

    // Circumferences
    double? ChestCm,
    double? WaistCm,
    double? HipsCm,
    double? LeftArmCm,
    double? RightArmCm,
    double? LeftThighCm,
    double? RightThighCm,
    double? LeftCalfCm,
    double? RightCalfCm,

    // Notes
    string? TrainerNotes,
    string? StudentNotes,
    string? Caption,

    // Visibility
    bool IsVisibleToStudent,
    bool IsPublic,

    DateTime CreatedAt
);

// Summary response for list views
public record ProgressPhotoSummaryResponse(
    Guid Id,
    Guid MediaId,
    string MediaUrl,
    string? ThumbnailUrl,
    ProgressPhotoType PhotoType,
    BodyAngle BodyAngle,
    DateTime PhotoDate,
    double? WeightKg,
    DateTime CreatedAt
);

// Request to update a progress photo
public record UpdateProgressPhotoRequest(
    ProgressPhotoType PhotoType,
    BodyAngle BodyAngle,
    DateTime PhotoDate,

    // Measurements
    double? WeightKg,
    double? BodyFatPercentage,
    double? MuscleMassKg,

    // Circumferences
    double? ChestCm,
    double? WaistCm,
    double? HipsCm,
    double? LeftArmCm,
    double? RightArmCm,
    double? LeftThighCm,
    double? RightThighCm,
    double? LeftCalfCm,
    double? RightCalfCm,

    // Notes
    string? TrainerNotes,
    string? StudentNotes,
    string? Caption,

    // Visibility
    bool IsVisibleToStudent,
    bool IsPublic
);

// Comparison between two photos
public record PhotoComparisonResponse(
    ProgressPhotoResponse BeforePhoto,
    ProgressPhotoResponse AfterPhoto,
    ComparisonMetrics? Metrics
);

// Metrics for comparison
public record ComparisonMetrics(
    int DaysBetween,
    double? WeightChangedKg,
    double? WeightChangePercentage,
    double? BodyFatChangePercentage,
    double? MuscleMassChangeKg,
    double? ChestChangeCm,
    double? WaistChangeCm,
    double? HipsChangeCm
);
