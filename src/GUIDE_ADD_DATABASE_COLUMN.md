# 📖 Hướng dẫn thêm Column vào Database

## 🎯 Tổng quan

Hướng dẫn chi tiết các bước thêm column vào bảng database trong RoomEnglish project (Clean Architecture + EF Core).

---

## ⚡ Quick Reference - Các lệnh cần chạy

```powershell
# 1. Tạo Migration (từ thư mục src/)
dotnet ef migrations add AddVietnameseMeaningToVocabularyWord `
  --project Infrastructure/Infrastructure.csproj `
  --startup-project Web/Web.csproj

# 2. Build project
dotnet build

# 3. Nếu build lỗi Static Assets - Sử dụng script tự động
.\clean-build.ps1

# HOẶC xóa thủ công:
cd Web/ClientApp
Remove-Item -Recurse -Force dist,public,.vite -ErrorAction SilentlyContinue
cd ..
Remove-Item -Recurse -Force wwwroot,obj,bin -ErrorAction SilentlyContinue
cd ..
dotnet clean
dotnet build

# 4. Apply Migration vào Database
dotnet ef database update `
  --project Infrastructure/Infrastructure.csproj `
  --startup-project Web/Web.csproj

# 5. Verify trong database
# Mở SQL Server Management Studio / Azure Data Studio
# Kiểm tra column mới đã xuất hiện

# 6. Test Backend API
cd Web
dotnet run
# Mở https://localhost:5001/swagger

# 7. Test Frontend
cd Web/ClientApp
npm run dev
# Mở http://localhost:5173
```

### **Rollback nếu cần:**

```powershell
# Remove migration chưa apply
dotnet ef migrations remove `
  --project Infrastructure/Infrastructure.csproj `
  --startup-project Web/Web.csproj

# Rollback database về migration trước
dotnet ef database update [PreviousMigrationName] `
  --project Infrastructure/Infrastructure.csproj `
  --startup-project Web/Web.csproj
```

---

## 📋 Các bước thực hiện

### **Bước 1: Cập nhật Domain Entity**

**File**: `Domain/Entities/[EntityName].cs`

Thêm property mới vào entity class:

```csharp
// Example: VocabularyWord.cs
public class VocabularyWord : BaseAuditableEntity
{
    public string Word { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    
    // ✅ Thêm property mới
    public string VietnameseMeaning { get; set; } = string.Empty;
    
    // ...existing properties...
}
```

