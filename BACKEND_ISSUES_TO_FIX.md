# Critical Backend Issues That Need Immediate Attention

## Issue 1: Personal Trainer Profile Data Not Persisting/Returning ❌

### **Problem:**
When a personal trainer fills in their public profile information and saves it:
1. The frontend sends data to `PUT /api/personal/profile`
2. The data appears to save (no error)
3. When refreshing or navigating away and back, the data is GONE
4. The profile never becomes public
5. Posts never become clickable for students

### **Root Cause:**
One of three possible backend issues:

**Option A:** The `/api/personal/profile` PUT endpoint is NOT saving the data to the database
**Option B:** The `/api/me` GET endpoint is NOT returning the profile fields
**Option C:** The User entity mapping is missing these fields in the response DTO

### **Frontend is Sending (Correctly):**
```json
{
  "ProfileSlug": "tiago-cordeiro",
  "Specialization": "Musculação e Hipertrofia",
  "Education": "CREF 12345, Bacharel em Ed. Física",
  "Experience": "10 anos de experiência",
  "PricingInfo": "R$ 150/mês",
  "IsPublicProfile": true,
  "InstagramUrl": "https://instagram.com/trainer",
  "FacebookUrl": "https://facebook.com/trainer",
  "WebsiteUrl": "https://mysite.com"
}
```

### **Backend Must Fix:**

1. **Ensure `/api/personal/profile` PUT endpoint SAVES to database:**
   - Update the User entity with all these fields
   - Persist to database
   - Return success response

2. **Ensure `/api/me` GET endpoint RETURNS these fields:**
   ```csharp
   public class UserDto
   {
       public string Id { get; set; }
       public string Email { get; set; }
       public string Name { get; set; }
       // ADD THESE FIELDS:
       public string? ProfileSlug { get; set; }
       public string? Specialization { get; set; }
       public string? Education { get; set; }
       public string? Experience { get; set; }
       public string? PricingInfo { get; set; }
       public bool IsPublicProfile { get; set; }
       public string? InstagramUrl { get; set; }
       public string? FacebookUrl { get; set; }
       public string? WebsiteUrl { get; set; }
   }
   ```

---

## Issue 2: Posts Missing `authorProfileSlug` Field ❌

### **Problem:**
Posts returned by the API don't include the `authorProfileSlug` field, so students can't click on trainer names/avatars to view their profiles.

### **Current Response:**
```json
{
  "id": "abc123",
  "title": "Dica de Treino",
  "content": "...",
  "authorId": "trainer-id",
  "authorName": "Tiago Cordeiro",
  "authorProfilePictureUrl": "...",
  // MISSING: authorProfileSlug
}
```

### **Required Response:**
```json
{
  "id": "abc123",
  "title": "Dica de Treino",
  "content": "...",
  "authorId": "trainer-id",
  "authorName": "Tiago Cordeiro",
  "authorProfilePictureUrl": "...",
  "authorProfileSlug": "tiago-cordeiro"  // ← ADD THIS
}
```

