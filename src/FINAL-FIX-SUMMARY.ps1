Write-Host "🚀 FINAL FIX SUMMARY - Access Denied Issue" -ForegroundColor Green -BackgroundColor Black
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "ĐÃ SỬA CÁC VẤN ĐỀ SAU:" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "✅ 1. Logout 405 Error" -ForegroundColor Green
Write-Host "   File: authService.ts" -ForegroundColor Gray
Write-Host "   Fix: Xóa call đến /api/Users/logout endpoint không tồn tại" -ForegroundColor Gray
Write-Host ""

Write-Host "✅ 2. initializeAuth() không được gọi" -ForegroundColor Green
Write-Host "   File: main.ts" -ForegroundColor Gray
Write-Host "   Fix: Thêm authStore.initializeAuth() sau app.mount()" -ForegroundColor Gray
Write-Host "   → User profile sẽ tự động load khi app khởi động" -ForegroundColor Gray
Write-Host ""

Write-Host "✅ 3. Thêm debug logs chi tiết" -ForegroundColor Green
Write-Host "   File: auth.ts, router/index.ts" -ForegroundColor Gray
Write-Host "   → Dễ dàng debug khi có vấn đề" -ForegroundColor Gray
Write-Host ""

Write-Host "✅ 4. API endpoint configuration" -ForegroundColor Green
Write-Host "   Vite proxy: /api → https://localhost:5001" -ForegroundColor Gray
Write-Host "   Frontend: http://localhost:3000" -ForegroundColor Gray
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "HƯỚNG DẪN TEST" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "BƯỚC 1: Restart Frontend" -ForegroundColor Yellow
Write-Host "   cd Web\ClientApp" -ForegroundColor White
Write-Host "   npm run dev" -ForegroundColor White
Write-Host ""

Write-Host "BƯỚC 2: Xóa browser cache" -ForegroundColor Yellow
Write-Host "   1. Mở http://localhost:3000" -ForegroundColor White
Write-Host "   2. Nhấn F12 (Developer Tools)" -ForegroundColor White
Write-Host "   3. Application tab → Local Storage → Clear All" -ForegroundColor White
Write-Host "   4. Console tab → Clear console" -ForegroundColor White
Write-Host "   5. Hard refresh: Ctrl+Shift+R" -ForegroundColor White
Write-Host ""

Write-Host "BƯỚC 3: Login và kiểm tra Console" -ForegroundColor Yellow
Write-Host "   1. Login: devphongvv198@gmail.com / P@ssword123" -ForegroundColor White
Write-Host ""
Write-Host "   2. Kiểm tra Console logs (phải thấy):" -ForegroundColor White
Write-Host "      🔐 Auth initialized on app startup" -ForegroundColor Green
Write-Host "      📡 Loading user profile from /api/users/me" -ForegroundColor Green
Write-Host "      ✅ User profile loaded: {email: '...', roles: ['Administrator']}" -ForegroundColor Green
Write-Host "      User roles: ['Administrator']" -ForegroundColor Green
Write-Host ""

Write-Host "BƯỚC 4: Truy cập Management Page" -ForegroundColor Yellow
Write-Host "   1. Click menu: Quản lý → Quản lý Tài khoản" -ForegroundColor White
Write-Host "      Hoặc: http://localhost:3000/management/users" -ForegroundColor White
Write-Host ""
Write-Host "   2. Kiểm tra Console logs (phải thấy):" -ForegroundColor White
Write-Host "      🔐 Admin check for route: /management/users" -ForegroundColor Green
Write-Host "      Current user: {...}" -ForegroundColor Green
Write-Host "      User roles: ['Administrator']" -ForegroundColor Green
Write-Host "      🔍 Checking role: Administrator User roles: ['Administrator']" -ForegroundColor Green
Write-Host "      Has Administrator role? true" -ForegroundColor Green
Write-Host "      ✅ Admin access granted" -ForegroundColor Green
Write-Host ""
Write-Host "   3. Page phải load thành công (không redirect đến Access Denied)" -ForegroundColor White
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "NẾU VẪN BỊ ACCESS DENIED" -ForegroundColor Red
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Gửi cho tôi screenshot các logs sau:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1️⃣ Console logs khi app khởi động" -ForegroundColor White
Write-Host "   (Phải có: 🔐 Auth initialized on app startup)" -ForegroundColor Gray
Write-Host ""
Write-Host "2️⃣ Console logs khi login" -ForegroundColor White
Write-Host "   (Phải có: ✅ User profile loaded)" -ForegroundColor Gray
Write-Host ""
Write-Host "3️⃣ Console logs khi truy cập /management/users" -ForegroundColor White
Write-Host "   (Phải có: 🔐 Admin check for route)" -ForegroundColor Gray
Write-Host ""
Write-Host "4️⃣ Network tab (F12 → Network)" -ForegroundColor White
Write-Host "   - Filter: /api/users/me" -ForegroundColor Gray
Write-Host "   - Xem Status code và Response" -ForegroundColor Gray
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "KIỂM TRA NHANH DATABASE" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$checkDb = Read-Host "Bạn có muốn kiểm tra database roles ngay không? (y/n)"
if ($checkDb -eq 'y') {
    Write-Host ""
    Write-Host "Chạy SQL query này trong SQL Server Management Studio:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host @"
SELECT 
    u.Email,
    u.UserName,
    u.EmailConfirmed,
    r.Name as RoleName,
    ur.RoleId
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'devphongvv198@gmail.com';
"@ -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Kết quả mong đợi:" -ForegroundColor Yellow
    Write-Host "Email: devphongvv198@gmail.com" -ForegroundColor White
    Write-Host "RoleName: Administrator" -ForegroundColor White
    Write-Host ""
    Write-Host "Nếu RoleName = NULL → Chạy assign-admin-role.sql" -ForegroundColor Red
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "SUMMARY" -ForegroundColor Green -BackgroundColor Black
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "✅ Backend API: Working (test-login-roles.ps1 đã confirm)" -ForegroundColor Green
Write-Host "✅ User có role Administrator trong database" -ForegroundColor Green
Write-Host "✅ Code đã được fix:" -ForegroundColor Green
Write-Host "   - authStore.initializeAuth() được gọi" -ForegroundColor White
Write-Host "   - hasRole() function đã sửa" -ForegroundColor White
Write-Host "   - Debug logs đã thêm" -ForegroundColor White
Write-Host "   - Logout 405 error đã fix" -ForegroundColor White
Write-Host ""
Write-Host "⏭️ NEXT: Restart frontend, clear cache, test lại" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
