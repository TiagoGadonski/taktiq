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
        // Verify the plan exists and the user has permission to modify it
        // Permission is granted if:
        // 1. User is the owner of the plan, OR
        // 2. User is the Personal Trainer of the plan's owner
        var plan = await _context.WorkoutPlans
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken);

        if (plan == null)
        {
            throw new NotFoundException($"WorkoutPlan with ID {request.PlanId} not found");
        }

        // Check if user has permission to modify this plan
        bool hasPermission = plan.OwnerId == request.OwnerId || // User is the owner
                           plan.Owner?.PersonalTrainerId == request.OwnerId; // User is the PT of the owner

        if (!hasPermission)
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
