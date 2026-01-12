using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
using GymHero.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymHero.Infrastructure.Services;

public class PeriodizationService : IPeriodizationService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<PeriodizationService> _logger;

    public PeriodizationService(
        IApplicationDbContext context,
        ILogger<PeriodizationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PeriodizedPlanResponse> GeneratePeriodizedPlanAsync(
        GeneratePeriodizedPlanRequest request,
        Guid trainerId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating periodized plan: {PlanName} for student {StudentId}",
            request.PlanName, request.StudentId);

        // Verify student belongs to trainer
        var student = await _context.Users
            .Where(u => u.Id == request.StudentId && u.PersonalTrainerId == trainerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (student == null)
        {
            throw new InvalidOperationException("Student not found or access denied");
        }

        // Calculate weekly progressions
        var progressions = CalculateProgressions(
            request.DurationWeeks,
            request.Model,
            request.StartingPhase,
            request.IncludeDeloadWeeks);

        // Create the workout plan
        var plan = new WorkoutPlan
        {
            Id = Guid.NewGuid(),
            OwnerId = trainerId,
            Name = request.PlanName,
            Description = request.Description ?? $"Periodized {request.Model} plan - {request.DurationWeeks} weeks",
            Goal = $"Progressive training using {request.Model} periodization",
            Duration = request.DurationWeeks,
            StartDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(request.DurationWeeks * 7),
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkoutPlans.Add(plan);

        // Get exercises for each muscle group
        var exercises = await GetExercisesForMuscleGroupsAsync(
            request.TargetMuscleGroups,
            cancellationToken);

        if (!exercises.Any())
        {
            throw new InvalidOperationException("No exercises found for the specified muscle groups");
        }

        // Generate workouts for each week
        var workouts = new List<Workout>();
        var phaseSchedule = new List<PhaseSchedule>();

        for (int week = 1; week <= request.DurationWeeks; week++)
        {
            var progression = progressions[week - 1];

            // Track phase changes
            if (week == 1 || progressions[week - 2].Phase != progression.Phase)
            {
                var phaseEnd = progressions.FindIndex(week - 1, p => p.Phase != progression.Phase);
                if (phaseEnd == -1) phaseEnd = request.DurationWeeks;
                else phaseEnd += week - 1;

                phaseSchedule.Add(new PhaseSchedule(
                    progression.Phase,
                    week,
                    phaseEnd,
                    (phaseEnd - week + 1) * request.WorkoutsPerWeek,
                    GetPhaseDescription(progression.Phase)
                ));
            }

            // Generate workouts for this week
            for (int day = 0; day < request.WorkoutsPerWeek; day++)
            {
                var workout = CreateWorkoutForWeek(
                    plan.Id,
                    week,
                    day,
                    progression,
                    exercises,
                    request.WorkoutsPerWeek);

                workouts.Add(workout);
            }
        }

        plan.Workouts = workouts;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully generated periodized plan {PlanId} with {WorkoutCount} workouts",
            plan.Id, workouts.Count);

        return new PeriodizedPlanResponse(
            plan.Id,
            plan.Name,
            request.DurationWeeks,
            phaseSchedule,
            plan.StartDate.Value,
            plan.ExpirationDate,
            $"Generated {workouts.Count} workouts across {phaseSchedule.Count} training phases"
        );
    }

    public List<WeeklyProgression> CalculateProgressions(
        int durationWeeks,
        PeriodizationModel model,
        TrainingPhase startingPhase,
        bool includeDeload)
    {
        var progressions = new List<WeeklyProgression>();

        switch (model)
        {
            case PeriodizationModel.Linear:
                progressions = GenerateLinearProgression(durationWeeks, startingPhase, includeDeload);
                break;

            case PeriodizationModel.Undulating:
                progressions = GenerateUndulatingProgression(durationWeeks, startingPhase, includeDeload);
                break;

            case PeriodizationModel.BlockPeriodization:
                progressions = GenerateBlockProgression(durationWeeks, startingPhase, includeDeload);
                break;

            case PeriodizationModel.WaveLoading:
                progressions = GenerateWaveProgression(durationWeeks, startingPhase, includeDeload);
                break;

            default:
                progressions = GenerateLinearProgression(durationWeeks, startingPhase, includeDeload);
                break;
        }

        return progressions;
    }

    private List<WeeklyProgression> GenerateLinearProgression(
        int durationWeeks,
        TrainingPhase startingPhase,
        bool includeDeload)
    {
        var progressions = new List<WeeklyProgression>();
        var phases = new[] { TrainingPhase.Hypertrophy, TrainingPhase.Strength, TrainingPhase.Power };
        var currentPhaseIndex = Array.IndexOf(phases, startingPhase);
        if (currentPhaseIndex == -1) currentPhaseIndex = 0;

        var weeksPerPhase = includeDeload ? 3 : 4;
        var currentPhase = phases[currentPhaseIndex];

        for (int week = 1; week <= durationWeeks; week++)
        {
            bool isDeload = includeDeload && week % 4 == 0;

            if (!isDeload && week > 1 && (week - 1) % weeksPerPhase == 0)
            {
                currentPhaseIndex = Math.Min(currentPhaseIndex + 1, phases.Length - 1);
                currentPhase = phases[currentPhaseIndex];
            }

            var (sets, reps, intensity) = GetPhaseParameters(currentPhase, week, isDeload);

            progressions.Add(new WeeklyProgression(
                week,
                isDeload ? TrainingPhase.Deload : currentPhase,
                intensity,
                sets,
                reps,
                isDeload
            ));
        }

        return progressions;
    }

    private List<WeeklyProgression> GenerateUndulatingProgression(
        int durationWeeks,
        TrainingPhase startingPhase,
        bool includeDeload)
    {
        var progressions = new List<WeeklyProgression>();
        var phases = new[] { TrainingPhase.Hypertrophy, TrainingPhase.Strength, TrainingPhase.Power };

        for (int week = 1; week <= durationWeeks; week++)
        {
            bool isDeload = includeDeload && week % 4 == 0;
            var phase = isDeload ? TrainingPhase.Deload : phases[(week - 1) % phases.Length];
            var (sets, reps, intensity) = GetPhaseParameters(phase, week, isDeload);

            progressions.Add(new WeeklyProgression(week, phase, intensity, sets, reps, isDeload));
        }

        return progressions;
    }

    private List<WeeklyProgression> GenerateBlockProgression(
        int durationWeeks,
        TrainingPhase startingPhase,
        bool includeDeload)
    {
        var progressions = new List<WeeklyProgression>();
        var blockSize = 4;
        var phases = new[] { TrainingPhase.Hypertrophy, TrainingPhase.Strength, TrainingPhase.Power };
        var currentPhaseIndex = 0;

        for (int week = 1; week <= durationWeeks; week++)
        {
            bool isDeload = includeDeload && week % blockSize == 0;

            if (week > 1 && week % blockSize == 1)
            {
                currentPhaseIndex = (currentPhaseIndex + 1) % phases.Length;
            }

            var phase = isDeload ? TrainingPhase.Deload : phases[currentPhaseIndex];
            var (sets, reps, intensity) = GetPhaseParameters(phase, week, isDeload);

            progressions.Add(new WeeklyProgression(week, phase, intensity, sets, reps, isDeload));
        }

        return progressions;
    }

    private List<WeeklyProgression> GenerateWaveProgression(
        int durationWeeks,
        TrainingPhase startingPhase,
        bool includeDeload)
    {
        var progressions = new List<WeeklyProgression>();

        for (int week = 1; week <= durationWeeks; week++)
        {
            bool isDeload = includeDeload && week % 4 == 0;

            // Wave pattern: week 1 (high), week 2 (medium), week 3 (low), week 4 (deload)
            var wavePosition = (week - 1) % 4;
            var intensity = isDeload ? 0.6 : (0.75 + (wavePosition * 0.05));
            var sets = isDeload ? 2 : (3 + wavePosition);
            var reps = isDeload ? "12-15" : wavePosition switch
            {
                0 => "8-10",
                1 => "6-8",
                2 => "4-6",
                _ => "12-15"
            };

            progressions.Add(new WeeklyProgression(
                week,
                isDeload ? TrainingPhase.Deload : TrainingPhase.Strength,
                intensity,
                sets,
                reps,
                isDeload
            ));
        }

        return progressions;
    }

    private (int sets, string reps, double intensity) GetPhaseParameters(
        TrainingPhase phase,
        int weekNumber,
        bool isDeload)
    {
        if (isDeload)
        {
            return (2, "12-15", 0.6);
        }

        return phase switch
        {
            TrainingPhase.Anatomical => (2, "15-20", 0.5),
            TrainingPhase.Hypertrophy => (3, "8-12", 0.7 + (weekNumber * 0.02)),
            TrainingPhase.Strength => (4, "4-6", 0.8 + (weekNumber * 0.01)),
            TrainingPhase.Power => (3, "3-5", 0.85),
            TrainingPhase.Peaking => (2, "1-3", 0.95),
            _ => (3, "8-12", 0.7)
        };
    }

    private Workout CreateWorkoutForWeek(
        Guid planId,
        int weekNumber,
        int dayNumber,
        WeeklyProgression progression,
        List<Exercise> availableExercises,
        int workoutsPerWeek)
    {
        var workout = new Workout
        {
            Id = Guid.NewGuid(),
            PlanId = planId,
            Name = progression.IsDeloadWeek
                ? $"Week {weekNumber} - Deload Day {dayNumber + 1}"
                : $"Week {weekNumber} - {progression.Phase} Day {dayNumber + 1}",
            DayOfWeek = dayNumber,
            Order = ((weekNumber - 1) * workoutsPerWeek) + dayNumber + 1,
            CreatedAt = DateTime.UtcNow
        };

        // Select 4-6 exercises per workout
        var exerciseCount = progression.IsDeloadWeek ? 4 : 6;
        var selectedExercises = availableExercises
            .OrderBy(x => Guid.NewGuid())
            .Take(exerciseCount)
            .ToList();

        var workoutExercises = new List<WorkoutExercise>();
        for (int i = 0; i < selectedExercises.Count; i++)
        {
            var exercise = selectedExercises[i];
            var workoutExercise = new WorkoutExercise
            {
                Id = Guid.NewGuid(),
                WorkoutId = workout.Id,
                ExerciseId = exercise.Id,
                Order = i + 1,
                TargetSets = progression.SetsPerExercise,
                TargetReps = int.TryParse(progression.RepsRange.Split('-')[0], out var reps) ? reps : 10,
                TargetRepsRange = progression.RepsRange,
                TargetLoad = 0,
                RestSeconds = progression.IsDeloadWeek ? 90 : 120,
                Notes = $"Intensity: {(progression.IntensityMultiplier * 100):F0}%",
                CreatedAt = DateTime.UtcNow
            };

            workoutExercises.Add(workoutExercise);
        }

        workout.Exercises = workoutExercises;
        return workout;
    }

    private async Task<List<Exercise>> GetExercisesForMuscleGroupsAsync(
        List<string> muscleGroups,
        CancellationToken cancellationToken)
    {
        // Parse string muscle groups to enums
        var muscleGroupEnums = muscleGroups
            .Select(mg => Enum.TryParse<MuscleGroup>(mg, true, out var result) ? result : (MuscleGroup?)null)
            .Where(mg => mg.HasValue)
            .Select(mg => mg!.Value)
            .ToList();

        if (!muscleGroupEnums.Any())
        {
            // If no valid muscle groups, return all exercises
            return await _context.Exercises.ToListAsync(cancellationToken);
        }

        var exercises = await _context.Exercises
            .Where(e => muscleGroupEnums.Contains(e.MuscleGroup))
            .ToListAsync(cancellationToken);

        return exercises;
    }

    private string GetPhaseDescription(TrainingPhase phase)
    {
        return phase switch
        {
            TrainingPhase.Anatomical => "Learning movement patterns and building base",
            TrainingPhase.Hypertrophy => "Muscle building phase with moderate intensity",
            TrainingPhase.Strength => "Strength building with heavy loads",
            TrainingPhase.Power => "Power development with explosive movements",
            TrainingPhase.Peaking => "Competition preparation",
            TrainingPhase.Deload => "Recovery and adaptation",
            _ => "Training phase"
        };
    }

    public async Task<List<PeriodizationTemplateDto>> GetPeriodizationTemplatesAsync(
        PeriodizationModel? model = null,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        // Return predefined templates
        var templates = new List<PeriodizationTemplateDto>
        {
            new PeriodizationTemplateDto(
                Guid.NewGuid(),
                "Classic Linear 12-Week",
                "Traditional linear periodization from hypertrophy to strength to power",
                PeriodizationModel.Linear,
                12,
                new List<TrainingPhaseConfig>
                {
                    new(TrainingPhase.Hypertrophy, 4, 3, "8-12", 90, 0.70, "Build muscle foundation"),
                    new(TrainingPhase.Strength, 4, 4, "4-6", 180, 0.85, "Build maximal strength"),
                    new(TrainingPhase.Power, 4, 3, "3-5", 120, 0.75, "Develop explosive power")
                },
                DateTime.UtcNow
            ),
            new PeriodizationTemplateDto(
                Guid.NewGuid(),
                "Daily Undulating 8-Week",
                "Vary intensity daily for continuous adaptation",
                PeriodizationModel.Undulating,
                8,
                new List<TrainingPhaseConfig>
                {
                    new(TrainingPhase.Hypertrophy, 2, 3, "8-12", 90, 0.70, "Hypertrophy focus"),
                    new(TrainingPhase.Strength, 2, 4, "4-6", 180, 0.85, "Strength focus"),
                    new(TrainingPhase.Power, 2, 3, "3-5", 120, 0.75, "Power focus")
                },
                DateTime.UtcNow
            ),
            new PeriodizationTemplateDto(
                Guid.NewGuid(),
                "Block Periodization 16-Week",
                "Focused training blocks for specific adaptations",
                PeriodizationModel.BlockPeriodization,
                16,
                new List<TrainingPhaseConfig>
                {
                    new(TrainingPhase.Anatomical, 4, 2, "15-20", 60, 0.50, "Movement learning"),
                    new(TrainingPhase.Hypertrophy, 4, 3, "8-12", 90, 0.70, "Muscle building"),
                    new(TrainingPhase.Strength, 4, 4, "4-6", 180, 0.85, "Strength building"),
                    new(TrainingPhase.Power, 4, 3, "3-5", 120, 0.75, "Power development")
                },
                DateTime.UtcNow
            )
        };

        if (model.HasValue)
        {
            templates = templates.Where(t => t.Model == model.Value).ToList();
        }

        return templates;
    }

    public async Task<PeriodizationTemplateDto> CreateTemplateAsync(
        CreatePeriodizationTemplateRequest request,
        Guid trainerId,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        // In a real implementation, this would save to database
        var template = new PeriodizationTemplateDto(
            Guid.NewGuid(),
            request.Name,
            request.Description,
            request.Model,
            request.RecommendedWeeks,
            request.Phases,
            DateTime.UtcNow
        );

        _logger.LogInformation("Created periodization template: {TemplateName} by trainer {TrainerId}",
            request.Name, trainerId);

        return template;
    }
}
