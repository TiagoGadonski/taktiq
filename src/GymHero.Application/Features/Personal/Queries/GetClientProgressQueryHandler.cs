using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Personal.Queries;

public class GetClientProgressQueryHandler : IRequestHandler<GetClientProgressQuery, ClientProgressDashboardResponse>
{
    private readonly IApplicationDbContext _context;
    // Podemos injetar o MediatR para reutilizar outros handlers, mas faremos a lógica direta por clareza.
    public GetClientProgressQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<ClientProgressDashboardResponse> Handle(GetClientProgressQuery request, CancellationToken cancellationToken)
    {
        // Validação de Segurança: O aluno existe e pertence a este personal?
        var isMyClient = await _context.Users
            .AnyAsync(u => u.Id == request.ClientId && u.PersonalTrainerId == request.PersonalId, cancellationToken);

        if (!isMyClient)
        {
            throw new NotFoundException("Client not found or not assigned to this trainer.");
        }

        // 1. Buscar as métricas corporais (peso, etc.) do aluno
        var bodyMetrics = await _context.ProgressMetrics
            .Where(m => m.OwnerId == request.ClientId)
            .OrderByDescending(m => m.Date)
            .Select(m => new ProgressMetricResponse(m.Id, m.Type, m.Value, m.Unit, m.Date))
            .ToListAsync(cancellationToken);

        // 2. Buscar os recordes pessoais (PRs) do aluno
        var personalRecords = await _context.WorkoutSets
            .Where(s => s.WorkoutSession.OwnerId == request.ClientId &&
                       s.WorkoutSession.CompletedAt != null &&
                       s.Reps.HasValue && s.Load.HasValue)
            .Include(s => s.Exercise)
            .Include(s => s.WorkoutSession)
            .GroupBy(s => new { s.ExerciseId, s.Reps })
            .Select(group => group.OrderByDescending(s => s.Load).First())
            .Select(s => new PersonalRecordResponse(
                s.ExerciseId,
                s.Exercise.Name,
                s.Reps!.Value,
                s.Load!.Value,
                s.WorkoutSession.CompletedAt!.Value))
            .ToListAsync(cancellationToken);

        return new ClientProgressDashboardResponse(bodyMetrics, personalRecords);
    }
}