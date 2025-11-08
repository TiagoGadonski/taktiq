using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Application.Features.Sessions.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Sessions.Commands;

public class CompleteWorkoutSessionCommandHandler : IRequestHandler<CompleteWorkoutSessionCommand, CompleteWorkoutSessionResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;

    private static readonly string[] MotivationalMessages =
    {
        "Excelente trabalho! Você está cada vez mais forte!",
        "Treino completo! Continue assim e os resultados virão!",
        "Parabéns! Você superou mais um desafio!",
        "Incrível! Seu esforço está te levando longe!",
        "Muito bem! Consistência é a chave do sucesso!",
        "Treino concluído! Você é imparável!",
        "Fantástico! Cada treino te aproxima dos seus objetivos!",
        "Brilhante! Continue a superar os seus limites!",
        "Tremendo trabalho! A disciplina sempre vence!",
        "Perfeito! Você está construindo hábitos de campeão!"
    };

    public CompleteWorkoutSessionCommandHandler(IApplicationDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<CompleteWorkoutSessionResponse> Handle(CompleteWorkoutSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.WorkoutSessions
            .Include(s => s.Sets)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null)
        {
            throw new NotFoundException("Workout Session not found.");
        }

        // Validate ownership
        if (session.OwnerId != request.OwnerId)
        {
            throw new NotFoundException("Workout Session not found.");
        }

        // A lógica principal é simplesmente registrar a data/hora da finalização e notas opcionais.
        session.CompletedAt = DateTime.UtcNow;
        session.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);

        // Publish event to trigger challenge progress updates
        await _publisher.Publish(new WorkoutSessionCompletedEvent(session), cancellationToken);

        // Selecionar uma mensagem motivacional aleatória
        var random = new Random();
        var message = MotivationalMessages[random.Next(MotivationalMessages.Length)];

        return new CompleteWorkoutSessionResponse(message);
    }
}