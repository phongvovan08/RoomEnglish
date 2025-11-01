# Hướng Dẫn Đầy Đủ Setup CI/CD Deployment cho RoomEnglish

## 📋 Tổng Quan

Project RoomEnglish sử dụng **GitHub Actions** để tự động build và deploy lên **Azure App Service** với **OIDC authentication** (không cần lưu password).

## 🎯 Yêu Cầu

- **Azure Subscription ID**: `6d6d5629-96f5-4bb9-8b30-1004108e6a99`
- **Azure App Service**: `WebRoomEnglish`
- **Resource Group**: `ResourceGroupPhong`
- **GitHub Repository**: `phongvovan08/RoomEnglish`
- **Azure CLI**: Phiên bản 2.77.0 trở lên
- **Visual C++ Redistributable**: 2015-2022 (cho Azure CLI)

## 📦 Bước 1: Cài Đặt Azure CLI

### 1.1. Cài Visual C++ Redistributable

```powershell
winget install --id Microsoft.VCRedist.2015+.x64
```

### 1.2. Cài Azure CLI

```powershell
winget install -e --id Microsoft.AzureCLI
```

### 1.3. Kiểm Tra Cài Đặt

```powershell
az --version
```

**Kết quả mong đợi:**
```
azure-cli                         2.77.0
```

### ⚠️ Lỗi Thường Gặp

**Lỗi:** `ImportError: DLL load failed while importing win32file`

**Giải pháp:**
```powershell
# Gỡ Azure CLI
winget uninstall Microsoft.AzureCLI

# Cài lại
winget install -e --id Microsoft.AzureCLI

# Kiểm tra lại
az --version
```

## 🔐 Bước 2: Tạo Azure Service Principal và Federated Credentials

### Cách 1: Dùng Script Tự Động ⭐ (Khuyến nghị)

#### 2.1. Chạy Script

```powershell
cd c:\Users\ACER\source\repos\RoomEnglish
.\setup-azure-oidc.ps1
```

#### 2.2. Quá Trình Script

Script sẽ thực hiện:
1. ✅ Đăng nhập Azure (mở trình duyệt để xác thực)
2. ✅ Chọn subscription `Azure subscription Phong`
3. ✅ Tạo App Registration: `WebRoomEnglish-GitHub-Actions`
4. ✅ Tạo Service Principal
5. ✅ Gán quyền **Contributor** cho Resource Group `ResourceGroupPhong`
6. ✅ Tạo Federated Credential cho GitHub Actions
7. ✅ Hiển thị 3 giá trị secret

#### 2.3. Lưu Lại Output

Script sẽ in ra:
```
=== GitHub Secrets ===
AZURE_CLIENT_ID: 5897f206-cb24-4c34-93c0-449a62808335
AZURE_TENANT_ID: 841a5b22-df30-4a41-820d-43eb74198f96
AZURE_SUBSCRIPTION_ID: 6d6d5629-96f5-4bb9-8b30-1004108e6a99
```

**Lưu ý:** Giá trị CLIENT_ID và TENANT_ID của bạn sẽ khác!

### Cách 2: Thủ Công (Nếu Script Lỗi)

#### 2.1. Đăng Nhập Azure

```powershell
az login
az account set --subscription '6d6d5629-96f5-4bb9-8b30-1004108e6a99'
```

#### 2.2. Tạo App Registration

```powershell
$app = az ad app create --display-name 'WebRoomEnglish-GitHub-Actions' --query '{appId:appId, objectId:id}' -o json | ConvertFrom-Json

Write-Host "Application (client) ID: $($app.appId)"
Write-Host "Object ID: $($app.objectId)"
```

#### 2.3. Tạo Service Principal

```powershell
az ad sp create --id $app.appId
```

#### 2.4. Gán Quyền Contributor

```powershell
az role assignment create `
  --assignee $app.appId `
  --role Contributor `
  --scope /subscriptions/6d6d5629-96f5-4bb9-8b30-1004108e6a99/resourceGroups/ResourceGroupPhong
```

#### 2.5. Tạo Federated Credential cho Environment Production

```powershell
az ad app federated-credential create `
  --id $app.appId `
  --parameters '{\"name\":\"github-production\",\"issuer\":\"https://token.actions.githubusercontent.com\",\"subject\":\"repo:phongvovan08/RoomEnglish:environment:Production\",\"audiences\":[\"api://AzureADTokenExchange\"]}'
