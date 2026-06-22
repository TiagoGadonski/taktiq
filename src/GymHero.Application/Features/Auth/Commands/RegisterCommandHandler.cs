using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GymHero.Application.Features.Auth.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (userExists)
            throw new Exception("User with this email already exists.");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            PreferredWorkoutLocation = request.PreferredWorkoutLocation,
            Role = request.IsPersonalTrainer ? "PersonalTrainer" : "Aluno",
        };

        if (request.IsPersonalTrainer)
        {
            user.ProfileSlug = await GenerateUniqueSlugAsync(request.Name, request.Email, cancellationToken);
            user.IsPublicProfile = true;
        }

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await AssignDefaultChallenges(user, cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(user);
        return new AuthResponse(user.Id, user.Name, user.Email, token);
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, string email, CancellationToken cancellationToken)
    {
        var baseSlug = ToSlug(name);

        if (string.IsNullOrEmpty(baseSlug) || baseSlug.Length < 2)
            baseSlug = ToSlug(email.Split('@')[0]);

        if (string.IsNullOrEmpty(baseSlug) || baseSlug.Length < 2)
            baseSlug = "personal";

        var candidate = baseSlug;
        var suffix = 2;

        while (await _context.Users.AnyAsync(u => u.ProfileSlug == candidate, cancellationToken))
        {
            candidate = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return candidate;
    }

    private static string ToSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var slug = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"[\s]+", "-");
        slug = slug.Trim('-');
        return slug;
    }

    private async Task AssignDefaultChallenges(User user, CancellationToken cancellationToken)
    {
        var defaultChallenges = await _context.Challenges
            .Where(c => c.IsDefault &&
                       (c.TargetType == Domain.Enums.ChallengeTargetType.AllUsers ||
                        (c.TargetType == Domain.Enums.ChallengeTargetType.AllTrainers && user.Role == "PersonalTrainer")))
            .ToListAsync(cancellationToken);

        foreach (var challenge in defaultChallenges)
        {
            var progress = new ChallengeProgress
            {
                ChallengeId = challenge.Id,
                ParticipantId = user.Id,
                CurrentValue = 0,
                LastUpdate = DateTime.UtcNow
            };

            await _context.ChallengeProgresses.AddAsync(progress, cancellationToken);
        }

        if (defaultChallenges.Any())
            await _context.SaveChangesAsync(cancellationToken);
    }
}
