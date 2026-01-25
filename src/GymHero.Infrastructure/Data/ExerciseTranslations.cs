namespace GymHero.Infrastructure.Data;

/// <summary>
/// Dicionario de traducoes de exercicios ingles -> portugues
/// </summary>
public static class ExerciseTranslations
{
    public static readonly Dictionary<string, string> Translations = new(StringComparer.OrdinalIgnoreCase)
    {
        // PEITO
        { "Bench Press", "Supino Reto com Barra" },
        { "Flat Bench Press", "Supino Reto com Barra" },
        { "Incline Bench Press", "Supino Inclinado com Barra" },
        { "Decline Bench Press", "Supino Declinado com Barra" },
        { "Dumbbell Bench Press", "Supino Reto com Halteres" },
        { "Dumbbell Press", "Supino com Halteres" },
        { "Incline Dumbbell Press", "Supino Inclinado com Halteres" },
        { "Decline Dumbbell Press", "Supino Declinado com Halteres" },
        { "Dumbbell Fly", "Crucifixo com Halteres" },
        { "Dumbbell Flyes", "Crucifixo com Halteres" },
        { "Incline Dumbbell Fly", "Crucifixo Inclinado com Halteres" },
        { "Decline Dumbbell Fly", "Crucifixo Declinado com Halteres" },
        { "Cable Fly", "Crucifixo no Cabo" },
        { "Cable Flyes", "Crucifixo no Cabo" },
        { "Cable Crossover", "Crossover no Cabo" },
        { "Chest Press Machine", "Supino na Maquina" },
        { "Machine Chest Press", "Supino na Maquina" },
        { "Pec Deck", "Peck Deck" },
        { "Pec Deck Machine", "Peck Deck na Maquina" },
        { "Push Up", "Flexao de Braco" },
        { "Push-Up", "Flexao de Braco" },
        { "Pushup", "Flexao de Braco" },
        { "Push Ups", "Flexoes de Braco" },
        { "Diamond Push Up", "Flexao Diamante" },
        { "Wide Push Up", "Flexao com Pegada Larga" },
        { "Decline Push Up", "Flexao Declinada" },
        { "Incline Push Up", "Flexao Inclinada" },
        { "Chest Dip", "Mergulho para Peito" },
        { "Dip", "Mergulho" },
        { "Dips", "Mergulhos" },
        { "Pullover", "Pullover" },
        { "Dumbbell Pullover", "Pullover com Halter" },
        { "Decline Dumbbell Pullover", "Pullover Declinado com Halter" },
        { "Reverse Grip Bench Press", "Supino com Pegada Supinada" },
        { "Reverse Grip Bench Press Barra", "Supino com Pegada Supinada" },
        { "Close Grip Bench Press", "Supino Pegada Fechada" },
        { "Floor Press", "Supino no Chao" },
        { "Landmine Press", "Press Landmine" },

        // COSTAS
        { "Pull Up", "Barra Fixa" },
        { "Pull-Up", "Barra Fixa" },
        { "Pullup", "Barra Fixa" },
        { "Pull Ups", "Barra Fixa" },
        { "Chin Up", "Barra Fixa Supinada" },
        { "Chin-Up", "Barra Fixa Supinada" },
        { "Chinup", "Barra Fixa Supinada" },
        { "Lat Pulldown", "Puxada Frontal" },
        { "Wide Grip Lat Pulldown", "Puxada Frontal Pegada Larga" },
        { "Close Grip Lat Pulldown", "Puxada Frontal Pegada Fechada" },
        { "Reverse Grip Lat Pulldown", "Puxada com Pegada Supinada" },
        { "Seated Cable Row", "Remada Baixa no Cabo" },
        { "Cable Row", "Remada no Cabo" },
        { "Bent Over Row", "Remada Curvada" },
        { "Barbell Row", "Remada Curvada com Barra" },
        { "Bent Over Barbell Row", "Remada Curvada com Barra" },
        { "Dumbbell Row", "Remada Unilateral com Halter" },
        { "One Arm Dumbbell Row", "Remada Unilateral com Halter" },
        { "Single Arm Dumbbell Row", "Remada Unilateral com Halter" },
        { "T-Bar Row", "Remada Cavalinho" },
        { "T Bar Row", "Remada Cavalinho" },
        { "Pendlay Row", "Remada Pendlay" },
        { "Yates Row", "Remada Yates" },
        { "Face Pull", "Face Pull" },
        { "Face Pulls", "Face Pull" },
        { "Deadlift", "Levantamento Terra" },
        { "Conventional Deadlift", "Levantamento Terra Convencional" },
        { "Romanian Deadlift", "Levantamento Terra Romeno" },
        { "RDL", "Levantamento Terra Romeno" },
        { "Sumo Deadlift", "Levantamento Terra Sumo" },
        { "Stiff Leg Deadlift", "Stiff" },
        { "Stiff Legged Deadlift", "Stiff" },
        { "Back Extension", "Hiperextensao" },
        { "Hyperextension", "Hiperextensao" },
        { "Good Morning", "Good Morning" },
        { "Good Mornings", "Good Morning" },
        { "Inverted Row", "Remada Invertida" },
        { "Straight Arm Pulldown", "Pulldown Bracos Estendidos" },
        { "Straight Arm Lat Pulldown", "Pulldown Bracos Estendidos" },
        { "Meadows Row", "Remada Meadows" },
        { "Seal Row", "Remada Seal" },
        { "Chest Supported Row", "Remada com Apoio no Peito" },

        // OMBROS
        { "Overhead Press", "Desenvolvimento Militar" },
        { "Military Press", "Desenvolvimento Militar" },
        { "Shoulder Press", "Desenvolvimento de Ombros" },
        { "Barbell Shoulder Press", "Desenvolvimento com Barra" },
        { "Dumbbell Shoulder Press", "Desenvolvimento com Halteres" },
        { "Seated Shoulder Press", "Desenvolvimento Sentado" },
        { "Standing Shoulder Press", "Desenvolvimento em Pe" },
        { "Arnold Press", "Desenvolvimento Arnold" },
        { "Lateral Raise", "Elevacao Lateral" },
        { "Lateral Raises", "Elevacao Lateral" },
        { "Side Lateral Raise", "Elevacao Lateral" },
        { "Dumbbell Lateral Raise", "Elevacao Lateral com Halteres" },
        { "Cable Lateral Raise", "Elevacao Lateral no Cabo" },
        { "Front Raise", "Elevacao Frontal" },
        { "Front Raises", "Elevacao Frontal" },
        { "Dumbbell Front Raise", "Elevacao Frontal com Halteres" },
        { "Barbell Front Raise", "Elevacao Frontal com Barra" },
        { "Plate Front Raise", "Elevacao Frontal com Anilha" },
        { "Rear Delt Fly", "Elevacao Posterior" },
        { "Rear Delt Flyes", "Elevacao Posterior" },
        { "Reverse Fly", "Crucifixo Invertido" },
        { "Reverse Flyes", "Crucifixo Invertido" },
        { "Bent Over Lateral Raise", "Elevacao Lateral Curvado" },
        { "Bent Over Rear Delt Raise", "Elevacao Posterior Curvado" },
        { "Upright Row", "Remada Alta" },
        { "Barbell Upright Row", "Remada Alta com Barra" },
        { "Dumbbell Upright Row", "Remada Alta com Halteres" },
        { "Shrug", "Encolhimento de Ombros" },
        { "Shrugs", "Encolhimento de Ombros" },
        { "Barbell Shrug", "Encolhimento com Barra" },
        { "Dumbbell Shrug", "Encolhimento com Halteres" },
        { "Trap Bar Shrug", "Encolhimento com Trap Bar" },
        { "External Rotation", "Rotacao Externa" },
        { "Internal Rotation", "Rotacao Interna" },
        { "Cuban Press", "Desenvolvimento Cubano" },
        { "Lu Raise", "Elevacao Lu" },
        { "Y Raise", "Elevacao em Y" },
        { "W Raise", "Elevacao em W" },

        // BICEPS
        { "Bicep Curl", "Rosca Direta" },
        { "Biceps Curl", "Rosca Direta" },
        { "Barbell Curl", "Rosca Direta com Barra" },
        { "Dumbbell Curl", "Rosca Direta com Halteres" },
        { "Dumbbell Curls", "Rosca Direta com Halteres" },
        { "Hammer Curl", "Rosca Martelo" },
        { "Hammer Curls", "Rosca Martelo" },
        { "Preacher Curl", "Rosca Scott" },
        { "Preacher Curls", "Rosca Scott" },
        { "Concentration Curl", "Rosca Concentrada" },
        { "Concentration Curls", "Rosca Concentrada" },
        { "Incline Dumbbell Curl", "Rosca Inclinada" },
        { "Incline Curl", "Rosca Inclinada" },
        { "Incline Hammer Curl", "Rosca Martelo Inclinada" },
        { "Cable Curl", "Rosca no Cabo" },
        { "Cable Curls", "Rosca no Cabo" },
        { "Spider Curl", "Rosca Spider" },
        { "Spider Curls", "Rosca Spider" },
        { "Spider Curl Barra EZ", "Rosca Spider com Barra W" },
        { "EZ Bar Curl", "Rosca com Barra W" },
        { "EZ Curl", "Rosca com Barra W" },
        { "Reverse Curl", "Rosca Inversa" },
        { "Reverse Curls", "Rosca Inversa" },
        { "Zottman Curl", "Rosca Zottman" },
        { "21s", "Rosca 21" },
        { "Drag Curl", "Rosca Drag" },
        { "Bayesian Curl", "Rosca Bayesiana" },

        // TRICEPS
        { "Tricep Pushdown", "Triceps Pulley" },
        { "Triceps Pushdown", "Triceps Pulley" },
        { "Cable Pushdown", "Triceps Pulley" },
        { "Tricep Extension", "Extensao de Triceps" },
        { "Triceps Extension", "Extensao de Triceps" },
        { "Rope Pushdown", "Triceps Corda" },
        { "Tricep Rope Pushdown", "Triceps Corda" },
        { "Tricep Dip", "Mergulho para Triceps" },
        { "Bench Dip", "Triceps no Banco" },
        { "Bench Dips", "Triceps no Banco" },
        { "Skull Crusher", "Triceps Testa" },
        { "Skull Crushers", "Triceps Testa" },
        { "Lying Tricep Extension", "Triceps Testa Deitado" },
        { "Overhead Tricep Extension", "Triceps Frances" },
        { "French Press", "Triceps Frances" },
        { "Dumbbell Kickback", "Triceps Coice" },
        { "Tricep Kickback", "Triceps Coice" },
        { "Kickback", "Triceps Coice" },
        { "JM Press", "JM Press" },
        { "Close Grip Push Up", "Flexao Pegada Fechada" },

        // PERNAS - QUADRICEPS
        { "Squat", "Agachamento Livre" },
        { "Squats", "Agachamento Livre" },
        { "Back Squat", "Agachamento com Barra" },
        { "Barbell Squat", "Agachamento com Barra" },
        { "Barbell Back Squat", "Agachamento com Barra" },
        { "Front Squat", "Agachamento Frontal" },
        { "Goblet Squat", "Agachamento Goblet" },
        { "Leg Press", "Leg Press" },
        { "45 Degree Leg Press", "Leg Press 45 Graus" },
        { "Hack Squat", "Hack Squat" },
        { "Hack Squat Machine", "Hack na Maquina" },
        { "Leg Extension", "Cadeira Extensora" },
        { "Leg Extensions", "Cadeira Extensora" },
        { "Lunge", "Afundo" },
        { "Lunges", "Afundos" },
        { "Walking Lunge", "Afundo Caminhando" },
        { "Walking Lunges", "Afundos Caminhando" },
        { "Reverse Lunge", "Afundo Reverso" },
        { "Bulgarian Split Squat", "Agachamento Bulgaro" },
        { "Split Squat", "Agachamento Unilateral" },
        { "Step Up", "Step Up" },
        { "Step Ups", "Step Up" },
        { "Sissy Squat", "Sissy Squat" },
        { "Sissy Squat Machine", "Sissy Squat na Maquina" },
        { "Pistol Squat", "Agachamento Pistol" },
        { "Box Squat", "Agachamento no Box" },
        { "Sumo Squat", "Agachamento Sumo" },
        { "Jump Squat", "Agachamento com Salto" },
        { "Pendulum Squat Machine", "Agachamento Pendulo" },
        { "Belt Squat Machine", "Agachamento com Cinto" },
        { "Belt Squat", "Agachamento com Cinto" },
        { "Zercher Squat", "Agachamento Zercher" },
        { "Anderson Squat", "Agachamento Anderson" },
        { "Pause Squat", "Agachamento com Pausa" },

        // PERNAS - POSTERIOR
        { "Leg Curl", "Mesa Flexora" },
        { "Leg Curls", "Mesa Flexora" },
        { "Lying Leg Curl", "Mesa Flexora Deitado" },
        { "Seated Leg Curl", "Cadeira Flexora" },
        { "Standing Leg Curl", "Flexora em Pe" },
        { "Nordic Curl", "Nordic Curl" },
        { "Nordic Curls", "Nordic Curl" },
        { "Glute Ham Raise", "Glute Ham Raise" },
        { "GHR", "Glute Ham Raise" },

        // PERNAS - GLUTEOS
        { "Hip Thrust", "Hip Thrust" },
        { "Barbell Hip Thrust", "Hip Thrust com Barra" },
        { "Glute Bridge", "Elevacao Pelvica" },
        { "Single Leg Glute Bridge", "Elevacao Pelvica Unilateral" },
        { "Cable Kickback", "Gluteo no Cabo" },
        { "Cable Glute Kickback", "Gluteo no Cabo" },
        { "Donkey Kick", "Coice de Burro" },
        { "Donkey Kicks", "Coice de Burro" },
        { "Fire Hydrant", "Hidrante" },
        { "Fire Hydrants", "Hidrante" },
        { "Clamshell", "Conchinha" },
        { "Clamshells", "Conchinha" },
        { "Hip Abduction", "Abducao de Quadril" },
        { "Hip Adduction", "Aducao de Quadril" },
        { "Cable Pull Through", "Pull Through no Cabo" },
        { "Cable Pull Through Sumo", "Pull Through Sumo no Cabo" },
        { "Frog Pump", "Frog Pump" },

        // PERNAS - PANTURRILHA
        { "Calf Raise", "Elevacao de Panturrilha" },
        { "Calf Raises", "Elevacao de Panturrilha" },
        { "Standing Calf Raise", "Panturrilha em Pe" },
        { "Seated Calf Raise", "Panturrilha Sentado" },
        { "Donkey Calf Raise", "Panturrilha Donkey" },
        { "Single Leg Calf Raise", "Panturrilha Unilateral" },
        { "Diamond Calf Raise", "Panturrilha Diamante" },
        { "Smith Machine Calf Raise", "Panturrilha no Smith" },
        { "Leg Press Calf Raise", "Panturrilha no Leg Press" },

        // CORE
        { "Crunch", "Abdominal Crunch" },
        { "Crunches", "Abdominal Crunch" },
        { "Sit Up", "Abdominal Tradicional" },
        { "Sit Ups", "Abdominal Tradicional" },
        { "Situp", "Abdominal Tradicional" },
        { "Plank", "Prancha" },
        { "Planks", "Prancha" },
        { "Side Plank", "Prancha Lateral" },
        { "Side Planks", "Prancha Lateral" },
        { "Mountain Climber", "Escalador" },
        { "Mountain Climbers", "Escaladores" },
        { "Leg Raise", "Elevacao de Pernas" },
        { "Leg Raises", "Elevacao de Pernas" },
        { "Hanging Leg Raise", "Elevacao de Pernas na Barra" },
        { "Hanging Knee Raise", "Elevacao de Joelhos na Barra" },
        { "Russian Twist", "Torcao Russa" },
        { "Russian Twists", "Torcao Russa" },
        { "Bicycle Crunch", "Abdominal Bicicleta" },
        { "Bicycle Crunches", "Abdominal Bicicleta" },
        { "Dead Bug", "Dead Bug" },
        { "Dead Bugs", "Dead Bug" },
        { "Bird Dog", "Bird Dog" },
        { "Bird Dogs", "Bird Dog" },
        { "Ab Wheel Rollout", "Roda Abdominal" },
        { "Ab Wheel", "Roda Abdominal" },
        { "Cable Crunch", "Abdominal no Cabo" },
        { "Cable Crunches", "Abdominal no Cabo" },
        { "Woodchop", "Woodchop" },
        { "Wood Chop", "Woodchop" },
        { "Cable Woodchop", "Woodchop no Cabo" },
        { "Pallof Press", "Pallof Press" },
        { "Hollow Body Hold", "Hollow Body" },
        { "Hollow Hold", "Hollow Body" },
        { "V-Up", "V-Up" },
        { "V Up", "V-Up" },
        { "V Ups", "V-Up" },
        { "Flutter Kick", "Flutter Kicks" },
        { "Flutter Kicks", "Flutter Kicks" },
        { "Toe Touch", "Toque nos Pes" },
        { "Toe Touches", "Toque nos Pes" },
        { "Reverse Crunch", "Abdominal Infra" },
        { "Reverse Crunches", "Abdominal Infra" },
        { "Dragon Flag", "Dragon Flag" },
        { "L-Sit", "L-Sit" },
        { "L Sit", "L-Sit" },

        // CARDIO/FUNCIONAL
        { "Burpee", "Burpee" },
        { "Burpees", "Burpees" },
        { "Jumping Jack", "Polichinelo" },
        { "Jumping Jacks", "Polichinelos" },
        { "High Knees", "Elevacao de Joelhos" },
        { "High Knee", "Elevacao de Joelhos" },
        { "Butt Kick", "Chute no Gluteo" },
        { "Butt Kicks", "Chute no Gluteo" },
        { "Box Jump", "Salto no Box" },
        { "Box Jumps", "Salto no Box" },
        { "Jump Rope", "Pular Corda" },
        { "Skipping Rope", "Pular Corda" },
        { "Kettlebell Swing", "Swing com Kettlebell" },
        { "KB Swing", "Swing com Kettlebell" },
        { "Thruster", "Thruster" },
        { "Thrusters", "Thruster" },
        { "Wall Ball", "Wall Ball" },
        { "Wall Balls", "Wall Ball" },
        { "Battle Rope", "Cordas Navais" },
        { "Battle Ropes", "Cordas Navais" },
        { "Rowing", "Remo" },
        { "Running", "Corrida" },
        { "Sprinting", "Sprint" },
        { "Sprint", "Sprint" },
        { "Cycling", "Ciclismo" },
        { "Elliptical", "Eliptico" },
        { "Stair Climbing", "Subir Escadas" },
        { "Sled Push", "Empurrar Trenó" },
        { "Sled Pull", "Puxar Trenó" },
        { "Farmers Walk", "Caminhada do Fazendeiro" },
        { "Farmers Carry", "Caminhada do Fazendeiro" },
        { "Clean and Jerk", "Arremesso" },
        { "Clean and Press", "Clean and Press" },
        { "Snatch", "Arranco" },
        { "Power Clean", "Power Clean" },
        { "Hang Clean", "Hang Clean" },

        // ALONGAMENTO
        { "Hamstring Stretch", "Alongamento de Posterior" },
        { "Quad Stretch", "Alongamento de Quadriceps" },
        { "Calf Stretch", "Alongamento de Panturrilha" },
        { "Hip Flexor Stretch", "Alongamento de Flexor de Quadril" },
        { "Chest Stretch", "Alongamento de Peito" },
        { "Shoulder Stretch", "Alongamento de Ombro" },
        { "Tricep Stretch", "Alongamento de Triceps" },
        { "Bicep Stretch", "Alongamento de Biceps" },
        { "Lat Stretch", "Alongamento de Latissimo" },
        { "Cat Cow", "Gato e Vaca" },
        { "Cat Cow Stretch", "Gato e Vaca" },
        { "Child's Pose", "Postura da Crianca" },
        { "Childs Pose", "Postura da Crianca" },
        { "Downward Dog", "Cachorro Olhando para Baixo" },
        { "Downward Facing Dog", "Cachorro Olhando para Baixo" },
        { "Pigeon Pose", "Postura do Pombo" },
        { "Butterfly Stretch", "Alongamento Borboleta" },
        { "Figure Four Stretch", "Alongamento Figura 4" },
        { "Frog Stretch", "Alongamento do Sapo" },
        { "World's Greatest Stretch", "Melhor Alongamento do Mundo" },
        { "90/90 Stretch", "Alongamento 90/90" },
        { "Couch Stretch", "Alongamento no Sofa" },
    };