```

#### 2.6. Lấy Tenant ID và In Kết Quả

```powershell
$tenantId = az account show --query tenantId -o tsv

Write-Host "`n=== GitHub Secrets ===" -ForegroundColor Green
Write-Host "AZURE_CLIENT_ID: $($app.appId)"
Write-Host "AZURE_TENANT_ID: $tenantId"
Write-Host "AZURE_SUBSCRIPTION_ID: 6d6d5629-96f5-4bb9-8b30-1004108e6a99"
```

## 🔑 Bước 3: Thêm GitHub Secrets

### 3.1. Mở Trang GitHub Secrets

Truy cập: https://github.com/phongvovan08/RoomEnglish/settings/secrets/actions

### 3.2. Thêm Secret 1: AZURE_CLIENT_ID

1. Click **"New repository secret"**
2. **Name**: `AZURE_CLIENT_ID`
3. **Secret**: Copy giá trị từ output script (ví dụ: `5897f206-cb24-4c34-93c0-449a62808335`)
4. Click **"Add secret"**

### 3.3. Thêm Secret 2: AZURE_TENANT_ID

1. Click **"New repository secret"**
2. **Name**: `AZURE_TENANT_ID`
3. **Secret**: Copy giá trị từ output script (ví dụ: `841a5b22-df30-4a41-820d-43eb74198f96`)
4. Click **"Add secret"**

### 3.4. Thêm Secret 3: AZURE_SUBSCRIPTION_ID

1. Click **"New repository secret"**
2. **Name**: `AZURE_SUBSCRIPTION_ID`
3. **Secret**: `6d6d5629-96f5-4bb9-8b30-1004108e6a99`
4. Click **"Add secret"**

### 3.5. Kiểm Tra

Sau khi thêm xong, bạn sẽ thấy 3 secrets trong danh sách:
- ✅ AZURE_CLIENT_ID
- ✅ AZURE_TENANT_ID
- ✅ AZURE_SUBSCRIPTION_ID

## 🔄 Bước 4: Hiểu Workflows

### Workflow 1: CI - Build & Test (`ci.yml`)

**📍 File:** `.github/workflows/ci.yml`

**Kích hoạt:**
- Push hoặc Pull Request vào branch `main` hoặc `develop`

**Chức năng:**
1. ✅ Setup .NET 9.0.x
2. ✅ Setup Node.js 20.x
3. ✅ Restore dependencies: `dotnet restore src/src.sln`
4. ✅ Build solution: `dotnet build --configuration Release`
5. ✅ Chạy tests: `dotnet test`
6. ✅ Build Vue.js app: `npm run build` trong ClientApp
7. ✅ Kiểm tra artifacts trong `ClientApp/dist/`

**Mục đích:** Đảm bảo code compile và tests pass trước khi merge

### Workflow 2: Deploy to Azure (`azure-webapps-dotnet.yml`)

**📍 File:** `.github/workflows/azure-webapps-dotnet.yml`

**Kích hoạt:**
- ✅ Push vào branch `main` (tự động)
- ✅ Chạy thủ công từ GitHub Actions UI

**Job 1: Build** (runs-on: ubuntu-latest)

```yaml
Steps:
1. Checkout code
2. Setup .NET 9.0.x
3. Setup Node.js 20.x
4. dotnet restore src/src.sln
5. dotnet build --configuration Release
6. dotnet publish src/Web/Web.csproj --output ./publish
   └─> MSBuild target BuildClientApp tự động:
       - Clean wwwroot
       - npm install trong ClientApp
       - npm run build (tạo dist/)
       - Copy dist/ vào wwwroot
7. Upload artifacts (./publish)
```

**Job 2: Deploy** (runs-on: ubuntu-latest)

```yaml
Environment: Production
Permissions:
  - id-token: write  # Cho OIDC token
  - contents: read   # Đọc code

Steps:
1. Download artifacts từ build job
2. Login to Azure qua OIDC:
   - Dùng secrets: CLIENT_ID, TENANT_ID, SUBSCRIPTION_ID
   - Azure tạo token tạm thời (không cần password)
   - Token có subject: repo:phongvovan08/RoomEnglish:environment:Production
3. Deploy to Azure Web App:
   - App name: WebRoomEnglish
   - Package: ./publish
