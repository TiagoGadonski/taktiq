using MediatR;

namespace GymHero.Application.Features.Sessions.Commands;

// Command to cancel a workout session
public record CancelWorkoutSessionCommand(Guid SessionId, Guid OwnerId) : IRequest;
