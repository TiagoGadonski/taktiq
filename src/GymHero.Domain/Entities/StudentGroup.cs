namespace GymHero.Domain.Entities;

public class StudentGroup : BaseEntity
{
    public Guid TrainerId { get; set; }
    public User Trainer { get; set; } = null!;

    public string Name { get; set; } = ""; // Ex: "Alunos com Lordose", "Grupo Avançado"
    public string? Description { get; set; }
    public string? Tags { get; set; } // JSON array for flexible tagging

    public ICollection<StudentGroupMember> Members { get; set; } = new List<StudentGroupMember>();

    // Helper properties
    public int MemberCount => Members?.Count ?? 0;
}
