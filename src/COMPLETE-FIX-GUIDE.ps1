Write-Host "🎯 HƯỚNG DẪN HOÀN CHỈNH - Fix Access Denied" -ForegroundColor Cyan -BackgroundColor Black
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "VẤN ĐỀ ĐÃ XÁC ĐỊNH" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Console log cho thấy:" -ForegroundColor White
Write-Host "  ⚠️ No token available for loadUserProfile" -ForegroundColor Red
Write-Host ""
Write-Host "→ Bạn đang cố truy cập trang admin TRƯỚC KHI LOGIN" -ForegroundColor Red
Write-Host "→ Hoặc token đã bị xóa/expired" -ForegroundColor Red
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "GIẢI PHÁP (Làm theo thứ tự)" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "BƯỚC 1: Restart Frontend Server" -ForegroundColor Yellow
Write-Host "  Mở terminal mới và chạy:" -ForegroundColor White
Write-Host ""
Write-Host "  cd c:\Users\ACER\source\repos\RoomEnglish\src\Web\ClientApp" -ForegroundColor Cyan
Write-Host "  npm run dev" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Đợi đến khi thấy:" -ForegroundColor White
Write-Host "  ➜  Local:   http://localhost:3000/" -ForegroundColor Green
Write-Host ""

Write-Host "BƯỚC 2: Mở Browser với DevTools" -ForegroundColor Yellow
Write-Host "  1. Mở Chrome/Edge (Incognito mode để tránh cache cũ)" -ForegroundColor White
Write-Host "  2. Truy cập: http://localhost:3000" -ForegroundColor White
Write-Host "  3. Nhấn F12 để mở Developer Tools" -ForegroundColor White
Write-Host "  4. Chọn tab 'Console'" -ForegroundColor White
Write-Host "  5. Clear console (click icon trash hoặc Ctrl+L)" -ForegroundColor White
Write-Host ""

Write-Host "BƯỚC 3: Kiểm tra log khởi tạo" -ForegroundColor Yellow
Write-Host "  Phải thấy log sau trong Console:" -ForegroundColor White
Write-Host ""
Write-Host "  🔐 Auth initialized on app startup" -ForegroundColor Green
Write-Host "  🔐 Initializing auth..." -ForegroundColor Green
Write-Host "  Stored token: null" -ForegroundColor Green
Write-Host "  ⚠️ No valid token found, user needs to login" -ForegroundColor Yellow
Write-Host ""
Write-Host "  → Đây là BÌNH THƯỜNG nếu chưa login" -ForegroundColor Gray
Write-Host ""

Write-Host "BƯỚC 4: Login" -ForegroundColor Yellow
Write-Host "  1. Click vào 'Login' hoặc truy cập: http://localhost:3000/auth/login" -ForegroundColor White
Write-Host ""
Write-Host "  2. Nhập thông tin:" -ForegroundColor White
Write-Host "     Email: devphongvv198@gmail.com" -ForegroundColor Cyan
Write-Host "     Password: P@ssword123" -ForegroundColor Cyan
Write-Host ""
Write-Host "  3. Click 'Login'" -ForegroundColor White
Write-Host ""
Write-Host "  4. Kiểm tra Console logs (PHẢI thấy các log sau):" -ForegroundColor White
Write-Host ""
Write-Host "     🔐 Logging in..." -ForegroundColor Green
Write-Host "     ✅ Login API successful, saving tokens..." -ForegroundColor Green
Write-Host "     Token saved: CfDJ8Ag8Ern9aCpGsluy..." -ForegroundColor Green
Write-Host "     📡 Loading user profile after login..." -ForegroundColor Green
Write-Host "     📡 Loading user profile from /api/users/me" -ForegroundColor Green
Write-Host "     ✅ User profile loaded: {id: '...', email: '...', roles: ['Administrator']}" -ForegroundColor Green
Write-Host "     User roles: ['Administrator']" -ForegroundColor Green
Write-Host ""

Write-Host "BƯỚC 5: Kiểm tra localStorage" -ForegroundColor Yellow
Write-Host "  1. Chọn tab 'Application' trong DevTools" -ForegroundColor White
Write-Host "  2. Bên trái: Storage → Local Storage → http://localhost:3000" -ForegroundColor White
Write-Host "  3. PHẢI thấy các key sau:" -ForegroundColor White
Write-Host ""
Write-Host "     ✅ access_token: CfDJ8Ag8..." -ForegroundColor Green
Write-Host "     ✅ refresh_token: CfDJ8..." -ForegroundColor Green
Write-Host "     ✅ token_expires_at: 1730822400000" -ForegroundColor Green
Write-Host ""
Write-Host "  NẾU KHÔNG CÓ → Login bị lỗi, xem console logs" -ForegroundColor Red
Write-Host ""

