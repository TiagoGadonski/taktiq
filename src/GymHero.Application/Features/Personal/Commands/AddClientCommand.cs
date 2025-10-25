using MediatR;
namespace GymHero.Application.Features.Personal.Commands;
public record AddClientCommand(Guid PersonalId, string ClientEmail) : IRequest;