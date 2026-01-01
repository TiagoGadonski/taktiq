using GymHero.Domain.Entities;
using GymHero.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Infrastructure.Data.Seeders;

public static class ExerciseSeeder
{
    public static async Task SeedExercisesAsync(ApplicationDbContext context)
    {
        // Verificar se já existem exercícios
        if (await context.Exercises.AnyAsync())
        {
            Console.WriteLine("✅ Exercises already seeded. Skipping...");
            return;
        }

        Console.WriteLine("🌱 Seeding comprehensive exercise database...");

        var exercises = new List<Exercise>();

        // ===== PEITO (CHEST) - 20 exercícios =====
        exercises.AddRange(new[]
        {
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Supino Reto com Barra",
                Description = "Exercício composto fundamental para desenvolvimento do peitoral. Deite no banco reto, abaixe a barra até o peito e empurre para cima.",
                MuscleGroup = "chest",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Manter escápulas retraídas, cotovelos a 45° do corpo"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Supino Inclinado com Halteres",
                Description = "Desenvolvimento do peitoral superior. Banco inclinado 30-45°, halteres em pronação.",
                MuscleGroup = "chest",
                Equipment = "dumbbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Ideal para ativar fibras claviculares do peitoral"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Supino Declinado com Barra",
                Description = "Ênfase no peitoral inferior. Banco declinado 15-30°.",
                MuscleGroup = "chest",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Flexão de Braço",
                Description = "Exercício corporal clássico. Mãos alinhadas com ombros, corpo reto da cabeça aos pés.",
                MuscleGroup = "chest",
                Equipment = "bodyweight",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Variações: diamante, ampla, declinada, pike"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Crucifixo com Halteres",
                Description = "Isolamento do peitoral. Abertura controlada com leve flexão de cotovelos.",
                MuscleGroup = "chest",
                Equipment = "dumbbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Evitar hiperextensão dos cotovelos"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Crucifixo Inclinado",
                Description = "Isolamento do peitoral superior com halteres em banco inclinado.",
                MuscleGroup = "chest",
                Equipment = "dumbbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Crossover no Cabo",
                Description = "Isolamento com tensão constante. Puxar cabos de cima para baixo cruzando na frente.",
                MuscleGroup = "chest",
                Equipment = "cable",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Manter leve inclinação do tronco à frente"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Peck Deck (Voador)",
                Description = "Isolamento em máquina. Aproximar cotovelos na frente do corpo.",
                MuscleGroup = "chest",
                Equipment = "machine",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Supino na Máquina (Chest Press)",
                Description = "Supino guiado por máquina, seguro para iniciantes.",
                MuscleGroup = "chest",
                Equipment = "machine",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Pullover com Halter",
                Description = "Expansão da caixa torácica. Halter acima da cabeça, descer atrás mantendo braços levemente flexionados.",
                MuscleGroup = "chest",
                Equipment = "dumbbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Trabalha também dorsal e serrátil"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Flexão com Mãos Elevadas",
                Description = "Flexão com mãos em superfície elevada, ênfase no peitoral inferior.",
                MuscleGroup = "chest",
                Equipment = "bodyweight",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Flexão Declinada (Pés Elevados)",
                Description = "Pés em superfície elevada, maior ativação do peitoral superior.",
                MuscleGroup = "chest",
                Equipment = "bodyweight",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Flexão Diamante",
                Description = "Mãos próximas formando diamante, maior ativação de tríceps e peitoral interno.",
                MuscleGroup = "chest",
                Equipment = "bodyweight",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Supino com Pegada Fechada",
                Description = "Pegada mais estreita que ombros, ênfase em tríceps e peitoral interno.",
                MuscleGroup = "chest",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Landmine Press",
                Description = "Empurrar barra ancorada em landmine, movimento angular.",
                MuscleGroup = "chest",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Flexão em Anéis",
                Description = "Flexão em anéis de ginástica, maior instabilidade e ativação muscular.",
                MuscleGroup = "chest",
                Equipment = "gymnastics rings",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Cable Fly Baixo para Alto",
                Description = "Crucifixo no cabo de baixo para cima, ênfase em peitoral superior.",
                MuscleGroup = "chest",
                Equipment = "cable",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Flexão com Apoio em Bola Suíça",
                Description = "Mãos ou pés em bola suíça, maior ativação de estabilizadores.",
                MuscleGroup = "chest",
                Equipment = "swiss ball",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Dips para Peito",
                Description = "Paralelas com inclinação frontal, cotovelos afastados.",
                MuscleGroup = "chest",
                Equipment = "parallel bars",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Manter tronco inclinado à frente (~45°)"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Svend Press",
                Description = "Pressionar dois anilhas juntos na frente do peito, empurrar e recolher.",
                MuscleGroup = "chest",
                Equipment = "plate",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Excelente para peitoral interno"
            }
        });

