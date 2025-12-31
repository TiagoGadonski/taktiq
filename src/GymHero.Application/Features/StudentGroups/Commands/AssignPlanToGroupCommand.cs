using MediatR;

namespace GymHero.Application.Features.StudentGroups.Commands;

public record AssignPlanToGroupCommand(
    Guid GroupId,
    Guid TrainerId,
    string PlanName,
    string? Goal,
    Guid? TemplatePlanId,
    DateTime? ExpirationDate
) : IRequest<IEnumerable<Guid>>;
