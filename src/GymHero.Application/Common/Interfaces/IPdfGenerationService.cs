namespace GymHero.Application.Common.Interfaces;

/// <summary>
/// Service for generating PDF documents
/// </summary>
public interface IPdfGenerationService
{
    /// <summary>
    /// Generate a student progress report PDF
    /// </summary>
    Task<byte[]> GenerateStudentProgressReportAsync(
        Guid studentId,
        Guid trainerId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a workout plan PDF
    /// </summary>
    Task<byte[]> GenerateWorkoutPlanPdfAsync(
        Guid workoutPlanId,
        Guid trainerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate an assessment report PDF
    /// </summary>
    Task<byte[]> GenerateAssessmentReportPdfAsync(
        Guid assessmentId,
        Guid trainerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a monthly summary report for trainer
    /// </summary>
    Task<byte[]> GenerateMonthlySummaryReportAsync(
        Guid trainerId,
        int year,
        int month,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a comparison report between two assessments
    /// </summary>
    Task<byte[]> GenerateAssessmentComparisonPdfAsync(
        Guid assessment1Id,
        Guid assessment2Id,
        Guid trainerId,
        CancellationToken cancellationToken = default);
}
