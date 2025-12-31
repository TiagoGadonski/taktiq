using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public class AddWorkoutToPlanCommandHandler : IRequestHandler<AddWorkoutToPlanCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AddWorkoutToPlanCommandHandler> _logger;

    public AddWorkoutToPlanCommandHandler(
        IApplicationDbContext context,
        ILogger<AddWorkoutToPlanCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> Handle(AddWorkoutToPlanCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AddWorkoutToPlan - PlanId: {PlanId}, OwnerId: {OwnerId}",
            request.PlanId, request.OwnerId);

        // Verify the plan exists
        var plan = await _context.WorkoutPlans
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken);

        if (plan == null)
        {
            _logger.LogWarning("AddWorkoutToPlan - Plan not found: {PlanId}", request.PlanId);
            throw new NotFoundException($"WorkoutPlan with ID {request.PlanId} not found");
        }

        // Check if user has permission to modify this plan
        // Permission is granted if:
        // 1. User is the owner of the plan, OR
        // 2. User is the Personal Trainer of the plan's owner
        bool hasPermission = false;
        Guid? ownerPersonalTrainerId = null;

        if (plan.OwnerId == request.OwnerId)
        {
            // User is the owner
            hasPermission = true;
        }
        else
        {
            // Check if user is the PT of the plan's owner
            // Query separately to avoid Include issues with Azure deployment
            ownerPersonalTrainerId = await _context.Users
                .Where(u => u.Id == plan.OwnerId)
                .Select(u => u.PersonalTrainerId)
                .FirstOrDefaultAsync(cancellationToken);

            hasPermission = ownerPersonalTrainerId == request.OwnerId;
        }

        _logger.LogInformation("AddWorkoutToPlan - Found plan. OwnerId: {PlanOwnerId}, OwnerPTId: {OwnerPTId}, RequesterId: {RequesterId}, HasPermission: {HasPermission}",
            plan.OwnerId, ownerPersonalTrainerId, request.OwnerId, hasPermission);

        _logger.LogInformation("AddWorkoutToPlan - Permission check: {HasPermission}", hasPermission);

        if (!hasPermission)
        {
            _logger.LogWarning("AddWorkoutToPlan - Permission denied for user {UserId} on plan {PlanId}",
                request.OwnerId, request.PlanId);
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
