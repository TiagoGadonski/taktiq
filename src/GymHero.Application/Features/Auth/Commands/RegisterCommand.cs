using GymHero.Shared.DTOs;
using GymHero.Shared.Enums;
using MediatR;

namespace GymHero.Application.Features.Auth.Commands;

public record RegisterCommand(
    string Name,
    string Email,
    string Password,
    WorkoutLocation PreferredWorkoutLocation,
    bool IsPersonalTrainer = false) : IRequest<AuthResponse>;
