using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Posts.Commands;

public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdatePostCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

        if (post == null)
            throw new NotFoundException("Post não encontrado");

        // Verify that the author is updating their own post
        if (post.AuthorId != request.AuthorId)
            throw new ValidationException("Você não tem permissão para editar este post");

        post.Title = request.Title;
        post.Content = request.Content;
        post.ImageUrl = request.ImageUrl;

        // If changing from draft to published, set PublishedAt
        if (!post.IsPublished && request.IsPublished)
        {
            post.PublishedAt = DateTime.UtcNow;
        }

        post.IsPublished = request.IsPublished;
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
