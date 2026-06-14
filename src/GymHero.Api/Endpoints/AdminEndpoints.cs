using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using GymHero.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using GymHero.Infrastructure.Services;
using GymHero.Infrastructure.Data;
using GymHero.Infrastructure.Data.Seeders;
using DomainChallengeTargetType = GymHero.Domain.Enums.ChallengeTargetType;
using ExerciseTranslations = GymHero.Infrastructure.Data.ExerciseTranslations;

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
            ApplicationDbContext context,
            System.Security.Claims.ClaimsPrincipal user,
            ILogger<Program> logger) =>
        {
            var adminId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                var pendingList = pendingMigrations.ToList();

                if (!pendingList.Any())
                {
                    logger.LogInformation("Admin {AdminId} checked database migrations - none pending", adminId);
                    return Results.Ok(new {
                        message = "No pending migrations",
                        timestamp = DateTime.UtcNow
                    });
                }

                logger.LogWarning("Admin {AdminId} is applying database migrations: {Migrations}", adminId, string.Join(", ", pendingList));

                // Apply all pending migrations
                await context.Database.MigrateAsync();

                logger.LogInformation("Admin {AdminId} successfully applied database migrations: {Migrations}", adminId, string.Join(", ", pendingList));

                return Results.Ok(new {
                    message = "Database migrations applied successfully",
                    migrationsApplied = pendingList,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Admin {AdminId} failed to apply database migrations", adminId);
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
            System.Security.Claims.ClaimsPrincipal user,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var adminId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

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

            logger.LogInformation("Admin {AdminId} created badge definition: {BadgeCode} - {BadgeTitle}", adminId, definition.Code, definition.Title);

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
        group.MapPut("/users/{userId}", async (Guid userId, [FromBody] UpdateUserRequest request, IApplicationDbContext context, System.Security.Claims.ClaimsPrincipal adminUser, ILogger<Program> logger, CancellationToken cancellationToken) =>
        {
            var adminId = adminUser.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await context.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);
            if (user is null)
            {
                logger.LogWarning("Admin {AdminId} attempted to update non-existent user {UserId}", adminId, userId);
                return Results.NotFound("User not found.");
            }

            var oldRole = user.Role;
            user.Role = request.Role;
            await context.SaveChangesAsync(cancellationToken);

            logger.LogWarning("Admin {AdminId} changed user {UserId} ({UserEmail}) role from {OldRole} to {NewRole}", adminId, userId, user.Email, oldRole, request.Role);

            return Results.Ok(new { message = "User updated successfully" });
        });

        // Activate user
        group.MapPost("/users/{userId}/activate", async (Guid userId, IApplicationDbContext context, System.Security.Claims.ClaimsPrincipal adminUser, ILogger<Program> logger, CancellationToken cancellationToken) =>
        {
            var adminId = adminUser.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await context.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);
            if (user is null)
            {
                logger.LogWarning("Admin {AdminId} attempted to activate non-existent user {UserId}", adminId, userId);
                return Results.NotFound("User not found.");
            }

            user.IsActive = true;
            await context.SaveChangesAsync(cancellationToken);

            logger.LogWarning("Admin {AdminId} activated user {UserId} ({UserEmail})", adminId, userId, user.Email);

            return Results.Ok(new { message = "User activated successfully" });
        });

        // Deactivate user
        group.MapPost("/users/{userId}/deactivate", async (Guid userId, IApplicationDbContext context, System.Security.Claims.ClaimsPrincipal adminUser, ILogger<Program> logger, CancellationToken cancellationToken) =>
        {
            var adminId = adminUser.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await context.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);
            if (user is null)
            {
                logger.LogWarning("Admin {AdminId} attempted to deactivate non-existent user {UserId}", adminId, userId);
                return Results.NotFound("User not found.");
            }

            user.IsActive = false;
            await context.SaveChangesAsync(cancellationToken);

            logger.LogWarning("Admin {AdminId} deactivated user {UserId} ({UserEmail})", adminId, userId, user.Email);

            return Results.Ok(new { message = "User deactivated successfully" });
        });

        // Change user password (admin only)
        group.MapPost("/users/{userId}/change-password", async (
            Guid userId,
            [FromBody] AdminChangePasswordRequest request,
            IApplicationDbContext context,
            System.Security.Claims.ClaimsPrincipal adminUser,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var adminId = adminUser.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await context.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);
            if (user is null)
            {
                logger.LogWarning("Admin {AdminId} attempted to change password for non-existent user {UserId}", adminId, userId);
                return Results.NotFound("User not found.");
            }

            // Validate new password
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            {
                return Results.BadRequest(new { message = "Password must be at least 6 characters long" });
            }

            // Hash the new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogWarning("Admin {AdminId} changed password for user {UserId} ({UserEmail})", adminId, userId, user.Email);

            return Results.Ok(new { message = "Password changed successfully" });
        });

        // Delete user
        group.MapDelete("/users/{userId}", async (Guid userId, IApplicationDbContext context, System.Security.Claims.ClaimsPrincipal adminUser, ILogger<Program> logger, CancellationToken cancellationToken) =>
        {
            var adminId = adminUser.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await context.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);
            if (user is null)
            {
                logger.LogWarning("Admin {AdminId} attempted to delete non-existent user {UserId}", adminId, userId);
                return Results.NotFound("User not found.");
            }

            var userEmail = user.Email;
            var userName = user.Name;
            var userRole = user.Role;

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

            logger.LogWarning("Admin {AdminId} DELETED user {UserId} ({UserEmail}, {UserName}, {UserRole})", adminId, userId, userEmail, userName, userRole);

            return Results.Ok(new { message = "User deleted successfully" });
        });

        // Get activity logs with filtering and pagination
        group.MapGet("/activity-logs", async (
            IActivityLogService activityLogService,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] Guid? userId = null,
            [FromQuery] string? action = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            CancellationToken cancellationToken = default) =>
        {
            // Validation
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var logs = await activityLogService.GetLogsAsync(
                page,
                pageSize,
                userId,
                action,
                startDate,
                endDate,
                cancellationToken);

            var totalCount = await activityLogService.GetLogCountAsync(
                userId,
                action,
                startDate,
                endDate,
                cancellationToken);

            // Transform to DTO to avoid circular references and provide proper field names
            var logDtos = logs.Select(log => new {
                id = log.Id,
                userId = log.UserId,
                userName = log.User?.Name,
                userEmail = log.User?.Email,
                action = log.Action,
                endpoint = log.Endpoint,
                httpMethod = log.HttpMethod,
                statusCode = log.StatusCode,
                responseTimeMs = log.ResponseTimeMs,
                ipAddress = log.IpAddress,
                userAgent = log.UserAgent,
                timestamp = log.Timestamp,
                errorMessage = log.ErrorMessage
            });

            return Results.Ok(new {
                logs = logDtos,
                pagination = new {
                    currentPage = page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            });
        })
        .WithName("GetActivityLogs")
        .WithSummary("Get activity logs with filtering and pagination (Admin only)");

        // Get activity log statistics
        group.MapGet("/activity-logs/stats", async (
            IApplicationDbContext context,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            CancellationToken cancellationToken = default) =>
        {
            var query = context.UserActivityLogs.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(log => log.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.Timestamp <= endDate.Value);
            }

            var totalRequests = await query.CountAsync(cancellationToken);
            var totalErrors = await query.Where(log => log.ErrorMessage != null).CountAsync(cancellationToken);
            var uniqueUsers = await query.Where(log => log.UserId != null).Select(log => log.UserId).Distinct().CountAsync(cancellationToken);
            var avgResponseTime = await query.Where(log => log.ResponseTimeMs.HasValue).AverageAsync(log => (double?)log.ResponseTimeMs, cancellationToken);

            var topActions = await query
                .GroupBy(log => log.Action)
                .Select(g => new { action = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(10)
                .ToListAsync(cancellationToken);

            var recentErrors = await query
                .Where(log => log.ErrorMessage != null)
                .OrderByDescending(log => log.Timestamp)
                .Take(10)
                .Select(log => new {
                    log.Timestamp,
                    log.Action,
                    log.Endpoint,
                    log.ErrorMessage,
                    log.StatusCode
                })
                .ToListAsync(cancellationToken);

            return Results.Ok(new {
                totalRequests,
                totalErrors,
                uniqueUsers,
                avgResponseTimeMs = avgResponseTime,
                topActions,
                recentErrors
            });
        })
        .WithName("GetActivityLogStats")
        .WithSummary("Get activity log statistics (Admin only)");

        // Create user (admin only)
        group.MapPost("/users", async (
            [FromBody] CreateUserRequest request,
            IApplicationDbContext context,
            System.Security.Claims.ClaimsPrincipal adminUser,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var adminId = adminUser.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Check if email already exists
            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (existingUser != null)
            {
                logger.LogWarning("Admin {AdminId} attempted to create user with existing email: {Email}", adminId, request.Email);
                return Results.BadRequest(new { message = "User with this email already exists" });
            }

            // Validate role
            var validRoles = new[] { "Aluno", "PersonalTrainer", "Admin" };
            if (!validRoles.Contains(request.Role))
            {
                logger.LogWarning("Admin {AdminId} attempted to create user with invalid role: {Role}", adminId, request.Role);
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

            logger.LogWarning("Admin {AdminId} created new user {UserId} ({UserEmail}, {UserRole})", adminId, user.Id, user.Email, user.Role);

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

        /*
        group.MapPost("/seed-exercises", async (ExerciseSeederService seeder, CancellationToken ct) =>
{
    await seeder.SeedExercisesAsync(ct);
    return Results.Ok("Importação de exercícios iniciada.");
})
.WithName("SeedExercisesFromApi")
.WithSummary("Busca exercícios de uma API externa e povoa a base de dados.");

        group.MapPost("/seed-comprehensive-exercises", async (
            ComprehensiveExerciseSeederService seeder,
            ILogger<Program> logger,
            CancellationToken ct) =>
        {
            try
            {
                var (added, skipped) = await seeder.SeedComprehensiveExercisesAsync(ct);

                logger.LogInformation("Comprehensive exercise seeding completed: {Added} added, {Skipped} skipped", added, skipped);

                return Results.Ok(new
                {
                    message = "Exercícios abrangentes importados com sucesso",
                    exercisesAdded = added,
                    exercisesSkipped = skipped,
                    total = added + skipped,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to seed comprehensive exercises");
                return Results.Problem(
                    title: "Seed failed",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("SeedComprehensiveExercises")
        .WithSummary("Importa 150+ exercícios curados (casa, academia, halteres, faixas elásticas, calistenia)");
        */

        group.MapPost("/enhance-exercises", async (
            ExerciseEnhancementService enhancer,
            ILogger<Program> logger,
            CancellationToken ct) =>
        {
            try
            {
                logger.LogInformation("Starting exercise enhancement process...");
                var result = await enhancer.EnhanceAllExercisesAsync(ct);

                logger.LogInformation("Exercise enhancement completed: {Enhanced} enhanced, {Skipped} skipped, {Errors} errors",
                    result.EnhancedCount, result.SkippedCount, result.Errors.Count);

                return Results.Ok(new
                {
                    message = "Exercícios aprimorados com sucesso",
                    enhanced = result.EnhancedCount,
                    skipped = result.SkippedCount,
                    errors = result.Errors,
                    total = result.EnhancedCount + result.SkippedCount,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to enhance exercises");
                return Results.Problem(
                    title: "Enhancement failed",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("EnhanceExercises")
        .WithSummary("Aprimora exercícios existentes com fotos, vídeos, descrições detalhadas e traduções para português");

        /*
        // Seed development data (ONLY FOR DEVELOPMENT)
        group.MapPost("/seed-dev-data", async (
            DevelopmentSeederService seeder,
            IWebHostEnvironment env,
            CancellationToken ct) =>
        {
            // Only allow in Development environment
            if (!env.IsDevelopment())
            {
                return Results.BadRequest(new { message = "This endpoint is only available in Development environment" });
            }

            try
            {
                await seeder.SeedAllAsync(ct);
                return Results.Ok(new
                {
                    message = "Development data seeded successfully",
                    note = "All users have password: Dev@123456",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        })
        .WithName("SeedDevelopmentData")
        .WithSummary("[DEV ONLY] Seeds database with fake data for development - PTs, Students, Plans, Friendships")
        .AllowAnonymous(); // Allow anonymous in dev for easy testing
        */

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
                },
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Círculo de Ferro",
                    Type = "Social",
                    TargetValue = 5,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "users",
                    CreatedAt = DateTime.UtcNow
                },

                // Advanced Workout Consistency
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Disciplina de Aço",
                    Type = "Treinos",
                    TargetValue = 25,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "shield",
                    CreatedAt = DateTime.UtcNow
                },
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Centurião",
                    Type = "Treinos",
                    TargetValue = 50,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "award",
                    CreatedAt = DateTime.UtcNow
                },
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Lenda do Ginásio",
                    Type = "Treinos",
                    TargetValue = 100,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "crown",
                    CreatedAt = DateTime.UtcNow
                },

                // Volume Challenges - Progressive
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Levantador",
                    Type = "Volume",
                    TargetValue = 5000,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "trending-up",
                    CreatedAt = DateTime.UtcNow
                },
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Titã de Ferro",
                    Type = "Volume",
                    TargetValue = 10000,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "activity",
                    CreatedAt = DateTime.UtcNow
                },
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Atlas",
                    Type = "Volume",
                    TargetValue = 25000,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "mountain",
                    CreatedAt = DateTime.UtcNow
                },

                // PR Challenges - More tiers
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Máquina de Recordes",
                    Type = "PR",
                    TargetValue = 5,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "target",
                    CreatedAt = DateTime.UtcNow
                },
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Imparável",
                    Type = "PR",
                    TargetValue = 10,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "zap",
                    CreatedAt = DateTime.UtcNow
                },

                // Streak Challenges (consecutive days)
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Sequência de 7",
                    Type = "Streak",
                    TargetValue = 7,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "flame",
                    CreatedAt = DateTime.UtcNow
                },
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Mês Perfeito",
                    Type = "Streak",
                    TargetValue = 30,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "fire",
                    CreatedAt = DateTime.UtcNow
                },

                // Time-based Challenges
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Madrugador",
                    Type = "Timing",
                    TargetValue = 5,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "sunrise",
                    CreatedAt = DateTime.UtcNow
                },
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Guerreiro Noturno",
                    Type = "Timing",
                    TargetValue = 5,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "moon",
                    CreatedAt = DateTime.UtcNow
                },

                // Exercise Variety
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Explorador",
                    Type = "Exercícios",
                    TargetValue = 10,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "compass",
                    CreatedAt = DateTime.UtcNow
                },
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Mestre de Movimentos",
                    Type = "Exercícios",
                    TargetValue = 25,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "layers",
                    CreatedAt = DateTime.UtcNow
                },

                // Workout Plan Challenges
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Arquiteto do Corpo",
                    Type = "Planos",
                    TargetValue = 3,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "layout",
                    CreatedAt = DateTime.UtcNow
                },
                new Challenge
                {
                    Id = Guid.NewGuid(),
                    CreatorId = Guid.Empty,
                    Title = "Estrategista",
                    Type = "Planos",
                    TargetValue = 5,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(10),
                    Status = "Ativo",
                    TargetType = DomainChallengeTargetType.AllUsers,
                    IsDefault = true,
                    IconName = "book-open",
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

        // Get platform revenue analytics
        group.MapGet("/platform-revenue", async (
            IApplicationDbContext context,
            System.Security.Claims.ClaimsPrincipal adminUser,
            ILogger<Program> logger,
            CancellationToken cancellationToken,
            [FromQuery] int days = 30) =>
        {
            var adminId = adminUser.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // Limit days to reasonable range
                days = Math.Min(Math.Max(days, 7), 365);
                var startDate = DateTime.UtcNow.AddDays(-days);

                var transactions = await context.Transactions
                    .Include(t => t.Seller)
                    .Where(t => t.Status == TransactionStatus.Completed && t.CompletedAt >= startDate)
                    .OrderByDescending(t => t.CompletedAt)
                    .ToListAsync(cancellationToken);

                // Calculate platform revenue metrics
                var totalPlatformRevenue = transactions.Sum(t => t.PlatformFee);
                var totalTransactionVolume = transactions.Sum(t => t.Amount);
                var totalSellerPayouts = transactions.Sum(t => t.SellerPayout);
                var transactionCount = transactions.Count;
                var averagePlatformFee = transactionCount > 0 ? totalPlatformRevenue / transactionCount : 0;

                // Revenue by day
                var dailyRevenue = transactions
                    .GroupBy(t => t.CompletedAt!.Value.Date)
                    .Select(g => new
                    {
                        date = g.Key.ToString("yyyy-MM-dd"),
                        platformFee = g.Sum(t => t.PlatformFee),
                        transactionVolume = g.Sum(t => t.Amount),
                        sellerPayout = g.Sum(t => t.SellerPayout),
                        transactionCount = g.Count()
                    })
                    .OrderBy(x => x.date)
                    .ToList();

                // Top sellers by platform fee contribution
                var topSellers = transactions
                    .GroupBy(t => new { t.SellerId, t.Seller.Name })
                    .Select(g => new
                    {
                        sellerId = g.Key.SellerId,
                        sellerName = g.Key.Name,
                        totalPlatformFees = g.Sum(t => t.PlatformFee),
                        totalSales = g.Sum(t => t.Amount),
                        transactionCount = g.Count()
                    })
                    .OrderByDescending(s => s.totalPlatformFees)
                    .Take(10)
                    .ToList();

                logger.LogInformation("Admin {AdminId} accessed platform revenue analytics for {Days} days. Total Revenue: R${Revenue}", adminId, days, totalPlatformRevenue);

                return Results.Ok(new
                {
                    period = new
                    {
                        startDate = startDate.ToString("yyyy-MM-dd"),
                        endDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                        days = days
                    },
                    summary = new
                    {
                        totalPlatformRevenue = totalPlatformRevenue,
                        totalTransactionVolume = totalTransactionVolume,
                        totalSellerPayouts = totalSellerPayouts,
                        transactionCount = transactionCount,
                        averagePlatformFee = averagePlatformFee,
                        averageFeePercentage = totalTransactionVolume > 0
                            ? (totalPlatformRevenue / totalTransactionVolume * 100)
                            : 0
                    },
                    dailyRevenue = dailyRevenue,
                    topSellers = topSellers
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Admin {AdminId} failed to access platform revenue analytics", adminId);
                return Results.Problem(
                    title: "Failed to fetch platform revenue",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("GetPlatformRevenue")
        .WithSummary("Get platform revenue analytics from marketplace fees (Admin only)");

        // ========================================
        // SEEDER ENDPOINTS
        // ========================================

        group.MapPost("/seed-all-exercises", async (
            ApplicationDbContext context,
            ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Starting exercise seeding from JSON files...");

                await ExerciseSeeder.SeedExercisesAsync(context, logger);

                var totalExercises = await context.Exercises.CountAsync();

                return Results.Ok(new
                {
                    message = "Exercícios importados com sucesso!",
                    totalExercises,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to seed exercises");
                return Results.Problem(
                    title: "Seed failed",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("SeedAllExercises")
        .WithSummary("Importa todos os exercícios (academia + calistenia) dos arquivos JSON");

        group.MapPost("/seed-assessment-protocols", async (
            ApplicationDbContext context,
            ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Starting assessment protocol seeding from JSON file...");

                await AssessmentProtocolSeeder.SeedProtocolsAsync(context, logger);

                var totalProtocols = await context.AssessmentProtocols.CountAsync();

                return Results.Ok(new
                {
                    message = "Protocolos de avaliação importados com sucesso!",
                    totalProtocols,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to seed assessment protocols");
                return Results.Problem(
                    title: "Seed failed",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("SeedAssessmentProtocols")
        .WithSummary("Importa todos os protocolos de avaliação do arquivo JSON");

        group.MapPost("/seed-all-data", async (
            ApplicationDbContext context,
            ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Starting full database seeding (exercises + protocols)...");

                // Seed exercises
                await ExerciseSeeder.SeedExercisesAsync(context, logger);
                var totalExercises = await context.Exercises.CountAsync();

                // Seed protocols
                await AssessmentProtocolSeeder.SeedProtocolsAsync(context, logger);
                var totalProtocols = await context.AssessmentProtocols.CountAsync();

                return Results.Ok(new
                {
                    message = "Todos os dados importados com sucesso!",
                    totalExercises,
                    totalProtocols,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to seed database");
                return Results.Problem(
                    title: "Seed failed",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("SeedAllData")
        .WithSummary("Importa TODOS os dados: exercícios (academia + calistenia) + protocolos de avaliação");

        // ========================================
        // EXERCISE ANALYSIS AND FIX ENDPOINTS
        // ========================================

        // Simple test endpoint to verify deployment
        group.MapGet("/test-deploy", () => Results.Ok(new
        {
            message = "Novo deploy funcionando!",
            version = "2026-01-24-v2",
            timestamp = DateTime.UtcNow
        }))
        .WithName("TestDeploy")
        .WithSummary("Endpoint de teste para verificar se o deploy foi bem sucedido");

        group.MapGet("/exercise-stats", async (
            ApplicationDbContext context,
            ILogger<Program> logger) =>
        {
            try
            {
                var exercises = await context.Exercises.ToListAsync();

                var semTraducao = exercises.Where(e => ExerciseTranslations.NeedsTranslation(e.Name)).ToList();
                var semDescricao = exercises.Where(e => string.IsNullOrWhiteSpace(e.Description)).ToList();
                var semVideo = exercises.Where(e => string.IsNullOrWhiteSpace(e.VideoUrl)).ToList();
                var semImagem = exercises.Where(e => string.IsNullOrWhiteSpace(e.ImageUrl)).ToList();
                var semInstrucoes = exercises.Where(e => e.Instructions == null || e.Instructions.Count < 3).ToList();

                var completos = exercises.Where(e =>
                    !ExerciseTranslations.NeedsTranslation(e.Name) &&
                    !string.IsNullOrWhiteSpace(e.Description) &&
                    !string.IsNullOrWhiteSpace(e.VideoUrl) &&
                    !string.IsNullOrWhiteSpace(e.ImageUrl) &&
                    e.Instructions != null && e.Instructions.Count >= 3).ToList();

                // Agrupar por grupo muscular
                var porGrupoMuscular = exercises
                    .GroupBy(e => e.MuscleGroup.ToString())
                    .Select(g => new { muscleGroup = g.Key, count = g.Count() })
                    .OrderByDescending(x => x.count)
                    .ToList();

                // Agrupar por categoria
                var porCategoria = exercises
                    .GroupBy(e => e.Category.ToString())
                    .Select(g => new { category = g.Key, count = g.Count() })
                    .OrderByDescending(x => x.count)
                    .ToList();

                // Lista de exercicios que precisam de traducao
                var exerciciosSemTraducao = semTraducao
                    .Take(50)
                    .Select(e => new { e.Id, e.Name, e.MuscleGroup })
                    .ToList();

                logger.LogInformation("Exercise stats: Total={Total}, NeedsTranslation={NeedsTranslation}, NoDesc={NoDesc}, NoVideo={NoVideo}, NoImage={NoImage}, Complete={Complete}",
                    exercises.Count, semTraducao.Count, semDescricao.Count, semVideo.Count, semImagem.Count, completos.Count);

                return Results.Ok(new
                {
                    total = exercises.Count,
                    semTraducao = semTraducao.Count,
                    semDescricao = semDescricao.Count,
                    semVideo = semVideo.Count,
                    semImagem = semImagem.Count,
                    semInstrucoes = semInstrucoes.Count,
                    completos = completos.Count,
                    porGrupoMuscular,
                    porCategoria,
                    exerciciosSemTraducao,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get exercise stats");
                return Results.Problem(
                    title: "Failed to get exercise stats",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("GetExerciseStats")
        .WithSummary("Retorna estatisticas dos exercicios (traducao, imagens, videos, etc.)");

        group.MapPost("/fix-exercises", async (
            ApplicationDbContext context,
            ILogger<Program> logger,
            [FromQuery] bool dryRun = false) =>
        {
            try
            {
                logger.LogInformation("Starting exercise fix process (dryRun={DryRun})...", dryRun);

                var exercises = await context.Exercises.ToListAsync();
                var fixes = new List<object>();
                int traduzidos = 0, descricoes = 0, videos = 0, imagens = 0;

                foreach (var exercise in exercises)
                {
                    var changes = new List<string>();

                    // 1. Traduzir nome se estiver em ingles
                    if (ExerciseTranslations.NeedsTranslation(exercise.Name))
                    {
                        var translated = ExerciseTranslations.Translate(exercise.Name);
                        if (translated != exercise.Name)
                        {
                            changes.Add($"Nome: '{exercise.Name}' -> '{translated}'");
                            if (!dryRun) exercise.Name = translated;
                            traduzidos++;
                        }
                    }

                    // 2. Adicionar descricao se nao tiver
                    if (string.IsNullOrWhiteSpace(exercise.Description))
                    {
                        var description = ExerciseTranslations.GenerateDescription(exercise.Name, exercise.MuscleGroup.ToString());
                        changes.Add($"Descricao adicionada");
                        if (!dryRun) exercise.Description = description;
                        descricoes++;
                    }

                    // 3. Adicionar URL de video do YouTube se nao tiver
                    if (string.IsNullOrWhiteSpace(exercise.VideoUrl))
                    {
                        var videoUrl = ExerciseTranslations.GenerateYouTubeSearchUrl(exercise.Name);
                        changes.Add($"Video URL adicionada");
                        if (!dryRun) exercise.VideoUrl = videoUrl;
                        videos++;
                    }

                    // 4. Adicionar URL de imagem placeholder se nao tiver
                    if (string.IsNullOrWhiteSpace(exercise.ImageUrl))
                    {
                        var imageUrl = ExerciseTranslations.GeneratePlaceholderImageUrl(exercise.Name, exercise.MuscleGroup.ToString());
                        changes.Add($"Imagem URL adicionada");
                        if (!dryRun) exercise.ImageUrl = imageUrl;
                        imagens++;
                    }

                    if (changes.Count > 0)
                    {
                        fixes.Add(new { id = exercise.Id, name = exercise.Name, changes });
                    }
                }

                if (!dryRun && (traduzidos > 0 || descricoes > 0 || videos > 0 || imagens > 0))
                {
                    await context.SaveChangesAsync();
                }

                logger.LogInformation("Exercise fix completed: {Traduzidos} traduzidos, {Descricoes} descricoes, {Videos} videos, {Imagens} imagens",
                    traduzidos, descricoes, videos, imagens);

                return Results.Ok(new
                {
                    message = dryRun ? "Simulacao concluida (nenhuma alteracao salva)" : "Exercicios corrigidos com sucesso!",
                    dryRun,
                    traduzidos,
                    descricoes,
                    videos,
                    imagens,
                    totalAlteracoes = traduzidos + descricoes + videos + imagens,
                    fixes = fixes.Take(100), // Limitar a 100 para nao sobrecarregar a resposta
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to fix exercises");
                return Results.Problem(
                    title: "Fix failed",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("FixExercises")
        .WithSummary("Corrige exercicios: traduz nomes, adiciona descricoes, videos e imagens. Use dryRun=true para simular.");

        group.MapGet("/exercises-needing-translation", async (
            ApplicationDbContext context,
            [FromQuery] int limit = 100) =>
        {
            var exercises = await context.Exercises
                .OrderBy(e => e.Name)
                .ToListAsync();

            var needingTranslation = exercises
                .Where(e => ExerciseTranslations.NeedsTranslation(e.Name))
                .Take(limit)
                .Select(e => new
                {
                    e.Id,
                    currentName = e.Name,
                    suggestedTranslation = ExerciseTranslations.Translate(e.Name),
                    muscleGroup = e.MuscleGroup.ToString(),
                    hasDescription = !string.IsNullOrWhiteSpace(e.Description),
                    hasVideo = !string.IsNullOrWhiteSpace(e.VideoUrl),
                    hasImage = !string.IsNullOrWhiteSpace(e.ImageUrl)
                })
                .ToList();

            return Results.Ok(new
            {
                total = needingTranslation.Count,
                exercises = needingTranslation
            });
        })
        .WithName("GetExercisesNeedingTranslation")
        .WithSummary("Lista exercicios que precisam de traducao com sugestoes");

        group.MapPost("/translate-exercise/{id}", async (
            Guid id,
            [FromBody] TranslateExerciseRequest request,
            ApplicationDbContext context,
            ILogger<Program> logger) =>
        {
            var exercise = await context.Exercises.FindAsync(id);
            if (exercise == null)
            {
                return Results.NotFound("Exercicio nao encontrado");
            }

            var oldName = exercise.Name;
            exercise.Name = request.NewName;

            if (!string.IsNullOrWhiteSpace(request.NewDescription))
            {
                exercise.Description = request.NewDescription;
            }

            await context.SaveChangesAsync();

            logger.LogInformation("Exercise translated: {OldName} -> {NewName}", oldName, request.NewName);

            return Results.Ok(new
            {
                message = "Exercicio traduzido com sucesso",
                id = exercise.Id,
                oldName,
                newName = exercise.Name
            });
        })
        .WithName("TranslateExercise")
        .WithSummary("Traduz manualmente um exercicio especifico");
    }
}

public record TranslateExerciseRequest(string NewName, string? NewDescription = null);