Write-Host "BƯỚC 6: Truy cập Management Page" -ForegroundColor Yellow
Write-Host "  1. Quay lại tab 'Console'" -ForegroundColor White
Write-Host "  2. Clear console (Ctrl+L)" -ForegroundColor White
Write-Host "  3. Click menu: Quản lý → Quản lý Tài khoản" -ForegroundColor White
Write-Host "     Hoặc truy cập: http://localhost:3000/management/users" -ForegroundColor White
Write-Host ""
Write-Host "  4. Kiểm tra Console logs (PHẢI thấy):" -ForegroundColor White
Write-Host ""
Write-Host "     🔐 Admin check for route: /management/users" -ForegroundColor Green
Write-Host "     Current user: {id: '...', email: '...', roles: ['Administrator']}" -ForegroundColor Green
Write-Host "     User roles: ['Administrator']" -ForegroundColor Green
Write-Host "     🔍 Checking role: Administrator User roles: ['Administrator']" -ForegroundColor Green
Write-Host "     Has Administrator role? true" -ForegroundColor Green
Write-Host "     ✅ Admin access granted" -ForegroundColor Green
Write-Host ""
Write-Host "  5. Page quản lý users phải hiển thị (KHÔNG redirect Access Denied)" -ForegroundColor White
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "NẾU VẪN BỊ LỖI" -ForegroundColor Red
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "❌ LỖI: Login không có log nào" -ForegroundColor Red
Write-Host "   → Kiểm tra Network tab (F12 → Network)" -ForegroundColor Yellow
Write-Host "   → Tìm request POST /api/users/login" -ForegroundColor Yellow
Write-Host "   → Xem Status code và Response" -ForegroundColor Yellow
Write-Host "   → Gửi screenshot cho tôi" -ForegroundColor Yellow
Write-Host ""

Write-Host "❌ LỖI: Login thành công nhưng không có access_token trong localStorage" -ForegroundColor Red
Write-Host "   → Kiểm tra Console có lỗi gì không" -ForegroundColor Yellow
Write-Host "   → Có thể localStorage bị block (Privacy settings)" -ForegroundColor Yellow
Write-Host "   → Thử browser khác hoặc Incognito mode" -ForegroundColor Yellow
Write-Host ""

Write-Host "❌ LỖI: User roles: [] (empty array)" -ForegroundColor Red
Write-Host "   → Database chưa có role Administrator cho user" -ForegroundColor Yellow
Write-Host "   → Chạy SQL script: assign-admin-role.sql" -ForegroundColor Yellow
Write-Host "   → Logout và login lại" -ForegroundColor Yellow
Write-Host ""

Write-Host "❌ LỖI: Has Administrator role? false (nhưng roles: ['Administrator'])" -ForegroundColor Red
Write-Host "   → hasRole() function bị lỗi" -ForegroundColor Yellow
Write-Host "   → Đảm bảo đã pull code mới nhất" -ForegroundColor Yellow
Write-Host "   → Restart frontend server" -ForegroundColor Yellow
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "DEBUG CHECKLIST" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Trước khi báo lỗi, kiểm tra:" -ForegroundColor White
Write-Host ""
Write-Host "  ☐ Backend đang chạy tại https://localhost:5001" -ForegroundColor White
Write-Host "  ☐ Frontend đang chạy tại http://localhost:3000" -ForegroundColor White
Write-Host "  ☐ Đã chạy assign-admin-role.sql" -ForegroundColor White
Write-Host "  ☐ Đã LOGIN (không phải chỉ truy cập trang)" -ForegroundColor White
Write-Host "  ☐ Console có log '✅ User profile loaded'" -ForegroundColor White
Write-Host "  ☐ localStorage có access_token" -ForegroundColor White
Write-Host "  ☐ Browser không phải Safari (Safari có vấn đề localStorage)" -ForegroundColor White
Write-Host ""

Write-Host "Nếu tất cả đã check mà vẫn lỗi, gửi cho tôi:" -ForegroundColor Yellow
Write-Host "  1. Screenshot Console logs khi app khởi động" -ForegroundColor White
Write-Host "  2. Screenshot Console logs khi login" -ForegroundColor White
Write-Host "  3. Screenshot Application tab (localStorage)" -ForegroundColor White
Write-Host "  4. Screenshot Console logs khi truy cập /management/users" -ForegroundColor White
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "TÓM TẮT" -ForegroundColor Green -BackgroundColor Black
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Vấn đề hiện tại:" -ForegroundColor Yellow
Write-Host "  → Bạn chưa LOGIN nhưng đang cố truy cập trang admin" -ForegroundColor Red
Write-Host ""
Write-Host "Giải pháp:" -ForegroundColor Yellow
Write-Host "  1. Restart frontend" -ForegroundColor White
Write-Host "  2. Mở browser với DevTools (F12)" -ForegroundColor White
Write-Host "  3. LOGIN với devphongvv198@gmail.com" -ForegroundColor White
Write-Host "  4. Kiểm tra localStorage có token" -ForegroundColor White
Write-Host "  5. Truy cập /management/users" -ForegroundColor White
Write-Host ""
Write-Host "Sau khi login thành công 1 lần, refresh page vẫn giữ login!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