        // ===== COSTAS (BACK) - 20 exercícios =====
        exercises.AddRange(new[]
        {
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Barra Fixa (Pull-up)",
                Description = "Suspensão com pegada pronada, puxar até queixo acima da barra.",
                MuscleGroup = "back",
                Equipment = "pull-up bar",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Rei dos exercícios de costas"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Barra Fixa Supinada (Chin-up)",
                Description = "Suspensão com pegada supinada, maior ativação de bíceps.",
                MuscleGroup = "back",
                Equipment = "pull-up bar",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Remada Curvada com Barra",
                Description = "Tronco inclinado 45°, puxar barra em direção ao abdômen.",
                MuscleGroup = "back",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Manter coluna neutra, core ativado"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Remada com Halteres (Unilateral)",
                Description = "Apoio unilateral no banco, puxar halter em direção ao quadril.",
                MuscleGroup = "back",
                Equipment = "dumbbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Evitar rotação do tronco"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Remada Cavalinho (Seal Row)",
                Description = "Deitado de bruços em banco inclinado, remada com halteres ou barra.",
                MuscleGroup = "back",
                Equipment = "dumbbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Elimina carga na lombar"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Levantamento Terra (Deadlift)",
                Description = "Exercício composto fundamental. Levantar barra do chão com coluna neutra.",
                MuscleGroup = "back",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Trabalha corpo inteiro, especialmente lombar e posteriores"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Levantamento Terra Romeno",
                Description = "Variação com pernas semi-estendidas, maior ênfase em isquiotibiais e lombar.",
                MuscleGroup = "back",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Pulldown na Polia (Lat Pulldown)",
                Description = "Puxada alta na polia, simulação de barra fixa.",
                MuscleGroup = "back",
                Equipment = "cable",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Remada Baixa no Cabo (Seated Cable Row)",
                Description = "Sentado, puxar cabo em direção ao abdômen.",
                MuscleGroup = "back",
                Equipment = "cable",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Face Pull",
                Description = "Puxar cabo na altura do rosto, cotovelos altos.",
                MuscleGroup = "back",
                Equipment = "cable",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Excelente para deltoide posterior e saúde dos ombros"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Remada Invertida (Inverted Row)",
                Description = "Suspensão em barra baixa, puxar corpo até barra na altura do peito.",
                MuscleGroup = "back",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Progressão para barra fixa"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "T-Bar Row",
                Description = "Remada com barra em landmine ou T-bar machine.",
                MuscleGroup = "back",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Remada Alta (High Pull)",
                Description = "Puxar barra ou cabo até altura do peito, cotovelos altos.",
                MuscleGroup = "back",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Pullover no Cabo",
                Description = "Puxar cabo de cima para baixo com braços estendidos.",
                MuscleGroup = "back",
                Equipment = "cable",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Remada em Máquina",
                Description = "Remada guiada por máquina, segura para iniciantes.",
                MuscleGroup = "back",
                Equipment = "machine",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Barra Fixa com Pegada Neutra",
                Description = "Pegada paralela, menor estresse nos ombros.",
                MuscleGroup = "back",
                Equipment = "pull-up bar",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Good Morning",
                Description = "Barra nos ombros, flexão de quadril mantendo joelhos semi-flexionados.",
                MuscleGroup = "back",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Fortalece lombar e isquiotibiais"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Remada Meadows",
                Description = "Remada unilateral com barra em landmine.",
                MuscleGroup = "back",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Shrug com Barra",
                Description = "Elevação de ombros com barra, trabalha trapézio.",
                MuscleGroup = "back",
                Equipment = "barbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Shrug com Halteres",
                Description = "Elevação de ombros com halteres, maior amplitude.",
                MuscleGroup = "back",
                Equipment = "dumbbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            }
        });