    /// <summary>
    /// Palavras em ingles que indicam que o nome precisa de traducao
    /// </summary>
    public static readonly HashSet<string> EnglishIndicators = new(StringComparer.OrdinalIgnoreCase)
    {
        "press", "curl", "row", "pull", "push", "raise", "fly", "flyes",
        "extension", "squat", "lunge", "deadlift", "bench", "crunch",
        "plank", "dip", "dips", "stretch", "kick", "jump", "swing",
        "hold", "walk", "carry", "thrust", "bridge", "rotation",
        "seated", "standing", "lying", "incline", "decline", "overhead",
        "cable", "machine", "barbell", "dumbbell", "kettlebell",
        "reverse", "single", "double", "wide", "close", "grip",
        "leg", "arm", "shoulder", "chest", "back", "glute", "calf",
        "hammer", "spider", "preacher", "concentration", "drag",
        "skull", "crusher", "kickback", "pulldown", "pushdown",
        "pullover", "crossover", "shrug", "twist", "climber"
    };

    /// <summary>
    /// Verifica se o nome do exercicio precisa de traducao
    /// </summary>
    public static bool NeedsTranslation(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var lower = name.ToLower();

        // Se contem acentos portugueses, provavelmente ja esta traduzido
        if (lower.Contains('ç') || lower.Contains('ã') || lower.Contains('õ') ||
            lower.Contains('á') || lower.Contains('é') || lower.Contains('í') ||
            lower.Contains('ó') || lower.Contains('ú') || lower.Contains('â') ||
            lower.Contains('ê') || lower.Contains('ô'))
        {
            return false;
        }

        // Verifica se contem palavras em ingles
        var words = lower.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        return words.Any(word => EnglishIndicators.Contains(word));
    }

