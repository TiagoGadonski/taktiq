const axios = require('axios');

// Configuration
const API_BASE_URL = 'https://localhost:7219/api';

// Workout Location enum: 0 = Gym, 1 = Home, 2 = Both
const WorkoutLocation = {
  Gym: 0,
  Home: 1,
  Both: 2
};

// Comprehensive Exercise Database
const exercises = [
  // ============================================
  // PEITO (CHEST) - Exercícios para Casa
  // ============================================
  {
    name: 'Flexão de Braço Tradicional',
    description: 'Exercício clássico de peso corporal para peito, ombros e tríceps',
    muscleGroup: 'Peito',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Mantenha o core contraído e corpo em linha reta. Desça até o peito quase tocar o chão.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Flexão Ampla',
    description: 'Variação com mãos mais afastadas, enfatiza peito',
    muscleGroup: 'Peito',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Mãos mais afastadas que os ombros. Ótimo para desenvolvimento peitoral.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Flexão Diamante',
    description: 'Mãos juntas formando diamante, foca em tríceps',
    muscleGroup: 'Tríceps',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Mãos próximas formando diamante. Excelente para tríceps.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Flexão Declinada',
    description: 'Pés elevados em cadeira ou sofá',
    muscleGroup: 'Peito',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Pés elevados. Trabalha peito superior e ombros.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Flexão Inclinada',
    description: 'Mãos elevadas em banco ou cadeira',
    muscleGroup: 'Peito',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Versão mais fácil, ideal para iniciantes. Trabalha peito inferior.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Flexão Archer',
    description: 'Uma mão perto do corpo, outra afastada alternando',
    muscleGroup: 'Peito',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Exercício avançado. Prepara para flexão de um braço.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Flexão com Palmas',
    description: 'Explosiva com palmas no ar',
    muscleGroup: 'Peito',
    category: 'Pliometria',
    equipment: 'Peso Corporal',
    notes: 'Exercício explosivo. Desenvolve potência e força.',
    workoutLocation: WorkoutLocation.Home
  },

  // ============================================
  // OMBROS (SHOULDERS) - Exercícios para Casa
  // ============================================
  {
    name: 'Flexão Pike',
    description: 'Quadril elevado em posição de pike',
    muscleGroup: 'Ombros',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Quadril alto. Progressão para handstand push-up.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Handstand Push-up (Parede)',
    description: 'Flexão invertida contra parede',
    muscleGroup: 'Ombros',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Exercício avançado. Desenvolve força incrível nos ombros.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Desenvolvimento com Halteres',
    description: 'Press militar com halteres',
    muscleGroup: 'Ombros',
    category: 'Força',
    equipment: 'Halteres',
    notes: 'Sentado ou em pé. Empurre os halteres acima da cabeça.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Elevação Lateral com Halteres',
    description: 'Braços abertos lateralmente',
    muscleGroup: 'Ombros',
    category: 'Isolamento',
    equipment: 'Halteres',
    notes: 'Eleva lateralmente até altura dos ombros. Trabalha deltóide lateral.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Elevação Frontal com Halteres',
    description: 'Braços elevados para frente',
    muscleGroup: 'Ombros',
    category: 'Isolamento',
    equipment: 'Halteres',
    notes: 'Eleva para frente até altura dos ombros. Trabalha deltóide frontal.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Crucifixo Inverso com Halteres',
    description: 'Curvado, elevação posterior',
    muscleGroup: 'Ombros',
    category: 'Isolamento',
    equipment: 'Halteres',
    notes: 'Corpo inclinado para frente. Trabalha deltóide posterior.',
    workoutLocation: WorkoutLocation.Both
  },

  // ============================================
  // TRÍCEPS - Exercícios para Casa
  // ============================================
  {
    name: 'Mergulho em Cadeira',
    description: 'Dips usando cadeira ou sofá',
    muscleGroup: 'Tríceps',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Use borda de cadeira. Pode adicionar peso no colo para progressão.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Tríceps Tate com Halteres',
    description: 'Deitado, halteres acima do peito',
    muscleGroup: 'Tríceps',
    category: 'Isolamento',
    equipment: 'Halteres',
    notes: 'Movimento único para tríceps. Excelente isolamento.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Tríceps Kickback com Halteres',
    description: 'Inclinado, extensão para trás',
    muscleGroup: 'Tríceps',
    category: 'Isolamento',
    equipment: 'Halteres',
    notes: 'Corpo inclinado. Estenda braço completamente para trás.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Tríceps Overhead com Halteres',
    description: 'Halter acima da cabeça, extensão',
    muscleGroup: 'Tríceps',
    category: 'Isolamento',
    equipment: 'Halteres',
    notes: 'Halter atrás da cabeça, estenda para cima. Trabalha cabeça longa.',
    workoutLocation: WorkoutLocation.Both
  },

  // ============================================
  // COSTAS (BACK) - Exercícios para Casa
  // ============================================
  {
    name: 'Barra Fixa Pronada',
    description: 'Pull-up com palmas para frente',
    muscleGroup: 'Costas',
    category: 'Força',
    equipment: 'Barra Fixa',
    notes: 'Rei dos exercícios de costas. Palmas para frente.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Barra Fixa Supinada',
    description: 'Chin-up com palmas para você',
    muscleGroup: 'Costas',
    category: 'Força',
    equipment: 'Barra Fixa',
    notes: 'Palmas para você. Ótimo para dorsais e bíceps.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Australian Pull-ups',
    description: 'Remada horizontal usando mesa',
    muscleGroup: 'Costas',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Use mesa resistente. Pés no chão, puxe peito até mesa.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Remada com Toalha',
    description: 'Toalha em porta, movimento de remada',
    muscleGroup: 'Costas',
    category: 'Força',
    equipment: 'Toalha',
    notes: 'Toalha em maçaneta. Trabalha dorsais e romboides.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Superman',
    description: 'Deitado de bruços, elevar braços e pernas',
    muscleGroup: 'Lombar',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Eleve braços e pernas simultaneamente. Excelente para lombar.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Remada Unilateral com Halter',
    description: 'Apoiado em banco, puxar halter',
    muscleGroup: 'Costas',
    category: 'Força',
    equipment: 'Halteres',
    notes: 'Uma mão apoiada. Puxe halter até quadril.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Remada Curvada com Halteres',
    description: 'Corpo inclinado, rema ambos halteres',
    muscleGroup: 'Costas',
    category: 'Força',
    equipment: 'Halteres',
    notes: 'Corpo inclinado 45°. Puxe halteres até abdômen.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Pullover com Halter',
    description: 'Deitado, halter acima e atrás da cabeça',
    muscleGroup: 'Costas',
    category: 'Força',
    equipment: 'Halteres',
    notes: 'Movimento arqueado. Trabalha dorsais e peitoral.',
    workoutLocation: WorkoutLocation.Both
  },

  // ============================================
  // BÍCEPS - Exercícios para Casa
  // ============================================
  {
    name: 'Rosca Direta com Halteres',
    description: 'Curl clássico de bíceps',
    muscleGroup: 'Bíceps',
    category: 'Isolamento',
    equipment: 'Halteres',
    notes: 'Braços ao lado do corpo. Curl completo.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Rosca Alternada com Halteres',
    description: 'Curl alternando braços',
    muscleGroup: 'Bíceps',
    category: 'Isolamento',
    equipment: 'Halteres',
    notes: 'Alterne braços. Permite foco em cada bíceps.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Rosca Martelo com Halteres',
    description: 'Palmas neutras, curl vertical',
    muscleGroup: 'Bíceps',
    category: 'Isolamento',
    equipment: 'Halteres',
    notes: 'Palmas uma de frente para outra. Trabalha braquial.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Rosca Concentrada com Halter',
    description: 'Sentado, cotovelo apoiado',
    muscleGroup: 'Bíceps',
    category: 'Isolamento',
    equipment: 'Halteres',
    notes: 'Cotovelo na coxa. Isolamento máximo do bíceps.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Rosca com Faixa Elástica',
    description: 'Resistência elástica para bíceps',
    muscleGroup: 'Bíceps',
    category: 'Isolamento',
    equipment: 'Faixa Elástica',
    notes: 'Pise na faixa. Curl com resistência elástica.',
    workoutLocation: WorkoutLocation.Home
  },

  // ============================================
  // PERNAS - QUADRÍCEPS
  // ============================================
  {
    name: 'Agachamento Livre',
    description: 'Exercício fundamental para pernas',
    muscleGroup: 'Quadríceps',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Pés largura dos ombros. Sente e levante. Mantenha peito erguido.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Agachamento Sumô',
    description: 'Pés bem afastados, ponta para fora',
    muscleGroup: 'Quadríceps',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Pés bem afastados. Trabalha interno de coxa.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Agachamento Búlgaro',
    description: 'Perna traseira elevada em cadeira',
    muscleGroup: 'Quadríceps',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Perna traseira em cadeira. Excelente para força unilateral.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Afundo',
    description: 'Passada para frente, joelho desce',
    muscleGroup: 'Quadríceps',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Andando ou estático. Ótimo para pernas e glúteos.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Afundo Reverso',
    description: 'Passada para trás',
    muscleGroup: 'Quadríceps',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Passada para trás. Mais fácil nos joelhos.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Agachamento com Salto',
    description: 'Explosivo com salto',
    muscleGroup: 'Quadríceps',
    category: 'Pliometria',
    equipment: 'Peso Corporal',
    notes: 'Agachamento explosivo. Desenvolve potência.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Pistol Squat',
    description: 'Agachamento em uma perna',
    muscleGroup: 'Quadríceps',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Exercício avançado. Requer força e equilíbrio.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Agachamento Goblet com Halter',
    description: 'Halter seguro no peito',
    muscleGroup: 'Quadríceps',
    category: 'Força',
    equipment: 'Halteres',
    notes: 'Halter no peito. Excelente para aprender técnica.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Cadeira (Wall Sit)',
    description: 'Costas na parede, 90° isométrico',
    muscleGroup: 'Quadríceps',
    category: 'Isométrico',
    equipment: 'Peso Corporal',
    notes: 'Costas na parede. Queima intensa nos quadríceps.',
    workoutLocation: WorkoutLocation.Home
  },

  // ============================================
  // PERNAS - POSTERIORES (Glúteos, Isquiotibiais)
  // ============================================
  {
    name: 'Ponte de Glúteos',
    description: 'Deitado, elevar quadril',
    muscleGroup: 'Glúteos',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Essencial para ativação glútea. Aperte no topo.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Ponte Unilateral',
    description: 'Ponte com uma perna',
    muscleGroup: 'Glúteos',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Uma perna. Dificuldade aumentada.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Stiff com Halteres',
    description: 'Pernas retas, inclinação do quadril',
    muscleGroup: 'Isquiotibiais',
    category: 'Força',
    equipment: 'Halteres',
    notes: 'Pernas levemente flexionadas. Trabalha posterior de coxa.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Stiff Unilateral',
    description: 'Romanian deadlift em uma perna',
    muscleGroup: 'Isquiotibiais',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Equilíbrio em uma perna. Excelente para isquios e equilíbrio.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Good Morning',
    description: 'Inclinação do tronco com pernas retas',
    muscleGroup: 'Isquiotibiais',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Mãos atrás da cabeça. Incline tronco para frente.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Elevação de Quadril (Hip Thrust)',
    description: 'Costas apoiadas, quadril para cima',
    muscleGroup: 'Glúteos',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Costas em sofá/banco. Melhor exercício para glúteos.',
    workoutLocation: WorkoutLocation.Both
  },

  // ============================================
  // PANTURRILHAS
  // ============================================
  {
    name: 'Panturrilha em Pé',
    description: 'Elevação na ponta dos pés',
    muscleGroup: 'Panturrilhas',
    category: 'Isolamento',
    equipment: 'Peso Corporal',
    notes: 'Borda de degrau. Pode fazer unilateral para progressão.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Panturrilha Unilateral',
    description: 'Uma perna de cada vez',
    muscleGroup: 'Panturrilhas',
    category: 'Isolamento',
    equipment: 'Peso Corporal',
    notes: 'Uma perna. Melhor amplitude e isolamento.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Panturrilha Sentado com Halter',
    description: 'Sentado, halter nos joelhos',
    muscleGroup: 'Panturrilhas',
    category: 'Isolamento',
    equipment: 'Halteres',
    notes: 'Trabalha sóleo. Complementa panturrilha em pé.',
    workoutLocation: WorkoutLocation.Both
  },

  // ============================================
  // CORE / ABDÔMEN
  // ============================================
  {
    name: 'Prancha Frontal',
    description: 'Posição de flexão nos antebraços',
    muscleGroup: 'Abdômen',
    category: 'Core',
    equipment: 'Peso Corporal',
    notes: 'Rei dos exercícios de core. Força incrível.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Prancha Lateral',
    description: 'Prancha de lado em um antebraço',
    muscleGroup: 'Oblíquos',
    category: 'Core',
    equipment: 'Peso Corporal',
    notes: 'Excelente para oblíquos e estabilidade lateral.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Mountain Climbers',
    description: 'Posição de flexão, joelhos ao peito',
    muscleGroup: 'Abdômen',
    category: 'Cardio',
    equipment: 'Peso Corporal',
    notes: 'Ótimo cardio e core. Alterne pernas rapidamente.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Abdominal Bicicleta',
    description: 'Cotovelo ao joelho oposto',
    muscleGroup: 'Abdômen',
    category: 'Core',
    equipment: 'Peso Corporal',
    notes: 'Movimento de pedalar. Excelente para oblíquos.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Elevação de Pernas',
    description: 'Deitado, elevar pernas retas',
    muscleGroup: 'Abdômen',
    category: 'Core',
    equipment: 'Peso Corporal',
    notes: 'Exercício avançado para abdômen inferior.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Flutter Kicks',
    description: 'Chutes alternados pequenos',
    muscleGroup: 'Abdômen',
    category: 'Core',
    equipment: 'Peso Corporal',
    notes: 'Queima intensa no abdômen inferior.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Russian Twist',
    description: 'Sentado, torção lateral',
    muscleGroup: 'Oblíquos',
    category: 'Core',
    equipment: 'Peso Corporal',
    notes: 'Pode segurar halter ou garrafa. Ótimo para oblíquos.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Dead Bug',
    description: 'Costas no chão, braço e perna opostos',
    muscleGroup: 'Abdômen',
    category: 'Core',
    equipment: 'Peso Corporal',
    notes: 'Excelente estabilidade de core.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Hollow Body Hold',
    description: 'Posição de prancha invertida no chão',
    muscleGroup: 'Abdômen',
    category: 'Core',
    equipment: 'Peso Corporal',
    notes: 'Força incrível para ginástica e calistenia.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Abdominal Crunch',
    description: 'Abdominal tradicional',
    muscleGroup: 'Abdômen',
    category: 'Core',
    equipment: 'Peso Corporal',
    notes: 'Clássico. Mãos atrás da cabeça ou cruzadas no peito.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Abdominal Reverso',
    description: 'Joelhos ao peito',
    muscleGroup: 'Abdômen',
    category: 'Core',
    equipment: 'Peso Corporal',
    notes: 'Trabalha abdômen inferior. Puxe joelhos ao peito.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'V-Up',
    description: 'Toque mãos nos pés formando V',
    muscleGroup: 'Abdômen',
    category: 'Core',
    equipment: 'Peso Corporal',
    notes: 'Exercício avançado. Forma V com corpo.',
    workoutLocation: WorkoutLocation.Home
  },

  // ============================================
  // CARDIO & CORPO INTEIRO
  // ============================================
  {
    name: 'Burpees',
    description: 'Movimento explosivo completo',
    muscleGroup: 'Corpo Inteiro',
    category: 'Cardio',
    description: 'Movimento completo: agachar, flexão, pular. Máximo condicionamento.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Polichinelos',
    description: 'Jumping jacks clássico',
    muscleGroup: 'Corpo Inteiro',
    category: 'Cardio',
    equipment: 'Peso Corporal',
    notes: 'Cardio clássico. Ótimo para aquecimento ou intervalos.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'High Knees',
    description: 'Corrida no lugar com joelhos altos',
    muscleGroup: 'Corpo Inteiro',
    category: 'Cardio',
    equipment: 'Peso Corporal',
    notes: 'Corrida parado com joelhos altos. Excelente cardio.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Butt Kicks',
    description: 'Calcanhar aos glúteos correndo',
    muscleGroup: 'Isquiotibiais',
    category: 'Cardio',
    equipment: 'Peso Corporal',
    notes: 'Calcanhar aos glúteos. Trabalho de posterior.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Inchworms',
    description: 'Caminhar mãos até prancha',
    muscleGroup: 'Corpo Inteiro',
    category: 'Mobilidade',
    equipment: 'Peso Corporal',
    notes: 'Alongamento dinâmico e core.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Pular Corda',
    description: 'Cardio com corda',
    muscleGroup: 'Corpo Inteiro',
    category: 'Cardio',
    equipment: 'Corda',
    notes: 'Excelente cardio. Melhora coordenação e queima calorias.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Shadow Boxing',
    description: 'Socos no ar com movimento',
    muscleGroup: 'Corpo Inteiro',
    category: 'Cardio',
    equipment: 'Peso Corporal',
    notes: 'Ótimo cardio e condicionamento de braços.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Skaters',
    description: 'Saltos laterais alternados',
    muscleGroup: 'Pernas',
    category: 'Cardio',
    equipment: 'Peso Corporal',
    notes: 'Saltos laterais. Trabalha lateral das pernas e cardio.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Box Jumps (Cadeira)',
    description: 'Saltar para cima de superfície elevada',
    muscleGroup: 'Pernas',
    category: 'Pliometria',
    equipment: 'Peso Corporal',
    notes: 'Use cadeira resistente ou banco. Desenvolve potência.',
    workoutLocation: WorkoutLocation.Home
  },

  // ============================================
  // EXERCÍCIOS COM FAIXA ELÁSTICA
  // ============================================
  {
    name: 'Remada com Faixa Elástica',
    description: 'Puxa faixa para o abdômen',
    muscleGroup: 'Costas',
    category: 'Força',
    equipment: 'Faixa Elástica',
    notes: 'Prenda faixa na altura do peito. Puxe para abdômen.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Supino com Faixa Elástica',
    description: 'Faixa atrás das costas, empurra',
    muscleGroup: 'Peito',
    category: 'Força',
    equipment: 'Faixa Elástica',
    notes: 'Faixa nas costas. Movimento de supino em pé ou deitado.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Agachamento com Faixa',
    description: 'Pise na faixa, segure extremidades',
    muscleGroup: 'Quadríceps',
    category: 'Força',
    equipment: 'Faixa Elástica',
    notes: 'Resistência elástica no agachamento.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Pull Apart com Faixa',
    description: 'Abrir braços esticando faixa',
    muscleGroup: 'Ombros',
    category: 'Força',
    equipment: 'Faixa Elástica',
    notes: 'Braços estendidos, abra lateralmente. Ótimo para posterior de ombro.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Abdução de Quadril com Faixa',
    description: 'Faixa nos tornozelos, abrir pernas',
    muscleGroup: 'Glúteos',
    category: 'Isolamento',
    equipment: 'Faixa Elástica',
    notes: 'Faixa nos tornozelos. Trabalha glúteo médio.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Kickback com Faixa',
    description: 'Chute para trás contra resistência',
    muscleGroup: 'Glúteos',
    category: 'Isolamento',
    equipment: 'Faixa Elástica',
    notes: 'Chute controlado para trás. Excelente para glúteos.',
    workoutLocation: WorkoutLocation.Home
  },

  // ============================================
  // MOBILIDADE & ALONGAMENTO
  // ============================================
  {
    name: 'Gato-Vaca',
    description: 'Alternância de extensão e flexão da coluna',
    muscleGroup: 'Core',
    category: 'Mobilidade',
    equipment: 'Peso Corporal',
    notes: 'De quatro, alterne arqueamento e curvatura da coluna.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Cachorro Olhando para Baixo',
    description: 'Posição de yoga para posteriores',
    muscleGroup: 'Corpo Inteiro',
    category: 'Mobilidade',
    equipment: 'Peso Corporal',
    notes: 'Posição de V invertido. Alonga posteriores e fortalece ombros.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Cobra',
    description: 'Extensão da coluna deitado de bruços',
    muscleGroup: 'Lombar',
    category: 'Mobilidade',
    equipment: 'Peso Corporal',
    notes: 'Eleva tronco com braços. Alonga abdômen e fortalece lombar.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Ponte (Ponte Completa)',
    description: 'Arqueamento completo da coluna',
    muscleGroup: 'Corpo Inteiro',
    category: 'Mobilidade',
    equipment: 'Peso Corporal',
    notes: 'Exercício avançado. Excelente mobilidade de coluna.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Agachamento Profundo (Deep Squat)',
    description: 'Agachamento de descanso',
    muscleGroup: 'Pernas',
    category: 'Mobilidade',
    equipment: 'Peso Corporal',
    notes: 'Agachamento completo. Melhora mobilidade de quadril e tornozelos.',
    workoutLocation: WorkoutLocation.Home
  },

  // ============================================
  // EXERCÍCIOS DE GINÁSTICA/CALISTENIA AVANÇADOS
  // ============================================
  {
    name: 'L-Sit',
    description: 'Sentar suspenso com pernas retas',
    muscleGroup: 'Core',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Exercício avançado. Força incrível de core.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Muscle-Up',
    description: 'Transição de pull-up para dip',
    muscleGroup: 'Corpo Inteiro',
    category: 'Força',
    equipment: 'Barra Fixa',
    notes: 'Exercício muito avançado. Combina pull-up e dip.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Front Lever',
    description: 'Corpo horizontal suspenso de frente',
    muscleGroup: 'Costas',
    category: 'Força',
    equipment: 'Barra Fixa',
    notes: 'Extremamente avançado. Requer força enorme.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Back Lever',
    description: 'Corpo horizontal suspenso de costas',
    muscleGroup: 'Costas',
    category: 'Força',
    equipment: 'Barra Fixa',
    notes: 'Extremamente avançado. Progressão para calistenia.',
    workoutLocation: WorkoutLocation.Both
  },
  {
    name: 'Handstand',
    description: 'Parada de mão',
    muscleGroup: 'Ombros',
    category: 'Equilíbrio',
    equipment: 'Peso Corporal',
    notes: 'Equilíbrio e força de ombros. Use parede para apoio inicial.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Pistol Squat',
    description: 'Agachamento em uma perna completo',
    muscleGroup: 'Quadríceps',
    category: 'Força',
    equipment: 'Peso Corporal',
    notes: 'Extremamente difícil. Requer força e mobilidade.',
    workoutLocation: WorkoutLocation.Home
  },
  {
    name: 'Dragon Flag',
    description: 'Corpo reto elevado segurando banco',
    muscleGroup: 'Abdômen',
    category: 'Core',
    equipment: 'Peso Corporal',
    notes: 'Um dos exercícios mais difíceis de core. Famoso por Bruce Lee.',
    workoutLocation: WorkoutLocation.Home
  }
];

