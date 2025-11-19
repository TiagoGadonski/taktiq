using GymHero.Domain.Enums;

namespace GymHero.Domain.Entities;

public class User : BaseEntity
{
    // 'string.Empty' inicializa a string para evitar avisos de nulidade do .NET.
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public ICollection<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();

    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; } // "M" (Masculino) ou "F" (Feminino)
    public string? Injuries { get; set; } // Lesões/limitações separadas por vírgula (ex: "knee pain,lower back,shoulder")
    public string? HealthConditions { get; set; } // Condições de saúde/doenças (ex: "diabetes,hipertensão,asma")
    public string? ExerciseGoal { get; set; } // Objetivo do exercício (ex: "perder peso", "ganhar massa muscular", "melhorar saúde")
    public string? Location { get; set; }
    public string? Bio { get; set; }
    public double? Height { get; set; } // em cm
    public double? Weight { get; set; } // em kg

    // Workout Location Preference (Gym, Home, or Both)
    public WorkoutLocation PreferredWorkoutLocation { get; set; } = WorkoutLocation.Gym;

    // ✅ NEW: Boxing Practice - User practices boxing as supplementary training
    public bool PracticesBoxing { get; set; } = false;

    // --- Propriedade para a Etapa 2 ---
    public string? ProfilePictureUrl { get; set; }
    public string? GymName { get; set; } // Nome do ginásio onde treina
    public string? PhoneNumber { get; set; } // Número de telefone

    // --- Training Split Configuration ---
    // JSON string mapping day of week (0-6) to muscle group focus
    // Example: {"0":"Rest","1":"Legs","2":"Chest & Triceps","3":"Back & Biceps","4":"Shoulders","5":"Full Body","6":"Rest"}
    public string? TrainingSplit { get; set; }

    public string Role { get; set; } = "Aluno";
    public bool IsActive { get; set; } = true; // Indica se o usuário está ativo ou não
    public DateTime? LastLoginAt { get; set; } // Data/hora do último login do usuário

    // --- PERSONAL TRAINER PROFILE FIELDS ---
    public string? ProfileSlug { get; set; } // Custom URL slug (e.g., "tiago-cordeiro" for taktiq.app/trainer/tiago-cordeiro)
    public string? Specialization { get; set; } // Área de especialização (ex: "Musculação", "Funcional", "Crossfit")
    public string? Education { get; set; } // Formação acadêmica e certificações
    public string? Experience { get; set; } // Experiência profissional e carreira
    public string? PricingInfo { get; set; } // Informações sobre preços e planos
    public bool IsPublicProfile { get; set; } = false; // Se o perfil público está ativo
    public string? InstagramUrl { get; set; } // URL do Instagram
    public string? FacebookUrl { get; set; } // URL do Facebook
    public string? WebsiteUrl { get; set; } // URL do site pessoal
    // --- RELAÇÃO PARA O ALUNO ---
    // Se este utilizador for um "Aluno", aqui guardaremos o ID do seu Personal Trainer.
    // A interrogação (?) indica que pode ser nulo (um aluno pode não ter um personal).
    public Guid? PersonalTrainerId { get; set; }
    public User? PersonalTrainer { get; set; }

    // --- RELAÇÃO PARA O PERSONAL TRAINER ---
    // Se este utilizador for um "Personal", esta coleção guardará a lista dos seus alunos.
    public ICollection<User> Clients { get; set; } = new List<User>();

    // Pedidos de amizade que ESTE utilizador enviou
    public ICollection<Friendship> SentFriendRequests { get; set; } = new List<Friendship>();
    
    // Pedidos de amizade que ESTE utilizador recebeu
    public ICollection<Friendship> ReceivedFriendRequests { get; set; } = new List<Friendship>();
}