        // ===== PERNAS (LEGS) - 25 exercícios =====
        exercises.AddRange(new[]
        {
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Agachamento Livre (Back Squat)",
                Description = "Rei dos exercícios de perna. Barra nas costas, descer até paralelo ou abaixo.",
                MuscleGroup = "legs",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Manter joelhos alinhados, peito erguido"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Agachamento Frontal (Front Squat)",
                Description = "Barra na frente dos ombros, tronco mais vertical.",
                MuscleGroup = "legs",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Maior ativação de quadríceps"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Agachamento Bulgaro",
                Description = "Pé traseiro elevado, foco unilateral em quadríceps e glúteos.",
                MuscleGroup = "legs",
                Equipment = "dumbbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Leg Press 45°",
                Description = "Empurrar plataforma em máquina inclinada.",
                MuscleGroup = "legs",
                Equipment = "machine",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Leg Extension (Cadeira Extensora)",
                Description = "Extensão de joelhos sentado, isolamento de quadríceps.",
                MuscleGroup = "legs",
                Equipment = "machine",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Leg Curl Deitado",
                Description = "Flexão de joelhos deitado de bruços, isolamento de isquiotibiais.",
                MuscleGroup = "legs",
                Equipment = "machine",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Leg Curl Sentado",
                Description = "Flexão de joelhos sentado, variação de leg curl.",
                MuscleGroup = "legs",
                Equipment = "machine",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Stiff (Levantamento Terra com Pernas Retas)",
                Description = "Pernas estendidas, ênfase em isquiotibiais e glúteos.",
                MuscleGroup = "legs",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Avanço (Lunge)",
                Description = "Passo à frente com flexão de joelhos, alternando pernas.",
                MuscleGroup = "legs",
                Equipment = "bodyweight",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Avanço com Halteres",
                Description = "Lunge com halteres nas mãos para resistência adicional.",
                MuscleGroup = "legs",
                Equipment = "dumbbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Afundo Reverso",
                Description = "Passo para trás com flexão de joelhos.",
                MuscleGroup = "legs",
                Equipment = "bodyweight",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Agachamento Sumô",
                Description = "Stance largo com pés apontados para fora, ênfase em adutores.",
                MuscleGroup = "legs",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Agachamento Goblet",
                Description = "Segurando halter ou kettlebell no peito, agachamento profundo.",
                MuscleGroup = "legs",
                Equipment = "dumbbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Excelente para aprender mecânica do agachamento"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Hip Thrust",
                Description = "Ombros apoiados no banco, empurrar quadril para cima com barra.",
                MuscleGroup = "legs",
                Equipment = "barbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Melhor exercício para glúteos"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Glute Bridge",
                Description = "Deitado, empurrar quadril para cima.",
                MuscleGroup = "legs",
                Equipment = "bodyweight",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Step-Up",
                Description = "Subir em caixa ou banco, alternando pernas.",
                MuscleGroup = "legs",
                Equipment = "bodyweight",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Cadeira Abdutora",
                Description = "Abertura de pernas sentado, isolamento de abdutores.",
                MuscleGroup = "legs",
                Equipment = "machine",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Cadeira Adutora",
                Description = "Fechamento de pernas sentado, isolamento de adutores.",
                MuscleGroup = "legs",
                Equipment = "machine",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Panturrilha em Pé (Calf Raise)",
                Description = "Elevação de calcanhares em pé, isolamento de panturrilhas.",
                MuscleGroup = "legs",
                Equipment = "machine",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Panturrilha Sentado",
                Description = "Elevação de calcanhares sentado, ênfase em sóleo.",
                MuscleGroup = "legs",
                Equipment = "machine",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Sissy Squat",
                Description = "Agachamento com joelhos à frente, inclinação posterior do tronco.",
                MuscleGroup = "legs",
                Equipment = "bodyweight",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Isolamento avançado de quadríceps"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Hack Squat",
                Description = "Agachamento em máquina inclinada.",
                MuscleGroup = "legs",
                Equipment = "machine",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Nordic Hamstring Curl",
                Description = "Flexão de isquiotibiais excêntrica, joelhos fixos.",
                MuscleGroup = "legs",
                Equipment = "bodyweight",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Excelente para prevenção de lesões"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Wall Sit (Cadeira na Parede)",
                Description = "Posição estática de agachamento apoiado na parede.",
                MuscleGroup = "legs",
                Equipment = "bodyweight",
                Category = "isometric",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Single Leg Deadlift",
                Description = "Levantamento terra unilateral, equilíbrio e isquiotibiais.",
                MuscleGroup = "legs",
                Equipment = "dumbbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            }
        });

