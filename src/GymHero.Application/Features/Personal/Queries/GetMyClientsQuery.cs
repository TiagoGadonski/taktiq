using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Personal.Queries;

// Query para buscar a lista de clientes de um personal trainer específico
public record GetMyClientsQuery(Guid PersonalId) : IRequest<IEnumerable<ClientResponse>>;