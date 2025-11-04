# Fix: Role USER does not exist

## 🐛 Problem
When updating user roles through the UI, got error:
```
Failed to update user roles: 500
Error details: "Role USER does not exist."
```

## 🔍 Root Cause
1. Frontend sent role name: `"User"`
2. ASP.NET Identity normalizes role names to UPPERCASE for comparison
3. Backend looked for role with `NormalizedName = "USER"`
4. Only `Administrator` role existed in database
5. Result: 500 Internal Server Error

## ✅ Solution Applied

### 1. Domain Layer - Added User Role Constant
**File**: `Domain/Constants/Roles.cs`
```csharp
public abstract class Roles
{
    public const string Administrator = nameof(Administrator);
    public const string User = nameof(User);  // ✅ Added
}
```

### 2. Application Layer - Updated Query
**File**: `Application/Users/Queries/GetAvailableRoles/GetAvailableRolesQuery.cs`
```csharp
var roles = new List<string>
{
    Domain.Constants.Roles.Administrator,
    Domain.Constants.Roles.User  // ✅ Changed from hardcoded "User"
};
```

### 3. Infrastructure Layer - Auto-Seed User Role
**File**: `Infrastructure/Data/ApplicationDbContextInitialiser.cs`
```csharp
public async Task TrySeedAsync()
{
    // Default roles
    var administratorRole = new IdentityRole(Roles.Administrator);
    var userRole = new IdentityRole(Roles.User);  // ✅ Added

    if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
    {
        await _roleManager.CreateAsync(administratorRole);
        _logger.LogInformation("Created Administrator role");
    }

    if (_roleManager.Roles.All(r => r.Name != userRole.Name))  // ✅ Added
    {
        await _roleManager.CreateAsync(userRole);
        _logger.LogInformation("Created User role");
    }
    // ...
}
```

### 4. Database - Manual Creation Script
**File**: `create-user-role.sql`
- Checks if User role exists
- Creates it if missing with proper NormalizedName
- Verifies all roles

## 🚀 How to Apply Fix

### Option 1: Automatic (Recommended)
1. Rebuild backend project
2. Run the application
3. `ApplicationDbContextInitialiser` will auto-create User role
4. Test user role assignment

### Option 2: Manual Database Update
1. Open `create-user-role.sql` in SSMS
2. Execute against your database
3. Verify roles with:
```sql
SELECT * FROM AspNetRoles;
```

## 📋 Verification

After fix, you should see both roles in database:
```
| Name          | NormalizedName |
|---------------|----------------|
| Administrator | ADMINISTRATOR  |
| User          | USER           |
```

## 🧪 Testing

Run the test script:
```powershell
.\fix-user-role.ps1
```

Then test in UI:
1. Go to `/management/users`
2. Click "Phân quyền" on any user
3. Select "User" role
4. Click "Lưu thay đổi"
5. Should succeed ✅

## 📝 Notes

- ASP.NET Identity always uses `NormalizedName` (UPPERCASE) for role lookups
- Both `Name` and `NormalizedName` are stored in database
- `Name` = "User", `NormalizedName` = "USER"
- Frontend can send either case, backend normalizes automatically
- Seeding happens on app startup, so rebuild is required for auto-creation

## 🎯 Impact

- ✅ Users can now be assigned to "User" role
- ✅ Users can be assigned to "Administrator" role
- ✅ Roles are created automatically on app startup
- ✅ No more 500 errors when updating roles
- ✅ Better error handling with detailed logs
