using GymHero.Domain.Entities;
using MediatR;

namespace GymHero.Application.Features.Sessions.Events;

// Usamos INotification do MediatR para definir um evento.
// Este evento carrega os dados da sessão que foi completada.
public record WorkoutSessionCompletedEvent(WorkoutSession Session) : INotification;