# Gym vs Home Workout Feature - Testing Guide

## Pre-Testing Setup

### 1. Apply Database Migration

```bash
cd src/GymHero.Infrastructure
dotnet ef database update --startup-project ../GymHero.Api
```

**Expected Result**: Migration `AddWorkoutLocationFields` is applied successfully.

**Verify**:
```sql
-- Check Users table has PreferredWorkoutLocation column
SELECT * FROM Users LIMIT 1;

-- Check Exercises table has WorkoutLocation column
SELECT * FROM Exercises LIMIT 1;
```

### 2. Seed Home Exercises

```bash
cd ../../
node seed-home-exercises.js
```

**Before running**: Update `API_BASE_URL` in script if needed (default: `https://localhost:7219/api`)

**Expected Result**:
```
✅ Successfully added: 50+ exercises
📝 Total: 50+ exercises
```

**Verify**:
```sql
-- Check home exercises exist
SELECT Name, WorkoutLocation FROM Exercises WHERE WorkoutLocation = 1 LIMIT 10;

-- Check gym exercises exist
SELECT Name, WorkoutLocation FROM Exercises WHERE WorkoutLocation = 0 LIMIT 10;

-- Check "Both" exercises exist
SELECT Name, WorkoutLocation FROM Exercises WHERE WorkoutLocation = 2 LIMIT 10;
```

---

## Test Cases

### Test 1: New User Registration (Gym Preference)

**Steps**:
1. Navigate to `/signup`
2. Fill in:
   - Name: "Test Gym User"
   - Email: "gym@test.com"
   - Password: "SecurePass123"
   - Confirm Password: "SecurePass123"
3. Click "Academia" (Gym) option - should highlight with primary color
4. Check terms checkbox
5. Click "Criar Conta"

**Expected Results**:
- ✅ Academia button is highlighted when selected
- ✅ Registration succeeds
- ✅ User is redirected to onboarding/dashboard
- ✅ Database: User's `PreferredWorkoutLocation` = 0

**Verify in Database**:
```sql
SELECT Name, Email, PreferredWorkoutLocation
FROM Users
WHERE Email = 'gym@test.com';
-- Should show PreferredWorkoutLocation = 0
```

---

### Test 2: New User Registration (Home Preference)

**Steps**:
1. Navigate to `/signup`
2. Fill in:
   - Name: "Test Home User"
   - Email: "home@test.com"
   - Password: "SecurePass123"
   - Confirm Password: "SecurePass123"
3. Click "Em Casa" (Home) option - should highlight with primary color
4. Check terms checkbox
5. Click "Criar Conta"

**Expected Results**:
- ✅ Em Casa button is highlighted when selected
- ✅ Registration succeeds
- ✅ User is redirected to onboarding/dashboard
- ✅ Database: User's `PreferredWorkoutLocation` = 1

**Verify in Database**:
```sql
SELECT Name, Email, PreferredWorkoutLocation
FROM Users
WHERE Email = 'home@test.com';
-- Should show PreferredWorkoutLocation = 1
```

---

### Test 3: User Data Endpoint Returns Preference

**Steps**:
1. Login as "gym@test.com"
2. Open browser DevTools → Network tab
3. Navigate to dashboard (or any authenticated page)
4. Find the request to `/api/auth/me`
5. Check response body

**Expected Results**:
```json
{
  "id": "...",
  "name": "Test Gym User",
  "email": "gym@test.com",
  "role": "Aluno",
  "preferredWorkoutLocation": 0,
  ...
}
```

**Repeat for Home User**:
- Login as "home@test.com"
- `/api/auth/me` should return `"preferredWorkoutLocation": 1`

---

### Test 4: Exercise API Filtering (Gym Exercises)

**Test via API Directly**:
```bash
# Get gym exercises
curl -k "https://localhost:7219/api/exercises?workoutLocation=0"
```

**Expected Results**:
- ✅ Returns exercises with `workoutLocation: 0` (Gym)
- ✅ Also includes exercises with `workoutLocation: 2` (Both)
- ✅ Does NOT include exercises with `workoutLocation: 1` (Home only)

