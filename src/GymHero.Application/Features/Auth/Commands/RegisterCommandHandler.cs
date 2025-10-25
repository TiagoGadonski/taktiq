using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore; // Usamos extensões como 'AnyAsync'

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
        // 1. Regra de Negócio: Validar se o email já existe
        var userExists = await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (userExists)
        {
            // Poderíamos usar uma exceção customizada aqui.
            throw new Exception("User with this email already exists.");
        }

        // 2. Criar a entidade de domínio
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        // 3. Adicionar ao DbContext e salvar no banco
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // 4. Gerar o token JWT
        var token = _jwtTokenGenerator.GenerateToken(user);

        // 5. Retornar a resposta
        return new AuthResponse(user.Id, user.Name, user.Email, token);
    }
}