using MediatR;

namespace GymHero.Application.Features.StudentGroups.Commands;

public record CreateStudentGroupCommand(
    Guid TrainerId,
    string Name,
    string? Description,
    string? Tags
) : IRequest<Guid>;