**Example exercises you should see**:
- Barbell Bench Press
- Dumbbell Curls
- Leg Press
- Cable Rows
- Jumping Rope (Both)

---

### Test 5: Exercise API Filtering (Home Exercises)

**Test via API Directly**:
```bash
# Get home exercises
curl -k "https://localhost:7219/api/exercises?workoutLocation=1"
```

**Expected Results**:
- ✅ Returns exercises with `workoutLocation: 1` (Home)
- ✅ Also includes exercises with `workoutLocation: 2` (Both)
- ✅ Does NOT include exercises with `workoutLocation: 0` (Gym only)

**Example exercises you should see**:
- Push-ups
- Pull-ups (Doorway Bar)
- Bodyweight Squats
- Planks
- Jumping Rope (Both)

---

### Test 6: Plan Creation - Gym User Experience

**Prerequisites**: Logged in as "gym@test.com"

**Steps**:
1. Navigate to `/plans/new`
2. Check the "Tipo de Treino" section

**Expected Results**:
- ✅ "Academia" button is pre-selected (highlighted)
- ✅ Text below reads: "Mostrando exercícios de academia"
- ✅ Can click other buttons to switch filters

**Steps** (continued):
3. Enter search query: "bench"
4. Click "Buscar"

**Expected Results**:
- ✅ Search returns gym exercises like "Barbell Bench Press"
- ✅ Does NOT return home-only exercises

**Steps** (switch to Home):
5. Click "Em Casa" button
6. Search for "push"
7. Click "Buscar"

**Expected Results**:
- ✅ "Em Casa" button now highlighted
- ✅ Text reads: "Mostrando exercícios para fazer em casa"
- ✅ Search returns "Push-ups" and variations
- ✅ Does NOT return gym-specific exercises

---

### Test 7: Plan Creation - Home User Experience

**Prerequisites**: Logged in as "home@test.com"

**Steps**:
1. Navigate to `/plans/new`
2. Check the "Tipo de Treino" section

**Expected Results**:
- ✅ "Em Casa" button is pre-selected (highlighted)
- ✅ Text below reads: "Mostrando exercícios para fazer em casa"
- ✅ Can click other buttons to switch filters

**Steps** (continued):
3. Enter search query: "squat"
4. Click "Buscar"

**Expected Results**:
- ✅ Search returns "Bodyweight Squats", "Bulgarian Split Squats"
- ✅ Does NOT return "Barbell Squats" or other gym equipment exercises

---

### Test 8: Plan Creation - "Todos" (All) Filter

**Prerequisites**: Logged in as any user

**Steps**:
1. Navigate to `/plans/new`
2. Click "Todos" button
3. Verify text reads: "Mostrando todos os exercícios"
4. Search for "bench"
5. Click "Buscar"

**Expected Results**:
- ✅ Returns ALL exercises matching "bench"
- ✅ Includes both gym and home exercises
- ✅ Shows exercises from all workout locations

---

### Test 9: Mobile Responsiveness

**Steps**:
1. Open DevTools → Toggle device toolbar (Ctrl+Shift+M)
2. Select "iPhone 12 Pro" or similar
3. Navigate to `/signup`

**Expected Results - Registration**:
- ✅ Gym/Home selection buttons stack properly
- ✅ Icons and text are visible
- ✅ Buttons are easily tappable
- ✅ Form fields don't overflow

**Steps** (continued):
4. Login and go to `/plans/new`

**Expected Results - Plan Creation**:
- ✅ Workout location filter buttons (Academia/Em Casa/Todos) fit in grid
- ✅ Buttons are tappable on mobile
- ✅ Search filters wrap properly on narrow screens
- ✅ No horizontal scrolling

---

### Test 10: Complete User Flow

**Full Journey Test**:

1. **Register** with Home preference
2. **Login** successfully
3. **Verify** `/api/auth/me` returns `preferredWorkoutLocation: 1`
4. **Navigate** to `/plans/new`
5. **Verify** "Em Casa" is pre-selected
6. **Create** a workout plan:
   - Name: "Home Workout Plan"
   - Add "Push-ups" to Day 1
   - Add "Bodyweight Squats" to Day 1
   - Add "Planks" to Day 1
