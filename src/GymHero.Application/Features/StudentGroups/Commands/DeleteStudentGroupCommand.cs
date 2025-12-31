using MediatR;

namespace GymHero.Application.Features.StudentGroups.Commands;

public record DeleteStudentGroupCommand(
    Guid GroupId,
    Guid TrainerId
) : IRequest;
