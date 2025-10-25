using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Auth.Commands;

// O comando encapsula os dados da requisição. Ele retorna uma AuthResponse.
public record RegisterCommand(string Name, string Email, string Password) : IRequest<AuthResponse>;