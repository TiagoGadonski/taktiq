using System.Security.Authentication; // Para a exceção
using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Auth.Queries;

public class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginQueryHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        // 1. Encontrar o usuário pelo email
        var user = await _context.Users
            .AsNoTracking() // Usamos AsNoTracking pois é uma operação de leitura
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        // 2. Verificar se o usuário existe e se a senha está correta
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            // Lançamos uma exceção específica para falha na autenticação
            throw new AuthenticationException("Invalid email or password.");
        }

        // 3. Se tudo estiver correto, gerar o token
        var token = _jwtTokenGenerator.GenerateToken(user);

        // 4. Retornar a resposta com os dados do usuário e o token
        return new AuthResponse(user.Id, user.Name, user.Email, token);
    }
}