using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using GymHero.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using GymHero.Infrastructure.Services;

namespace GymHero.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
                       .WithTags("Admin")
                       .RequireAuthorization("RequireAdminRole");

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
                    u.ProfilePictureUrl
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

        // Delete user
        group.MapDelete("/users/{userId}", async (Guid userId, IApplicationDbContext context, CancellationToken cancellationToken) =>
        {
            var user = await context.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);
            if (user is null) return Results.NotFound("User not found.");

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
            var validRoles = new[] { "User", "PersonalTrainer", "Admin" };
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
    }
}