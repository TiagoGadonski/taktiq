namespace GymHero.Domain.Entities;

public abstract class BaseEntity
{
    // Usamos Guid como chave primária para evitar conflitos em sistemas distribuídos.
    public Guid Id { get; init; } = Guid.NewGuid();

    // init; torna a propriedade "setável" apenas durante a inicialização do objeto.
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}