using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Posts.Queries;

public class GetMyPostsQueryHandler : IRequestHandler<GetMyPostsQuery, List<PostResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetMyPostsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PostResponse>> Handle(GetMyPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await _context.Posts
            .Include(p => p.Author)
            .Where(p => p.AuthorId == request.AuthorId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        var postResponses = new List<PostResponse>();

        foreach (var post in posts)
        {
            // Calculate analytics for each post
            var uniqueViewers = await _context.PostViews
                .Where(pv => pv.PostId == post.Id && pv.ViewerId != null)
                .Select(pv => pv.ViewerId)
                .Distinct()
                .CountAsync(cancellationToken);

            var profileClicks = await _context.PostViews
                .Where(pv => pv.PostId == post.Id && pv.ClickedProfile)
                .CountAsync(cancellationToken);

            // Calculate engagement rate: (profile clicks / total views) * 100
            var engagementRate = post.ViewCount > 0
                ? (double)profileClicks / post.ViewCount * 100
                : 0.0;

            postResponses.Add(new PostResponse(
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
            ));
        }

        return postResponses;
    }
}
