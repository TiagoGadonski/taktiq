using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Posts.Queries;

public class GetAllPublishedPostsQueryHandler : IRequestHandler<GetAllPublishedPostsQuery, List<PostSummaryResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetAllPublishedPostsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PostSummaryResponse>> Handle(GetAllPublishedPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await _context.Posts
            .Include(p => p.Author)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PostSummaryResponse(
                p.Id,
                p.Title,
                p.Content.Length > 200 ? p.Content.Substring(0, 200) + "..." : p.Content,
                p.ImageUrl,
                p.AuthorId,
                p.Author.Name,
                p.Author.ProfilePictureUrl,
                p.IsPublished,
                p.PublishedAt,
                p.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return posts;
    }
}
