using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Sessions.Commands;

public class CompleteWorkoutSessionCommandHandler : IRequestHandler<CompleteWorkoutSessionCommand, CompleteWorkoutSessionResponse>
{
    private readonly IApplicationDbContext _context;

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

    public CompleteWorkoutSessionCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<CompleteWorkoutSessionResponse> Handle(CompleteWorkoutSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.WorkoutSessions
            .Include(s => s.WorkoutPlan)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null)
        {
            throw new NotFoundException("Workout Session not found.");
        }

        // Validate ownership: if there's a plan, check if it belongs to the user
        // Free workouts (without a plan) can be completed by anyone who has the session ID
        if (session.WorkoutPlan is not null && session.WorkoutPlan.OwnerId != request.OwnerId)
        {
            throw new NotFoundException("Workout Session not found.");
        }

        // A lógica principal é simplesmente registrar a data/hora da finalização.
        session.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Selecionar uma mensagem motivacional aleatória
        var random = new Random();
        var message = MotivationalMessages[random.Next(MotivationalMessages.Length)];

        return new CompleteWorkoutSessionResponse(message);
    }
}