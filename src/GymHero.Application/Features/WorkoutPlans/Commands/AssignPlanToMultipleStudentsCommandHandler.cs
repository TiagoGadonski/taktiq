using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public class AssignPlanToMultipleStudentsCommandHandler : IRequestHandler<AssignPlanToMultipleStudentsCommand, IEnumerable<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public AssignPlanToMultipleStudentsCommandHandler(
        IApplicationDbContext context,
        INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<Guid>> Handle(AssignPlanToMultipleStudentsCommand request, CancellationToken cancellationToken)
    {
        // Validate all students belong to this PT
        var students = await _context.Users
            .Where(u => request.StudentIds.Contains(u.Id))
            .ToListAsync(cancellationToken);

        if (students.Count != request.StudentIds.Count())
        {
            throw new NotFoundException("One or more students not found");
        }

        var invalidStudents = students.Where(s => s.PersonalTrainerId != request.PersonalId).ToList();
        if (invalidStudents.Any())
        {
            throw new UnauthorizedAccessException("One or more students don't belong to this Personal Trainer");
        }

        // Load template plan if provided
        WorkoutPlan? templatePlan = null;
        if (request.TemplatePlanId.HasValue)
        {
            templatePlan = await _context.WorkoutPlans
                .Include(p => p.Workouts)
                    .ThenInclude(w => w.Exercises)
                .FirstOrDefaultAsync(p => p.Id == request.TemplatePlanId.Value, cancellationToken);

            if (templatePlan == null)
            {
                throw new NotFoundException("Template plan not found");
            }

            // Verify PT owns the template
            if (templatePlan.OwnerId != request.PersonalId)
            {
                throw new UnauthorizedAccessException("You don't have permission to use this template");
            }
        }

        var createdPlanIds = new List<Guid>();

        // Create a plan for each student
        foreach (var student in students)
        {
            WorkoutPlan newPlan;

            if (templatePlan != null)
            {
                // Clone the template plan
                newPlan = new WorkoutPlan
                {
                    Name = request.PlanName,
                    Goal = request.Goal,
                    OwnerId = student.Id,
                    Duration = templatePlan.Duration,
                    IsPublic = false,
                    ExpirationDate = request.ExpirationDate
                };

                _context.WorkoutPlans.Add(newPlan);
                await _context.SaveChangesAsync(cancellationToken);

                // Clone workouts
                foreach (var templateWorkout in templatePlan.Workouts)
                {
                    var newWorkout = new Workout
                    {
                        PlanId = newPlan.Id,
                        Name = templateWorkout.Name,
                        DayOfWeek = templateWorkout.DayOfWeek,
                        Order = templateWorkout.Order
                    };

                    _context.Workouts.Add(newWorkout);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Clone exercises
                    foreach (var templateExercise in templateWorkout.Exercises)
                    {
                        var newExercise = new WorkoutExercise
                        {
                            WorkoutId = newWorkout.Id,
                            ExerciseId = templateExercise.ExerciseId,
                            Order = templateExercise.Order,
                            TargetSets = templateExercise.TargetSets,
                            TargetReps = templateExercise.TargetReps,
                            TargetLoad = templateExercise.TargetLoad,
                            TargetRepsRange = templateExercise.TargetRepsRange,
                            TargetRpe = templateExercise.TargetRpe,
                            RestSeconds = templateExercise.RestSeconds,
                            Notes = templateExercise.Notes
                        };

                        _context.WorkoutExercises.Add(newExercise);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // Create empty plan
                newPlan = new WorkoutPlan
                {
                    Name = request.PlanName,
                    Goal = request.Goal,
                    OwnerId = student.Id,
                    Duration = 4, // Default 4 weeks
                    IsPublic = false,
                    ExpirationDate = request.ExpirationDate
                };

                _context.WorkoutPlans.Add(newPlan);
                await _context.SaveChangesAsync(cancellationToken);
            }

            createdPlanIds.Add(newPlan.Id);

            // Send notification to student
            await _notificationService.CreatePlanSharedNotificationAsync(
                student.Id,
                newPlan.Id,
                request.PlanName,
                "Seu Personal Trainer",
                cancellationToken);
        }

        return createdPlanIds;
    }
}
