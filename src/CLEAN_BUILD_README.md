# 🧹 Clean Build Script

## Tổng quan

Script PowerShell tự động clean và build lại project để fix lỗi **Static Web Assets duplicate key** - lỗi phổ biến nhất khi develop RoomEnglish.

## Tại sao cần script này?

Khi develop với ASP.NET Core + SPA (Vue), bạn sẽ thường xuyên gặp lỗi:

```
error MSB4018: The "DefineStaticWebAssets" task failed unexpectedly.
System.ArgumentException: An item with the same key has already been added.
```

**Nguyên nhân:**
- Folder `ClientApp/public` tích tụ file duplicate từ nhiều lần build
- Cache `.vite` bị corrupt sau crash/force stop
- `wwwroot` chứa static assets cũ conflict với file mới
- `obj/bin` có metadata lỗi thời

**Giải pháp thủ công mất thời gian:**
- Phải nhớ xóa 5-7 folders khác nhau
- Phải chạy 10+ câu lệnh PowerShell
- Dễ quên bước → lỗi vẫn tái diễn

**Script này tự động hóa toàn bộ quá trình!**

## 🚀 Cách sử dụng

### Sử dụng cơ bản

```powershell
# Từ thư mục src/
.\clean-build.ps1
```

Script sẽ:
1. ✅ Xóa `ClientApp/dist`, `public`, `.vite`, `node_modules/.vite`
2. ✅ Xóa `Web/wwwroot`, `obj`, `bin`
3. ✅ Xóa tất cả `obj/bin` trong solution
4. ✅ Chạy `dotnet clean`
5. ✅ Chạy `dotnet build`

### Chỉ clean không build

```powershell
.\clean-build.ps1 -SkipBuild
```

Hữu ích khi:
- Chỉ muốn xóa cache
- Sẽ build từ Visual Studio/Rider
- Muốn chạy custom build command

### Xem chi tiết quá trình

```powershell
.\clean-build.ps1 -Verbose
```

Hiển thị từng file/folder đang được xóa.

## 📋 Checklist trước khi chạy

- [ ] Đóng Visual Studio / Rider (tránh file lock)
- [ ] Commit code quan trọng (script xóa build outputs)
- [ ] Đang ở thư mục `src/`

## ⚠️ Lưu ý quan trọng

### An toàn
- ✅ Script CHỈ xóa build artifacts (`dist`, `obj`, `bin`, `wwwroot`)
- ✅ KHÔNG xóa source code (`src/`, `*.cs`, `*.ts`, `*.vue`)
- ✅ KHÔNG xóa config files (`appsettings.json`, `package.json`)
- ✅ KHÔNG xóa `node_modules` (chỉ xóa `.vite` cache)

### Khi nào cần reinstall node_modules

Nếu sau khi chạy script, lỗi vẫn tái diễn:

```powershell
# Xóa toàn bộ node_modules
cd Web/ClientApp
Remove-Item -Recurse -Force node_modules

# Reinstall
npm install

# Build lại
cd ../..
dotnet build
```

### Khi nào cần clear NuGet cache

Trường hợp đặc biệt (rất hiếm):

```powershell
# Clear tất cả NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# Chạy clean build
.\clean-build.ps1
```

## 🔧 Troubleshooting

### Script không chạy được

```powershell
# Kiểm tra ExecutionPolicy
Get-ExecutionPolicy

# Nếu là "Restricted", cho phép chạy script:
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### File bị lock

```
Remove-Item: Access to the path '...' is denied.
```

**Giải pháp:**
1. Đóng Visual Studio / Rider
2. Đóng terminal đang chạy `dotnet run` / `npm run dev`
3. Chạy lại script

### Build vẫn lỗi sau khi clean

**Thử các bước sau theo thứ tự:**

1. **Reinstall node_modules:**
```powershell
cd Web/ClientApp
Remove-Item -Recurse -Force node_modules
npm install
cd ../..
.\clean-build.ps1
```

2. **Clear NuGet cache:**
```powershell
dotnet nuget locals all --clear
dotnet restore
.\clean-build.ps1
```

3. **Restart máy tính** (cache OS level)

4. **Check git status:**
```powershell
git status

# Nếu có file tracked trong wwwroot/dist:
git rm -r --cached Web/wwwroot
git rm -r --cached Web/ClientApp/dist
git rm -r --cached Web/ClientApp/public
```

## 📚 Tài liệu liên quan

- **GUIDE_ADD_DATABASE_COLUMN.md** - Hướng dẫn chi tiết thêm column
- **README.md** - Tổng quan project
- **.gitignore** - Danh sách folders được ignore (đã update)

## 💡 Tips

### Tích hợp vào workflow

**Khi bắt đầu coding session:**
```powershell
.\clean-build.ps1
```

**Khi switch branch:**
```powershell
git checkout [branch]
.\clean-build.ps1
```

**Khi pull code mới:**
```powershell
git pull
.\clean-build.ps1
```

**Trước khi commit:**
```powershell
.\clean-build.ps1
# Đảm bảo build thành công trước khi commit
git add .
git commit -m "feat: ..."
```

### Alias PowerShell (optional)

Thêm vào PowerShell profile để gọi nhanh:

```powershell
# Mở profile
notepad $PROFILE

# Thêm dòng:
function Clean-RoomEnglish {
    Set-Location "C:\Users\ACER\source\repos\RoomEnglish\src"
    .\clean-build.ps1 $args
}
Set-Alias cb Clean-RoomEnglish

# Bây giờ có thể gọi từ bất kỳ đâu:
cb
cb -SkipBuild
cb -Verbose
```

## 🎯 Summary

| Tình huống | Command |
|-----------|---------|
| Build lỗi Static Web Assets | `.\clean-build.ps1` |
| Chỉ muốn clean cache | `.\clean-build.ps1 -SkipBuild` |
| Xem chi tiết quá trình | `.\clean-build.ps1 -Verbose` |
| Sau khi switch branch | `.\clean-build.ps1` |
| Sau khi pull code mới | `.\clean-build.ps1` |
| Lỗi vẫn tái diễn | Xóa `node_modules` + reinstall |
| Lỗi rất hiếm gặp | Clear NuGet cache |

---

**🔗 Quick Links:**
- [Main README](./README.md)
- [Database Column Guide](./GUIDE_ADD_DATABASE_COLUMN.md)
- [Frontend Mapping](./Web/ClientApp/FRONTEND_MAPPING.md)
