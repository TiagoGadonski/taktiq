using GymHero.Domain.Entities;
namespace GymHero.Application.Common.Interfaces;
public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}