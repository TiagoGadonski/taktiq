using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Friends.Commands;

public class SendFriendRequestByIdCommandHandler : IRequestHandler<SendFriendRequestByIdCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public SendFriendRequestByIdCommandHandler(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(SendFriendRequestByIdCommand request, CancellationToken cancellationToken)
    {
        // Validação 1: O destinatário existe?
        var addresseeExists = await _context.Users.AnyAsync(u => u.Id == request.AddresseeId, cancellationToken);
        if (!addresseeExists)
            throw new NotFoundException("Utilizador não encontrado.");

        // Regra de Negócio: Não se pode adicionar a si mesmo
        if (request.RequesterId == request.AddresseeId)
            throw new ValidationException("Não pode enviar um pedido de amizade para si mesmo.");

        // Validação 2: Já existe uma amizade ou pedido?
        var existingFriendship = await _context.Friendships
            .FirstOrDefaultAsync(f => 
                (f.RequesterId == request.RequesterId && f.AddresseeId == request.AddresseeId) ||
                (f.RequesterId == request.AddresseeId && f.AddresseeId == request.RequesterId), 
                cancellationToken);

        if (existingFriendship is not null)
        {
            if (existingFriendship.Status == FriendshipStatus.Accepted) throw new ValidationException("Vocês já são amigos.");
            if (existingFriendship.Status == FriendshipStatus.Pending) throw new ValidationException("Já existe um pedido de amizade pendente.");
        }

        // Se tudo estiver OK, criar o pedido
        var newFriendship = new Friendship
        {
            RequesterId = request.RequesterId,
            AddresseeId = request.AddresseeId,
            Status = FriendshipStatus.Pending
        };

        await _context.Friendships.AddAsync(newFriendship, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Send notification to the addressee
        var requester = await _context.Users.FindAsync(new object[] { request.RequesterId }, cancellationToken);
        if (requester != null)
        {
            await _notificationService.CreateFriendRequestNotificationAsync(
                request.AddresseeId,
                requester.Id,
                requester.Name,
                cancellationToken);
        }
    }
}