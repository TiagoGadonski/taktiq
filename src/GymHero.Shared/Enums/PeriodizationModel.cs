namespace GymHero.Shared.Enums;

/// <summary>
/// Different periodization models for workout programming
/// </summary>
public enum PeriodizationModel
{
    Linear,              // Classic linear progression (volume decreases, intensity increases)
    Undulating,          // Daily Undulating Periodization (DUP) - varies daily
    BlockPeriodization,  // Focused training blocks
    Conjugate,           // Westside Barbell style
    WaveLoading         // Wave-like progression
}

/// <summary>
/// Training phases in a periodized program
/// </summary>
public enum TrainingPhase
{
    Anatomical,      // Anatomical Adaptation - learning movements, high reps
    Hypertrophy,     // Muscle building - moderate weight, moderate reps
    Strength,        // Strength building - heavy weight, low reps
    Power,           // Power/explosiveness - moderate weight, explosive
    Peaking,         // Competition prep - very heavy, very low reps
    Deload           // Recovery week - reduced volume/intensity
}

/// <summary>
/// Progression strategy for loads
/// </summary>
public enum ProgressionStrategy
{
    Linear,          // Add fixed amount each week
    DoubleProgression, // Increase reps first, then weight
    Percentage,      // Based on % of 1RM
    RPE,            // Based on Rate of Perceived Exertion
    WaveLoading     // Undulating loads
}
