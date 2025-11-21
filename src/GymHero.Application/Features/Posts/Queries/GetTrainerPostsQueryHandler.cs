using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Posts.Queries;

public class GetTrainerPostsQueryHandler : IRequestHandler<GetTrainerPostsQuery, List<PostSummaryResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetTrainerPostsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PostSummaryResponse>> Handle(GetTrainerPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await _context.Posts
            .Include(p => p.Author)
            .Where(p => p.AuthorId == request.TrainerId && p.IsPublished)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Select(p => new PostSummaryResponse(
                p.Id,
                p.Title,
                p.Content.Length > 200 ? p.Content.Substring(0, 200) + "..." : p.Content,
                p.ImageUrl,
                p.AuthorId,
                p.Author.Name,
                p.Author.ProfilePictureUrl,
                p.Author.ProfileSlug,
                p.IsPublished,
                p.PublishedAt,
                p.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return posts;
    }
}
