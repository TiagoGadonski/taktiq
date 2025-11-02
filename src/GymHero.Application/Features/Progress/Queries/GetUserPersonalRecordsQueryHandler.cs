using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Progress.Queries;

public class GetUserPersonalRecordsQueryHandler : IRequestHandler<GetUserPersonalRecordsQuery, IEnumerable<PersonalRecordResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetUserPersonalRecordsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<PersonalRecordResponse>> Handle(GetUserPersonalRecordsQuery request, CancellationToken cancellationToken)
    {
        // Esta consulta complexa fará todo o trabalho pesado:
        var personalRecords = await _context.WorkoutSets
            // Incluímos os dados relacionados de que vamos precisar
            .Include(s => s.Exercise)
            .Include(s => s.WorkoutSession)
            // Filtramos apenas por séries de sessões que pertencem ao utilizador E que já foram completadas
            // E que têm valores de Reps e Load preenchidos
            .Where(s => s.WorkoutSession.OwnerId == request.OwnerId &&
                       s.WorkoutSession.CompletedAt != null &&
                       s.Reps.HasValue && s.Load.HasValue)
            // Agrupamos todas as séries pelo ID do Exercício e pelo número de Repetições
            // Ex: ("Supino", 5 reps), ("Agachamento", 3 reps), etc.
            .GroupBy(s => new { s.ExerciseId, s.Reps })
            // Para cada um desses grupos, selecionamos apenas a série que teve a maior carga (Load)
            .Select(group => group.OrderByDescending(s => s.Load).First())
            // Agora que temos apenas as melhores séries (os PRs), projetamo-las para o nosso DTO de resposta
            .Select(s => new PersonalRecordResponse(
                s.ExerciseId,
                s.Exercise.Name,
                s.Reps!.Value,
                s.Load!.Value,
                s.WorkoutSession.CompletedAt!.Value)) // Usamos a data de conclusão da sessão como a data do PR
            .ToListAsync(cancellationToken);

        return personalRecords;
    }
}