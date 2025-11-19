using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using MediatR;

namespace GymHero.Application.Features.Posts.Commands;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreatePostCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = new Post
        {
            Title = request.Title,
            Content = request.Content,
            ImageUrl = request.ImageUrl,
            AuthorId = request.AuthorId,
            IsPublished = request.IsPublished,
            PublishedAt = request.IsPublished ? DateTime.UtcNow : null,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Posts.AddAsync(post, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return post.Id;
    }
}
