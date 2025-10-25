# Admin-Only User Registration

This document explains the admin-controlled user registration system implemented in GymHero.

## Overview

Public user registration has been **disabled** to give administrators complete control over who can access the application. Only administrators can create new user accounts through the admin panel.

## What Changed

### 1. Public Signup Page Disabled ✅

**File**: `frontend/apps/web/src/app/(auth)/signup/page.tsx`

- The signup form has been replaced with an informational page
- Users are informed that only admins can create accounts
- Provides a link back to the login page

### 2. Login Page Updated ✅

**File**: `frontend/apps/web/src/app/(auth)/login/page.tsx`

- Removed "Create account" link
- Changed message to: "Não tem acesso? Entre em contato com um administrador."

### 3. Admin Panel Enhanced ✅

**File**: `frontend/apps/web/src/app/(app)/admin/page.tsx`

The admin panel now has **full user management capabilities**:

#### ✅ **Create Users**
- New "Criar Usuário" button in the admin panel
- Create dialog with fields:
  - Name (required)
  - Email (required)
  - Temporary password (required)
  - Role: User, PersonalTrainer, or Admin (required)
- Validates email uniqueness
- All users start as active by default

#### ✅ **Edit Users**
- Change user role (User ↔ PersonalTrainer ↔ Admin)
- Update user information

#### ✅ **Activate/Deactivate Users**
- Toggle user active status
- Deactivated users cannot log in
- Useful for temporary access suspension

#### ✅ **Delete Users**
- Permanently remove user accounts
- Confirmation required before deletion

#### ✅ **Search & Filter**
- Search by name or email
- Real-time filtering

#### ✅ **User Statistics**
- Total users count
- Personal trainers count
- Active users count

### 4. Backend API Endpoint Added ✅

**File**: `src/GymHero.Api/Endpoints/AdminEndpoints.cs`

New endpoint: `POST /api/admin/users`

**Request Body**:
```json
{
  "name": "User Name",
  "email": "user@example.com",
  "password": "TemporaryPassword123!",
  "role": "User"
}
```

**Response** (201 Created):
```json
{
  "id": "guid",
  "name": "User Name",
  "email": "user@example.com",
  "role": "User",
  "isActive": true,
  "createdAt": "2025-10-18T..."
}
```

**Validations**:
- Email must be unique
- Role must be one of: User, PersonalTrainer, Admin
- Password is hashed using BCrypt
- Only admins can access this endpoint

### 5. DTO Added ✅

**File**: `src/GymHero.Shared/DTOs/AdminDtos.cs`

```csharp
public record CreateUserRequest(
    string Name,
    string Email,
    string Password,
    string Role
);
```

### 6. Security Improvements ✅

**All hardcoded URLs removed from admin page**:
- Uses `api` client from `@/lib/api` for all API calls
- Uses `getAssetUrl()` helper for profile pictures
- Respects environment variables for API endpoints

## How to Create Users as Admin

1. **Log in as Admin**
   - Use your admin credentials to log in

2. **Navigate to Admin Panel**
   - Click on "Admin" in the sidebar
   - Only visible to users with Admin role

3. **Create New User**
   - Click "Criar Usuário" button
   - Fill in the form:
     - **Name**: User's full name
     - **Email**: Must be unique
     - **Temporary Password**: User will need to change this
     - **Role**: Select User, PersonalTrainer, or Admin
   - Click "Criar Usuário"

4. **Provide Credentials to User**
   - Share the email and temporary password securely
   - Recommend user changes password immediately

## User Roles Explained

### User (Default)
- Can track workouts
- Can view progress
- Can participate in challenges
- Can connect with friends
- Can use AI workout generator

### PersonalTrainer
- All User permissions +
- Can view assigned clients
- Can create workout plans for clients
- Can track client progress
- Access to instructor panel

### Admin
- All permissions +
- User management (create, edit, delete, activate/deactivate)
- System configuration
- Badge management
- Exercise database management

## Best Practices

### When Creating Users

1. **Use Strong Temporary Passwords**
   - Minimum 8 characters
   - Include uppercase, lowercase, numbers, symbols
   - Example: `GymHero2025!`

