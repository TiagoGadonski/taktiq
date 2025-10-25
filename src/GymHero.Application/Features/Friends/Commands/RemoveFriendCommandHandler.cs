using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Friends.Commands;

public class RemoveFriendCommandHandler : IRequestHandler<RemoveFriendCommand>
{
    private readonly IApplicationDbContext _context;
    public RemoveFriendCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(RemoveFriendCommand request, CancellationToken cancellationToken)
    {
        var friendship = await _context.Friendships
            .FirstOrDefaultAsync(f => f.Id == request.FriendshipId, cancellationToken);

        if (friendship is null)
            throw new NotFoundException("Amizade não encontrada.");

        // Validação de segurança: o utilizador logado tem de fazer parte desta amizade para a poder apagar.
        if (friendship.RequesterId != request.CurrentUserId && friendship.AddresseeId != request.CurrentUserId)
            throw new ValidationException("Você não tem permissão para remover esta amizade.");
            
        _context.Friendships.Remove(friendship);
        await _context.SaveChangesAsync(cancellationToken);
    }
}