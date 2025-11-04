-- Script: Assign Administrator role to user devphongvv198@gmail.com
-- Date: 2025-11-04
-- Description: This script assigns the Administrator role to the specified user account

-- Step 1: Check if the user exists
DECLARE @UserId NVARCHAR(450);
DECLARE @RoleId NVARCHAR(450);
DECLARE @Email NVARCHAR(256) = 'devphongvv198@gmail.com';
DECLARE @RoleName NVARCHAR(256) = 'Administrator';

-- Get User ID
SELECT @UserId = Id 
FROM AspNetUsers 
WHERE Email = @Email OR NormalizedEmail = UPPER(@Email);

IF @UserId IS NULL
BEGIN
    PRINT '❌ User not found: ' + @Email;
    PRINT 'Please make sure the user account exists first.';
END
ELSE
BEGIN
    PRINT '✅ User found: ' + @Email;
    PRINT 'User ID: ' + @UserId;
    
    -- Step 2: Check if Administrator role exists, if not create it
    SELECT @RoleId = Id 
    FROM AspNetRoles 
    WHERE Name = @RoleName OR NormalizedName = UPPER(@RoleName);
    
    IF @RoleId IS NULL
    BEGIN
        PRINT '⚠️ Administrator role does not exist. Creating...';
        
        SET @RoleId = NEWID();
        
        INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
        VALUES (@RoleId, @RoleName, UPPER(@RoleName), NEWID());
        
        PRINT '✅ Administrator role created successfully';
        PRINT 'Role ID: ' + @RoleId;
    END
    ELSE
    BEGIN
        PRINT '✅ Administrator role found';
        PRINT 'Role ID: ' + @RoleId;
    END
    
    -- Step 3: Check if user already has the role
    IF EXISTS (
        SELECT 1 
        FROM AspNetUserRoles 
        WHERE UserId = @UserId AND RoleId = @RoleId
    )
    BEGIN
        PRINT '⚠️ User already has Administrator role';
    END
    ELSE
    BEGIN
        -- Assign role to user
        INSERT INTO AspNetUserRoles (UserId, RoleId)
        VALUES (@UserId, @RoleId);
        
        PRINT '✅ Administrator role assigned successfully!';
        PRINT '';
        PRINT '================================================';
        PRINT 'User: ' + @Email;
        PRINT 'Role: Administrator';
        PRINT 'Status: ✅ Active';
        PRINT '================================================';
    END
END

-- Step 4: Verify the assignment
PRINT '';
PRINT '--- Current Roles for ' + @Email + ' ---';
SELECT 
    u.Email,
    u.UserName,
    r.Name AS RoleName,
    ur.RoleId,
    ur.UserId
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = @Email OR u.NormalizedEmail = UPPER(@Email);

PRINT '';
PRINT '--- All Users with Administrator Role ---';
SELECT 
    u.Email,
    u.UserName,
    u.EmailConfirmed,
    r.Name AS RoleName
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'Administrator';
