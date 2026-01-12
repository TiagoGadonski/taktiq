using GymHero.Shared.DTOs;
using GymHero.Shared.Enums;
using MediatR;

namespace GymHero.Application.Features.Auth.Commands;

// O comando encapsula os dados da requisição. Ele retorna uma AuthResponse.
public record RegisterCommand(string Name, string Email, string Password, WorkoutLocation PreferredWorkoutLocation) : IRequest<AuthResponse>;