7. **Save** the plan
8. **Verify** plan is created successfully

**Expected Results**:
- ✅ Entire flow works seamlessly
- ✅ User only sees home-appropriate exercises
- ✅ Plan is saved with home exercises
- ✅ No errors or bugs encountered

---

## Database Verification Queries

### Check User Preferences Distribution
```sql
SELECT
  PreferredWorkoutLocation,
  COUNT(*) as UserCount,
  CASE
    WHEN PreferredWorkoutLocation = 0 THEN 'Gym'
    WHEN PreferredWorkoutLocation = 1 THEN 'Home'
    WHEN PreferredWorkoutLocation = 2 THEN 'Both'
  END as LocationType
FROM Users
GROUP BY PreferredWorkoutLocation;
```

### Check Exercise Distribution
```sql
SELECT
  WorkoutLocation,
  COUNT(*) as ExerciseCount,
  CASE
    WHEN WorkoutLocation = 0 THEN 'Gym'
    WHEN WorkoutLocation = 1 THEN 'Home'
    WHEN WorkoutLocation = 2 THEN 'Both'
  END as LocationType
FROM Exercises
GROUP BY WorkoutLocation;
```

### Find Exercises by Location
```sql
-- Gym exercises
SELECT Name, MuscleGroup, Equipment
FROM Exercises
WHERE WorkoutLocation = 0
ORDER BY MuscleGroup;

-- Home exercises
SELECT Name, MuscleGroup, Equipment
FROM Exercises
WHERE WorkoutLocation = 1
ORDER BY MuscleGroup;

-- Both/Universal exercises
SELECT Name, MuscleGroup, Equipment
FROM Exercises
WHERE WorkoutLocation = 2
ORDER BY MuscleGroup;
```

---

## Troubleshooting

### Issue: Migration fails to apply

**Solution**:
```bash
# Remove the migration
dotnet ef migrations remove --startup-project ../GymHero.Api

# Re-create it
dotnet ef migrations add AddWorkoutLocationFields --startup-project ../GymHero.Api

# Apply again
dotnet ef database update --startup-project ../GymHero.Api
```

### Issue: Seed script fails with connection error

**Check**:
1. Backend is running: `dotnet run --project src/GymHero.Api`
2. API URL is correct in seed script
3. Try with authorization header if API requires auth

### Issue: User preference not showing in plan creation

**Debug**:
1. Check `/api/auth/me` response includes `preferredWorkoutLocation`
2. Check browser console for errors
3. Verify `useAuth()` hook is importing user data correctly
4. Add `console.log(user)` in component to debug

### Issue: Exercise filtering not working

**Debug**:
1. Check Network tab - is `workoutLocation` parameter being sent?
2. Verify exercises in database have correct `WorkoutLocation` values
3. Test API directly with curl to isolate frontend vs backend issue

---

## Success Criteria

✅ **All test cases pass**
✅ **Database migration applied successfully**
✅ **50+ home exercises seeded**
✅ **Registration flow works for both Gym and Home users**
✅ **API filtering returns correct exercises**
✅ **Plan creation page pre-selects user preference**
✅ **Users can switch between workout locations**
✅ **Mobile experience is smooth and responsive**
✅ **No console errors**
✅ **Build completes without errors**

---

## Performance Notes

- Exercise filtering is done at database level (indexed queries)
- No additional API calls required
- Filter state is managed locally (instant UI feedback)
- Works offline once user data is cached

---

## Known Limitations

1. **AI Search Endpoint**: If using `/ai/search-exercises`, verify it supports `workoutLocation` parameter
2. **Existing Users**: Users created before this feature will have `PreferredWorkoutLocation = 0` (Gym) by default
3. **Mobile App**: This feature is currently only implemented for web, not mobile app

---

## Next Steps After Testing

Once all tests pass:
1. ✅ Commit changes to version control
2. ✅ Deploy to staging environment
3. ✅ Perform user acceptance testing
4. ✅ Deploy to production
5. ✅ Monitor for issues in production
6. 📝 Consider adding:
   - Settings page to change workout preference
   - Analytics on gym vs home user preferences
   - Mobile app implementation
