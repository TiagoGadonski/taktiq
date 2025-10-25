using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public class AddWorkoutToPlanCommandHandler : IRequestHandler<AddWorkoutToPlanCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddWorkoutToPlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddWorkoutToPlanCommand request, CancellationToken cancellationToken)
    {
        // Verify the plan exists and belongs to the user
        var plan = await _context.WorkoutPlans
            .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.OwnerId == request.OwnerId, cancellationToken);

        if (plan == null)
        {
            throw new NotFoundException($"WorkoutPlan with ID {request.PlanId} not found");
        }

        // Create the new workout
        var workout = new Workout
        {
            PlanId = request.PlanId,
            Name = request.Name,
            DayOfWeek = request.DayOfWeek,
            Order = request.Order
        };

        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync(cancellationToken);

        return workout.Id;
    }
}
