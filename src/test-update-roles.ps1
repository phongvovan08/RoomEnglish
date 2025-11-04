#!/usr/bin/env pwsh

# Test Update User Roles API
Write-Host "🧪 Testing Update User Roles API" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
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
    Write-Host "Token: $($token.Substring(0, 20))..." -ForegroundColor Gray
    Write-Host ""

    # Step 2: Get users list
    Write-Host "2️⃣  Getting users list..." -ForegroundColor Yellow
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }

    $usersResponse = Invoke-RestMethod `
        -Uri "$baseUrl/api/users/list?pageNumber=1&pageSize=10" `
        -Method Get `
        -Headers $headers `
        -SkipCertificateCheck

    Write-Host "✅ Found $($usersResponse.totalCount) users" -ForegroundColor Green
    
    # Display users
    $usersResponse.users | ForEach-Object {
        Write-Host "  - $($_.email) (ID: $($_.id.Substring(0, 8))...)" -ForegroundColor Gray
        Write-Host "    Roles: $($_.roles -join ', ')" -ForegroundColor Cyan
    }
    Write-Host ""

    # Step 3: Select first user to test
    $testUser = $usersResponse.users[0]
    Write-Host "3️⃣  Testing role update for: $($testUser.email)" -ForegroundColor Yellow
    Write-Host "Current roles: $($testUser.roles -join ', ')" -ForegroundColor Gray
    Write-Host ""

    # Step 4: Update roles - Add Administrator
    Write-Host "4️⃣  Updating roles to [Administrator]..." -ForegroundColor Yellow
    $updateBody = @{
        userId = $testUser.id
        roles = @("Administrator")
    } | ConvertTo-Json

    Write-Host "Request body:" -ForegroundColor Gray
    Write-Host $updateBody -ForegroundColor DarkGray
    Write-Host ""

    $updateResponse = Invoke-RestMethod `
        -Uri "$baseUrl/api/users/$($testUser.id)/roles" `
        -Method Put `
        -Body $updateBody `
        -Headers $headers `
        -SkipCertificateCheck

    Write-Host "✅ Update successful!" -ForegroundColor Green
    Write-Host "Response: $($updateResponse | ConvertTo-Json)" -ForegroundColor Gray
    Write-Host ""

    # Step 5: Verify update
    Write-Host "5️⃣  Verifying update..." -ForegroundColor Yellow
    $verifyResponse = Invoke-RestMethod `
        -Uri "$baseUrl/api/users/list?pageNumber=1&pageSize=10" `
        -Method Get `
        -Headers $headers `
        -SkipCertificateCheck

    $updatedUser = $verifyResponse.users | Where-Object { $_.id -eq $testUser.id }
    Write-Host "Updated roles: $($updatedUser.roles -join ', ')" -ForegroundColor Cyan
    Write-Host ""

    # Step 6: Test removing all roles
    Write-Host "6️⃣  Testing role removal (empty array)..." -ForegroundColor Yellow
    $removeBody = @{
        userId = $testUser.id
        roles = @()
    } | ConvertTo-Json

    Write-Host "Request body:" -ForegroundColor Gray
    Write-Host $removeBody -ForegroundColor DarkGray
    Write-Host ""

    $removeResponse = Invoke-RestMethod `
        -Uri "$baseUrl/api/users/$($testUser.id)/roles" `
        -Method Put `
        -Body $removeBody `
        -Headers $headers `
        -SkipCertificateCheck

    Write-Host "✅ Remove roles successful!" -ForegroundColor Green
    Write-Host ""

    # Step 7: Verify removal
    Write-Host "7️⃣  Verifying removal..." -ForegroundColor Yellow
    $finalVerify = Invoke-RestMethod `
        -Uri "$baseUrl/api/users/list?pageNumber=1&pageSize=10" `
        -Method Get `
        -Headers $headers `
        -SkipCertificateCheck

    $finalUser = $finalVerify.users | Where-Object { $_.id -eq $testUser.id }
    Write-Host "Final roles: $($finalUser.roles -join ', ')" -ForegroundColor Cyan
    
    if ($finalUser.roles.Count -eq 0) {
        Write-Host "✅ All roles removed successfully!" -ForegroundColor Green
    }
    Write-Host ""

    Write-Host "🎉 All tests passed!" -ForegroundColor Green
    
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "Status Code: $statusCode" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Test completed!" -ForegroundColor Cyan
