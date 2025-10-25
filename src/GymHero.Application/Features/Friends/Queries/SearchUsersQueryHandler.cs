using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Friends.Queries;

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, IEnumerable<UserSearchResponse>>
{
    private readonly IApplicationDbContext _context;
    public SearchUsersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<UserSearchResponse>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm) || request.SearchTerm.Length < 3)
        {
            return Enumerable.Empty<UserSearchResponse>();
        }

        var searchTermLower = request.SearchTerm.ToLower();

        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Id != request.CurrentUserId && u.Name.ToLower().Contains(searchTermLower))
            .Select(u => new UserSearchResponse(u.Id, u.Name))
            .Take(10) // Limita o número de resultados
            .ToListAsync(cancellationToken);
    }
}