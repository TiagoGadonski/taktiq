using GymHero.Application.Common.Interfaces;

namespace GymHero.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    // Usamos a biblioteca BCrypt.Net-Next
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}