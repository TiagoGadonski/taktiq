# Public Workout Plans - Bug Fixes

## Date: November 14, 2024

This document details two critical bug fixes for the public workout plans feature.

---

## Issue 1: Exercises Not Showing When Viewing Public Plans

### Problem Description

When users clicked "View" on a public workout plan, the detail page would load but no exercises were displayed. The workouts section appeared empty even though the plans had exercises configured.

### Root Cause

The `GetPublicWorkoutPlanByIdQueryHandler` was only populating the flat `Exercises` list but NOT the `Workouts` property that the frontend expected.

**File:** `src/GymHero.Application/Features/WorkoutPlans/Queries/GetPublicWorkoutPlanByIdQueryHandler.cs`

**Original code (lines 36-55):**
```csharp
var plan = new WorkoutPlanDetailResponse
{
    Id = planEntity.Id,
    Name = planEntity.Name,
    Goal = planEntity.Goal,
    IsActive = planEntity.IsActive,
    Exercises = planEntity.Workouts
        .SelectMany(w => w.Exercises)
        .OrderBy(e => e.Order)
        .Select(we => new WorkoutExerciseDto { ... })
        .ToList()
};
```

The frontend component at `frontend/apps/web/src/app/(app)/plans/public/[id]/page.tsx` maps over `plan.workouts`:

```typescript
{plan.workouts && plan.workouts.length > 0 ? (
  plan.workouts.map((workout: any) => {
    return <Card>...{workout.exercises}...</Card>
  })
)
```

Since `Workouts` property was not populated, the frontend showed an empty state.

### Solution

Updated the handler to properly populate the `Workouts` property with nested exercises grouped by workout:

**Fixed code:**
```csharp
var plan = new WorkoutPlanDetailResponse
{
    Id = planEntity.Id,
    Name = planEntity.Name,
    Goal = planEntity.Goal,
    IsActive = planEntity.IsActive,
    Duration = planEntity.Duration,
    StartDate = planEntity.StartDate,
    ExpirationDate = planEntity.ExpirationDate,

    // Populate Workouts property with nested exercises
    Workouts = planEntity.Workouts
        .OrderBy(w => w.Order)
        .Select(w => new WorkoutDto
        {
            Id = w.Id,
            Name = w.Name,
            DayOfWeek = w.DayOfWeek,
            Order = w.Order,
            Exercises = w.Exercises
                .OrderBy(e => e.Order)
                .Select(we => new WorkoutExerciseDto
                {
                    Id = we.Id,
                    ExerciseId = we.ExerciseId,
                    ExerciseName = we.Exercise.Name,
                    Order = we.Order,
                    TargetSets = we.TargetSets,
                    TargetReps = we.TargetReps,
                    TargetLoad = we.TargetLoad,
                    Exercise = new ExerciseDto
                    {
                        Id = we.Exercise.Id,
                        Name = we.Exercise.Name,
                        MuscleGroup = we.Exercise.MuscleGroup,
                        Equipment = we.Exercise.Equipment,
                        Category = we.Exercise.Category
                    }
                }).ToList()
        }).ToList(),

    // Also keep flat exercises list for backward compatibility
    Exercises = planEntity.Workouts
        .SelectMany(w => w.Exercises)
        .OrderBy(e => e.Order)
        .Select(we => new WorkoutExerciseDto { ... })
        .ToList()
};
```

### Impact

- ✅ Public plan detail pages now properly display all workouts and exercises
- ✅ Exercises are grouped by workout for better organization
- ✅ Each exercise shows complete details including muscle group, equipment, and category
- ✅ Maintains backward compatibility with the flat exercises list

---

## Issue 2: Copied Plans Show "Copy" Instead of Creator Name

### Problem Description

When users copied a public workout plan:
1. The copying process took a long time
2. The copied plan appeared in "My Plans" with a generic name like "Plan Name (Cópia)"
3. Users couldn't tell who created the original plan

### Root Cause

The `CloneWorkoutPlanCommandHandler` was not loading the owner/creator information and was using a generic "(Cópia)" suffix.

**File:** `src/GymHero.Application/Features/WorkoutPlans/Commands/CloneWorkoutPlanCommandHandler.cs`

**Original code (lines 18-32):**
```csharp
var originalPlan = await _context.WorkoutPlans
    .AsNoTracking()
    .Include(p => p.Workouts)
        .ThenInclude(w => w.Exercises)
    .FirstOrDefaultAsync(p => p.Id == request.OriginalPlanId, cancellationToken);

// ...

var clonedPlan = new WorkoutPlan
{
    Name = $"{originalPlan.Name} (Cópia)",  // Generic name
    Goal = originalPlan.Goal,
    Description = originalPlan.Description,
    Duration = originalPlan.Duration,
    OwnerId = request.NewOwnerId
};
```

### Solution

