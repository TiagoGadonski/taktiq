using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace GymHero.Infrastructure.Services;

/// <summary>
/// Seeds development environment with fake data for testing
/// DO NOT USE IN PRODUCTION!
/// </summary>
public class DevelopmentSeederService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    // Common password for all dev users: "Dev@123456"
    private const string DEV_PASSWORD = "Dev@123456";

    public DevelopmentSeederService(
        IApplicationDbContext context,
        IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAllAsync(CancellationToken cancellationToken = default)
    {
        // Check if already seeded
        if (await _context.Users.AnyAsync(cancellationToken))
        {
            Console.WriteLine("[DEV SEED] Database already has users. Skipping seed.");
            return;
        }

        Console.WriteLine("[DEV SEED] Starting development data seed...");

        // 1. Seed Personal Trainers
        var trainers = await SeedPersonalTrainersAsync(cancellationToken);
        Console.WriteLine($"[DEV SEED] ✓ Created {trainers.Count} Personal Trainers");

        // 2. Seed Students
        var students = await SeedStudentsAsync(trainers, cancellationToken);
        Console.WriteLine($"[DEV SEED] ✓ Created {students.Count} Students");

        // 3. Seed Exercises (using existing seeder if available)
        // Note: This should be called separately or you can integrate here
        Console.WriteLine("[DEV SEED] ℹ Exercises should be seeded via ExerciseSeederService");

        // 4. Seed Workout Plans
        var plans = await SeedWorkoutPlansAsync(trainers, students, cancellationToken);
        Console.WriteLine($"[DEV SEED] ✓ Created {plans.Count} Workout Plans");

        // 5. Seed Friendships
        var friendships = await SeedFriendshipsAsync(students, cancellationToken);
        Console.WriteLine($"[DEV SEED] ✓ Created {friendships} Friendships");

        Console.WriteLine("[DEV SEED] ✅ Development seed completed!");
        Console.WriteLine($"[DEV SEED] Login with any user using password: {DEV_PASSWORD}");
        Console.WriteLine("[DEV SEED] Example PTs:");
        foreach (var trainer in trainers.Take(3))
        {
            Console.WriteLine($"  - {trainer.Email}");
        }
        Console.WriteLine("[DEV SEED] Example Students:");
        foreach (var student in students.Take(3))
        {
            Console.WriteLine($"  - {student.Email}");
        }
    }

    private async Task<List<User>> SeedPersonalTrainersAsync(CancellationToken cancellationToken)
    {
        var trainers = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Tiago Cordeiro",
                Email = "tiago@taktiq.app",
                PasswordHash = _passwordHasher.HashPassword(null!, DEV_PASSWORD),
                Role = "PersonalTrainer",
                IsActive = true,
                ProfileSlug = "tiago-cordeiro",
                Bio = "Personal Trainer especializado em musculação e hipertrofia. Transforme seu corpo com treinos personalizados!",
                Location = "São Paulo, SP",
                Specialization = "Musculação, Hipertrofia",
                Education = "CREF 123456-G/SP, Bacharel em Educação Física - USP",
                Experience = "10 anos de experiência, ex-atleta de fisiculturismo",
                PricingInfo = "Planos a partir de R$ 150/mês",
                Philosophy = "Treino inteligente e progressivo para resultados duradouros",
                YearsExperience = 10,
                ClientsCount = 150,
                SuccessStoriesCount = 45,
                IsPublicProfile = true,
                InstagramUrl = "https://instagram.com/tiagocordeiro",
                DateOfBirth = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                Gender = "M",
                Height = 178,
                Weight = 82,
                CreatedAt = DateTime.UtcNow.AddMonths(-24)
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Mariana Silva",
                Email = "mariana@taktiq.app",
                PasswordHash = _passwordHasher.HashPassword(null!, DEV_PASSWORD),
                Role = "PersonalTrainer",
                IsActive = true,
                ProfileSlug = "mariana-silva",
                Bio = "Especialista em treinamento funcional e emagrecimento. Vamos alcançar seus objetivos juntos!",
                Location = "Rio de Janeiro, RJ",
                Specialization = "Funcional, Emagrecimento, HIIT",
                Education = "CREF 789012-G/RJ, Pós-graduação em Treinamento Funcional",
                Experience = "7 anos ajudando pessoas a transformarem suas vidas",
                PricingInfo = "Consulte planos personalizados",
                Philosophy = "Movimento é vida. Treino funcional para o dia a dia.",
                YearsExperience = 7,
                ClientsCount = 95,
                SuccessStoriesCount = 30,
                IsPublicProfile = true,
                InstagramUrl = "https://instagram.com/marianasilvapt",
                FacebookUrl = "https://facebook.com/marianasilvapt",
                DateOfBirth = new DateTime(1992, 8, 22, 0, 0, 0, DateTimeKind.Utc),
                Gender = "F",
                Height = 165,
                Weight = 58,
                CreatedAt = DateTime.UtcNow.AddMonths(-18)
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Carlos Mendes",
                Email = "carlos@taktiq.app",
                PasswordHash = _passwordHasher.HashPassword(null!, DEV_PASSWORD),
                Role = "PersonalTrainer",
                IsActive = true,
                ProfileSlug = "carlos-mendes",
                Bio = "CrossFit Coach e especialista em condicionamento físico. Supere seus limites!",
                Location = "Belo Horizonte, MG",
                Specialization = "CrossFit, Condicionamento Físico",
                Education = "CREF 345678-G/MG, CrossFit Level 2 Trainer",
                Experience = "5 anos no CrossFit, ex-atleta de atletismo",
                PricingInfo = "Treinos em grupo e individuais disponíveis",
                Philosophy = "Intensidade, comunidade e resultados",
                YearsExperience = 5,
                ClientsCount = 120,
                SuccessStoriesCount = 35,
                IsPublicProfile = true,
                InstagramUrl = "https://instagram.com/carlosmendesfit",
                WebsiteUrl = "https://carlosmendesfit.com",
                DateOfBirth = new DateTime(1988, 3, 10, 0, 0, 0, DateTimeKind.Utc),
                Gender = "M",
                Height = 180,
                Weight = 85,
                CreatedAt = DateTime.UtcNow.AddMonths(-12)
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Fernanda Costa",
                Email = "fernanda@taktiq.app",
                PasswordHash = _passwordHasher.HashPassword(null!, DEV_PASSWORD),
                Role = "PersonalTrainer",
                IsActive = true,
                ProfileSlug = "fernanda-costa",
                Bio = "Yoga, Pilates e bem-estar. Encontre o equilíbrio perfeito entre corpo e mente.",
                Location = "Curitiba, PR",
                Specialization = "Yoga, Pilates, Alongamento",
                Education = "CREF 901234-G/PR, Instrutora de Yoga certificada (200h)",
                Experience = "8 anos ensinando práticas de bem-estar",
                PricingInfo = "Sessões individuais e em grupo",
                Philosophy = "Saúde holística: corpo, mente e espírito",
                YearsExperience = 8,
                ClientsCount = 75,
                SuccessStoriesCount = 25,
                IsPublicProfile = true,
                InstagramUrl = "https://instagram.com/fernandacostayoga",
                DateOfBirth = new DateTime(1991, 11, 5, 0, 0, 0, DateTimeKind.Utc),
                Gender = "F",
                Height = 168,
                Weight = 60,
                CreatedAt = DateTime.UtcNow.AddMonths(-15)
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Roberto Alves",
                Email = "roberto@taktiq.app",
                PasswordHash = _passwordHasher.HashPassword(null!, DEV_PASSWORD),
                Role = "PersonalTrainer",
                IsActive = true,
                ProfileSlug = "roberto-alves",
                Bio = "Especialista em reabilitação e treino para terceira idade. Nunca é tarde para começar!",
                Location = "Porto Alegre, RS",
                Specialization = "Reabilitação, Terceira Idade",
                Education = "CREF 567890-G/RS, Especialização em Gerontologia",
                Experience = "12 anos trabalhando com reabilitação física",
                PricingInfo = "Atendimento especializado para todas as idades",
                Philosophy = "Movimento é remédio. Qualidade de vida em qualquer idade.",
                YearsExperience = 12,
                ClientsCount = 85,
                SuccessStoriesCount = 40,
                IsPublicProfile = true,
                FacebookUrl = "https://facebook.com/robertoalvespt",
                WebsiteUrl = "https://robertoalves.fit",
                DateOfBirth = new DateTime(1985, 1, 20, 0, 0, 0, DateTimeKind.Utc),
                Gender = "M",
                Height = 175,
                Weight = 78,
                CreatedAt = DateTime.UtcNow.AddMonths(-30)
            }
        };

        await _context.Users.AddRangeAsync(trainers, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return trainers;
    }

    private async Task<List<User>> SeedStudentsAsync(List<User> trainers, CancellationToken cancellationToken)
    {
        var students = new List<User>();
        var random = new Random(42); // Fixed seed for reproducibility

        var firstNames = new[] { "Ana", "Bruno", "Carla", "Daniel", "Eduardo", "Flávia", "Gabriel", "Helena", "Igor", "Juliana",
                                 "Lucas", "Marina", "Nicolas", "Olivia", "Pedro", "Rafaela", "Samuel", "Tatiana", "Vitor", "Yasmin" };
        var lastNames = new[] { "Santos", "Oliveira", "Pereira", "Costa", "Rodrigues", "Almeida", "Nascimento", "Lima", "Araújo", "Fernandes" };
        var goals = new[] { "Perder peso", "Ganhar massa muscular", "Melhorar saúde", "Aumentar resistência", "Definição muscular" };
        var locations = new[] { "São Paulo, SP", "Rio de Janeiro, RJ", "Belo Horizonte, MG", "Brasília, DF", "Salvador, BA" };

        for (int i = 0; i < 20; i++)
        {
            var firstName = firstNames[i];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var fullName = $"{firstName} {lastName}";
            var email = $"{firstName.ToLower()}.{lastName.ToLower()}{(i > 9 ? i.ToString() : "")}@email.com";

            // Assign to a trainer (distribute evenly)
            var assignedTrainer = trainers[i % trainers.Count];

            var student = new User
            {
                Id = Guid.NewGuid(),
                Name = fullName,
                Email = email,
                PasswordHash = _passwordHasher.HashPassword(null!, DEV_PASSWORD),
                Role = "Aluno",
                IsActive = true,
                PersonalTrainerId = random.Next(100) < 70 ? assignedTrainer.Id : null, // 70% have a PT assigned
                Bio = $"Praticante de exercícios há {random.Next(1, 5)} anos",
                Location = locations[random.Next(locations.Length)],
                ExerciseGoal = goals[random.Next(goals.Length)],
                DateOfBirth = DateTime.UtcNow.AddYears(-random.Next(20, 50)),
                Gender = i % 2 == 0 ? "M" : "F",
                Height = random.Next(155, 195),
                Weight = random.Next(55, 100),
                PreferredWorkoutLocation = (WorkoutLocation)random.Next(0, 3),
                PracticesBoxing = random.Next(100) < 20, // 20% practice boxing
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 365))
            };

            students.Add(student);
        }

        await _context.Users.AddRangeAsync(students, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return students;
    }

    private async Task<List<WorkoutPlan>> SeedWorkoutPlansAsync(List<User> trainers, List<User> students, CancellationToken cancellationToken)
    {
        var plans = new List<WorkoutPlan>();
        var random = new Random(42);

        var planNames = new[]
        {
            "Hipertrofia Iniciante 8 Semanas",
            "Emagrecimento HIIT 12 Semanas",
            "Funcional Full Body",
            "CrossFit WODs Avançado",
            "Yoga & Pilates Combo",
            "Treino em Casa - Sem Equipamento",
            "Musculação ABC 12 Semanas",
            "Definição Muscular Verão",
            "Treino para Iniciantes",
            "Força e Potência"
        };

        var planDescriptions = new[]
        {
            "Programa completo focado em ganho de massa muscular para iniciantes",
            "Treinos de alta intensidade para máxima queima de gordura",
            "Treino funcional para todas as partes do corpo",
            "WODs desafiadores para atletas experientes",
            "Combinação perfeita de flexibilidade e força",
            "Treinos eficazes que você pode fazer em casa sem nenhum equipamento",
            "Divisão clássica ABC para desenvolvimento muscular",
            "Prepare seu corpo para o verão com este plano de definição",
            "Perfeito para quem está começando sua jornada fitness",
            "Desenvolva força e potência explosiva"
        };

        for (int i = 0; i < planNames.Length; i++)
        {
            var owner = i < 5 ? trainers[i % trainers.Count] : students[i % students.Count];
            var isFromTrainer = i < 5;

            var plan = new WorkoutPlan
            {
                Id = Guid.NewGuid(),
                OwnerId = owner.Id,
                Name = planNames[i],
                Description = planDescriptions[i],
                Goal = i % 3 == 0 ? "Hipertrofia" : i % 3 == 1 ? "Emagrecimento" : "Condicionamento",
                Duration = random.Next(4, 16),
                IsActive = random.Next(100) < 30, // 30% are active
                StartDate = DateTime.UtcNow.AddDays(-random.Next(0, 60)),
                ExpirationDate = DateTime.UtcNow.AddDays(random.Next(30, 120)),
                VisibilityLevel = (VisibilityLevel)random.Next(0, 3),
                IsPublic = isFromTrainer && random.Next(100) < 60, // 60% of trainer plans are public
                AllowCopying = true,
                ViewCount = random.Next(0, 500),
                ForSale = isFromTrainer && random.Next(100) < 40, // 40% of trainer plans are for sale
                Price = isFromTrainer && random.Next(100) < 40 ? random.Next(50, 300) : null,
                PublishedAt = isFromTrainer ? DateTime.UtcNow.AddDays(-random.Next(0, 30)) : null,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 90))
            };

            plans.Add(plan);
        }

        await _context.WorkoutPlans.AddRangeAsync(plans, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return plans;
    }

    private async Task<int> SeedFriendshipsAsync(List<User> students, CancellationToken cancellationToken)
    {
        var friendships = new List<Friendship>();
        var random = new Random(42);
        var friendshipCount = 0;

        // Create some friendships between students
        for (int i = 0; i < 15; i++)
        {
            var user1 = students[random.Next(students.Count)];
            var user2 = students[random.Next(students.Count)];

            if (user1.Id == user2.Id) continue;

            // Check if friendship already exists
            var exists = friendships.Any(f =>
                (f.RequesterId == user1.Id && f.AddresseeId == user2.Id) ||
                (f.RequesterId == user2.Id && f.AddresseeId == user1.Id));

            if (exists) continue;

            var status = random.Next(100) < 70 ? FriendshipStatus.Accepted : FriendshipStatus.Pending;

            var friendship = new Friendship
            {
                Id = Guid.NewGuid(),
                RequesterId = user1.Id,
                AddresseeId = user2.Id,
                Status = status,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180))
            };

            friendships.Add(friendship);
            friendshipCount++;
        }

        await _context.Friendships.AddRangeAsync(friendships, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return friendshipCount;
    }
}
