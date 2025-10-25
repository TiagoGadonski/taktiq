using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Auth.Queries;

// A Query que representa a intenção de fazer login.
// Ela recebe os dados de login e espera uma AuthResponse como resultado.
public record LoginQuery(string Email, string Password) : IRequest<AuthResponse>;