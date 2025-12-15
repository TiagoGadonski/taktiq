using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Infrastructure.Services;

public class ComprehensiveExerciseSeederService
{
    private readonly IApplicationDbContext _context;

    public ComprehensiveExerciseSeederService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(int added, int skipped)> SeedComprehensiveExercisesAsync(CancellationToken cancellationToken = default)
    {
        var exercises = GetComprehensiveExerciseList();
        int addedCount = 0;
        int skippedCount = 0;

        foreach (var exercise in exercises)
        {
            var exists = await _context.Exercises
                .AnyAsync(e => e.Name == exercise.Name, cancellationToken);

            if (exists)
            {
                skippedCount++;
                continue;
            }

            await _context.Exercises.AddAsync(exercise, cancellationToken);
            addedCount++;
        }

        if (addedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return (addedCount, skippedCount);
    }

    private List<Exercise> GetComprehensiveExerciseList()
    {
        return new List<Exercise>
        {
            // ============================================
            // PEITO (CHEST) - Exercícios para Casa
            // ============================================
            new Exercise
            {
                Name = "Flexão de Braço Tradicional",
                Description = "Exercício clássico de peso corporal para peito, ombros e tríceps. Mantenha o core contraído e corpo em linha reta.",
                MuscleGroup = "Peito",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Desça até o peito quase tocar o chão. Suba com explosão.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Flexão Ampla",
                Description = "Variação com mãos mais afastadas que enfatiza o peitoral",
                MuscleGroup = "Peito",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Mãos mais afastadas que os ombros. Ótimo para desenvolvimento peitoral.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Flexão Diamante",
                Description = "Mãos juntas formando diamante, foco em tríceps",
                MuscleGroup = "Tríceps",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Mãos próximas formando diamante com dedos indicadores e polegares.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Flexão Declinada",
                Description = "Pés elevados em cadeira ou sofá, trabalha peito superior",
                MuscleGroup = "Peito",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Pés elevados aumentam dificuldade e trabalham peito superior.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Flexão Inclinada",
                Description = "Mãos elevadas em banco ou cadeira, ideal para iniciantes",
                MuscleGroup = "Peito",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Versão mais fácil. Trabalha peito inferior.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Flexão Archer",
                Description = "Uma mão perto do corpo, outra afastada alternando",
                MuscleGroup = "Peito",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Exercício avançado. Preparação para flexão de um braço.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Flexão com Palmas",
                Description = "Flexão explosiva com palmas no ar",
                MuscleGroup = "Peito",
                Category = "Pliometria",
                Equipment = "Peso Corporal",
                Notes = "Desenvolvimento de potência explosiva.",
                WorkoutLocation = WorkoutLocation.Home
            },

            // ============================================
            // OMBROS (SHOULDERS)
            // ============================================
            new Exercise
            {
                Name = "Flexão Pike",
                Description = "Quadril elevado em posição de V invertido",
                MuscleGroup = "Ombros",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Progressão para handstand push-up.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Handstand Push-up (Parede)",
                Description = "Flexão invertida contra parede",
                MuscleGroup = "Ombros",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Exercício avançado. Desenvolve força extrema nos ombros.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Desenvolvimento com Halteres",
                Description = "Press militar com halteres, sentado ou em pé",
                MuscleGroup = "Ombros",
                Category = "Força",
                Equipment = "Halteres",
                Notes = "Empurre halteres acima da cabeça até extensão completa.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Elevação Lateral com Halteres",
                Description = "Braços abertos lateralmente até altura dos ombros",
                MuscleGroup = "Ombros",
                Category = "Isolamento",
                Equipment = "Halteres",
                Notes = "Trabalha deltóide lateral. Cotovelos levemente flexionados.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Elevação Frontal com Halteres",
                Description = "Braços elevados para frente até altura dos ombros",
                MuscleGroup = "Ombros",
                Category = "Isolamento",
                Equipment = "Halteres",
                Notes = "Trabalha deltóide frontal.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Crucifixo Inverso com Halteres",
                Description = "Corpo inclinado, elevação posterior dos braços",
                MuscleGroup = "Ombros",
                Category = "Isolamento",
                Equipment = "Halteres",
                Notes = "Trabalha deltóide posterior e parte superior das costas.",
                WorkoutLocation = WorkoutLocation.Both
            },

            // ============================================
            // TRÍCEPS
            // ============================================
            new Exercise
            {
                Name = "Mergulho em Cadeira",
                Description = "Dips usando cadeira ou sofá",
                MuscleGroup = "Tríceps",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Pode adicionar peso no colo para progressão.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Tríceps Tate com Halteres",
                Description = "Deitado, movimento único com halteres",
                MuscleGroup = "Tríceps",
                Category = "Isolamento",
                Equipment = "Halteres",
                Notes = "Halteres se movem em arco sobre o peito.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Tríceps Kickback com Halteres",
                Description = "Corpo inclinado, extensão para trás",
                MuscleGroup = "Tríceps",
                Category = "Isolamento",
                Equipment = "Halteres",
                Notes = "Estenda braço completamente para trás.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Tríceps Overhead com Halteres",
                Description = "Halter atrás da cabeça, extensão para cima",
                MuscleGroup = "Tríceps",
                Category = "Isolamento",
                Equipment = "Halteres",
                Notes = "Trabalha cabeça longa do tríceps.",
                WorkoutLocation = WorkoutLocation.Both
            },

            // ============================================
            // COSTAS (BACK)
            // ============================================
            new Exercise
            {
                Name = "Barra Fixa Pronada",
                Description = "Pull-up com palmas para frente",
                MuscleGroup = "Costas",
                Category = "Força",
                Equipment = "Barra Fixa",
                Notes = "Rei dos exercícios de costas.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Barra Fixa Supinada",
                Description = "Chin-up com palmas voltadas para você",
                MuscleGroup = "Costas",
                Category = "Força",
                Equipment = "Barra Fixa",
                Notes = "Ótimo para dorsais e bíceps.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Australian Pull-ups",
                Description = "Remada horizontal usando mesa resistente",
                MuscleGroup = "Costas",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Pés no chão, puxe peito até a borda da mesa.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Remada com Toalha",
                Description = "Toalha em porta, movimento de remada",
                MuscleGroup = "Costas",
                Category = "Força",
                Equipment = "Toalha",
                Notes = "Toalha enrolada em maçaneta. Trabalha dorsais.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Superman",
                Description = "Deitado de bruços, elevar braços e pernas simultaneamente",
                MuscleGroup = "Lombar",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Excelente para lombar e cadeia posterior.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Remada Unilateral com Halter",
                Description = "Apoiado em banco, puxar halter até quadril",
                MuscleGroup = "Costas",
                Category = "Força",
                Equipment = "Halteres",
                Notes = "Uma mão apoiada. Permite foco unilateral.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Remada Curvada com Halteres",
                Description = "Corpo inclinado 45°, rema ambos halteres",
                MuscleGroup = "Costas",
                Category = "Força",
                Equipment = "Halteres",
                Notes = "Puxe halteres até abdômen mantendo costas retas.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Pullover com Halter",
                Description = "Deitado, halter em movimento arqueado acima e atrás da cabeça",
                MuscleGroup = "Costas",
                Category = "Força",
                Equipment = "Halteres",
                Notes = "Trabalha dorsais e expansão da caixa torácica.",
                WorkoutLocation = WorkoutLocation.Both
            },

            // ============================================
            // BÍCEPS
            // ============================================
            new Exercise
            {
                Name = "Rosca Direta com Halteres",
                Description = "Curl clássico de bíceps com halteres",
                MuscleGroup = "Bíceps",
                Category = "Isolamento",
                Equipment = "Halteres",
                Notes = "Braços ao lado do corpo. Curl completo.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Rosca Alternada com Halteres",
                Description = "Curl alternando braços",
                MuscleGroup = "Bíceps",
                Category = "Isolamento",
                Equipment = "Halteres",
                Notes = "Permite maior foco em cada bíceps.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Rosca Martelo com Halteres",
                Description = "Palmas neutras, curl vertical",
                MuscleGroup = "Bíceps",
                Category = "Isolamento",
                Equipment = "Halteres",
                Notes = "Trabalha braquial e antebraços além do bíceps.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Rosca Concentrada com Halter",
                Description = "Sentado, cotovelo apoiado na coxa interna",
                MuscleGroup = "Bíceps",
                Category = "Isolamento",
                Equipment = "Halteres",
                Notes = "Máximo isolamento do bíceps.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Rosca com Faixa Elástica",
                Description = "Pise na faixa, curl com resistência elástica",
                MuscleGroup = "Bíceps",
                Category = "Isolamento",
                Equipment = "Faixa Elástica",
                Notes = "Tensão constante durante todo movimento.",
                WorkoutLocation = WorkoutLocation.Home
            },

            // ============================================
            // PERNAS - QUADRÍCEPS
            // ============================================
            new Exercise
            {
                Name = "Agachamento Livre",
                Description = "Exercício fundamental para desenvolvimento de pernas",
                MuscleGroup = "Quadríceps",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Pés largura dos ombros. Desça até coxas paralelas ao chão.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Agachamento Sumô",
                Description = "Pés bem afastados com pontas para fora",
                MuscleGroup = "Quadríceps",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Trabalha interno de coxa e glúteos.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Agachamento Búlgaro",
                Description = "Perna traseira elevada em cadeira ou banco",
                MuscleGroup = "Quadríceps",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Excelente para força e equilíbrio unilateral.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Afundo",
                Description = "Passada para frente com joelho descendo",
                MuscleGroup = "Quadríceps",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Andando ou estático. Trabalha pernas e glúteos.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Afundo Reverso",
                Description = "Passada para trás",
                MuscleGroup = "Quadríceps",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Mais fácil nos joelhos que afundo tradicional.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Agachamento com Salto",
                Description = "Agachamento explosivo com salto vertical",
                MuscleGroup = "Quadríceps",
                Category = "Pliometria",
                Equipment = "Peso Corporal",
                Notes = "Desenvolve potência e explosão nas pernas.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Pistol Squat",
                Description = "Agachamento completo em uma perna",
                MuscleGroup = "Quadríceps",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Exercício muito avançado. Requer força, mobilidade e equilíbrio.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Agachamento Goblet com Halter",
                Description = "Halter seguro junto ao peito",
                MuscleGroup = "Quadríceps",
                Category = "Força",
                Equipment = "Halteres",
                Notes = "Excelente para aprender técnica de agachamento.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Cadeira Isométrica",
                Description = "Costas na parede, posição sentada de 90°",
                MuscleGroup = "Quadríceps",
                Category = "Isométrico",
                Equipment = "Peso Corporal",
                Notes = "Queima intensa nos quadríceps. Ótimo para resistência.",
                WorkoutLocation = WorkoutLocation.Home
            },

            // ============================================
            // PERNAS - POSTERIORES (Glúteos, Isquiotibiais)
            // ============================================
            new Exercise
            {
                Name = "Ponte de Glúteos",
                Description = "Deitado de costas, elevar quadril",
                MuscleGroup = "Glúteos",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Essencial para ativação glútea. Aperte glúteos no topo.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Ponte Unilateral",
                Description = "Ponte de glúteos com uma perna",
                MuscleGroup = "Glúteos",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Versão mais difícil. Desenvolvimento unilateral.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Stiff com Halteres",
                Description = "Pernas levemente flexionadas, inclinação do quadril",
                MuscleGroup = "Isquiotibiais",
                Category = "Força",
                Equipment = "Halteres",
                Notes = "Trabalha posterior de coxa e lombar.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Stiff Unilateral",
                Description = "Romanian deadlift em uma perna sem peso",
                MuscleGroup = "Isquiotibiais",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Excelente para isquiotibiais, glúteos e equilíbrio.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Good Morning",
                Description = "Inclinação do tronco para frente com pernas levemente flexionadas",
                MuscleGroup = "Isquiotibiais",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Mãos atrás da cabeça. Trabalha cadeia posterior.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Elevação de Quadril (Hip Thrust)",
                Description = "Costas apoiadas em banco ou sofá, quadril para cima",
                MuscleGroup = "Glúteos",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Melhor exercício isolado para glúteos.",
                WorkoutLocation = WorkoutLocation.Both
            },

            // ============================================
            // PANTURRILHAS
            // ============================================
            new Exercise
            {
                Name = "Panturrilha em Pé",
                Description = "Elevação na ponta dos pés",
                MuscleGroup = "Panturrilhas",
                Category = "Isolamento",
                Equipment = "Peso Corporal",
                Notes = "Use borda de degrau para maior amplitude.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Panturrilha Unilateral",
                Description = "Elevação em uma perna",
                MuscleGroup = "Panturrilhas",
                Category = "Isolamento",
                Equipment = "Peso Corporal",
                Notes = "Melhor amplitude e isolamento.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Panturrilha Sentado com Halter",
                Description = "Sentado, halter nos joelhos",
                MuscleGroup = "Panturrilhas",
                Category = "Isolamento",
                Equipment = "Halteres",
                Notes = "Trabalha músculo sóleo. Complementa panturrilha em pé.",
                WorkoutLocation = WorkoutLocation.Both
            },

            // ============================================
            // CORE / ABDÔMEN
            // ============================================
            new Exercise
            {
                Name = "Prancha Frontal",
                Description = "Posição de flexão apoiado nos antebraços",
                MuscleGroup = "Abdômen",
                Category = "Core",
                Equipment = "Peso Corporal",
                Notes = "Rei dos exercícios de core. Corpo em linha reta.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Prancha Lateral",
                Description = "Prancha de lado apoiado em um antebraço",
                MuscleGroup = "Oblíquos",
                Category = "Core",
                Equipment = "Peso Corporal",
                Notes = "Excelente para oblíquos e estabilidade lateral.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Mountain Climbers",
                Description = "Posição de flexão, joelhos alternados ao peito",
                MuscleGroup = "Abdômen",
                Category = "Cardio",
                Equipment = "Peso Corporal",
                Notes = "Ótimo para cardio e core simultaneamente.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Abdominal Bicicleta",
                Description = "Cotovelo ao joelho oposto em movimento de pedalar",
                MuscleGroup = "Abdômen",
                Category = "Core",
                Equipment = "Peso Corporal",
                Notes = "Excelente para oblíquos e abdômen completo.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Elevação de Pernas",
                Description = "Deitado, elevar pernas retas até 90°",
                MuscleGroup = "Abdômen",
                Category = "Core",
                Equipment = "Peso Corporal",
                Notes = "Exercício avançado para abdômen inferior.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Flutter Kicks",
                Description = "Chutes alternados pequenos e rápidos",
                MuscleGroup = "Abdômen",
                Category = "Core",
                Equipment = "Peso Corporal",
                Notes = "Queima intensa no abdômen inferior.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Russian Twist",
                Description = "Sentado, torção lateral alternada",
                MuscleGroup = "Oblíquos",
                Category = "Core",
                Equipment = "Peso Corporal",
                Notes = "Pode segurar halter ou garrafa para resistência adicional.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Dead Bug",
                Description = "De costas, braço e perna opostos se estendem",
                MuscleGroup = "Abdômen",
                Category = "Core",
                Equipment = "Peso Corporal",
                Notes = "Excelente para estabilidade de core e coordenação.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Hollow Body Hold",
                Description = "Corpo em forma de banana invertida no chão",
                MuscleGroup = "Abdômen",
                Category = "Core",
                Equipment = "Peso Corporal",
                Notes = "Força de core para ginástica. Muito desafiador.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Abdominal Crunch",
                Description = "Abdominal tradicional parcial",
                MuscleGroup = "Abdômen",
                Category = "Core",
                Equipment = "Peso Corporal",
                Notes = "Clássico. Mãos atrás da cabeça sem puxar pescoço.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Abdominal Reverso",
                Description = "Joelhos ao peito elevando quadril",
                MuscleGroup = "Abdômen",
                Category = "Core",
                Equipment = "Peso Corporal",
                Notes = "Foco em abdômen inferior.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "V-Up",
                Description = "Toque simultâneo de mãos nos pés formando V",
                MuscleGroup = "Abdômen",
                Category = "Core",
                Equipment = "Peso Corporal",
                Notes = "Exercício avançado de abdômen completo.",
                WorkoutLocation = WorkoutLocation.Home
            },

            // ============================================
            // CARDIO & CORPO INTEIRO
            // ============================================
            new Exercise
            {
                Name = "Burpees",
                Description = "Agachar, flexão, pular - movimento explosivo completo",
                MuscleGroup = "Corpo Inteiro",
                Category = "Cardio",
                Equipment = "Peso Corporal",
                Notes = "Máximo condicionamento físico. Queima muitas calorias.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Polichinelos",
                Description = "Jumping jacks - pular abrindo e fechando pernas e braços",
                MuscleGroup = "Corpo Inteiro",
                Category = "Cardio",
                Equipment = "Peso Corporal",
                Notes = "Ótimo para aquecimento ou intervalos de alta intensidade.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "High Knees",
                Description = "Corrida no lugar elevando joelhos alto",
                MuscleGroup = "Corpo Inteiro",
                Category = "Cardio",
                Equipment = "Peso Corporal",
                Notes = "Excelente cardio e trabalho de pernas.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Butt Kicks",
                Description = "Corrida no lugar levando calcanhar aos glúteos",
                MuscleGroup = "Isquiotibiais",
                Category = "Cardio",
                Equipment = "Peso Corporal",
                Notes = "Trabalha posteriores e eleva frequência cardíaca.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Inchworms",
                Description = "Caminhar com mãos até prancha e voltar",
                MuscleGroup = "Corpo Inteiro",
                Category = "Mobilidade",
                Equipment = "Peso Corporal",
                Notes = "Alongamento dinâmico e fortalecimento de core.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Pular Corda",
                Description = "Cardio com corda de pular",
                MuscleGroup = "Corpo Inteiro",
                Category = "Cardio",
                Equipment = "Corda de Pular",
                Notes = "Melhora coordenação, resistência e queima calorias.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Shadow Boxing",
                Description = "Socos e movimentos de boxe no ar",
                MuscleGroup = "Corpo Inteiro",
                Category = "Cardio",
                Equipment = "Peso Corporal",
                Notes = "Ótimo cardio e condicionamento de membros superiores.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Skaters",
                Description = "Saltos laterais alternados imitando patinador",
                MuscleGroup = "Pernas",
                Category = "Cardio",
                Equipment = "Peso Corporal",
                Notes = "Trabalha lateral das pernas e sistema cardiovascular.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Box Jumps",
                Description = "Saltar para cima de superfície elevada",
                MuscleGroup = "Pernas",
                Category = "Pliometria",
                Equipment = "Peso Corporal",
                Notes = "Use banco ou cadeira resistente. Desenvolve potência.",
                WorkoutLocation = WorkoutLocation.Home
            },

            // ============================================
            // EXERCÍCIOS COM FAIXA ELÁSTICA
            // ============================================
            new Exercise
            {
                Name = "Remada com Faixa Elástica",
                Description = "Faixa presa na frente, puxar para abdômen",
                MuscleGroup = "Costas",
                Category = "Força",
                Equipment = "Faixa Elástica",
                Notes = "Prenda faixa na altura do peito. Movimento de remada.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Supino com Faixa Elástica",
                Description = "Faixa atrás das costas, empurrar para frente",
                MuscleGroup = "Peito",
                Category = "Força",
                Equipment = "Faixa Elástica",
                Notes = "Pode fazer em pé ou deitado.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Agachamento com Faixa",
                Description = "Pisar na faixa, segurar extremidades nos ombros",
                MuscleGroup = "Quadríceps",
                Category = "Força",
                Equipment = "Faixa Elástica",
                Notes = "Adiciona resistência progressiva ao agachamento.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Pull Apart com Faixa",
                Description = "Abrir braços esticando faixa horizontalmente",
                MuscleGroup = "Ombros",
                Category = "Força",
                Equipment = "Faixa Elástica",
                Notes = "Excelente para posterior de ombro e postura.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Abdução de Quadril com Faixa",
                Description = "Faixa nos tornozelos, abrir pernas lateralmente",
                MuscleGroup = "Glúteos",
                Category = "Isolamento",
                Equipment = "Faixa Elástica",
                Notes = "Trabalha glúteo médio. Importante para estabilidade.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Kickback com Faixa",
                Description = "Chute controlado para trás contra resistência",
                MuscleGroup = "Glúteos",
                Category = "Isolamento",
                Equipment = "Faixa Elástica",
                Notes = "Excelente isolamento de glúteos.",
                WorkoutLocation = WorkoutLocation.Home
            },

            // ============================================
            // MOBILIDADE & ALONGAMENTO
            // ============================================
            new Exercise
            {
                Name = "Gato-Vaca",
                Description = "De quatro, alternar extensão e flexão da coluna",
                MuscleGroup = "Core",
                Category = "Mobilidade",
                Equipment = "Peso Corporal",
                Notes = "Excelente para mobilidade da coluna vertebral.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Cachorro Olhando para Baixo",
                Description = "Posição de V invertido, yoga",
                MuscleGroup = "Corpo Inteiro",
                Category = "Mobilidade",
                Equipment = "Peso Corporal",
                Notes = "Alonga posteriores e fortalece ombros.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Posição da Cobra",
                Description = "Deitado de bruços, extensão da coluna com braços",
                MuscleGroup = "Lombar",
                Category = "Mobilidade",
                Equipment = "Peso Corporal",
                Notes = "Alonga abdômen e fortalece lombar.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Ponte Completa",
                Description = "Arqueamento completo da coluna em ponte",
                MuscleGroup = "Corpo Inteiro",
                Category = "Mobilidade",
                Equipment = "Peso Corporal",
                Notes = "Exercício avançado. Excelente mobilidade de coluna.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Agachamento Profundo",
                Description = "Agachamento de descanso completo",
                MuscleGroup = "Pernas",
                Category = "Mobilidade",
                Equipment = "Peso Corporal",
                Notes = "Melhora mobilidade de quadril, joelhos e tornozelos.",
                WorkoutLocation = WorkoutLocation.Home
            },

            // ============================================
            // CALISTENIA AVANÇADA
            // ============================================
            new Exercise
            {
                Name = "L-Sit",
                Description = "Sentar suspenso com pernas retas paralelas ao chão",
                MuscleGroup = "Core",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Exercício muito avançado de força de core.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Muscle-Up",
                Description = "Transição explosiva de pull-up para dip",
                MuscleGroup = "Corpo Inteiro",
                Category = "Força",
                Equipment = "Barra Fixa",
                Notes = "Exercício extremamente avançado. Requer muita prática.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Front Lever",
                Description = "Corpo horizontal suspenso de frente para baixo",
                MuscleGroup = "Costas",
                Category = "Força",
                Equipment = "Barra Fixa",
                Notes = "Extremamente avançado. Força tremenda de costas e core.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Back Lever",
                Description = "Corpo horizontal suspenso de costas para baixo",
                MuscleGroup = "Costas",
                Category = "Força",
                Equipment = "Barra Fixa",
                Notes = "Progressão para calistenia avançada.",
                WorkoutLocation = WorkoutLocation.Both
            },
            new Exercise
            {
                Name = "Handstand",
                Description = "Parada de mão vertical",
                MuscleGroup = "Ombros",
                Category = "Equilíbrio",
                Equipment = "Peso Corporal",
                Notes = "Use parede para apoio. Desenvolve força e equilíbrio.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Dragon Flag",
                Description = "Corpo reto elevado segurando banco",
                MuscleGroup = "Abdômen",
                Category = "Core",
                Equipment = "Peso Corporal",
                Notes = "Um dos exercícios mais difíceis de core. Famoso por Bruce Lee.",
                WorkoutLocation = WorkoutLocation.Home
            },
            new Exercise
            {
                Name = "Planche",
                Description = "Corpo horizontal suspenso apenas com as mãos",
                MuscleGroup = "Corpo Inteiro",
                Category = "Força",
                Equipment = "Peso Corporal",
                Notes = "Exercício supremo de calistenia. Extremamente difícil.",
                WorkoutLocation = WorkoutLocation.Home
            }
        };
    }
}