2. **Assign Correct Roles**
   - Start with "User" role by default
   - Only elevate to PersonalTrainer or Admin when necessary
   - Follow principle of least privilege

3. **Verify Email Addresses**
   - Ensure email is correct before creating account
   - Cannot be changed easily later

4. **Document User Creation**
   - Keep a record of who created which accounts
   - Note the reason for account creation if relevant

### Security Considerations

1. **Admin Account Protection**
   - Keep admin credentials secure
   - Use strong passwords
   - Don't share admin access

2. **Regular Audits**
   - Review user list periodically
   - Deactivate unused accounts
   - Remove test accounts

3. **User Lifecycle**
   - Create → User logs in → User changes password
   - If user leaves: Deactivate (don't delete) to preserve data
   - Only delete if absolutely necessary

## Creating Your First Admin (Development)

If you need to create the first admin account in development:

```bash
# Option 1: Use the seed endpoint (development only)
POST http://localhost:5001/api/admin/dev/seed-admin

# Option 2: Create custom admin (development only)
POST http://localhost:5001/api/admin/dev/create-admin
{
  "name": "Admin User",
  "email": "admin@gymhero.com",
  "password": "YourSecurePassword123!"
}
```

**⚠️ IMPORTANT**: These dev endpoints are **automatically disabled in production** for security.

## Creating Your First Admin (Production)

For production, you have two options:

### Option 1: Database Migration (Recommended)

Create a migration to insert the first admin:

```csharp
// In a new migration file
migrationBuilder.Sql(@"
    INSERT INTO Users (Id, Name, Email, PasswordHash, Role, IsActive, CreatedAt)
    VALUES (
        gen_random_uuid(),
        'Admin',
        'admin@yourdomain.com',
        '$2a$11$your-bcrypt-hashed-password-here',
        'Admin',
        true,
        NOW()
    );
");
```

Generate BCrypt hash:
```bash
# Using online tool or:
dotnet run -- hash-password "YourSecurePassword"
```

### Option 2: Direct Database Insert

Connect to your production database and run:

```sql
INSERT INTO "Users" ("Id", "Name", "Email", "PasswordHash", "Role", "IsActive", "CreatedAt")
VALUES (
    gen_random_uuid(),
    'System Admin',
    'admin@yourdomain.com',
    '$2a$11$your-bcrypt-hashed-password-here',
    'Admin',
    true,
    CURRENT_TIMESTAMP
);
```

## Troubleshooting

### "User with this email already exists"
- Check if email is already registered
- Try a different email address
- Delete or deactivate the existing user first

### "Access Denied" when creating users
- Ensure you're logged in as Admin
- Check browser console for errors
- Verify JWT token is valid

### Cannot see admin panel
- Only users with role "Admin" can access `/admin`
- Check your user role in the database
- Log out and log back in if role was just changed

### Created user cannot login
- Verify user is marked as Active (isActive = true)
- Check password was entered correctly
- Ensure email matches exactly (no spaces)

## API Reference

### List Users
```
GET /api/admin/users
Authorization: Bearer {admin-token}
```

### Create User
```
POST /api/admin/users
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "name": "string",
  "email": "string",
  "password": "string",
  "role": "User" | "PersonalTrainer" | "Admin"
}
```

### Update User Role
```
PUT /api/admin/users/{userId}
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "role": "User" | "PersonalTrainer" | "Admin"
}
```

### Activate User
```
POST /api/admin/users/{userId}/activate
Authorization: Bearer {admin-token}
```

### Deactivate User
```
POST /api/admin/users/{userId}/deactivate
Authorization: Bearer {admin-token}
```

### Delete User
```
DELETE /api/admin/users/{userId}
Authorization: Bearer {admin-token}
```

---

## Summary

✅ Public signup is **disabled**
✅ Only **admins** can create users
✅ Admin panel has **full user management**
✅ Users can be created, edited, activated, deactivated, and deleted
✅ All API calls use **environment variables** (no hardcoded URLs)
✅ System is secure and **production-ready**

This approach gives you complete control over your user base and ensures only authorized people can access your GymHero application! 🎉
