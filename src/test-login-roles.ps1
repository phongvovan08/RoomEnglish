# Script kiểm tra login và roles
# Đảm bảo backend đang chạy tại https://localhost:7074

$baseUrl = "https://localhost:5001"
$email = "devphongvv198@gmail.com"
$password = "P@ssword123"  # Thay đổi password thực tế

Write-Host "🔐 Testing Login and Role Claims..." -ForegroundColor Cyan
Write-Host ""

# Step 1: Login
Write-Host "Step 1: Login with $email" -ForegroundColor Yellow
try {
    $loginBody = @{
        email = $email
        password = $password
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/users/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $loginBody `
        -SkipCertificateCheck

    $token = $loginResponse.accessToken
    
    if ($token) {
        Write-Host "✅ Login successful!" -ForegroundColor Green
        Write-Host "Token (first 50 chars): $($token.Substring(0, [Math]::Min(50, $token.Length)))..." -ForegroundColor Gray
    } else {
        Write-Host "❌ Login failed - No token received" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.ErrorDetails.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 2: Get User Profile (including roles)
Write-Host "Step 2: Get user profile with roles" -ForegroundColor Yellow
try {
    $headers = @{
        "Authorization" = "Bearer $token"
    }

    $userProfile = Invoke-RestMethod -Uri "$baseUrl/api/users/me" `
        -Method GET `
        -Headers $headers `
        -SkipCertificateCheck

    Write-Host "✅ User profile retrieved successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "USER INFORMATION" -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "Email:        $($userProfile.email)"
    Write-Host "Display Name: $($userProfile.displayName)"
    Write-Host "User ID:      $($userProfile.id)"
    
    if ($userProfile.roles -and $userProfile.roles.Count -gt 0) {
        Write-Host "Roles:        $($userProfile.roles -join ', ')" -ForegroundColor Green
        
        if ($userProfile.roles -contains "Administrator") {
            Write-Host ""
            Write-Host "✅ ADMINISTRATOR ROLE DETECTED!" -ForegroundColor Green -BackgroundColor Black
            Write-Host "✅ User has full admin access" -ForegroundColor Green
        } else {
            Write-Host ""
            Write-Host "⚠️ No Administrator role found" -ForegroundColor Yellow
        }
    } else {
        Write-Host "Roles:        (None)" -ForegroundColor Red
        Write-Host ""
        Write-Host "❌ WARNING: User has no roles assigned!" -ForegroundColor Red
        Write-Host "Run assign-admin-role.sql to assign Administrator role" -ForegroundColor Yellow
    }
    Write-Host "================================================" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Failed to get user profile: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.ErrorDetails.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 3: Test Admin-only endpoint
Write-Host "Step 3: Test admin-only endpoint (/api/users/roles)" -ForegroundColor Yellow
try {
    $rolesResponse = Invoke-RestMethod -Uri "$baseUrl/api/users/roles" `
        -Method GET `
        -Headers $headers `
        -SkipCertificateCheck

    Write-Host "✅ Admin endpoint accessible!" -ForegroundColor Green
    Write-Host "Available roles: $($rolesResponse.roles -join ', ')" -ForegroundColor Green
} catch {
    if ($_.Exception.Response.StatusCode -eq 403) {
        Write-Host "❌ Access Denied (403)" -ForegroundColor Red
        Write-Host "User does not have Administrator role" -ForegroundColor Red
        Write-Host "Run assign-admin-role.sql to fix this" -ForegroundColor Yellow
    } else {
        Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""

# Step 4: Decode JWT token (if jq is available or manual inspection)
Write-Host "Step 4: JWT Token Claims" -ForegroundColor Yellow
try {
    # Split JWT into parts
    $parts = $token.Split('.')
    if ($parts.Length -eq 3) {
        # Decode payload (second part)
        $payload = $parts[1]
        
        # Add padding if needed
        while ($payload.Length % 4 -ne 0) {
            $payload += "="
        }
        
        # Base64 decode
        $payloadBytes = [Convert]::FromBase64String($payload)
        $payloadJson = [System.Text.Encoding]::UTF8.GetString($payloadBytes)
        $claims = $payloadJson | ConvertFrom-Json
        
        Write-Host "Token Claims:" -ForegroundColor Cyan
        $claims | Format-List
        
        # Check for role claim
        if ($claims.role) {
            Write-Host "✅ Role claim found in token: $($claims.role)" -ForegroundColor Green
        } elseif ($claims.'http://schemas.microsoft.com/ws/2008/06/identity/claims/role') {
            Write-Host "✅ Role claim found in token: $($claims.'http://schemas.microsoft.com/ws/2008/06/identity/claims/role')" -ForegroundColor Green
        } else {
            Write-Host "⚠️ No role claim found in JWT token" -ForegroundColor Yellow
            Write-Host "This might be OK if roles are checked server-side only" -ForegroundColor Gray
        }
    }
} catch {
    Write-Host "⚠️ Could not decode JWT token: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "SUMMARY" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "✅ Login: Working"
Write-Host "✅ /api/users/me: Working (returns roles)"
if ($userProfile.roles -contains "Administrator") {
    Write-Host "✅ Administrator Role: Assigned" -ForegroundColor Green
} else {
    Write-Host "❌ Administrator Role: NOT assigned" -ForegroundColor Red
    Write-Host ""
    Write-Host "To fix: Run assign-admin-role.sql in database" -ForegroundColor Yellow
}
Write-Host "================================================" -ForegroundColor Cyan
