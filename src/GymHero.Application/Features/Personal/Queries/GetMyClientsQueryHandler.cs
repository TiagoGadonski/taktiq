using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Personal.Queries;

public class GetMyClientsQueryHandler : IRequestHandler<GetMyClientsQuery, IEnumerable<ClientResponse>>
{
    private readonly IApplicationDbContext _context;
    public GetMyClientsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<ClientResponse>> Handle(GetMyClientsQuery request, CancellationToken cancellationToken)
    {
        var clients = await _context.Users
            .AsNoTracking()
            // A lógica é simples: encontrar todos os utilizadores (que são 'Alunos')
            // cujo PersonalTrainerId seja o ID do personal que fez a requisição.
            .Where(user => user.Role == "Aluno" && user.PersonalTrainerId == request.PersonalId)
            .Select(client => new ClientResponse(client.Id, client.Name, client.Email))
            .ToListAsync(cancellationToken);

        return clients;
    }
}