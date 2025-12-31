using MediatR;

namespace GymHero.Application.Features.StudentGroups.Commands;

public record AddStudentsToGroupCommand(
    Guid GroupId,
    Guid TrainerId,
    IEnumerable<Guid> StudentIds
) : IRequest;
