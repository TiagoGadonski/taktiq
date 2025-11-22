using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Posts.Commands;

/// <summary>
/// Command to track a post view for analytics
/// </summary>
public record TrackPostViewCommand(
    Guid PostId,
    Guid? ViewerId,
    string? Source
) : IRequest<Unit>;

public class TrackPostViewCommandHandler : IRequestHandler<TrackPostViewCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public TrackPostViewCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(TrackPostViewCommand request, CancellationToken cancellationToken)
    {
        // Check if post exists and is published
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId && p.IsPublished, cancellationToken);

        if (post == null)
        {
            // Don't track views for non-existent or unpublished posts
            return Unit.Value;
        }

        // Only track one view per viewer per post per day to prevent spam
        var today = DateTime.UtcNow.Date;
        var existingView = await _context.PostViews
            .FirstOrDefaultAsync(pv =>
                pv.PostId == request.PostId &&
                pv.ViewerId == request.ViewerId &&
                pv.ViewedAt >= today,
                cancellationToken);

        if (existingView != null)
        {
            // Already tracked a view from this user today
            return Unit.Value;
        }

        // Create new post view record
        var postView = new PostView
        {
            Id = Guid.NewGuid(),
            PostId = request.PostId,
            ViewerId = request.ViewerId,
            Source = request.Source,
            ViewedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.PostViews.Add(postView);

        // Increment the cached view count on the post
        post.ViewCount++;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
