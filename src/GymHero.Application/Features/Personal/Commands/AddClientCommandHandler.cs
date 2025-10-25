using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Personal.Commands;

public class AddClientCommandHandler : IRequestHandler<AddClientCommand>
{
    private readonly IApplicationDbContext _context;
    public AddClientCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(AddClientCommand request, CancellationToken cancellationToken)
    {
        var client = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.ClientEmail, cancellationToken);

        if (client is null)
            throw new NotFoundException("Client with the specified email not found.");

        if (client.Role != "Aluno")
            throw new ValidationException("The specified user is not a client.");

        if (client.PersonalTrainerId is not null)
            throw new ValidationException("Client is already assigned to another Personal Trainer.");
        
        client.PersonalTrainerId = request.PersonalId;
        await _context.SaveChangesAsync(cancellationToken);
    }
}