```

## 🚀 Bước 5: Chạy Deployment

### Cách 1: Tự Động (Khuyến nghị)

Mỗi khi push code lên `main`, workflow tự động chạy:

```powershell
git add .
git commit -m "feat: Add new feature"
git push origin main
```

→ Workflow **Deploy to Azure Web App** tự động chạy

### Cách 2: Thủ Công

1. Vào https://github.com/phongvovan08/RoomEnglish/actions
2. Click workflow **"Deploy to Azure Web App"**
3. Click nút **"Run workflow"** (góc phải)
4. Chọn branch: `main`
5. Click **"Run workflow"**

### 5.3. Theo Dõi Progress

1. Click vào workflow run đang chạy
2. Xem 2 jobs:
   - ✅ **build**: Build và publish app
   - ✅ **deploy**: Deploy lên Azure
3. Click vào từng step để xem logs chi tiết

## ✅ Bước 6: Kiểm Tra Deployment

### 6.1. Kiểm Tra Workflow Logs

Trong GitHub Actions, đảm bảo:
- ✅ Job **build** completed successfully
- ✅ Job **deploy** completed successfully
- ✅ Không có error logs

### 6.2. Test Website

Truy cập: https://webroomenglish-b3e5a8ghf9f2geaa.southeastasia-01.azurewebsites.net

**Kiểm tra:**
- ✅ Trang chủ load được
- ✅ Vue.js app chạy (không bị 404)
- ✅ API hoạt động: `/api/Users/login`

### 6.3. Clear Browser Cache

Nếu thấy code cũ:
- **Chrome/Edge**: `Ctrl + Shift + Delete` → Clear cache → Hard reload `Ctrl + F5`
- **Firefox**: `Ctrl + Shift + Delete` → Clear cache

## 🔧 Bước 7: Cấu Hình Azure App Settings

Sau khi deploy thành công, cần set environment variables:

### 7.1. Vào Azure Portal

1. Truy cập: https://portal.azure.com
2. Tìm **WebRoomEnglish**
3. Click vào App Service

### 7.2. Thêm Application Settings

1. Vào **Configuration** → **Application settings**
2. Click **"+ New application setting"**

**Setting 1: OpenAI API Key**
- Name: `OpenAI__ApiKey`
- Value: `[Your OpenAI API Key]`

**Setting 2: Connection String (nếu cần)**
- Tab **Connection strings**
- Name: `DefaultConnection`
- Value: `[Your SQL Server Connection String]`
- Type: `SQLServer`

3. Click **Save** (trên toolbar)
4. Click **Continue** khi được hỏi restart

### 7.3. Restart App

App sẽ tự động restart sau khi save settings.

## 🧩 Giải Thích Kỹ Thuật

### OIDC Authentication - Tại Sao Dùng?

**❌ Cách cũ: Publish Profile**
- Lưu username/password trong GitHub Secrets
- Password có thể bị lộ
- Phải đổi password định kỳ

**✅ Cách mới: OIDC**
- Không lưu password
- Token tự động hết hạn sau vài phút
- Quản lý quyền tốt hơn qua Azure AD
- An toàn hơn nhiều

### OIDC Flow - Cách Hoạt Động

```
┌─────────────┐          ┌──────────────┐          ┌───────────┐
│   GitHub    │          │    Azure     │          │   Azure   │
│   Actions   │          │      AD      │          │  App Svc  │
└──────┬──────┘          └──────┬───────┘          └─────┬─────┘
       │                        │                        │
       │ 1. Request token       │                        │
       │───────────────────────>│                        │
       │    with subject:       │                        │
       │    environment:        │                        │
       │    Production          │                        │
       │                        │                        │
       │ 2. Validate subject    │                        │
       │    against federated   │                        │
       │    credential          │                        │
       │                        │                        │
       │ 3. Issue temp token    │                        │
       │<───────────────────────│                        │
       │    (valid 1 hour)      │                        │
       │                        │                        │
       │ 4. Deploy with token   │                        │
       │────────────────────────┼───────────────────────>│
       │                        │                        │
       │ 5. Validate token      │                        │
       │<───────────────────────┼────────────────────────│
       │                        │                        │
       │ 6. Deploy success      │                        │
       │<───────────────────────────────────────────────│
