# Debug Access Denied Issue
# Hướng dẫn:

Write-Host "🔍 DEBUG CHECKLIST - Access Denied Issue" -ForegroundColor Cyan
Write-Host ""

Write-Host "1️⃣ Kiểm tra backend đang chạy:" -ForegroundColor Yellow
Write-Host "   - Mở: https://localhost:5001/api/users/me (phải thấy Unauthorized)"
Write-Host "   - Hoặc: https://localhost:5001/api" -ForegroundColor Gray
Write-Host ""

Write-Host "2️⃣ Kiểm tra database có role chưa:" -ForegroundColor Yellow
Write-Host "   SQL Query:" -ForegroundColor Gray
Write-Host @"
   SELECT u.Email, r.Name as RoleName
   FROM AspNetUsers u
   LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
   LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
   WHERE u.Email = 'devphongvv198@gmail.com';
"@ -ForegroundColor DarkGray
Write-Host ""

Write-Host "3️⃣ Kiểm tra frontend (Browser Console):" -ForegroundColor Yellow
Write-Host "   - Mở: http://localhost:5173 (hoặc port frontend của bạn)" -ForegroundColor Gray
Write-Host "   - Login với: devphongvv198@gmail.com" -ForegroundColor Gray
Write-Host "   - Mở Developer Tools (F12) -> Console tab" -ForegroundColor Gray
Write-Host "   - Tìm các log sau:" -ForegroundColor Gray
Write-Host ""
Write-Host "   Khi login thành công:" -ForegroundColor DarkYellow
Write-Host "   ✅ User profile loaded: {...}" -ForegroundColor Green
Write-Host "   User roles: ['Administrator']" -ForegroundColor Green
Write-Host ""
Write-Host "   Khi truy cập /management/users:" -ForegroundColor DarkYellow
Write-Host "   🔐 Admin check for route: /management/users" -ForegroundColor Cyan
Write-Host "   Current user: {...}" -ForegroundColor Cyan
Write-Host "   User roles: ['Administrator']" -ForegroundColor Cyan
Write-Host "   🔍 Checking role: Administrator User roles: ['Administrator']" -ForegroundColor Cyan
Write-Host "   Has Administrator role? true" -ForegroundColor Green
Write-Host "   ✅ Admin access granted" -ForegroundColor Green
Write-Host ""

Write-Host "4️⃣ Các lỗi thường gặp và cách fix:" -ForegroundColor Yellow
Write-Host ""
Write-Host "   ❌ Lỗi: User roles: undefined hoặc []" -ForegroundColor Red
Write-Host "   🔧 Fix: Chạy assign-admin-role.sql" -ForegroundColor Green
Write-Host ""
Write-Host "   ❌ Lỗi: Failed to load user profile: 404" -ForegroundColor Red
Write-Host "   🔧 Fix: Backend chưa chạy, start backend server" -ForegroundColor Green
Write-Host ""
Write-Host "   ❌ Lỗi: Token is invalid (401)" -ForegroundColor Red
Write-Host "   🔧 Fix: Logout và login lại" -ForegroundColor Green
Write-Host ""
Write-Host "   ❌ Lỗi: User roles: ['Administrator'] nhưng vẫn Access Denied" -ForegroundColor Red
Write-Host "   🔧 Fix: hasRole function bị lỗi, đã sửa trong auth.ts" -ForegroundColor Green
Write-Host ""

Write-Host "5️⃣ Test nhanh API:" -ForegroundColor Yellow
$email = Read-Host "Nhập email (Enter = devphongvv198@gmail.com)"
if ([string]::IsNullOrWhiteSpace($email)) {
    $email = "devphongvv198@gmail.com"
}

$password = Read-Host "P@ssword123" -AsSecureString
$passwordText = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
)

Write-Host ""
Write-Host "Testing login..." -ForegroundColor Cyan

try {
    $loginBody = @{
        email = $email
        password = $passwordText
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "https://localhost:5001/api/users/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $loginBody `
        -SkipCertificateCheck

    $token = $loginResponse.accessToken
    
    Write-Host "✅ Login successful!" -ForegroundColor Green
    
    # Get user profile
    $headers = @{
        "Authorization" = "Bearer $token"
    }

    $userProfile = Invoke-RestMethod -Uri "https://localhost:5001/api/users/me" `
        -Method GET `
        -Headers $headers `
        -SkipCertificateCheck

    Write-Host ""
    Write-Host "User Email: $($userProfile.email)" -ForegroundColor Cyan
    Write-Host "User Roles: $($userProfile.roles -join ', ')" -ForegroundColor Cyan
    
    if ($userProfile.roles -contains "Administrator") {
        Write-Host ""
        Write-Host "✅✅✅ USER HAS ADMINISTRATOR ROLE ✅✅✅" -ForegroundColor Green -BackgroundColor Black
        Write-Host ""
        Write-Host "Nếu vẫn bị Access Denied trên frontend:" -ForegroundColor Yellow
        Write-Host "1. Xóa cache browser (Ctrl+Shift+Delete)" -ForegroundColor White
        Write-Host "2. Logout và login lại" -ForegroundColor White
        Write-Host "3. Kiểm tra console logs trong Developer Tools (F12)" -ForegroundColor White
        Write-Host "4. Đảm bảo .env có VITE_API_URL=https://localhost:5001" -ForegroundColor White
    } else {
        Write-Host ""
        Write-Host "❌❌❌ USER DOES NOT HAVE ADMINISTRATOR ROLE ❌❌❌" -ForegroundColor Red -BackgroundColor Black
        Write-Host ""
        Write-Host "Chạy script sau để gán quyền:" -ForegroundColor Yellow
        Write-Host "  assign-admin-role.sql" -ForegroundColor White
    }
    
} catch {
    Write-Host "❌ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.ErrorDetails.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Nếu cần hỗ trợ thêm, gửi screenshot console logs (F12)" -ForegroundColor Gray
Write-Host "================================================" -ForegroundColor Cyan
