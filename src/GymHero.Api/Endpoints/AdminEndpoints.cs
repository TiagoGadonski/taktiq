using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using GymHero.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using GymHero.Infrastructure.Services;
using GymHero.Infrastructure.Data;
using DomainChallengeTargetType = GymHero.Domain.Enums.ChallengeTargetType;

namespace GymHero.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
                       .WithTags("Admin")
                       .RequireAuthorization("RequireAdminRole");

        // Apply database migrations
        group.MapPost("/migrate-database", async (
            ApplicationDbContext context) =>
        {
            try
            {
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                var pendingList = pendingMigrations.ToList();

                if (!pendingList.Any())
                {
                    return Results.Ok(new {
                        message = "No pending migrations",
                        timestamp = DateTime.UtcNow
                    });
                }

                // Apply all pending migrations
                await context.Database.MigrateAsync();

                return Results.Ok(new {
                    message = "Database migrations applied successfully",
                    migrationsApplied = pendingList,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Migration failed",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("MigrateDatabase")
        .WithSummary("Apply pending database migrations (Admin only)");

        group.MapPost("/badge-definitions", async (
            [FromBody] BadgeDefinition request,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var definition = new BadgeDefinition
            {
                Code = request.Code,
                Title = request.Title,
                Description = request.Description,
                TriggerType = request.TriggerType,
                ThresholdValue = request.ThresholdValue
            };

            await context.BadgeDefinitions.AddAsync(definition, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/v1/admin/badge-definitions/{definition.Id}", definition);
        });

        group.MapGet("/users", async (
            IApplicationDbContext context,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default) =>
        {
            // Validation
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100); // Max 100 per page

            var query = context.Users.AsNoTracking();

            // Search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(u =>
                    u.Name.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Role,
                    IsActive = u.IsActive,
                    u.CreatedAt,
                    u.ProfilePictureUrl,
                    u.LastLoginAt
                })
                .ToListAsync(cancellationToken);

            return Results.Ok(new {
                users,
                pagination = new {
                    currentPage = page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            });
        });

        // Update user (role)
        group.MapPut("/users/{userId}", async (Guid userId, [FromBody] UpdateUserRequest request, IApplicationDbContext context, CancellationToken cancellationToken) =>
        {
            var user = await context.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);
            if (user is null) return Results.NotFound("User not found.");

            user.Role = request.Role;
            await context.SaveChangesAsync(cancellationToken);
            return Results.Ok(new { message = "User updated successfully" });
        });

        // Activate user
        group.MapPost("/users/{userId}/activate", async (Guid userId, IApplicationDbContext context, CancellationToken cancellationToken) =>
        {
            var user = await context.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);
            if (user is null) return Results.NotFound("User not found.");

            user.IsActive = true;
            await context.SaveChangesAsync(cancellationToken);
            return Results.Ok(new { message = "User activated successfully" });
        });

        // Deactivate user
        group.MapPost("/users/{userId}/deactivate", async (Guid userId, IApplicationDbContext context, CancellationToken cancellationToken) =>
        {
            var user = await context.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);
            if (user is null) return Results.NotFound("User not found.");

            user.IsActive = false;
            await context.SaveChangesAsync(cancellationToken);
            return Results.Ok(new { message = "User deactivated successfully" });
        });

        // Change user password (admin only)
        group.MapPost("/users/{userId}/change-password", async (
            Guid userId,
            [FromBody] AdminChangePasswordRequest request,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var user = await context.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);
            if (user is null) return Results.NotFound("User not found.");

            // Validate new password
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            {
                return Results.BadRequest(new { message = "Password must be at least 6 characters long" });
            }

            // Hash the new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await context.SaveChangesAsync(cancellationToken);

            return Results.Ok(new { message = "Password changed successfully" });
        });

        // Delete user
        group.MapDelete("/users/{userId}", async (Guid userId, IApplicationDbContext context, CancellationToken cancellationToken) =>
        {
            var user = await context.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);
            if (user is null) return Results.NotFound("User not found.");

            // First, delete all friendships where this user is involved
            // This includes both sent and received friend requests
            var friendships = await context.Friendships
                .Where(f => f.RequesterId == userId || f.AddresseeId == userId)
                .ToListAsync(cancellationToken);

            if (friendships.Any())
            {
                context.Friendships.RemoveRange(friendships);
            }

            // Now delete the user (cascade will handle other related entities)
            context.Users.Remove(user);
            await context.SaveChangesAsync(cancellationToken);
            return Results.Ok(new { message = "User deleted successfully" });
        });

        // Create user (admin only)
        group.MapPost("/users", async (
            [FromBody] CreateUserRequest request,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            // Check if email already exists
            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (existingUser != null)
            {
                return Results.BadRequest(new { message = "User with this email already exists" });
            }

            // Validate role
            var validRoles = new[] { "Aluno", "PersonalTrainer", "Admin" };
            if (!validRoles.Contains(request.Role))
            {
                return Results.BadRequest(new { message = "Invalid role specified" });
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(user, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Results.Created($"/api/admin/users/{user.Id}", new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                role = user.Role,
                isActive = user.IsActive,
                createdAt = user.CreatedAt
            });
        });

        // Seed admin user (ONLY FOR DEVELOPMENT) - DISABLED IN PRODUCTION
        // SECURITY: This endpoint is only available in Development environment
        // To create the first admin in production, use a database migration or secure script
        var seedGroup = app.MapGroup("/api/admin/dev")
                          .WithTags("Admin - Development Only")
                          .AllowAnonymous();

        seedGroup.MapPost("/seed-admin", async (
            IApplicationDbContext context,
            IWebHostEnvironment env,
            CancellationToken cancellationToken) =>
        {
            // SECURITY: Block in production
            if (!env.IsDevelopment())
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var adminEmail = "admin@gymhero.com";
            var existingAdmin = await context.Users
                .FirstOrDefaultAsync(u => u.Email == adminEmail, cancellationToken);

            if (existingAdmin != null)
            {
                return Results.Ok(new { message = "Admin user already exists", email = adminEmail });
            }

            var admin = new User
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(admin, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            // SECURITY: Don't return password in response, even in dev
            return Results.Ok(new {
                message = "Admin user created successfully. Check server logs for credentials.",
                email = adminEmail
            });
        })
        .WithName("SeedAdminUser")
        .WithSummary("[DEV ONLY] Creates a default admin user - DISABLED IN PRODUCTION");

        seedGroup.MapPost("/create-admin", async (
            [FromBody] CreateAdminRequest request,
            IApplicationDbContext context,
            IWebHostEnvironment env,
            CancellationToken cancellationToken) =>
        {
            // SECURITY: Block in production
            if (!env.IsDevelopment())
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (existingUser != null)
            {
                return Results.BadRequest(new { message = "User with this email already exists" });
            }

            var admin = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(admin, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Results.Ok(new {
                message = "Admin user created successfully",
                email = request.Email,
                name = request.Name
            });
        })
        .WithName("CreateCustomAdmin")
        .WithSummary("[DEV ONLY] Creates a custom admin user - DISABLED IN PRODUCTION");

        group.MapPost("/seed-exercises", async (ExerciseSeederService seeder, CancellationToken ct) =>
{
    await seeder.SeedExercisesAsync(ct);
    return Results.Ok("Importação de exercícios iniciada.");
})
.WithName("SeedExercisesFromApi")
.WithSummary("Busca exercícios de uma API externa e povoa a base de dados.");

        // Seed predefined system challenges
        group.MapPost("/seed-challenges", async (IApplicationDbContext context, CancellationToken cancellationToken) =>
        {
            var existingChallenges = await context.Challenges
                .Where(c => c.IsDefault)
                .ToListAsync(cancellationToken);

            if (existingChallenges.Any())
            {
                return Results.Ok(new { message = "System challenges already exist", count = existingChallenges.Count });
            }

            var systemChallenges = new List<Challenge>
            {
                // Onboarding and Setup Challenges
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty, // System-created
                    Title = "TaktIQ Iniciante",
                    Type = "Setup",
                    TargetValue = 1,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "user-check",
                    CreatedAt = DateTime.UtcNow
                },
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Planejador Pro",
                    Type = "Planos",
                    TargetValue = 1,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "clipboard-list",
                    CreatedAt = DateTime.UtcNow
                },
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Meu Arsenal",
                    Type = "Exercícios",
                    TargetValue = 1,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "dumbbell",
                    CreatedAt = DateTime.UtcNow
                },

                // Consistency Challenges
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Primeira Semana",
                    Type = "Treinos",
                    TargetValue = 3,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "calendar-check",
                    CreatedAt = DateTime.UtcNow
                },
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Maratonista",
                    Type = "Treinos",
                    TargetValue = 10,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "footprints",
                    CreatedAt = DateTime.UtcNow
                },
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Fim de Semana Ativo",
                    Type = "Treinos",
                    TargetValue = 1,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "sun",
                    CreatedAt = DateTime.UtcNow
                },

                // Progress and Strength Challenges
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Força Bruta",
                    Type = "PR",
                    TargetValue = 1,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "zap",
                    CreatedAt = DateTime.UtcNow
                },
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Monstro de Volume",
                    Type = "Volume",
                    TargetValue = 1000,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "weight",
                    CreatedAt = DateTime.UtcNow
                },
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Superador",
                    Type = "PR",
                    TargetValue = 3,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "star",
                    CreatedAt = DateTime.UtcNow
                },

                // Social Challenges
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Conexão",
                    Type = "Social",
                    TargetValue = 1,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "users",
                    CreatedAt = DateTime.UtcNow
                },
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Incentivador",
                    Type = "Social",
                    TargetValue = 1,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "share-2",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Challenges.AddRangeAsync(systemChallenges, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Results.Ok(new { message = "System challenges seeded successfully", count = systemChallenges.Count });
        })
        .WithName("SeedSystemChallenges")
        .WithSummary("Creates predefined system challenges for all users");

        // Assign default challenges to existing users who don't have them
        group.MapPost("/assign-default-challenges", async (IApplicationDbContext context, CancellationToken cancellationToken) =>
        {
            // Get all default challenges
            var defaultChallenges = await context.Challenges
                .Where(c => c.IsDefault)
                .ToListAsync(cancellationToken);

            if (!defaultChallenges.Any())
            {
                return Results.BadRequest(new { message = "No default challenges found. Please seed challenges first using /api/admin/seed-challenges" });
            }

            // Get all users
            var allUsers = await context.Users.ToListAsync(cancellationToken);

            // Get all existing challenge progresses
            var existingProgresses = await context.ChallengeProgresses
                .Select(cp => new { cp.ParticipantId, cp.ChallengeId })
                .ToListAsync(cancellationToken);

            var existingProgressSet = existingProgresses
                .Select(ep => $"{ep.ParticipantId}_{ep.ChallengeId}")
                .ToHashSet();

            int totalAssigned = 0;
            int usersProcessed = 0;

            foreach (var user in allUsers)
            {
                foreach (var challenge in defaultChallenges)
                {
                    // Check if challenge is applicable to this user
                    bool isApplicable = challenge.TargetType == DomainChallengeTargetType.AllUsers ||
                                       (challenge.TargetType == DomainChallengeTargetType.AllTrainers && user.Role == "PersonalTrainer");

                    // Create a key for this user-challenge combination
                    var progressKey = $"{user.Id}_{challenge.Id}";

                    // If applicable and user doesn't have this challenge yet, assign it
                    if (isApplicable && !existingProgressSet.Contains(progressKey))
                    {
                        var progress = new ChallengeProgress
                        {
                            ChallengeId = challenge.Id,
                            ParticipantId = user.Id,
                            CurrentValue = 0,
                            LastUpdate = DateTime.UtcNow
                        };

                        await context.ChallengeProgresses.AddAsync(progress, cancellationToken);
                        totalAssigned++;
                    }
                }

                usersProcessed++;
            }

            if (totalAssigned > 0)
            {
                await context.SaveChangesAsync(cancellationToken);
            }

            return Results.Ok(new {
                message = "Default challenges assigned successfully",
                usersProcessed,
                challengesAssigned = totalAssigned
            });
        })
        .WithName("AssignDefaultChallenges")
        .WithSummary("Assigns all default challenges to existing users who don't have them yet");
    }
}