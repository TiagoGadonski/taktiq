namespace GymHero.Shared.DTOs;

/// <summary>
/// Resposta da análise postural por IA
/// </summary>
public record PosturalAnalysisResponse(
    string ForwardHead,           // "None", "Mild", "Moderate", "Severe"
    string RoundedShoulders,      // "None", "Mild", "Moderate", "Severe"
    string AnteriorPelvicTilt,    // "None", "Mild", "Moderate", "Severe"
    string PosteriorPelvicTilt,   // "None", "Mild", "Moderate", "Severe"
    string KneeValgus,            // "None", "Mild", "Moderate", "Severe"
    string KneeVarus,             // "None", "Mild", "Moderate", "Severe"
    string Scoliosis,             // "None", "Mild", "Moderate", "Severe"
    string FlatFeet,              // "None", "Mild", "Moderate", "Severe"
    string Observations,          // Observações detalhadas da IA
    string Recommendations        // Recomendações de exercícios corretivos
);

/// <summary>
/// Resposta da API do OpenAI Vision
/// </summary>
public record OpenAIVisionResponse(
    List<OpenAIChoice> Choices
);

public record OpenAIChoice(
    OpenAIMessage Message
);

public record OpenAIMessage(
    string Content
);
