-- =============================================
-- Script: Create User Role
-- Description: Ensure "User" role exists in AspNetRoles
-- Date: 2025-11-05
-- =============================================

DECLARE @RoleId NVARCHAR(450);
DECLARE @RoleName NVARCHAR(256) = 'User';
DECLARE @NormalizedRoleName NVARCHAR(256) = 'USER';

-- Check if User role exists
SELECT @RoleId = Id FROM AspNetRoles WHERE NormalizedName = @NormalizedRoleName;

IF @RoleId IS NULL
BEGIN
    -- Create new role
    SET @RoleId = NEWID();
    
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (
        @RoleId, 
        @RoleName, 
        @NormalizedRoleName,
        NEWID()
    );
    
    PRINT '✅ User role created successfully!';
    PRINT 'Role ID: ' + @RoleId;
END
ELSE
BEGIN
    PRINT 'ℹ️  User role already exists.';
    PRINT 'Role ID: ' + @RoleId;
END

-- Verify both roles exist
PRINT '';
PRINT '📋 All Roles:';
SELECT 
    Id,
    Name,
    NormalizedName,
    ConcurrencyStamp
FROM AspNetRoles
ORDER BY Name;
