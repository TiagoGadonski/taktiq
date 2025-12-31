using MediatR;

namespace GymHero.Application.Features.Personal.Commands;

public record RespondToPTRequestCommand(
    Guid RequestId,
    Guid StudentId,
    bool Accepted
) : IRequest;
