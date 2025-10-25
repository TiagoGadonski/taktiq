using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Friends.Commands;

public class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommand>
{
    private readonly IApplicationDbContext _context;
    public SendFriendRequestCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(SendFriendRequestCommand request, CancellationToken cancellationToken)
    {
        // 1. Encontrar o utilizador que vai receber o pedido
        var addressee = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.AddresseeEmail, cancellationToken);

        if (addressee is null)
            throw new NotFoundException("Utilizador não encontrado com o email especificado.");

        // 2. Regras de Negócio
        if (request.RequesterId == addressee.Id)
            throw new ValidationException("Não pode enviar um pedido de amizade para si mesmo.");
            
        // 3. Verificar se já existe uma amizade (em qualquer direção)
        var existingFriendship = await _context.Friendships
            .FirstOrDefaultAsync(f => 
                (f.RequesterId == request.RequesterId && f.AddresseeId == addressee.Id) ||
                (f.RequesterId == addressee.Id && f.AddresseeId == request.RequesterId), 
                cancellationToken);

        if (existingFriendship is not null)
        {
            if (existingFriendship.Status == FriendshipStatus.Accepted)
                throw new ValidationException("Vocês já são amigos.");
            if (existingFriendship.Status == FriendshipStatus.Pending)
                throw new ValidationException("Já existe um pedido de amizade pendente.");
        }

        // 4. Se todas as regras passarem, criar o novo pedido de amizade
        var newFriendship = new Friendship
        {
            RequesterId = request.RequesterId,
            AddresseeId = addressee.Id,
            Status = FriendshipStatus.Pending
        };

        await _context.Friendships.AddAsync(newFriendship, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}