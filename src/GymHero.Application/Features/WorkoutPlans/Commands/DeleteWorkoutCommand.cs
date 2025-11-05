using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

// Command to delete a workout (day) from a plan
// Requires WorkoutId and OwnerId for security
public record DeleteWorkoutCommand(Guid WorkoutId, Guid OwnerId) : IRequest;