async function seedExercises() {
  console.log(`🏋️ Iniciando seed de ${exercises.length} exercícios...\n`);
  console.log('📊 Distribuição:');
  console.log(`   🏠 Casa: ${exercises.filter(e => e.workoutLocation === WorkoutLocation.Home).length}`);
  console.log(`   🏋️ Academia: ${exercises.filter(e => e.workoutLocation === WorkoutLocation.Gym).length}`);
  console.log(`   🔄 Ambos: ${exercises.filter(e => e.workoutLocation === WorkoutLocation.Both).length}`);
  console.log('');

  let successCount = 0;
  let errorCount = 0;
  let skippedCount = 0;

  for (const exercise of exercises) {
    try {
      const response = await axios.post(
        `${API_BASE_URL}/exercises`,
        exercise,
        {
          headers: {
            'Content-Type': 'application/json'
          },
          httpsAgent: new (require('https').Agent)({
            rejectUnauthorized: false
          })
        }
      );

      successCount++;
      console.log(`✅ ${exercise.name} (${exercise.muscleGroup})`);

    } catch (error) {
      if (error.response?.status === 409) {
        skippedCount++;
        console.log(`⏭️  ${exercise.name} (já existe)`);
      } else {
        errorCount++;
        const errorMsg = error.response?.data?.message || error.response?.statusText || error.message;
        console.error(`❌ ${exercise.name}: ${errorMsg}`);
      }
    }
  }

  console.log(`\n📊 Resumo:`);
  console.log(`✅ Adicionados: ${successCount}`);
  console.log(`⏭️  Ignorados (duplicados): ${skippedCount}`);
  console.log(`❌ Erros: ${errorCount}`);
  console.log(`📝 Total processado: ${exercises.length}`);
}

// Executar
seedExercises()
  .then(() => {
    console.log('\n🎉 Seed concluído!');
    process.exit(0);
  })
  .catch((error) => {
    console.error('\n💥 Falha no seed:', error.message);
    process.exit(1);
  });
