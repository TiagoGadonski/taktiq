using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Personal.Commands;

public class GenerateWorkoutPlanForClientCommandHandler : IRequestHandler<GenerateWorkoutPlanForClientCommand, WorkoutPlanResponse>
{
    private readonly IApplicationDbContext _context;
    public GenerateWorkoutPlanForClientCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<WorkoutPlanResponse> Handle(GenerateWorkoutPlanForClientCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar se o cliente pertence ao personal (lógica que já conhecemos)
        var client = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ClientId && u.PersonalTrainerId == request.PersonalId, cancellationToken);
        if (client is null) throw new NotFoundException("Client not found.");

        // 2. Criar a base do novo plano de treino
        var newPlan = new WorkoutPlan
        {
            OwnerId = request.ClientId,
            Name = $"Plano de {request.Goal} - {request.Level}",
            Goal = request.Goal
        };

        // 3. LÓGICA DA "IA": Selecionar exercícios com base nas regras
        // Esta é uma implementação SIMPLES. Poderia ser muito mais complexa.
        
        var allExercises = await _context.Exercises.AsNoTracking().ToListAsync(cancellationToken);

        // Regra 1: Selecionar exercícios compostos primeiro
        var peitoExercises = allExercises.Where(e => e.MuscleGroup == "Peito").Take(2);
        var costasExercises = allExercises.Where(e => e.MuscleGroup == "Costas").Take(2);
        var pernasExercises = allExercises.Where(e => e.MuscleGroup == "Pernas").Take(2);
        var ombrosExercises = allExercises.Where(e => e.MuscleGroup == "Ombros").Take(1);
        var bicepsExercises = allExercises.Where(e => e.MuscleGroup == "Bíceps").Take(1);
        var tricepsExercises = allExercises.Where(e => e.MuscleGroup == "Tríceps").Take(1);

        // Selecionar exercícios de finalização (abs sempre, cardio ocasionalmente)
        var absExercises = allExercises.Where(e => e.MuscleGroup == "Abdômen" || e.MuscleGroup == "Abdomen").Take(2);
        var cardioExercises = allExercises.Where(e => e.MuscleGroup == "Cardio").Take(1);

        var selectedExercises = peitoExercises.Concat(costasExercises)
                                              .Concat(pernasExercises)
                                              .Concat(ombrosExercises)
                                              .Concat(bicepsExercises)
                                              .Concat(tricepsExercises)
                                              .Concat(absExercises)
                                              .Concat(cardioExercises)
                                              .ToList();

        // Regra 2: Definir séries e repetições com base no objetivo
        int targetSets = request.Goal == "Força" ? 5 : 4;
        int targetReps = request.Goal switch {
            "Força" => 5,
            "Resistência" => 15,
            _ => 10 // Padrão para Hipertrofia
        };

        // 4. Criar um treino padrão e adicionar os exercícios selecionados
        var defaultWorkout = new Workout
        {
            Name = "Treino Completo",
            Order = 1,
            DayOfWeek = null // Treino sem dia específico
        };

        int order = 1;
        foreach (var exercise in selectedExercises)
        {
            defaultWorkout.Exercises.Add(new WorkoutExercise
            {
                ExerciseId = exercise.Id,
                Order = order++,
                TargetSets = targetSets,
                TargetReps = targetReps,
                TargetLoad = 30 // Carga inicial de exemplo
            });
        }

        newPlan.Workouts.Add(defaultWorkout);
        
        // 5. Salvar o plano completo
        await _context.WorkoutPlans.AddAsync(newPlan, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new WorkoutPlanResponse(newPlan.Id, newPlan.OwnerId, newPlan.Name, newPlan.Goal, newPlan.IsActive, newPlan.CreatedAt);
    }
}