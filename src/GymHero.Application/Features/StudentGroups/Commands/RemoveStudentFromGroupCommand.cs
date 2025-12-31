using MediatR;

namespace GymHero.Application.Features.StudentGroups.Commands;

public record RemoveStudentFromGroupCommand(
    Guid GroupId,
    Guid StudentId,
    Guid TrainerId
) : IRequest;
