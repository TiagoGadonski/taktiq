using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public class RenewWorkoutPlanCommandHandler : IRequestHandler<RenewWorkoutPlanCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RenewWorkoutPlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RenewWorkoutPlanCommand request, CancellationToken cancellationToken)
    {
        // Find the plan and verify ownership
        var plan = await _context.WorkoutPlans
            .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.OwnerId == request.OwnerId, cancellationToken);

        if (plan == null)
            return false;

        // Calculate new expiration date
        var now = DateTime.UtcNow;
        DateTime newExpirationDate;

        if (plan.ExpirationDate.HasValue && plan.ExpirationDate.Value > now)
        {
            // If plan hasn't expired yet, extend from current expiration
            newExpirationDate = plan.ExpirationDate.Value.AddDays(request.AdditionalWeeks * 7);
        }
        else
        {
            // If plan is expired or has no expiration, extend from now
            newExpirationDate = now.AddDays(request.AdditionalWeeks * 7);
        }

        // Update the plan
        plan.ExpirationDate = newExpirationDate;

        // Update duration if it exists
        if (plan.Duration.HasValue)
        {
            plan.Duration += request.AdditionalWeeks;
        }
        else
        {
            plan.Duration = request.AdditionalWeeks;
        }

        // If plan was expired, update start date
        if (!plan.StartDate.HasValue || (plan.ExpirationDate.HasValue && plan.ExpirationDate.Value < now))
        {
            plan.StartDate = now;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
