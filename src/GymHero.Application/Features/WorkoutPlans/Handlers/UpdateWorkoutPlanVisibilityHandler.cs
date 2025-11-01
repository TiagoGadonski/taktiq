using GymHero.Application.Common.Interfaces;
using GymHero.Application.Features.WorkoutPlans.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainVisibility = GymHero.Domain.Enums.VisibilityLevel;
using SharedVisibility = GymHero.Shared.Enums.VisibilityLevel;

namespace GymHero.Application.Features.WorkoutPlans.Handlers;

public class UpdateWorkoutPlanVisibilityHandler : IRequestHandler<UpdateWorkoutPlanVisibilityCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateWorkoutPlanVisibilityHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateWorkoutPlanVisibilityCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.WorkoutPlans
            .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.OwnerId == request.UserId, cancellationToken);

        if (plan == null)
            return false;

        // Convert from Shared enum to Domain enum
        plan.VisibilityLevel = (DomainVisibility)(int)request.VisibilityLevel;
        plan.IsPublic = request.VisibilityLevel == SharedVisibility.Public;
        plan.AllowCopying = request.AllowCopying;

        // Set PublishedAt timestamp when first made public
        if (plan.IsPublic && plan.PublishedAt == null)
        {
            plan.PublishedAt = DateTime.UtcNow;
        }
        // Clear PublishedAt if made private again
        else if (!plan.IsPublic)
        {
            plan.PublishedAt = null;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
