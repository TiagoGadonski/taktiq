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
            .Where(p => p.Id == request.PostId)
            .Select(p => new PostResponse(
                p.Id,
                p.Title,
                p.Content,
                p.ImageUrl,
                p.AuthorId,
                p.Author.Name,
                p.Author.ProfilePictureUrl,
                p.IsPublished,
                p.PublishedAt,
                p.CreatedAt,
                p.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (post == null)
            throw new NotFoundException("Post não encontrado");

        return post;
    }
}
