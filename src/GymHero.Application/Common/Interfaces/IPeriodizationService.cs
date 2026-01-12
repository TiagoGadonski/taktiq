using GymHero.Shared.DTOs;
using GymHero.Shared.Enums;

namespace GymHero.Application.Common.Interfaces;

/// <summary>
/// Service for generating periodized workout plans
/// </summary>
public interface IPeriodizationService
{
    /// <summary>
    /// Generate a complete periodized workout plan
    /// </summary>
    Task<PeriodizedPlanResponse> GeneratePeriodizedPlanAsync(
        GeneratePeriodizedPlanRequest request,
        Guid trainerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get predefined periodization templates
    /// </summary>
    Task<List<PeriodizationTemplateDto>> GetPeriodizationTemplatesAsync(
        PeriodizationModel? model = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a custom periodization template
    /// </summary>
    Task<PeriodizationTemplateDto> CreateTemplateAsync(
        CreatePeriodizationTemplateRequest request,
        Guid trainerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate weekly progressions for a periodization plan
    /// </summary>
    List<WeeklyProgression> CalculateProgressions(
        int durationWeeks,
        PeriodizationModel model,
        TrainingPhase startingPhase,
        bool includeDeload);
}
