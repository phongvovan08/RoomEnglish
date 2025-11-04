Write-Host "🔄 TEST LOGIN/LOGOUT MENU UPDATE" -ForegroundColor Cyan -BackgroundColor Black
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "HƯỚNG DẪN TEST" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "SETUP:" -ForegroundColor Yellow
Write-Host "  1. Đảm bảo backend đang chạy (https://localhost:5001)" -ForegroundColor White
Write-Host "  2. Đảm bảo frontend đang chạy (http://localhost:3000)" -ForegroundColor White
Write-Host "  3. Mở browser (Chrome/Edge)" -ForegroundColor White
Write-Host "  4. Mở Developer Tools (F12) → Console tab" -ForegroundColor White
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "TEST 1: Menu khi chưa login" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1. Truy cập: http://localhost:3000" -ForegroundColor White
Write-Host "  2. Kiểm tra menu (không có nút Login/Register thì refresh)" -ForegroundColor White
Write-Host ""
Write-Host "  ✅ Expected Console Logs:" -ForegroundColor Green
Write-Host "     🔐 Auth initialized on app startup" -ForegroundColor Gray
Write-Host "     🔐 Initializing auth..." -ForegroundColor Gray
Write-Host "     ⚠️ No valid token found, user needs to login" -ForegroundColor Gray
Write-Host "     🔍 Menu rendering - User: undefined Is Admin: false" -ForegroundColor Gray
Write-Host "     ⚠️ Management menu hidden (not admin)" -ForegroundColor Gray
Write-Host ""
Write-Host "  ✅ Expected Menu:" -ForegroundColor Green
Write-Host "     • Home" -ForegroundColor White
Write-Host "     • Vocabulary Learning" -ForegroundColor White
Write-Host "     • (KHÔNG có 'Quản lý')" -ForegroundColor Red
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "TEST 2: Login với Admin account" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1. Click 'Login' hoặc truy cập /auth/login" -ForegroundColor White
Write-Host "  2. Login với:" -ForegroundColor White
Write-Host "     Email: devphongvv198@gmail.com" -ForegroundColor Cyan
Write-Host "     Password: P@ssword123" -ForegroundColor Cyan
Write-Host ""
Write-Host "  ✅ Expected Console Logs:" -ForegroundColor Green
Write-Host "     🔐 Logging in..." -ForegroundColor Gray
Write-Host "     ✅ Login API successful, saving tokens..." -ForegroundColor Gray
Write-Host "     📡 Loading user profile after login..." -ForegroundColor Gray
Write-Host "     ✅ User profile loaded: {roles: ['Administrator']}" -ForegroundColor Gray
Write-Host "     🔍 Menu rendering - User: devphongvv198@gmail.com Is Admin: true" -ForegroundColor Gray
Write-Host "     ✅ Adding Management menu" -ForegroundColor Gray
Write-Host ""
Write-Host "  ✅ Expected Menu:" -ForegroundColor Green
Write-Host "     • Home" -ForegroundColor White
Write-Host "     • Vocabulary Learning" -ForegroundColor White
Write-Host "     • Quản lý ✅ (PHẢI HIỆN)" -ForegroundColor Green
Write-Host "       - Quản lý Danh mục" -ForegroundColor White
Write-Host "       - Quản lý Tài khoản" -ForegroundColor White
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "TEST 3: Logout" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1. Click vào user menu (góc phải)" -ForegroundColor White
Write-Host "  2. Click 'Logout'" -ForegroundColor White
Write-Host ""
Write-Host "  ✅ Expected Console Logs:" -ForegroundColor Green
Write-Host "     🔓 Logging out..." -ForegroundColor Gray
Write-Host "     ✅ Logged out successfully (client-side)" -ForegroundColor Gray
Write-Host "     ✅ Auth cleared from both useAuth and authStore" -ForegroundColor Gray
Write-Host "     🔍 Menu rendering - User: undefined Is Admin: false" -ForegroundColor Gray
Write-Host "     ⚠️ Management menu hidden (not admin)" -ForegroundColor Gray
Write-Host ""
Write-Host "  ✅ Expected Behavior:" -ForegroundColor Green
Write-Host "     • Redirect to /auth/login" -ForegroundColor White
Write-Host "     • Menu 'Quản lý' BIẾN MẤT" -ForegroundColor Red
Write-Host "     • Hiện nút Login/Register" -ForegroundColor White
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "TEST 4: Login lại (kiểm tra menu hiện lại)" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1. Login lại với devphongvv198@gmail.com" -ForegroundColor White
Write-Host ""
Write-Host "  ✅ Expected Behavior:" -ForegroundColor Green
Write-Host "     • Menu 'Quản lý' HIỆN LẠI" -ForegroundColor Green
Write-Host "     • Console log: '✅ Adding Management menu'" -ForegroundColor Green
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "TEST 5: Refresh page (kiểm tra menu persist)" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1. Sau khi login, nhấn F5 (refresh page)" -ForegroundColor White
Write-Host ""
Write-Host "  ✅ Expected Console Logs:" -ForegroundColor Green
Write-Host "     🔐 Auth initialized on app startup" -ForegroundColor Gray
Write-Host "     🔐 Initializing auth..." -ForegroundColor Gray
Write-Host "     ✅ Valid token found, loading user profile..." -ForegroundColor Gray
Write-Host "     ✅ User profile loaded: {roles: ['Administrator']}" -ForegroundColor Gray
Write-Host "     🔍 Menu rendering - User: devphongvv198@gmail.com Is Admin: true" -ForegroundColor Gray
Write-Host "     ✅ Adding Management menu" -ForegroundColor Gray
Write-Host ""
Write-Host "  ✅ Expected Behavior:" -ForegroundColor Green
Write-Host "     • Vẫn giữ login" -ForegroundColor White
Write-Host "     • Menu 'Quản lý' vẫn hiển thị" -ForegroundColor White
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "TROUBLESHOOTING" -ForegroundColor Red
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "❌ Problem: Menu không cập nhật sau login/logout" -ForegroundColor Red
Write-Host "   Console log shows:" -ForegroundColor Yellow
Write-Host "   - Login successful nhưng menu không hiện 'Quản lý'" -ForegroundColor Gray
Write-Host ""
Write-Host "   Debug steps:" -ForegroundColor Yellow
Write-Host "   1. Kiểm tra console có log '🔍 Menu rendering' không?" -ForegroundColor White
Write-Host "      → Nếu không → computed() không reactive" -ForegroundColor Gray
Write-Host "   2. Kiểm tra 'Is Admin: true' hay 'false'?" -ForegroundColor White
Write-Host "      → Nếu false nhưng có roles → hasRole() bị lỗi" -ForegroundColor Gray
Write-Host "   3. Hard refresh (Ctrl+Shift+R)" -ForegroundColor White
Write-Host ""

