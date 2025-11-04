#!/usr/bin/env pwsh

Write-Host "🔧 Quick Fix: Create User Role" -ForegroundColor Cyan
Write-Host "===============================" -ForegroundColor Cyan
Write-Host ""

Write-Host "📝 Instructions:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Run the SQL script to create User role in database:" -ForegroundColor White
Write-Host "   - Open SQL Server Management Studio (SSMS)" -ForegroundColor Gray
Write-Host "   - Connect to your database server" -ForegroundColor Gray
Write-Host "   - Open file: create-user-role.sql" -ForegroundColor Gray
Write-Host "   - Execute the script" -ForegroundColor Gray
Write-Host ""

Write-Host "2. Restart the backend application:" -ForegroundColor White
Write-Host "   - The ApplicationDbContextInitialiser will ensure both roles exist" -ForegroundColor Gray
Write-Host "   - Administrator role" -ForegroundColor Cyan
Write-Host "   - User role" -ForegroundColor Cyan
Write-Host ""

Write-Host "3. Test the update roles functionality:" -ForegroundColor White
Write-Host "   - Go to http://localhost:3000/management/users" -ForegroundColor Gray
Write-Host "   - Click 'Phân quyền' on any user" -ForegroundColor Gray
Write-Host "   - Select 'User' role" -ForegroundColor Gray
Write-Host "   - Click 'Lưu thay đổi'" -ForegroundColor Gray
Write-Host ""

Write-Host "🎯 What was fixed:" -ForegroundColor Green
Write-Host ""
Write-Host "✅ Added Roles.User constant to Domain/Constants/Roles.cs" -ForegroundColor Green
Write-Host "✅ Updated GetAvailableRolesQuery to use Roles.User constant" -ForegroundColor Green
Write-Host "✅ Updated ApplicationDbContextInitialiser to seed User role" -ForegroundColor Green
Write-Host "✅ Created create-user-role.sql for manual database setup" -ForegroundColor Green
Write-Host ""

Write-Host "🐛 Root Cause:" -ForegroundColor Yellow
Write-Host "The error 'Role USER does not exist' occurred because:" -ForegroundColor White
Write-Host "- ASP.NET Identity uses NormalizedName (uppercase) to find roles" -ForegroundColor Gray
Write-Host "- Frontend sent role 'User' → Backend looked for 'USER' in NormalizedName" -ForegroundColor Gray
Write-Host "- Only 'Administrator' role existed in database" -ForegroundColor Gray
Write-Host "- Solution: Create 'User' role with NormalizedName='USER'" -ForegroundColor Gray
Write-Host ""

Write-Host "📋 Available Roles After Fix:" -ForegroundColor Cyan
Write-Host "- Administrator (full access)" -ForegroundColor White
Write-Host "- User (standard user)" -ForegroundColor White
Write-Host ""

Write-Host "💡 Next time you rebuild the project, roles will be created automatically!" -ForegroundColor Green
Write-Host ""

# Ask if user wants to run the SQL script
$runSql = Read-Host "Do you want to open the SQL script now? (y/n)"
if ($runSql -eq 'y' -or $runSql -eq 'Y') {
    $sqlFile = "c:\Users\ACER\source\repos\RoomEnglish\src\create-user-role.sql"
    if (Test-Path $sqlFile) {
        Start-Process $sqlFile
        Write-Host "✅ Opened create-user-role.sql" -ForegroundColor Green
    } else {
        Write-Host "❌ SQL file not found at: $sqlFile" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Done! 🎉" -ForegroundColor Green
