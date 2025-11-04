Write-Host "🔍 PROBLEM IDENTIFIED!" -ForegroundColor Red -BackgroundColor Black
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "VẤN ĐỀ: No token available" -ForegroundColor Red
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Console log cho thấy:" -ForegroundColor Yellow
Write-Host "  ⚠️ No token available for loadUserProfile" -ForegroundColor Red
Write-Host "  → Token không tồn tại trong localStorage" -ForegroundColor Red
Write-Host ""

Write-Host "Nguyên nhân có thể:" -ForegroundColor Yellow
Write-Host "  1. Chưa login" -ForegroundColor White
Write-Host "  2. Token bị xóa khi refresh page" -ForegroundColor White
Write-Host "  3. localStorage key không đúng" -ForegroundColor White
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "GIẢI PHÁP: Kiểm tra localStorage" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "BƯỚC 1: Mở Browser Developer Tools" -ForegroundColor Yellow
Write-Host "  1. Mở http://localhost:3000" -ForegroundColor White
Write-Host "  2. Nhấn F12" -ForegroundColor White
Write-Host "  3. Chọn tab 'Application'" -ForegroundColor White
Write-Host "  4. Bên trái: Storage → Local Storage → http://localhost:3000" -ForegroundColor White
Write-Host ""

Write-Host "BƯỚC 2: Kiểm tra các key sau có tồn tại không:" -ForegroundColor Yellow
Write-Host "  ✅ access_token" -ForegroundColor Green
Write-Host "  ✅ refresh_token" -ForegroundColor Green
Write-Host "  ✅ token_expires_at" -ForegroundColor Green
Write-Host ""

Write-Host "NẾU KHÔNG CÓ → Bạn chưa login hoặc token bị mất" -ForegroundColor Red
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "CÁCH FIX" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Option 1: Login lại" -ForegroundColor Yellow
Write-Host "  1. Mở http://localhost:3000/auth/login" -ForegroundColor White
Write-Host "  2. Login: devphongvv198@gmail.com / P@ssword123" -ForegroundColor White
Write-Host "  3. Sau khi login, kiểm tra localStorage lại" -ForegroundColor White
Write-Host "     → Phải có access_token, refresh_token" -ForegroundColor Gray
Write-Host "  4. Thử truy cập /management/users lại" -ForegroundColor White
Write-Host ""

Write-Host "Option 2: Kiểm tra login flow có lưu token không" -ForegroundColor Yellow
Write-Host "  Mở Console tab khi login, phải thấy:" -ForegroundColor White
Write-Host "    📡 Loading user profile from /api/users/me" -ForegroundColor Green
Write-Host "    ✅ User profile loaded: {...}" -ForegroundColor Green
Write-Host ""
Write-Host "  Nếu KHÔNG thấy → Login flow bị lỗi" -ForegroundColor Red
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "DEBUG: Kiểm tra login flow" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Trong Console tab, khi login phải thấy các bước sau:" -ForegroundColor White
Write-Host ""
Write-Host "Step 1: Login request" -ForegroundColor Yellow
Write-Host "  POST /api/users/login" -ForegroundColor Gray
Write-Host "  Response: {tokenType: 'Bearer', accessToken: '...', ...}" -ForegroundColor Gray
Write-Host ""
Write-Host "Step 2: Save tokens" -ForegroundColor Yellow
Write-Host "  → localStorage.setItem('access_token', ...)" -ForegroundColor Gray
Write-Host ""
Write-Host "Step 3: Load user profile" -ForegroundColor Yellow
Write-Host "  📡 Loading user profile from /api/users/me" -ForegroundColor Gray
Write-Host "  ✅ User profile loaded: {...roles: ['Administrator']}" -ForegroundColor Gray
Write-Host ""
Write-Host "NẾU thiếu bước nào → Tìm lỗi ở bước đó" -ForegroundColor Red
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "QUICK TEST" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$test = Read-Host "Bạn muốn test login flow ngay không? (y/n)"
if ($test -eq 'y') {
    Write-Host ""
    Write-Host "Làm theo các bước sau:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "1. Mở http://localhost:3000 trong Chrome/Edge" -ForegroundColor White
    Write-Host "2. Nhấn F12, chọn Console tab" -ForegroundColor White
    Write-Host "3. Clear console (icon trash hoặc Ctrl+L)" -ForegroundColor White
    Write-Host "4. Chọn Application tab, xóa toàn bộ Local Storage" -ForegroundColor White
    Write-Host "5. Quay lại Console tab" -ForegroundColor White
    Write-Host "6. Truy cập /auth/login và đăng nhập" -ForegroundColor White
    Write-Host "7. Quan sát console logs" -ForegroundColor White
    Write-Host ""
    Write-Host "Gửi screenshot console logs cho tôi nếu vẫn lỗi!" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "LƯU Ý QUAN TRỌNG" -ForegroundColor Red
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "⚠️ initializeAuth() chỉ load token ĐÃ TỒN TẠI" -ForegroundColor Yellow
Write-Host "⚠️ Nó KHÔNG tự động login nếu chưa có token" -ForegroundColor Yellow
Write-Host "⚠️ Bạn PHẢI login thủ công lần đầu tiên" -ForegroundColor Yellow
Write-Host ""
Write-Host "Sau khi login → token được lưu → refresh page vẫn giữ login" -ForegroundColor Green
Write-Host ""
