namespace GymHero.Shared.Enums;

/// <summary>
/// Body angle/view for progress photos
/// </summary>
public enum BodyAngle
{
    Front = 0,         // Vista frontal
    Back = 1,          // Vista traseira/posterior
    LeftSide = 2,      // Vista lateral esquerda
    RightSide = 3,     // Vista lateral direita
    FrontRelaxed = 4,  // Frontal relaxado
    FrontFlexed = 5,   // Frontal flexionado/pose
    BackRelaxed = 6,   // Posterior relaxado
    BackFlexed = 7,    // Posterior flexionado/pose
    Custom = 99        // Ângulo customizado
}
