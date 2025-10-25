using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public class ShareWorkoutPlanCommandHandler : IRequestHandler<ShareWorkoutPlanCommand>
{
    private readonly IApplicationDbContext _context;

    public ShareWorkoutPlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ShareWorkoutPlanCommand request, CancellationToken cancellationToken)
    {
        // Load the original plan with all its workouts and exercises
        var originalPlan = await _context.WorkoutPlans
            .AsNoTracking()
            .Include(p => p.Workouts)
                .ThenInclude(w => w.Exercises)
            .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.OwnerId == request.SharerId, cancellationToken);

        if (originalPlan is null)
        {
            throw new NotFoundException("Workout plan not found or you don't have permission to share it.");
        }

        // Verify all friends exist and are actually friends with the sharer
        var friendships = await _context.Friendships
            .Where(f =>
                (f.RequesterId == request.SharerId && request.FriendIds.Contains(f.AddresseeId) && f.Status == FriendshipStatus.Accepted) ||
                (f.AddresseeId == request.SharerId && request.FriendIds.Contains(f.RequesterId) && f.Status == FriendshipStatus.Accepted))
            .ToListAsync(cancellationToken);

        var validFriendIds = new HashSet<Guid>();
        foreach (var friendship in friendships)
        {
            var friendId = friendship.RequesterId == request.SharerId ? friendship.AddresseeId : friendship.RequesterId;
            validFriendIds.Add(friendId);
        }

        if (validFriendIds.Count == 0)
        {
            throw new BadRequestException("No valid friends found to share with.");
        }

        // Create a copy of the plan for each friend
        foreach (var friendId in validFriendIds)
        {
            var newPlan = new WorkoutPlan
            {
                Id = Guid.NewGuid(),
                OwnerId = friendId,
                Name = $"{originalPlan.Name} (compartilhado)",
                Description = originalPlan.Description,
                Goal = originalPlan.Goal,
                Duration = originalPlan.Duration,
                IsActive = false, // Don't automatically activate the shared plan
                CreatedAt = DateTime.UtcNow
            };

            _context.WorkoutPlans.Add(newPlan);

            // Copy all workouts
            foreach (var originalWorkout in originalPlan.Workouts)
            {
                var newWorkout = new Workout
                {
                    Id = Guid.NewGuid(),
                    PlanId = newPlan.Id,
                    Name = originalWorkout.Name,
                    DayOfWeek = originalWorkout.DayOfWeek,
                    Order = originalWorkout.Order,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Workouts.Add(newWorkout);

                // Copy all exercises in this workout
                foreach (var originalExercise in originalWorkout.Exercises)
                {
                    var newExercise = new WorkoutExercise
                    {
                        Id = Guid.NewGuid(),
                        WorkoutId = newWorkout.Id,
                        ExerciseId = originalExercise.ExerciseId,
                        Order = originalExercise.Order,
                        TargetSets = originalExercise.TargetSets,
                        TargetReps = originalExercise.TargetReps,
                        TargetLoad = originalExercise.TargetLoad,
                        TargetRepsRange = originalExercise.TargetRepsRange,
                        TargetRpe = originalExercise.TargetRpe,
                        RestSeconds = originalExercise.RestSeconds,
                        Notes = originalExercise.Notes,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.WorkoutExercises.Add(newExercise);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