**Lưu ý:**
- Đặt tên property theo PascalCase (C# convention)
- Thêm default value hoặc đánh dấu nullable (`?`) nếu cần
- Thêm comment để giải thích mục đích

---

### **Bước 2: Tạo Migration**

#### **2.1. Chạy lệnh tạo migration**

Từ thư mục `src/`:

```powershell
dotnet ef migrations add [MigrationName] `
  --project Infrastructure/Infrastructure.csproj `
  --startup-project Web/Web.csproj
```

**Example:**
```powershell
dotnet ef migrations add AddVietnameseMeaningToVocabularyWord `
  --project Infrastructure/Infrastructure.csproj `
  --startup-project Web/Web.csproj
```

**Lưu ý đặt tên migration:**
- Format: `Add[ColumnName]To[TableName]`
- Hoặc: `Update[TableName]Add[ColumnName]`
- Dễ hiểu, mô tả đúng thay đổi

#### **2.2. Kiểm tra migration file**

Migration được tạo tại: `Infrastructure/Migrations/[Timestamp]_[MigrationName].cs`

```csharp
public partial class AddVietnameseMeaningToVocabularyWord : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "VietnameseMeaning",
            table: "VocabularyWords",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "VietnameseMeaning",
            table: "VocabularyWords");
    }
}
```

**Kiểm tra:**
- ✅ Tên column đúng
- ✅ Tên table đúng
- ✅ Data type phù hợp
- ✅ Nullable/Not Null hợp lý
- ✅ Default value nếu cần

---

### **Bước 3: Build Project**

```powershell
dotnet build
```

**Nếu build thất bại** → Xem [Phần xử lý lỗi](#-xử-lý-lỗi-thường-gặp)

---

### **Bước 4: Apply Migration vào Database**

```powershell
dotnet ef database update `
  --project Infrastructure/Infrastructure.csproj `
  --startup-project Web/Web.csproj
```

**Output mong đợi:**
```
Build started...
Build succeeded.
Done.
```

**Kiểm tra database:**
- Mở SQL Server Management Studio / Azure Data Studio
- Refresh bảng và kiểm tra column mới đã xuất hiện

---

### **Bước 5: Cập nhật Frontend Types**

**File**: `Web/ClientApp/src/modules/[module]/types/[module].types.ts`

```typescript
// Example: vocabulary.types.ts
export interface VocabularyWord {
  id: number
  word: string
  definition: string
  vietnameseMeaning: string  // ✅ Thêm property mới (camelCase)
  // ...existing properties...
}
```

**Lưu ý:**
- Frontend dùng **camelCase** (TypeScript/JavaScript convention)
- Backend dùng **PascalCase** (C# convention)
- JSON serializer tự động convert

---

### **Bước 6: Cập nhật UI Components**

#### **6.1. Hiển thị trong Card/Detail View**

```vue
<!-- VocabularyCard.vue -->
<template>
  <div class="definition-section">
    <div class="definition-card">
      <h3>Definition (English)</h3>
      <p>{{ word.definition }}</p>
    </div>
    
    <!-- ✅ Thêm section mới -->
    <div class="definition-card vietnamese-meaning" v-if="word.vietnameseMeaning">
      <h3>Nghĩa tiếng Việt</h3>
      <p>{{ word.vietnameseMeaning }}</p>
    </div>
  </div>
</template>
```

#### **6.2. Thêm vào DataGrid**

```typescript
// VocabularyDataGrid.vue
const columns = computed<GridColumn[]>(() => [
  {
    key: 'word',
    label: 'Từ vựng',
    sortable: true,
    type: 'text'
  },
  {
    key: 'definition',
    label: 'Định nghĩa (EN)',
    sortable: false,
    type: 'text'
  },
  // ✅ Thêm column mới
  {
    key: 'vietnameseMeaning',
    label: 'Nghĩa (VI)',
    sortable: false,
    type: 'text'
  },
  // ...existing columns...
])
```

#### **6.3. Thêm vào Form (Create/Edit)**

```vue
<!-- VocabularyForm.vue -->
<div class="form-group">
  <label for="vietnameseMeaning">Nghĩa tiếng Việt</label>
  <textarea
    id="vietnameseMeaning"
    v-model="form.vietnameseMeaning"
    rows="3"
    placeholder="Nhập nghĩa tiếng Việt của definition..."
  />
</div>
```

---

### **Bước 7: Test chức năng**

#### **7.1. Test Backend API**

```bash
# Swagger UI
https://localhost:5001/swagger

# Test GET endpoint
GET /api/vocabulary/words/{id}

# Kiểm tra response có vietnameseMeaning
{
  "id": 1,
  "word": "example",
  "definition": "A thing characteristic of its kind...",
  "vietnameseMeaning": "Ví dụ, mẫu",  // ✅ Column mới
  ...
}
```

#### **7.2. Test Frontend**

1. Chạy frontend: `cd Web/ClientApp && npm run dev`
2. Mở http://localhost:5173
3. Test các trang:
   - ✅ Danh sách từ vựng (DataGrid)
   - ✅ Chi tiết từ vựng (Card)
   - ✅ Form tạo/sửa từ vựng
   - ✅ Learning session

---

## 🐛 Xử lý lỗi thường gặp

### **Lỗi 1: Build failed - Static Web Assets Error** ⚠️ **PHỔ BIẾN**

```
error MSB4018: The "DefineStaticWebAssets" task failed unexpectedly.
System.ArgumentException: An item with the same key has already been added.
Key: znxnUBlQv6Bvr3kuw2SzOCXe...
```

**❗ Đây là lỗi phổ biến nhất và có thể tái diễn nhiều lần!**

**Nguyên nhân:** 
- Folder `ClientApp/public` có file trùng lặp từ build trước
- Cache `.vite` hoặc `dist` bị corrupt
- `wwwroot` có file cũ conflict với file mới  
- Folder `obj/bin` chứa metadata lỗi thời

**Tại sao lỗi này lại xảy ra?**

.NET SDK sử dụng **Static Web Assets** để tích hợp SPA (Vue/React):
1. Build scan tất cả files trong `ClientApp/dist`, `public`, `vendor`
2. Tạo hash (SHA256) cho mỗi file để track changes
3. Lưu vào dictionary với key là hash
4. Nếu 2 files khác nhau có cùng hash → **DUPLICATE KEY ERROR**

Nguyên nhân thường gặp:
- Build nhiều lần → `public` folder tích tụ file duplicate
- `dist` có file cũ không được xóa sạch
- `.vite` cache corrupt sau crash/force stop
- Copy-paste files giữa các thư mục

**✅ Giải pháp TỐT NHẤT - Sử dụng script tự động:**

```powershell
# Từ thư mục src/
.\clean-build.ps1

# Script này sẽ:
# - Xóa ClientApp/dist, public, .vite
# - Xóa Web/wwwroot, obj, bin
# - Xóa tất cả obj/bin trong solution
# - Chạy dotnet clean && build
```

**Giải pháp thủ công (nếu chưa có script):**

```powershell
# Bước 1: Xóa cache ClientApp
cd Web/ClientApp
Remove-Item -Recurse -Force dist,public,.vite,node_modules/.vite -ErrorAction SilentlyContinue

# Bước 2: Xóa cache Web project  
cd ..
Remove-Item -Recurse -Force wwwroot,obj,bin -ErrorAction SilentlyContinue

# Bước 3: Xóa tất cả obj/bin trong solution
cd ..
Get-ChildItem -Path . -Include obj,bin -Recurse -Directory | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

# Bước 4: Clean và build lại
dotnet clean
dotnet build
```

**🛡️ Giải pháp PHÒNG NGỪA (làm 1 lần để tránh tái diễn):**

1. **Cập nhật .gitignore** để không commit các folder gây lỗi:

```gitignore
# Web/ClientApp/.gitignore (đã update tự động)
dist
public        # ✅ Bảo vệ không commit folder này
.vite         # ✅ Cache của Vite
node_modules

# Web/.gitignore (đã tạo tự động)
bin/
obj/
wwwroot/      # ✅ Build output không được commit
*.StaticWebAssets.xml
```

2. **Reinstall node_modules nếu lỗi vẫn tái diễn:**

```powershell
cd Web/ClientApp
Remove-Item -Recurse -Force node_modules
npm install
cd ../..
dotnet build
```

3. **Clear NuGet cache (trường hợp đặc biệt):**

```powershell
# Xóa tất cả NuGet cache
dotnet nuget locals all --clear

# Restore lại packages
dotnet restore
dotnet build
```

---

### **Lỗi 2: Migration already exists**

```
The migration '[MigrationName]' has already been applied to the database.
```

**Giải pháp 1: Remove migration chưa apply**

```powershell
dotnet ef migrations remove `
  --project Infrastructure/Infrastructure.csproj `
  --startup-project Web/Web.csproj
```

**Giải pháp 2: Rollback database**

```powershell
# Rollback về migration trước đó
dotnet ef database update [PreviousMigrationName] `
  --project Infrastructure/Infrastructure.csproj `
  --startup-project Web/Web.csproj

# Sau đó remove migration
dotnet ef migrations remove `
  --project Infrastructure/Infrastructure.csproj `
  --startup-project Web/Web.csproj
```

---

### **Lỗi 3: Column already exists**

```
There is already an object named 'ColumnName' in the database.
```

**Nguyên nhân:** Migration đã chạy nhưng code không đồng bộ

**Giải pháp:**

```powershell
# Option 1: Skip migration này
dotnet ef migrations remove `
  --project Infrastructure/Infrastructure.csproj `
  --startup-project Web/Web.csproj

# Option 2: Sửa migration file (xóa phần AddColumn)
# Mở: Infrastructure/Migrations/[Timestamp]_[Name].cs
# Comment hoặc xóa migrationBuilder.AddColumn(...)
```

---

### **Lỗi 4: Cannot find path**

```
Cannot find path 'C:\...\Infrastructure' because it does not exist.
```

**Nguyên nhân:** Đang ở sai thư mục

**Giải pháp:**

```powershell
# Luôn chạy từ thư mục src/
cd C:\Users\ACER\source\repos\RoomEnglish\src

# Hoặc dùng đường dẫn tuyệt đối
dotnet ef migrations add [Name] `
  --project C:\...\Infrastructure\Infrastructure.csproj `
  --startup-project C:\...\Web\Web.csproj
```

---

### **Lỗi 5: Data type mismatch**

```
Conversion failed when converting the nvarchar value 'xxx' to data type int.
```

**Nguyên nhân:** Data type không khớp với dữ liệu hiện có

**Giải pháp:**

```csharp
// Trong migration file, thêm data conversion
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Option 1: Cho phép NULL trước
    migrationBuilder.AddColumn<string>(
        name: "VietnameseMeaning",
        table: "VocabularyWords",
        type: "nvarchar(max)",
        nullable: true);  // ✅ Cho phép NULL
    
    // Option 2: Set default value
    migrationBuilder.AddColumn<string>(
        name: "VietnameseMeaning",
        table: "VocabularyWords",
        type: "nvarchar(max)",
        nullable: false,
        defaultValue: "");  // ✅ Default empty string
    
    // Option 3: Update existing data trước
    migrationBuilder.Sql(
        "UPDATE VocabularyWords SET VietnameseMeaning = 'N/A' WHERE VietnameseMeaning IS NULL");
}
```

---

### **Lỗi 6: TypeScript compile error**

```
Property 'vietnameseMeaning' does not exist on type 'VocabularyWord'.
```

**Nguyên nhân:** Chưa cập nhật TypeScript interface

**Giải pháp:**

```typescript
// 1. Cập nhật interface
export interface VocabularyWord {
  // ...existing properties...
  vietnameseMeaning: string  // ✅ Thêm property
}

// 2. Restart TypeScript server
// VS Code: Ctrl+Shift+P → "TypeScript: Restart TS Server"

// 3. Clear npm cache nếu cần
cd Web/ClientApp
Remove-Item -Recurse -Force node_modules/.vite
npm run dev
```

---

## ✅ Checklist hoàn chỉnh

- [ ] **Domain Entity** - Thêm property vào entity class
- [ ] **Migration** - Tạo và kiểm tra migration file
- [ ] **Build** - Build thành công không lỗi
- [ ] **Database** - Apply migration thành công
- [ ] **TypeScript Types** - Cập nhật interface
- [ ] **UI Components** - Cập nhật Card/Grid/Form
- [ ] **API Test** - Test endpoints với Swagger
- [ ] **Frontend Test** - Test trên UI
- [ ] **Git Commit** - Commit với message rõ ràng

---

## 📝 Best Practices

### **1. Naming Conventions**

```
✅ GOOD:
- Property: VietnameseMeaning (PascalCase - C#)
- Property: vietnameseMeaning (camelCase - TypeScript)
- Migration: AddVietnameseMeaningToVocabularyWord
- Column: VietnameseMeaning (database)

❌ BAD:
- vietnameseMeaning (C#)
- VietnameseMeaning (TypeScript)
- add_column_meaning
```

### **2. Default Values**

```csharp
// ✅ Cho existing data
public string VietnameseMeaning { get; set; } = string.Empty;

// ✅ Nullable nếu không bắt buộc
public string? VietnameseMeaning { get; set; }

// ❌ Không có default và not null (lỗi với existing data)
public string VietnameseMeaning { get; set; }
```

### **3. Migration Testing**

```powershell
# Test trên local database trước
dotnet ef database update

# Nếu OK, mới commit migration files
git add Infrastructure/Migrations/*
git commit -m "feat: Add VietnameseMeaning column to VocabularyWords"
```

### **4. Rollback Plan**

```powershell
# Luôn biết cách rollback
dotnet ef database update [PreviousMigration]

# Hoặc tạo script rollback
dotnet ef migrations script [CurrentMigration] [PreviousMigration] `
  --output rollback.sql
```

---

## 🚀 Automation Script (Optional)

Tạo file `add-column.ps1` để tự động hóa:

```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$EntityName,
    
    [Parameter(Mandatory=$true)]
    [string]$PropertyName,
    
    [Parameter(Mandatory=$true)]
    [string]$PropertyType
)

$MigrationName = "Add$($PropertyName)To$($EntityName)"

Write-Host "🚀 Adding column $PropertyName to $EntityName..." -ForegroundColor Cyan

# Step 1: Create migration
Write-Host "📝 Creating migration..." -ForegroundColor Yellow
dotnet ef migrations add $MigrationName `
  --project Infrastructure/Infrastructure.csproj `
  --startup-project Web/Web.csproj

# Step 2: Build
Write-Host "🔨 Building project..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed! Cleaning cache..." -ForegroundColor Red
    
    cd Web/ClientApp
    Remove-Item -Recurse -Force dist -ErrorAction SilentlyContinue
    cd ../..
    Remove-Item -Recurse -Force Web/wwwroot -ErrorAction SilentlyContinue
    
    dotnet clean
    dotnet build
}

# Step 3: Apply migration
Write-Host "💾 Applying migration to database..." -ForegroundColor Yellow
dotnet ef database update `
  --project Infrastructure/Infrastructure.csproj `
  --startup-project Web/Web.csproj

Write-Host "✅ Done! Don't forget to update TypeScript types and UI!" -ForegroundColor Green
```

**Sử dụng:**
```powershell
.\add-column.ps1 -EntityName "VocabularyWord" -PropertyName "VietnameseMeaning" -PropertyType "string"
```

---

## 📚 Tài liệu tham khảo

- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Clean Architecture Guide](https://github.com/jasontaylordev/CleanArchitecture)
- [TypeScript Best Practices](https://www.typescriptlang.org/docs/handbook/declaration-files/do-s-and-don-ts.html)

---

**Last Updated:** October 22, 2025  
**Version:** 1.0.0
