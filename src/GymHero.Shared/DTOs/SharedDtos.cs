namespace GymHero.Shared.DTOs;

// DTO genérico para ler mensagens de erro da API
public record ErrorResponse(string Message);

// DTO genérico para respostas paginadas
public record PaginatedResponse<T>
{
    public required List<T> Data { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public required int TotalPages { get; init; }
}