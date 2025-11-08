using System.Reflection;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Implementação da interface IApplicationDbContext
    public DbSet<User> Users => Set<User>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<WorkoutPlan> WorkoutPlans => Set<WorkoutPlan>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
    public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();
    public DbSet<WorkoutSet> WorkoutSets => Set<WorkoutSet>();
    public DbSet<ProgressMetric> ProgressMetrics => Set<ProgressMetric>();
    public DbSet<Challenge> Challenges => Set<Challenge>();
    public DbSet<ChallengeProgress> ChallengeProgresses => Set<ChallengeProgress>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<BadgeDefinition> BadgeDefinitions => Set<BadgeDefinition>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Notification> Notifications => Set<Notification>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as configurações de entidades que estiverem no mesmo assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Exemplo de configuração manual (Fluent API)
        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
            builder.HasIndex(u => u.Email).IsUnique(); // Garante que o email seja único
        });

        modelBuilder.Entity<User>()
            // Um Personal Trainer (PersonalTrainer) tem muitos Alunos (Clients)
            .HasMany(personal => personal.Clients) 
            // Cada Aluno tem um (ou nenhum) Personal Trainer
            .WithOne(client => client.PersonalTrainer) 
            // A chave estrangeira que liga os dois é PersonalTrainerId
            .HasForeignKey(client => client.PersonalTrainerId)
            // Define o comportamento de exclusão. Se um Personal for apagado,
            // o PersonalTrainerId dos seus alunos ficará nulo (eles ficam sem personal).
            .OnDelete(DeleteBehavior.SetNull); 

        modelBuilder.Entity<Friendship>(entity =>
    {
        // Define uma chave primária composta para evitar pedidos duplicados (A -> B)
        entity.HasKey(f => new { f.RequesterId, f.AddresseeId });

        // Configura a relação do lado do "Requester" (quem envia)
        entity.HasOne(f => f.Requester)
              .WithMany(u => u.SentFriendRequests)
              .HasForeignKey(f => f.RequesterId)
              .OnDelete(DeleteBehavior.Restrict); // Impede que um utilizador seja apagado se tiver amizades

        // Configura a relação do lado do "Addressee" (quem recebe)
        entity.HasOne(f => f.Addressee)
              .WithMany(u => u.ReceivedFriendRequests)
              .HasForeignKey(f => f.AddresseeId)
              .OnDelete(DeleteBehavior.Restrict);
    });

        // Performance indexes for frequently queried columns
        modelBuilder.Entity<WorkoutSession>(entity =>
        {
            // Index for user-specific session queries
            entity.HasIndex(s => s.OwnerId)
                .HasDatabaseName("IX_WorkoutSessions_OwnerId");

            // Composite index for common query pattern (user sessions by completion date)
            entity.HasIndex(s => new { s.OwnerId, s.CompletedAt })
                .HasDatabaseName("IX_WorkoutSessions_OwnerId_CompletedAt");
        });

        modelBuilder.Entity<WorkoutSet>(entity =>
        {
            // Index for personal records and exercise-specific queries
            entity.HasIndex(s => s.ExerciseId)
                .HasDatabaseName("IX_WorkoutSets_ExerciseId");

            // Index for session-specific set queries
            entity.HasIndex(s => s.WorkoutSessionId)
                .HasDatabaseName("IX_WorkoutSets_WorkoutSessionId");
        });

        modelBuilder.Entity<Challenge>(entity =>
        {
            // Index for creator-specific challenge queries
            entity.HasIndex(c => c.CreatorId)
                .HasDatabaseName("IX_Challenges_CreatorId");
        });

        modelBuilder.Entity<WorkoutPlan>(entity =>
        {
            // Index for user-specific workout plan queries
            entity.HasIndex(wp => wp.OwnerId)
                .HasDatabaseName("IX_WorkoutPlans_OwnerId");
        });

        base.OnModelCreating(modelBuilder);
    }
}