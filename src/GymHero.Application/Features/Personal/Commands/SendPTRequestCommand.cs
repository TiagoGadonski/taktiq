using MediatR;

namespace GymHero.Application.Features.Personal.Commands;

public record SendPTRequestCommand(
    Guid TrainerId,
    Guid StudentId,
    string? Message
) : IRequest<Guid>;