Write-Host "❌ Problem: Logout không ẩn menu" -ForegroundColor Red
Write-Host "   Menu 'Quản lý' vẫn hiện sau logout" -ForegroundColor Yellow
Write-Host ""
Write-Host "   Debug steps:" -ForegroundColor Yellow
Write-Host "   1. Kiểm tra console có log '✅ Auth cleared from both'" -ForegroundColor White
Write-Host "   2. Kiểm tra Application tab → localStorage" -ForegroundColor White
Write-Host "      → Phải RỖNG (không có access_token)" -ForegroundColor Gray
Write-Host "   3. Hard refresh (Ctrl+Shift+R)" -ForegroundColor White
Write-Host ""

Write-Host "❌ Problem: Refresh page mất menu admin" -ForegroundColor Red
Write-Host "   Sau F5, menu 'Quản lý' biến mất" -ForegroundColor Yellow
Write-Host ""
Write-Host "   Debug steps:" -ForegroundColor Yellow
Write-Host "   1. Kiểm tra console log 'Valid token found'" -ForegroundColor White
Write-Host "   2. Kiểm tra 'User profile loaded' có roles không?" -ForegroundColor White
Write-Host "   3. Nếu không load user profile → initializeAuth() bị lỗi" -ForegroundColor White
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "SUMMARY - Expected Flow" -ForegroundColor Green -BackgroundColor Black
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Page load (chưa login):" -ForegroundColor Yellow
Write-Host "   → Menu: Home, Vocabulary (KHÔNG có Quản lý)" -ForegroundColor White
Write-Host ""
Write-Host "2. Login với admin:" -ForegroundColor Yellow
Write-Host "   → Menu: Home, Vocabulary, Quản lý ✅" -ForegroundColor White
Write-Host ""
Write-Host "3. Logout:" -ForegroundColor Yellow
Write-Host "   → Menu: Home, Vocabulary (Quản lý biến mất)" -ForegroundColor White
Write-Host ""
Write-Host "4. Login lại:" -ForegroundColor Yellow
Write-Host "   → Menu: Home, Vocabulary, Quản lý ✅ (hiện lại)" -ForegroundColor White
Write-Host ""
Write-Host "5. Refresh page:" -ForegroundColor Yellow
Write-Host "   → Menu: Home, Vocabulary, Quản lý ✅ (vẫn giữ)" -ForegroundColor White
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Nếu tất cả test pass → ✅ Feature hoạt động hoàn hảo!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
