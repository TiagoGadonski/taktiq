namespace GymHero.Domain.Entities;

public class User : BaseEntity
{
    // 'string.Empty' inicializa a string para evitar avisos de nulidade do .NET.
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public ICollection<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();

    public DateTime? DateOfBirth { get; set; }
    public string? Location { get; set; }
    public string? Bio { get; set; }
    public double? Height { get; set; } // em cm
    public double? Weight { get; set; } // em kg

    // --- Propriedade para a Etapa 2 ---
    public string? ProfilePictureUrl { get; set; }
    public string? GymName { get; set; } // Nome do ginásio onde treina
    public string? PhoneNumber { get; set; } // Número de telefone

    public string Role { get; set; } = "Aluno";
    public bool IsActive { get; set; } = true; // Indica se o usuário está ativo ou não
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