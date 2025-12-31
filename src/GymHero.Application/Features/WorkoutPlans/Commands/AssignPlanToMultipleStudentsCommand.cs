using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public record AssignPlanToMultipleStudentsCommand(
    Guid PersonalId,
    IEnumerable<Guid> StudentIds,
    string PlanName,
    string? Goal,
    Guid? TemplatePlanId,
    DateTime? ExpirationDate
) : IRequest<IEnumerable<Guid>>;