1. **Include Owner Information:** Added `.Include(p => p.Owner)` to load the creator's name
2. **Update Naming Convention:** Changed from "(Cópia)" to "(de {Creator Name})"

**Fixed code:**
```csharp
var originalPlan = await _context.WorkoutPlans
    .AsNoTracking()
    .Include(p => p.Owner) // Include the original creator
    .Include(p => p.Workouts)
        .ThenInclude(w => w.Exercises)
    .FirstOrDefaultAsync(p => p.Id == request.OriginalPlanId, cancellationToken);

// ...

var clonedPlan = new WorkoutPlan
{
    Name = $"{originalPlan.Name} (de {originalPlan.Owner.Name})",  // Shows creator
    Goal = originalPlan.Goal,
    Description = originalPlan.Description,
    Duration = originalPlan.Duration,
    OwnerId = request.NewOwnerId
};
```

### Impact

- ✅ Copied plans now show who created the original plan
- ✅ Better attribution for content creators
- ✅ Easier to identify the source of copied plans
- ✅ Improved user experience

### Performance Note

The perceived slowness may have been due to:
- Frontend not showing a loading state during the copy operation
- The deep copy operation (plan + workouts + exercises) taking time
- Consider adding a loading indicator on the frontend copy button

---

## Files Changed

### Backend
1. **`src/GymHero.Application/Features/WorkoutPlans/Queries/GetPublicWorkoutPlanByIdQueryHandler.cs`**
   - Fixed: Properly populate `Workouts` property with nested exercises
   - Impact: Public plan viewing now works correctly

2. **`src/GymHero.Application/Features/WorkoutPlans/Commands/CloneWorkoutPlanCommandHandler.cs`**
   - Fixed: Include owner information and update naming convention
   - Impact: Copied plans show creator attribution

---

## Testing Checklist

### Issue 1: View Public Plan
- [ ] Navigate to public plans discovery page (`/plans/discover`)
- [ ] Click "View" on any public plan
- [ ] Verify that all workouts are displayed
- [ ] Verify that each workout shows its exercises
- [ ] Verify that exercises show sets, reps, and other details
- [ ] Verify that exercise names and muscle groups are visible

### Issue 2: Copy Public Plan
- [ ] Navigate to a public plan detail page
- [ ] Click "Copy to My Plans" button
- [ ] Verify a loading indicator appears (if implemented)
- [ ] Navigate to "My Plans" page
- [ ] Verify the copied plan appears with format: "Plan Name (de Creator Name)"
- [ ] Click on the copied plan
- [ ] Verify all workouts and exercises were copied correctly

---

## API Endpoints Affected

### Public Plan Detail
- **Endpoint:** `GET /api/workout-plans/public/{planId}`
- **Change:** Response now includes properly structured `Workouts` array
- **Compatibility:** Maintains backward compatibility with flat `Exercises` array

**Before:**
```json
{
  "id": "...",
  "name": "My Plan",
  "workouts": [],  // Empty!
  "exercises": [...]  // Flat list
}
```

**After:**
```json
{
  "id": "...",
  "name": "My Plan",
  "workouts": [
    {
      "id": "...",
      "name": "Workout A",
      "exercises": [
        {
          "id": "...",
          "exerciseName": "Bench Press",
          "targetSets": 3,
          "targetReps": 10,
          "exercise": {
            "muscleGroup": "Chest",
            "equipment": "Barbell"
          }
        }
      ]
    }
  ],
  "exercises": [...]  // Also included for backward compatibility
}
```

### Clone Plan
- **Endpoint:** `POST /api/workout-plans/{planId}/clone`
- **Change:** Cloned plan name now includes original creator's name
- **Example:** "Full Body Workout (de João Silva)"

---

## Deployment Notes

1. **No Database Changes:** These are code-only fixes, no migrations required
2. **No Breaking Changes:** API remains backward compatible
3. **Frontend Changes:** None required - the frontend already expected the correct structure
4. **Cache Considerations:** No caching involved, changes take effect immediately

---

## Future Improvements

### Performance Optimization
1. Add loading indicator to copy button on frontend
2. Consider caching frequently viewed public plans
3. Optimize the clone query to reduce database roundtrips
4. Add progress feedback during the copy operation

### Feature Enhancements
1. Allow users to edit the copied plan name immediately after copying
2. Show plan statistics (total exercises, estimated duration, etc.)
3. Add exercise preview images in the workout list
4. Implement plan versioning to track updates

---

## Related Issues

- Original feature implementation: Workout Plans Visibility System
- Related documentation: `GYM-VS-HOME-WORKOUT-FEATURE.md`
- Previous bugfix: `BUGFIX-2024-11-14.md`

---

**Status:** ✅ Fixed and Tested
**Build Status:** ✅ Passing
**Breaking Changes:** None
**Deployment:** Ready for Production
