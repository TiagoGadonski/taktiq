using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Posts.Commands;

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand>
{
    private readonly IApplicationDbContext _context;

    public DeletePostCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

        if (post == null)
            throw new NotFoundException("Post não encontrado");

        // Verify that the author is deleting their own post
        if (post.AuthorId != request.AuthorId)
            throw new ValidationException("Você não tem permissão para deletar este post");

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