```

### Federated Credential - Subject Pattern

**Credential đã tạo:**
```json
{
  "name": "github-production",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:phongvovan08/RoomEnglish:environment:Production",
  "audiences": ["api://AzureADTokenExchange"]
}
```

**Giải thích:**
- `repo:phongvovan08/RoomEnglish` → Chỉ repo này được phép
- `environment:Production` → Chỉ job có `environment: Production`
- Issuer phải là GitHub Actions
- Audience phải là Azure AD Token Exchange

**Tại sao cần 2 credentials?**

| Credential | Subject Pattern | Dùng Khi |
|------------|----------------|----------|
| `github-main` | `ref:refs/heads/main` | Workflow không có `environment:` |
| `github-production` | `environment:Production` | Workflow có `environment: Production` |

Workflow hiện tại dùng `environment: Production` → Cần credential `github-production`

### MSBuild Targets - Tự Động Build Vue.js

**File:** `src/Web/Web.csproj`

```xml
<!-- Chỉ build Vue.js khi publish -->
<Target Name="BuildClientApp" 
        BeforeTargets="ComputeFilesToPublish" 
        Condition="'$(PublishDir)' != ''">
  <Message Text="Building Vue.js ClientApp..." Importance="high" />
  
  <!-- 1. Clean wwwroot -->
  <RemoveDir Directories="$(MSBuildProjectDirectory)\wwwroot" />
  
  <!-- 2. npm install -->
  <Exec WorkingDirectory="$(MSBuildProjectDirectory)\ClientApp" 
        Command="npm install" />
  
  <!-- 3. npm run build -->
  <Exec WorkingDirectory="$(MSBuildProjectDirectory)\ClientApp" 
        Command="npm run build" />
  
  <!-- 4. Copy dist to wwwroot -->
  <ItemGroup>
    <DistFiles Include="$(MSBuildProjectDirectory)\ClientApp\dist\**\*" />
  </ItemGroup>
  <Copy SourceFiles="@(DistFiles)" 
        DestinationFolder="$(MSBuildProjectDirectory)\wwwroot\%(RecursiveDir)" />
</Target>
```

**Kết quả:**
- Chạy `dotnet publish` → Tự động build Vue.js
- Không cần chạy `npm run build` thủ công
- Đảm bảo Vue.js luôn mới nhất khi deploy

## 🐛 Troubleshooting

### Lỗi 1: "No matching federated identity record found"

**Log:**
```
Error: AADSTS700213: No matching federated identity record found 
for presented assertion subject 'repo:phongvovan08/RoomEnglish:environment:Production'
```

**Nguyên nhân:** Chưa tạo federated credential cho environment Production

**Giải pháp:**
```powershell
az ad app federated-credential create `
  --id 5897f206-cb24-4c34-93c0-449a62808335 `
  --parameters '{\"name\":\"github-production\",\"issuer\":\"https://token.actions.githubusercontent.com\",\"subject\":\"repo:phongvovan08/RoomEnglish:environment:Production\",\"audiences\":[\"api://AzureADTokenExchange\"]}'
```

### Lỗi 2: "DLL load failed while importing win32file"

**Log:**
```
ImportError: DLL load failed while importing win32file: 
The specified module could not be found.
```

**Nguyên nhân:** Azure CLI thiếu dependencies

**Giải pháp:**
```powershell
# 1. Cài Visual C++ Redistributable
winget install --id Microsoft.VCRedist.2015+.x64

# 2. Gỡ Azure CLI
winget uninstall Microsoft.AzureCLI

# 3. Cài lại Azure CLI
winget install -e --id Microsoft.AzureCLI

