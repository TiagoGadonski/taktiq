using MediatR;

namespace GymHero.Application.Features.StudentGroups.Commands;

public record UpdateStudentGroupCommand(
    Guid GroupId,
    Guid TrainerId,
    string Name,
    string? Description,
    string? Tags
) : IRequest;
