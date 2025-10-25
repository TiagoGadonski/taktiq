using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Progress.Queries;

// Query para buscar todos os recordes pessoais de um utilizador
public record GetUserPersonalRecordsQuery(Guid OwnerId) : IRequest<IEnumerable<PersonalRecordResponse>>;