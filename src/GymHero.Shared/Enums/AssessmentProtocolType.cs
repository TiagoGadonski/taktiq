namespace GymHero.Shared.Enums;

public enum AssessmentProtocolType
{
    // Body Composition
    SkinfoldJacksonPollock7 = 0,    // 7 dobras cutâneas Jackson-Pollock
    SkinfoldJacksonPollock3 = 1,    // 3 dobras cutâneas Jackson-Pollock
    Circumferences = 2,              // Circunferências corporais

    // Cardiovascular
    CooperTest = 10,                 // Teste de Cooper (12 min)
    RockportWalkTest = 11,           // Teste de caminhada 1 milha
    StepTest = 12,                   // Teste de step
    RestingHeartRate = 13,           // Frequência cardíaca de repouso

    // Muscular Strength
    PushUpTest = 20,                 // Teste de flexão
    SitUpTest = 21,                  // Teste abdominal
    PlankTest = 22,                  // Teste de prancha
    GripStrength = 23,               // Força de preensão manual
    OneRepMax = 24,                  // 1RM (força máxima)

    // Flexibility
    SitAndReach = 30,                // Teste sentar e alcançar
    ShoulderFlexibility = 31,        // Flexibilidade de ombro

    // Functional Movement
    SquatAssessment = 40,            // Avaliação de agachamento
    OverheadSquat = 41,              // Agachamento overhead
    SingleLegBalance = 42,           // Equilíbrio unipodal

    // Postural
    PosturalAnalysis = 50,           // Análise postural completa

    // Custom
    Custom = 99                      // Protocolo customizado
}
