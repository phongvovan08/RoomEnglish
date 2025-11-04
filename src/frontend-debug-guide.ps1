Write-Host "🔍 FRONTEND ACCESS DENIED DEBUGGER" -ForegroundColor Cyan
Write-Host ""
Write-Host "Hướng dẫn kiểm tra chi tiết:" -ForegroundColor Yellow
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "BƯỚC 1: Mở Browser Developer Tools" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "1. Mở Chrome/Edge" -ForegroundColor White
Write-Host "2. Truy cập: http://localhost:5173" -ForegroundColor White
Write-Host "3. Nhấn F12 để mở Developer Tools" -ForegroundColor White
Write-Host "4. Chọn tab 'Console'" -ForegroundColor White
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "BƯỚC 2: Login" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "1. Login với: devphongvv198@gmail.com / P@ssword123" -ForegroundColor White
Write-Host "2. Kiểm tra Console, phải thấy:" -ForegroundColor White
Write-Host ""
Write-Host "   📡 Loading user profile from https://localhost:5001/api/users/me" -ForegroundColor Gray
Write-Host "   ✅ User profile loaded: {id: '...', email: '...', roles: ['Administrator']}" -ForegroundColor Green
Write-Host "   User roles: ['Administrator']" -ForegroundColor Green
Write-Host ""
Write-Host "❌ NẾU KHÔNG THẤY LOG TRÊN:" -ForegroundColor Red
Write-Host "   → User profile không được load" -ForegroundColor Red
Write-Host "   → Kiểm tra VITE_API_URL trong .env" -ForegroundColor Yellow
Write-Host "   → Phải là: VITE_API_URL=https://localhost:5001" -ForegroundColor Yellow
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "BƯỚC 3: Truy cập /management/users" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "1. Click vào menu 'Quản lý' -> 'Quản lý Tài khoản'" -ForegroundColor White
Write-Host "   Hoặc truy cập trực tiếp: http://localhost:5173/management/users" -ForegroundColor White
Write-Host ""
Write-Host "2. Kiểm tra Console, phải thấy:" -ForegroundColor White
Write-Host ""
Write-Host "   🔐 Admin check for route: /management/users" -ForegroundColor Cyan
Write-Host "   Current user: {id: '...', email: '...', roles: ['Administrator']}" -ForegroundColor Cyan
Write-Host "   User roles: ['Administrator']" -ForegroundColor Cyan
Write-Host "   🔍 Checking role: Administrator User roles: ['Administrator']" -ForegroundColor Cyan
Write-Host "   Has Administrator role? true" -ForegroundColor Green
Write-Host "   ✅ Admin access granted" -ForegroundColor Green
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "CÁC TRƯỜNG HỢP LỖI THƯỜNG GẶP" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "❌ TRƯỜNG HỢP 1: User roles: undefined" -ForegroundColor Red
Write-Host "   Console log:" -ForegroundColor Gray
Write-Host "   Current user: {id: '...', email: '...', roles: undefined}" -ForegroundColor DarkGray
Write-Host ""
Write-Host "   Nguyên nhân:" -ForegroundColor Yellow
Write-Host "   - User profile không load được" -ForegroundColor White
Write-Host "   - API /api/users/me không trả về roles" -ForegroundColor White
Write-Host ""
Write-Host "   Giải pháp:" -ForegroundColor Green
Write-Host "   1. Logout và login lại" -ForegroundColor White
Write-Host "   2. Xóa localStorage (F12 -> Application -> Local Storage -> Clear)" -ForegroundColor White
Write-Host "   3. Hard refresh (Ctrl+Shift+R)" -ForegroundColor White
Write-Host ""

Write-Host "❌ TRƯỜNG HỢP 2: User roles: []" -ForegroundColor Red
Write-Host "   Console log:" -ForegroundColor Gray
Write-Host "   Current user: {id: '...', email: '...', roles: []}" -ForegroundColor DarkGray
Write-Host "   Has Administrator role? false" -ForegroundColor DarkGray
Write-Host ""
Write-Host "   Nguyên nhân:" -ForegroundColor Yellow
Write-Host "   - Database chưa có role cho user" -ForegroundColor White
Write-Host ""
Write-Host "   Giải pháp:" -ForegroundColor Green
Write-Host "   1. Chạy SQL script: assign-admin-role.sql" -ForegroundColor White
Write-Host "   2. Logout và login lại" -ForegroundColor White
Write-Host ""