    /// <summary>
    /// Traduz o nome do exercicio se houver traducao disponivel
    /// </summary>
    public static string Translate(string englishName)
    {
        if (string.IsNullOrWhiteSpace(englishName))
            return englishName;

        // Tenta traducao exata
        if (Translations.TryGetValue(englishName, out var translation))
            return translation;

        // Tenta encontrar traducao parcial removendo sufixos comuns
        var normalized = englishName
            .Replace(" Barra", "")
            .Replace(" Halter", "")
            .Replace(" Halteres", "")
            .Replace(" Machine", "")
            .Replace(" Maquina", "")
            .Trim();

        if (Translations.TryGetValue(normalized, out translation))
            return translation;

        // Se nao encontrar traducao, retorna o original
        return englishName;
    }

    /// <summary>
    /// Gera uma URL de busca do YouTube para o exercicio
    /// </summary>
    public static string GenerateYouTubeSearchUrl(string exerciseName)
    {
        var searchQuery = Uri.EscapeDataString($"{exerciseName} execucao correta tecnica");
        return $"https://www.youtube.com/results?search_query={searchQuery}";
    }

    /// <summary>
    /// Gera uma URL de imagem placeholder para o exercicio
    /// </summary>
    public static string GeneratePlaceholderImageUrl(string exerciseName, string muscleGroup)
    {
        var encodedName = Uri.EscapeDataString(exerciseName.Length > 30 ? exerciseName[..30] + "..." : exerciseName);
        return $"https://placehold.co/400x300/1a1a2e/ffffff?text={encodedName}";
    }

    /// <summary>
    /// Gera uma descricao basica para o exercicio baseada no grupo muscular
    /// </summary>
    public static string GenerateDescription(string exerciseName, string muscleGroup)
    {
        var muscleNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Chest", "peitoral" },
            { "Back", "costas" },
            { "Shoulders", "ombros" },
            { "Biceps", "biceps" },
            { "Triceps", "triceps" },
            { "Forearms", "antebracos" },
            { "Core", "abdomen" },
            { "Quadriceps", "quadriceps" },
            { "Hamstrings", "posterior de coxa" },
            { "Glutes", "gluteos" },
            { "Calves", "panturrilhas" },
            { "FullBody", "corpo inteiro" },
            { "Cardio", "sistema cardiovascular" },
            { "LowerBack", "lombar" },
        };

        var muscle = muscleNames.GetValueOrDefault(muscleGroup, "musculos");
        return $"Exercicio para desenvolvimento do {muscle}. Execute com tecnica controlada para melhores resultados.";
    }
}
