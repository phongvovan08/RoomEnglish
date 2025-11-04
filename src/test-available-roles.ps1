#!/usr/bin/env pwsh

# Test Available Roles API
Write-Host "🧪 Testing Get Available Roles API" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$baseUrl = "https://localhost:5001"
$email = "devphongvv198@gmail.com"
$password = "Test@123"

try {
    # Step 1: Login
    Write-Host "1️⃣  Logging in..." -ForegroundColor Yellow
    $loginBody = @{
        email = $email
        password = $password
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod `
        -Uri "$baseUrl/login" `
        -Method Post `
        -Body $loginBody `
        -ContentType "application/json" `
        -SkipCertificateCheck

    $token = $loginResponse.accessToken
    Write-Host "✅ Login successful!" -ForegroundColor Green
    Write-Host ""

    # Step 2: Get available roles
    Write-Host "2️⃣  Getting available roles from API..." -ForegroundColor Yellow
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }

    $rolesResponse = Invoke-RestMethod `
        -Uri "$baseUrl/api/users/roles" `
        -Method Get `
        -Headers $headers `
        -SkipCertificateCheck

    Write-Host "✅ Roles retrieved successfully!" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "📋 Available Roles from Database:" -ForegroundColor Cyan
    Write-Host "===================================" -ForegroundColor Cyan
    
    if ($rolesResponse.roles) {
        $rolesResponse.roles | ForEach-Object {
            Write-Host "  ✓ $($_.name)" -ForegroundColor Green
            Write-Host "    Description: $($_.description)" -ForegroundColor Gray
            Write-Host ""
        }
        Write-Host "Total roles in database: $($rolesResponse.roles.Count)" -ForegroundColor Yellow
    } else {
        Write-Host "  ⚠️  No roles found" -ForegroundColor Yellow
    }
    
    Write-Host ""
    
    # Step 3: Verify expected roles
    Write-Host "3️⃣  Verifying expected roles..." -ForegroundColor Yellow
    $expectedRoles = @('Administrator', 'User')
    $missingRoles = @()
    
    foreach ($expectedRole in $expectedRoles) {
        $found = $rolesResponse.roles | Where-Object { $_.name -eq $expectedRole }
        if ($found) {
            Write-Host "  ✅ $expectedRole - Found" -ForegroundColor Green
            Write-Host "     Description: $($found.description)" -ForegroundColor Gray
        } else {
            Write-Host "  ❌ $expectedRole - Missing!" -ForegroundColor Red
            $missingRoles += $expectedRole
        }
    }
    
    Write-Host ""
    
    if ($missingRoles.Count -eq 0) {
        Write-Host "🎉 All expected roles are available!" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Warning: Some roles are missing:" -ForegroundColor Yellow
        $missingRoles | ForEach-Object {
            Write-Host "  - $_" -ForegroundColor Red
        }
        Write-Host ""
        Write-Host "💡 Run create-user-role.sql to create missing roles" -ForegroundColor Cyan
    }
    
    Write-Host ""
    Write-Host "📊 Role Descriptions (Frontend will display):" -ForegroundColor Cyan
    Write-Host "==============================================" -ForegroundColor Cyan
    
    $rolesResponse.roles | ForEach-Object {
        Write-Host "  • $($_.name) - $($_.description)" -ForegroundColor White
    }
    
    Write-Host ""
    Write-Host "✅ Test completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🔍 Data Flow:" -ForegroundColor Cyan
    Write-Host "1. AspNetRoles table in database" -ForegroundColor Gray
    Write-Host "2. RoleManager queries all roles" -ForegroundColor Gray
    Write-Host "3. IdentityService.GetAllRolesAsync() adds descriptions" -ForegroundColor Gray
    Write-Host "4. GetAvailableRolesQuery returns RoleDto[]" -ForegroundColor Gray
    Write-Host "5. API endpoint /api/users/roles" -ForegroundColor Gray
    Write-Host "6. Frontend receives { roles: RoleDto[] }" -ForegroundColor Gray
    Write-Host "7. UI displays role.name and role.description" -ForegroundColor Gray
    
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.ErrorDetails.Message) {
        Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
    
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "Status Code: $statusCode" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "🔍 How roles are loaded in UI:" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host "1. Component mounts → loadAvailableRoles() is called" -ForegroundColor Gray
Write-Host "2. API call to GET /api/users/roles" -ForegroundColor Gray
Write-Host "3. Backend queries AspNetRoles table via RoleManager" -ForegroundColor Gray
Write-Host "4. IdentityService maps each role to RoleDto with description" -ForegroundColor Gray
Write-Host "5. Response: { roles: [{ name, description }, ...] }" -ForegroundColor Gray
Write-Host "6. Frontend stores in availableRoles ref" -ForegroundColor Gray
Write-Host "7. Modal renders checkboxes from database roles" -ForegroundColor Gray
Write-Host "8. Each checkbox shows role.name and role.description" -ForegroundColor Gray
Write-Host ""
Write-Host "✨ Benefits:" -ForegroundColor Green
Write-Host "• Roles load directly from AspNetRoles table" -ForegroundColor White
Write-Host "• Add new role in database → Auto appears in UI" -ForegroundColor White
Write-Host "• No hardcoding needed" -ForegroundColor White
Write-Host "• Descriptions from backend, not frontend" -ForegroundColor White
Write-Host ""