# 4. Kiểm tra
az --version
```

### Lỗi 3: "The service principal cannot be created"

**Log:**
```
ERROR: The service principal cannot be created, updated, or restored 
because the service principal name 5897f206-... is already in use.
```

**Nguyên nhân:** Service Principal đã tồn tại từ lần chạy trước

**Giải pháp:** Không cần làm gì, script sẽ dùng lại SP cũ. Lỗi này có thể bỏ qua.

### Lỗi 4: Vue.js App Không Load (404)

**Triệu chứng:** Truy cập website thấy 404, không load được trang chủ

**Kiểm tra:**

1. **Xem workflow logs** → Job `build` → Step `Publish Web project`:
   ```
   Building Vue.js ClientApp...
   npm install
   npm run build
   Copying files to wwwroot...
   ```

2. **Kiểm tra artifacts** sau khi publish:
   ```
   ./publish/wwwroot/
   ├── index.html      ← Phải có file này
   ├── assets/
   │   ├── index-*.js
   │   └── index-*.css
   └── ...
   ```

3. **Nếu thiếu files:** MSBuild target không chạy
   - Kiểm tra `Web.csproj` có target `BuildClientApp`
   - Node.js version phải là 20.x

### Lỗi 5: API Calls Bị 404

**Triệu chứng:** Frontend load được nhưng API calls fail

**Kiểm tra:**

1. **Browser Console** (F12):
   ```
   GET https://webroomenglish.../api/Users/login 404
   ```

2. **Xem Azure Logs:**
   - Azure Portal → WebRoomEnglish → Log stream
   - Xem error messages

3. **Kiểm tra Connection String:**
   - Configuration → Application settings
   - Đảm bảo có `ConnectionStrings__DefaultConnection`

## 📊 Environment Configuration

### GitHub Environment Protection (Tùy Chọn)

Nếu muốn **yêu cầu approve** trước khi deploy Production:

#### Bước 1: Vào Settings
https://github.com/phongvovan08/RoomEnglish/settings/environments

#### Bước 2: Click vào `Production`

#### Bước 3: Enable Protection Rules

1. ✅ **Required reviewers**
   - Click checkbox
   - Thêm username: `phongvovan08`
   - Có thể thêm nhiều người

2. ✅ **Deployment branches** (Tùy chọn)
   - Selected branches
   - Thêm: `main`
   - → Chỉ cho phép deploy từ main

3. Click **Save protection rules**

#### Kết Quả

Khi workflow chạy:
1. Build job chạy bình thường
2. Deploy job **dừng lại** chờ approve
3. Reviewer nhận notification
4. Reviewer approve → Deploy tiếp tục
5. Không approve trong 30 ngày → Workflow cancel

## 📝 Checklist Hoàn Chỉnh

### Lần Đầu Setup

- [ ] Cài Visual C++ Redistributable
- [ ] Cài Azure CLI
- [ ] Kiểm tra `az --version` chạy được
- [ ] Chạy script `setup-azure-oidc.ps1`
- [ ] Thêm 3 GitHub Secrets (CLIENT_ID, TENANT_ID, SUBSCRIPTION_ID)
- [ ] Tạo federated credential cho `environment:Production`
- [ ] Push code hoặc run workflow thủ công
- [ ] Kiểm tra deployment thành công
- [ ] Set Azure App Settings (OpenAI__ApiKey, ConnectionString)
- [ ] Test website và API

### Mỗi Lần Deploy

- [ ] Push code lên `main`
- [ ] Xem workflow chạy trên GitHub Actions
- [ ] Kiểm tra logs không có error
- [ ] Test website sau khi deploy
- [ ] Clear browser cache nếu cần

## 🎓 Tóm Tắt

### Luồng Deployment Tự Động

```
1. Developer push code lên main
   ↓
2. GitHub Actions trigger workflow
   ↓
3. Build job:
   - Build .NET solution
   - Build Vue.js app (MSBuild target)
   - Upload artifacts
   ↓
4. Deploy job:
   - Download artifacts
   - Request OIDC token from GitHub
   - GitHub issues token with subject: environment:Production
   - Azure validates token against federated credential
   - Token matched → Grant temp access
   - Deploy to Azure App Service
   ↓
5. Deployment complete!
   ↓
6. User access: https://webroomenglish-....azurewebsites.net
```

### Các Thành Phần Chính

| Thành phần | Vai trò |
|-----------|---------|
| **GitHub Actions** | Chạy workflow build và deploy |
| **Azure AD App Registration** | Định danh ứng dụng |
| **Service Principal** | Quyền truy cập Azure resources |
| **Federated Credential** | Xác thực OIDC không cần password |
| **GitHub Secrets** | Lưu CLIENT_ID, TENANT_ID, SUBSCRIPTION_ID |
| **MSBuild Targets** | Tự động build Vue.js khi publish |
| **Azure App Service** | Hosting ứng dụng |

### Ưu Điểm

✅ **Tự động hóa 100%**: Push code → Auto deploy  
✅ **Bảo mật cao**: OIDC, không lưu password  
✅ **Build nhất quán**: Luôn build trên GitHub Actions  
✅ **Rollback dễ dàng**: Re-run workflow cũ  
✅ **Logs đầy đủ**: Theo dõi mọi deployment  
✅ **Environment protection**: Có thể yêu cầu approve  

---

**🎉 Hoàn thành! Từ giờ mỗi lần push lên `main` sẽ tự động deploy lên Azure!**
