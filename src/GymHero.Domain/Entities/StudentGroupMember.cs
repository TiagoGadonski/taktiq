namespace GymHero.Domain.Entities;

public class StudentGroupMember : BaseEntity
{
    public Guid GroupId { get; set; }
    public StudentGroup Group { get; set; } = null!;

    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