### **Backend Must Fix:**
Update ALL post endpoints to include `authorProfileSlug` in the response:
- `GET /api/posts` (all posts)
- `GET /api/posts/trainer/{trainerId}` (trainer's posts)
- `GET /api/personal/posts` (instructor's own posts)

Include a JOIN to the User table to get the ProfileSlug.

---

## Issue 3: `IsExpired` Computed Property Breaking Analytics 🔥

### **Error from Logs:**
```
Error in GetPTAnalytics: The LINQ expression 'DbSet<StudentInvitation>()
.Count(s => s.TrainerId == __trainerId_0 && s.Status == "Pending" && !(s.IsExpired))' could not be translated.
Translation of member 'IsExpired' on entity type 'StudentInvitation' failed.
```

### **Location:**
`PersonalEndpoints.cs` line 326

### **Problem:**
`IsExpired` is likely a computed property in the C# entity that can't be translated to SQL.

### **Fix:**
Replace the computed property query with a direct date comparison:

**BEFORE (Broken):**
```csharp
var pendingInvitations = await context.StudentInvitations
    .CountAsync(s => s.TrainerId == trainerId &&
                     s.Status == "Pending" &&
                     !s.IsExpired);  // ← Can't translate!
```

**AFTER (Fixed):**
```csharp
var pendingInvitations = await context.StudentInvitations
    .CountAsync(s => s.TrainerId == trainerId &&
                     s.Status == "Pending" &&
                     s.ExpiresAt > DateTime.UtcNow);  // ← Direct comparison
```

---

## Issue 4: Public Trainer Profile Endpoint Missing ❌

### **Problem:**
The frontend expects `GET /api/trainer/{slug}` to return a trainer's public profile, but this endpoint returns 404.

### **Required Endpoint:**
```http
GET /api/trainer/{slug}
```

### **Expected Response:**
```json
{
  "id": "trainer-id",
  "name": "Tiago Cordeiro",
  "profilePictureUrl": "...",
  "bio": "Personal trainer especializado...",
  "location": "São Paulo, SP",
  "specialization": "Musculação e Hipertrofia",
  "education": "CREF 12345, Bacharel em Ed. Física",
  "experience": "10 anos de experiência com atletas...",
  "pricingInfo": "Plano mensal: R$ 150",
  "instagramUrl": "https://instagram.com/trainer",
  "facebookUrl": "https://facebook.com/trainer",
  "websiteUrl": "https://mysite.com",
  "studentCount": 25
}
```

### **Backend Must Implement:**
1. Create `GET /api/trainer/{slug}` endpoint
2. Query User where `ProfileSlug == slug` AND `IsPublicProfile == true`
3. Return 404 if not found or profile not public
4. Return public profile data (no sensitive info like email)

---

## Issue 5: Trainer Search Possibly Broken ❌

### **Problem:**
The user reported "The screen for searching for a personal trainer is also broken."

### **Suspected Issues:**
1. `GET /api/trainer` endpoint may not exist or is returning wrong data structure
2. May not be filtering by public profiles
3. May not be returning all required fields

### **Required Endpoint:**
```http
GET /api/trainer?search={term}&specialization={spec}&location={loc}
```

### **Expected Response:**
```json
[
  {
    "id": "trainer-id",
    "name": "Tiago Cordeiro",
    "profileSlug": "tiago-cordeiro",
    "profilePictureUrl": "...",
    "bio": "...",
    "location": "São Paulo, SP",
    "specialization": "Musculação e Hipertrofia",
    "studentCount": 25
  }
]
```

### **Backend Must:**
1. Only return trainers where `IsPublicProfile == true`
2. Include `profileSlug` in response
3. Support search filters
4. Return student count

---

## Testing the Fixes

### **Test 1: Profile Saving**
1. Login as personal trainer
2. Go to Instructor → Public Profile
3. Fill in all fields
4. Save
5. **Refresh page** - data should still be there
6. **Logout and login again** - data should still be there

### **Test 2: Profile Public Access**
1. Set profile to public and save
2. Access `/trainer/{your-slug}` (logged out)
3. Should see public profile page with all info

### **Test 3: Clickable Posts**
1. Create a public post as trainer
2. Login as student
3. View dashboard
4. Click on trainer name/avatar in post
5. Should navigate to trainer's public profile

### **Test 4: Trainer Search**
1. Logout (or login as student)
2. Go to "Encontrar Personal Trainer"
3. Should see list of public trainers
4. Search should work
5. Clicking on trainer should go to public profile

---

## Priority

🔥 **CRITICAL - Fix Immediately:**
1. Issue #1 - Profile data not persisting
2. Issue #3 - IsExpired breaking analytics
3. Issue #4 - Public trainer profile endpoint

⚠️ **HIGH - Fix Soon:**
4. Issue #2 - Posts missing authorProfileSlug
5. Issue #5 - Trainer search

---

## Frontend Diagnostic Tool Added

The frontend now includes console logging when saving profiles. Open browser DevTools Console to see:
- What data is being sent to the backend
- What response is received from the backend
- Whether `/me` endpoint returns profile fields

This will help diagnose which of the three possible issues is occurring.