        // ===== OMBROS (SHOULDERS) - 18 exercícios =====
        exercises.AddRange(new[]
        {
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Desenvolvimento com Barra (Overhead Press)",
                Description = "Empurrar barra acima da cabeça em pé ou sentado.",
                MuscleGroup = "shoulders",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Exercício fundamental para ombros"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Desenvolvimento com Halteres",
                Description = "Empurrar halteres acima da cabeça, maior amplitude que barra.",
                MuscleGroup = "shoulders",
                Equipment = "dumbbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Desenvolvimento Arnold",
                Description = "Rotação de halteres durante o desenvolvimento, criado por Arnold Schwarzenegger.",
                MuscleGroup = "shoulders",
                Equipment = "dumbbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Elevação Lateral com Halteres",
                Description = "Elevar halteres lateralmente até altura dos ombros.",
                MuscleGroup = "shoulders",
                Equipment = "dumbbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Isolamento de deltoide medial"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Elevação Frontal",
                Description = "Elevar halteres ou barra à frente até altura dos ombros.",
                MuscleGroup = "shoulders",
                Equipment = "dumbbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Isolamento de deltoide anterior"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Remada Alta com Barra",
                Description = "Puxar barra até altura do peito, cotovelos altos.",
                MuscleGroup = "shoulders",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Crucifixo Inverso",
                Description = "Inclinado à frente, abertura lateral com halteres.",
                MuscleGroup = "shoulders",
                Equipment = "dumbbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Isolamento de deltoide posterior"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Peck Deck Inverso",
                Description = "Máquina de crucifixo inverso para deltoide posterior.",
                MuscleGroup = "shoulders",
                Equipment = "machine",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Desenvolvimento na Máquina",
                Description = "Press de ombros guiado por máquina.",
                MuscleGroup = "shoulders",
                Equipment = "machine",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Elevação Lateral no Cabo",
                Description = "Elevação lateral com cabo para tensão constante.",
                MuscleGroup = "shoulders",
                Equipment = "cable",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Face Pull",
                Description = "Puxar cabo na altura do rosto, excelente para deltoide posterior.",
                MuscleGroup = "shoulders",
                Equipment = "cable",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Crucial para saúde dos ombros"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Handstand Push-up",
                Description = "Flexão em parada de mão, avançado.",
                MuscleGroup = "shoulders",
                Equipment = "bodyweight",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Requer força e equilíbrio avançados"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Pike Push-up",
                Description = "Flexão em V invertido, progressão para handstand push-up.",
                MuscleGroup = "shoulders",
                Equipment = "bodyweight",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Rotação Externa com Cabo",
                Description = "Rotação externa de ombros, fortalecimento do manguito rotador.",
                MuscleGroup = "shoulders",
                Equipment = "cable",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Prevenção de lesões"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Y-Raise",
                Description = "Elevação em Y inclinado à frente, ativa deltoide posterior e trapézio.",
                MuscleGroup = "shoulders",
                Equipment = "dumbbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "W-Raise",
                Description = "Movimento em W, trabalha toda musculatura posterior do ombro.",
                MuscleGroup = "shoulders",
                Equipment = "dumbbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Landmine Press Unilateral",
                Description = "Press unilateral com barra em landmine.",
                MuscleGroup = "shoulders",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Band Pull-Apart",
                Description = "Abrir faixa elástica na altura do peito.",
                MuscleGroup = "shoulders",
                Equipment = "resistance band",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Excelente para aquecimento e reabilitação"
            }
        });

        // ===== BÍCEPS - 12 exercícios =====
        exercises.AddRange(new[]
        {
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Rosca Direta com Barra",
                Description = "Flexão de cotovelos com barra reta, pegada supinada.",
                MuscleGroup = "biceps",
                Equipment = "barbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Rosca Direta com Barra W (EZ Bar)",
                Description = "Rosca com barra angulada, menor estresse nos pulsos.",
                MuscleGroup = "biceps",
                Equipment = "ez bar",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Rosca Alternada com Halteres",
                Description = "Flexão alternada de cotovelos com halteres.",
                MuscleGroup = "biceps",
                Equipment = "dumbbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Rosca Martelo",
                Description = "Flexão com pegada neutra, trabalha braquial e braquiorradial.",
                MuscleGroup = "biceps",
                Equipment = "dumbbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Rosca Concentrada",
                Description = "Sentado, cotovelo apoiado na coxa interna.",
                MuscleGroup = "biceps",
                Equipment = "dumbbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Máxima contração e isolamento"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Rosca Scott (Preacher Curl)",
                Description = "Braços apoiados em banco scott.",
                MuscleGroup = "biceps",
                Equipment = "ez bar",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Elimina balanço, isolamento puro"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Rosca no Cabo",
                Description = "Flexão de cotovelos na polia baixa.",
                MuscleGroup = "biceps",
                Equipment = "cable",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Tensão constante"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Rosca Inversa",
                Description = "Pegada pronada, trabalha braquiorradial e antebraços.",
                MuscleGroup = "biceps",
                Equipment = "barbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Rosca 21 (21s)",
                Description = "7 reps parciais inferior + 7 superior + 7 completas.",
                MuscleGroup = "biceps",
                Equipment = "barbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Técnica de intensidade"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Rosca Aranha (Spider Curl)",
                Description = "Banco inclinado, braços pendentes à frente.",
                MuscleGroup = "biceps",
                Equipment = "dumbbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Rosca Inclinada",
                Description = "Banco inclinado 45°, maior alongamento do bíceps.",
                MuscleGroup = "biceps",
                Equipment = "dumbbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Chin-up (Barra Fixa Supinada)",
                Description = "Suspensão com pegada supinada, composto para bíceps.",
                MuscleGroup = "biceps",
                Equipment = "pull-up bar",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            }
        });

        // ===== TRÍCEPS - 12 exercícios =====
        exercises.AddRange(new[]
        {
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Tríceps Testa (Skull Crusher)",
                Description = "Deitado, extensão de cotovelos baixando barra em direção à testa.",
                MuscleGroup = "triceps",
                Equipment = "ez bar",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Tríceps Francês",
                Description = "Sentado ou em pé, extensão de cotovelos atrás da cabeça.",
                MuscleGroup = "triceps",
                Equipment = "dumbbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Tríceps Pulley (Puxada no Cabo)",
                Description = "Extensão de cotovelos puxando cabo para baixo.",
                MuscleGroup = "triceps",
                Equipment = "cable",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Tríceps Corda (Rope Pushdown)",
                Description = "Puxada com corda, abertura no final do movimento.",
                MuscleGroup = "triceps",
                Equipment = "cable",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Tríceps Coice (Kickback)",
                Description = "Inclinado à frente, extensão de cotovelos para trás.",
                MuscleGroup = "triceps",
                Equipment = "dumbbell",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Dips para Tríceps",
                Description = "Paralelas com tronco vertical, cotovelos próximos ao corpo.",
                MuscleGroup = "triceps",
                Equipment = "parallel bars",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Manter tronco vertical para focar tríceps"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Flexão com Pegada Fechada",
                Description = "Flexão com mãos próximas, ênfase em tríceps.",
                MuscleGroup = "triceps",
                Equipment = "bodyweight",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Tríceps Banco",
                Description = "Dips em banco, pés no chão ou elevados.",
                MuscleGroup = "triceps",
                Equipment = "bench",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "JM Press",
                Description = "Híbrido entre supino fechado e tríceps testa.",
                MuscleGroup = "triceps",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Overhead Extension no Cabo",
                Description = "Costas para o cabo, extensão acima da cabeça.",
                MuscleGroup = "triceps",
                Equipment = "cable",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Supino Fechado (Close-Grip Bench)",
                Description = "Supino com pegada estreita, trabalha tríceps e peitoral.",
                MuscleGroup = "triceps",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Diamond Push-up",
                Description = "Flexão com mãos formando diamante.",
                MuscleGroup = "triceps",
                Equipment = "bodyweight",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both
            }
        });

        // ===== CORE/ABDÔMEN - 18 exercícios =====
        exercises.AddRange(new[]
        {
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Prancha (Plank)",
                Description = "Posição estática de flexão sobre antebraços.",
                MuscleGroup = "core",
                Equipment = "bodyweight",
                Category = "isometric",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Fundamental para core stability"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Prancha Lateral",
                Description = "Apoio lateral sobre um antebraço.",
                MuscleGroup = "core",
                Equipment = "bodyweight",
                Category = "isometric",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Trabalha oblíquos"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Abdominal Supra (Crunch)",
                Description = "Deitado, elevação do tronco superior.",
                MuscleGroup = "core",
                Equipment = "bodyweight",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Abdominal Infra (Leg Raise)",
                Description = "Deitado, elevação das pernas.",
                MuscleGroup = "core",
                Equipment = "bodyweight",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Russian Twist",
                Description = "Sentado, rotação do tronco com ou sem peso.",
                MuscleGroup = "core",
                Equipment = "bodyweight",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Trabalha oblíquos"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Bicycle Crunch",
                Description = "Deitado, movimento de bicicleta com cotovelo tocando joelho oposto.",
                MuscleGroup = "core",
                Equipment = "bodyweight",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Mountain Climbers",
                Description = "Posição de flexão, alternando joelhos em direção ao peito.",
                MuscleGroup = "core",
                Equipment = "bodyweight",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Também trabalha cardio"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Dead Bug",
                Description = "Deitado de costas, extensão alternada de braço e perna oposta.",
                MuscleGroup = "core",
                Equipment = "bodyweight",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Excelente para estabilidade lombar"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Hollow Body Hold",
                Description = "Deitado em formato de banana, posição estática.",
                MuscleGroup = "core",
                Equipment = "bodyweight",
                Category = "isometric",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Sit-up",
                Description = "Elevação completa do tronco da posição deitada.",
                MuscleGroup = "core",
                Equipment = "bodyweight",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Abdominal na Polia (Cable Crunch)",
                Description = "Ajoelhado, puxar cabo fazendo flexão do tronco.",
                MuscleGroup = "core",
                Equipment = "cable",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Hanging Knee Raise",
                Description = "Suspenso na barra, elevar joelhos.",
                MuscleGroup = "core",
                Equipment = "pull-up bar",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Hanging Leg Raise",
                Description = "Suspenso na barra, elevar pernas estendidas.",
                MuscleGroup = "core",
                Equipment = "pull-up bar",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Versão avançada do knee raise"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Dragon Flag",
                Description = "Deitado em banco, elevar corpo inteiro mantendo apenas ombros apoiados.",
                MuscleGroup = "core",
                Equipment = "bench",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Exercício muito avançado"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Pallof Press",
                Description = "Anti-rotação com cabo, excelente para core stability.",
                MuscleGroup = "core",
                Equipment = "cable",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Ab Wheel Rollout",
                Description = "Rolar roda abdominal para frente e voltar.",
                MuscleGroup = "core",
                Equipment = "ab wheel",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Exercício muito eficaz e desafiador"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "V-Up",
                Description = "Deitado, elevar simultaneamente tronco e pernas formando V.",
                MuscleGroup = "core",
                Equipment = "bodyweight",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Woodchop (Lenhador)",
                Description = "Rotação diagonal puxando cabo de cima para baixo.",
                MuscleGroup = "core",
                Equipment = "cable",
                Category = "isolation",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Trabalha rotação e oblíquos"
            }
        });

        // ===== CARDIO E FUNCIONAIS - 15 exercícios =====
        exercises.AddRange(new[]
        {
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Burpee",
                Description = "Agachar, flexão, pular. Exercício full-body de alta intensidade.",
                MuscleGroup = "full body",
                Equipment = "bodyweight",
                Category = "cardio",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Excelente para HIIT"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Jumping Jacks (Polichinelos)",
                Description = "Saltar abrindo pernas e braços simultaneamente.",
                MuscleGroup = "full body",
                Equipment = "bodyweight",
                Category = "cardio",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Box Jump",
                Description = "Salto sobre caixa ou plataforma elevada.",
                MuscleGroup = "legs",
                Equipment = "box",
                Category = "plyometric",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Desenvolve potência"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Kettlebell Swing",
                Description = "Balanço de kettlebell usando quadril.",
                MuscleGroup = "full body",
                Equipment = "kettlebell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Explosão de quadril, cardio e força"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Battle Ropes",
                Description = "Ondulações com cordas grossas.",
                MuscleGroup = "full body",
                Equipment = "battle ropes",
                Category = "cardio",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Farmer's Walk",
                Description = "Caminhar carregando pesos pesados nas mãos.",
                MuscleGroup = "full body",
                Equipment = "dumbbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Força de pegada e core"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Turkish Get-Up",
                Description = "Levantar do chão para em pé segurando peso acima da cabeça.",
                MuscleGroup = "full body",
                Equipment = "kettlebell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Both,
                Notes = "Exercício complexo e funcional"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Thruster",
                Description = "Agachamento frontal + desenvolvimento em um movimento contínuo.",
                MuscleGroup = "full body",
                Equipment = "barbell",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Popular no CrossFit"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Wall Ball",
                Description = "Agachamento + arremesso de medicine ball na parede.",
                MuscleGroup = "full body",
                Equipment = "medicine ball",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Sled Push",
                Description = "Empurrar trenó carregado.",
                MuscleGroup = "full body",
                Equipment = "sled",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym,
                Notes = "Condicionamento sem impacto"
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Sled Pull",
                Description = "Puxar trenó carregado.",
                MuscleGroup = "full body",
                Equipment = "sled",
                Category = "compound",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Remo Ergométrico (Rowing Machine)",
                Description = "Cardio de baixo impacto, trabalha corpo inteiro.",
                MuscleGroup = "full body",
                Equipment = "rowing machine",
                Category = "cardio",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Assault Bike",
                Description = "Bicicleta com braços e pernas simultâneas.",
                MuscleGroup = "full body",
                Equipment = "assault bike",
                Category = "cardio",
                WorkoutLocation = WorkoutLocation.Gym
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "Pular Corda",
                Description = "Cardio clássico, coordenação e resistência.",
                MuscleGroup = "full body",
                Equipment = "jump rope",
                Category = "cardio",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Id = Guid.NewGuid(),
                Name = "High Knees",
                Description = "Corrida no lugar elevando joelhos alto.",
                MuscleGroup = "full body",
                Equipment = "bodyweight",
                Category = "cardio",
                WorkoutLocation = WorkoutLocation.Both
            }
        });

        // Salvar no banco
        await context.Exercises.AddRangeAsync(exercises);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Successfully seeded {exercises.Count} exercises!");
        Console.WriteLine($"   📊 Breakdown:");
        Console.WriteLine($"      • Chest: 20");
        Console.WriteLine($"      • Back: 20");
        Console.WriteLine($"      • Legs: 25");
        Console.WriteLine($"      • Shoulders: 18");
        Console.WriteLine($"      • Biceps: 12");
        Console.WriteLine($"      • Triceps: 12");
        Console.WriteLine($"      • Core/Abs: 18");
        Console.WriteLine($"      • Cardio/Functional: 15");
        Console.WriteLine($"   🎯 Total: 140 exercises");
    }
}
