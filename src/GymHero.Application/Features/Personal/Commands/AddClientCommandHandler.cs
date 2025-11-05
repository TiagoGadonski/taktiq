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
            throw new NotFoundException("Nenhum usuário encontrado com este email.");

        if (client.Role != "Aluno")
            throw new ValidationException($"Este usuário não é um aluno. Role atual: {client.Role}. Para adicionar como cliente, o usuário deve ter a role 'Aluno'.");

        if (client.PersonalTrainerId is not null)
            throw new ValidationException("Este aluno já está atribuído a outro Personal Trainer.");

        client.PersonalTrainerId = request.PersonalId;
        await _context.SaveChangesAsync(cancellationToken);
    }
}