Write-Host "❌ TRƯỜNG HỢP 3: Current user: null" -ForegroundColor Red
Write-Host "   Console log:" -ForegroundColor Gray
Write-Host "   🔐 Admin check for route: /management/users" -ForegroundColor DarkGray
Write-Host "   Current user: null" -ForegroundColor DarkGray
Write-Host "   ⏳ User data not loaded, loading..." -ForegroundColor DarkGray
Write-Host ""
Write-Host "   Nguyên nhân:" -ForegroundColor Yellow
Write-Host "   - AuthStore chưa khởi tạo user data" -ForegroundColor White
Write-Host "   - initializeAuth() chưa được gọi" -ForegroundColor White
Write-Host ""
Write-Host "   Giải pháp:" -ForegroundColor Green
Write-Host "   1. Kiểm tra main.ts có gọi authStore.initializeAuth()" -ForegroundColor White
Write-Host "   2. Logout và login lại" -ForegroundColor White
Write-Host ""

Write-Host "❌ TRƯỜNG HỢP 4: Failed to load user profile: 404" -ForegroundColor Red
Write-Host "   Console log:" -ForegroundColor Gray
Write-Host "   📡 Loading user profile from http://localhost:3000/api/users/me" -ForegroundColor DarkGray
Write-Host "   ❌ Failed to load user profile: 404" -ForegroundColor DarkGray
Write-Host ""
Write-Host "   Nguyên nhân:" -ForegroundColor Yellow
Write-Host "   - API URL sai (đang gọi localhost:3000 thay vì localhost:5001)" -ForegroundColor White
Write-Host "   - VITE_API_URL chưa được set" -ForegroundColor White
Write-Host ""
Write-Host "   Giải pháp:" -ForegroundColor Green
Write-Host "   1. Tạo/sửa file .env trong ClientApp folder:" -ForegroundColor White
Write-Host "      VITE_API_URL=https://localhost:5001" -ForegroundColor Cyan
Write-Host "   2. Restart Vite dev server" -ForegroundColor White
Write-Host "   3. Hard refresh browser (Ctrl+Shift+R)" -ForegroundColor White
Write-Host ""

Write-Host "❌ TRƯỜNG HỢP 5: Has Administrator role? false (nhưng roles: ['Administrator'])" -ForegroundColor Red
Write-Host "   Console log:" -ForegroundColor Gray
Write-Host "   User roles: ['Administrator']" -ForegroundColor DarkGray
Write-Host "   🔍 Checking role: Administrator User roles: ['Administrator']" -ForegroundColor DarkGray
Write-Host "   Has Administrator role? false" -ForegroundColor DarkGray
Write-Host ""
Write-Host "   Nguyên nhân:" -ForegroundColor Yellow
Write-Host "   - hasRole() function bị lỗi" -ForegroundColor White
Write-Host ""
Write-Host "   Giải pháp:" -ForegroundColor Green
Write-Host "   - ĐÃ FIX trong auth.ts (commit mới nhất)" -ForegroundColor White
Write-Host "   - Pull code mới nhất và restart frontend" -ForegroundColor White
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "QUICK FIX CHECKLIST" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "□ 1. Kiểm tra .env file có VITE_API_URL=https://localhost:5001" -ForegroundColor White
Write-Host "□ 2. Backend đang chạy tại https://localhost:5001" -ForegroundColor White
Write-Host "□ 3. Frontend đang chạy tại http://localhost:5173" -ForegroundColor White
Write-Host "□ 4. Đã chạy SQL script assign-admin-role.sql" -ForegroundColor White
Write-Host "□ 5. Đã logout và login lại" -ForegroundColor White
Write-Host "□ 6. Đã xóa browser cache/localStorage" -ForegroundColor White
Write-Host "□ 7. Đã hard refresh (Ctrl+Shift+R)" -ForegroundColor White
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Sau khi làm xong checklist, gửi cho tôi:" -ForegroundColor Yellow
Write-Host "1. Screenshot Console logs khi login" -ForegroundColor White
Write-Host "2. Screenshot Console logs khi truy cập /management/users" -ForegroundColor White
Write-Host "================================================" -ForegroundColor Cyan
