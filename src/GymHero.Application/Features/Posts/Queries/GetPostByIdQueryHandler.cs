using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Posts.Queries;

public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, PostResponse>
{
    private readonly IApplicationDbContext _context;

    public GetPostByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PostResponse> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

        if (post == null)
            throw new NotFoundException("Post não encontrado");

        // Calculate analytics
        var uniqueViewers = await _context.PostViews
            .Where(pv => pv.PostId == post.Id && pv.ViewerId != null)
            .Select(pv => pv.ViewerId)
            .Distinct()
            .CountAsync(cancellationToken);

        var profileClicks = await _context.PostViews
            .Where(pv => pv.PostId == post.Id && pv.ClickedProfile)
            .CountAsync(cancellationToken);

        var engagementRate = post.ViewCount > 0
            ? (double)profileClicks / post.ViewCount * 100
            : 0.0;

        return new PostResponse(
            post.Id,
            post.Title,
            post.Content,
            post.ImageUrl,
            post.AuthorId,
            post.Author.Name,
            post.Author.ProfilePictureUrl,
            post.Author.ProfileSlug,
            post.IsPublished,
            post.PublishedAt,
            post.CreatedAt,
            post.UpdatedAt,
            post.ViewCount,
            uniqueViewers,
            profileClicks,
            Math.Round(engagementRate, 2)
        );
    }
}
