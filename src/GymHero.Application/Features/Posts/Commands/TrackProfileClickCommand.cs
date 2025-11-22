using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Posts.Commands;

/// <summary>
/// Command to track when a user clicks on the author's profile from a post
/// </summary>
public record TrackProfileClickCommand(
    Guid PostId,
    Guid? ViewerId
) : IRequest<Unit>;

public class TrackProfileClickCommandHandler : IRequestHandler<TrackProfileClickCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public TrackProfileClickCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(TrackProfileClickCommand request, CancellationToken cancellationToken)
    {
        // Find the most recent view from this viewer
        var recentView = await _context.PostViews
            .Where(pv => pv.PostId == request.PostId && pv.ViewerId == request.ViewerId)
            .OrderByDescending(pv => pv.ViewedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (recentView != null)
        {
            // Update the existing view record to mark that profile was clicked
            recentView.ClickedProfile = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
        else if (request.ViewerId != null)
        {
            // Create a new view record with profile click
            // This handles the case where the user clicked directly without tracking a view first
            var postView = new Domain.Entities.PostView
            {
                Id = Guid.NewGuid(),
                PostId = request.PostId,
                ViewerId = request.ViewerId,
                ClickedProfile = true,
                ViewedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.PostViews.Add(postView);

            // Also increment view count
            var post = await _context.Posts.FindAsync(new object[] { request.PostId }, cancellationToken);
            if (post != null)
            {
                post.ViewCount++;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
