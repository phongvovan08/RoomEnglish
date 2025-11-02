# Deploy Vue.js Client App to Azure

Bạn đã có API trên Azure App Service. Giờ có 3 cách deploy Vue.js client app:

---

## 🎯 So Sánh 3 Phương Án

| Feature | Static Web Apps | Same App Service | Storage + CDN |
|---------|----------------|------------------|---------------|
| **Cost** | **FREE** ✅ | ~$13/month | ~$1/month |
| **Setup** | Easy | **Easiest** ✅ | Medium |
| **Performance** | Excellent (CDN) | Good | Excellent (CDN) |
| **SSL** | Free | Free | Free |
| **CI/CD** | Auto | Manual/GitHub | GitHub Actions |
| **Separation** | Yes | No | Yes |
| **Best for** | Production | Small/Demo | Large scale |

---

## 📦 Option 1: Cùng App Service (SIMPLEST - RECOMMENDED)

### ✅ Ưu Điểm:
- **Đã có sẵn!** Không cần setup gì thêm
- Client và API cùng domain → Không cần CORS phức tạp
- Deploy 1 lần có cả frontend + backend
- Đơn giản nhất

### 🚀 Cách Hoạt Động:

```
https://your-app.azurewebsites.net/
  ├── /api/*          → Backend API (ASP.NET)
  └── /*              → Vue.js SPA (wwwroot)
```

### 📝 Setup Steps:

#### **Bước 1: Verify Program.cs**

File: `src/Web/Program.cs`

```csharp
// Make sure these exist:
app.UseStaticFiles();           // Serve static files from wwwroot
app.UseRouting();

// At the end, after MapControllers():
app.MapFallbackToFile("index.html");  // SPA routing fallback
```

#### **Bước 2: Build và Deploy**

**Cách 1: Visual Studio**
1. Right-click project `Web` → **Publish**
2. Chọn existing Azure App Service
3. Click **Publish**

**Cách 2: CLI**
```bash
cd src/Web/ClientApp
npm install
npm run build

cd ..
dotnet publish -c Release -o ./publish

# Upload to Azure
# (Sử dụng Azure Portal hoặc Azure CLI)
```

**Cách 3: GitHub Actions (Tự động)**
```bash
# Đã có workflow rồi, chỉ cần push
git add .
git commit -m "Update client app"
git push origin main
```

#### **Bước 3: Test**

Truy cập: `https://your-app.azurewebsites.net`

✅ Done! Client app đã chạy cùng API.

---

## ⚡ Option 2: Azure Static Web Apps (BEST FOR PRODUCTION)

### ✅ Ưu Điểm:
- **Miễn phí 100%**
- Global CDN (nhanh trên toàn thế giới)
- CI/CD tự động
- Tách biệt frontend/backend
- SSL miễn phí
- Custom domain miễn phí

### 📝 Setup Steps:

#### **Bước 1: Create Static Web App**

**Azure Portal:**
1. Search **Static Web Apps** → **Create**
2. Fill in:
   - **Subscription**: Your subscription
   - **Resource Group**: Chọn cùng với App Service
   - **Name**: `roomenglish-app`
   - **Plan type**: **Free**
   - **Region**: East Asia hoặc Southeast Asia
   - **Deployment details**:
     - Source: **GitHub**
     - Organization: Your GitHub username
     - Repository: **RoomEnglish**
     - Branch: **main**
   - **Build details**:
     - Build Presets: **Vue**
     - App location: `/src/Web/ClientApp`
     - Api location: *(leave empty)*
     - Output location: `dist`

3. Click **Review + create** → **Create**

#### **Bước 2: Configure Environment Variables**

**Trong Azure Portal → Static Web App → Configuration:**

Add:
```
VITE_API_URL = https://your-api-name.azurewebsites.net/api
```

#### **Bước 3: Update Code to Use API_CONFIG**

Files đã được update:
- `src/Web/ClientApp/src/config/api.config.ts` ✅
- `src/Web/ClientApp/src/modules/vocabulary/composables/useVocabulary.ts` ✅

Còn cần update:
- `src/Web/ClientApp/src/composables/useAudioCacheAPI.ts`

**Update useAudioCacheAPI.ts:**

```typescript
import { createAuthHeaders } from '@/utils/auth'
import { API_CONFIG } from '@/config/api.config'

const API_BASE = `${API_CONFIG.baseURL}/audio-cache`
```

#### **Bước 4: Configure CORS on API**

**File: `src/Web/Program.cs`**

```csharp
// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowStaticWebApp",
        policy =>
        {
            policy.WithOrigins(
                "https://your-static-web-app.azurestaticapps.net",
                "http://localhost:3000"  // For development
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
        });
});

// Before app.Run():
app.UseCors("AllowStaticWebApp");
```

**Hoặc trong Azure Portal:**
1. Vào App Service (API) → **CORS**
2. Add: `https://your-static-web-app.azurestaticapps.net`

#### **Bước 5: GitHub Workflow**

Azure tự động tạo workflow file: `.github/workflows/azure-static-web-apps-xxx.yml`

Customize để add environment variables:

```yaml
- name: Build And Deploy
  uses: Azure/static-web-apps-deploy@v1
  with:
    azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
    repo_token: ${{ secrets.GITHUB_TOKEN }}
    action: "upload"
    app_location: "/src/Web/ClientApp"
    api_location: ""
    output_location: "dist"
  env:
    VITE_API_URL: https://your-api-name.azurewebsites.net/api
```

#### **Bước 6: Deploy**

```bash
git add .
git commit -m "Configure Static Web Apps"
git push origin main
```

✅ App deploy tự động lên: `https://your-app.azurestaticapps.net`

---

## 💾 Option 3: Azure Storage Static Website

### ✅ Ưu Điểm:
- Rẻ nhất (~$0.01/GB/month)
- Scalable
- CDN integration
- Phù hợp production scale lớn

### 📝 Setup Steps:

#### **Bước 1: Create Storage Account**

```bash
# Azure CLI
az storage account create \
  --name roomenglishstorage \
  --resource-group YourResourceGroup \
  --location eastus \
  --sku Standard_LRS \
  --kind StorageV2

# Enable static website
az storage blob service-properties update \
  --account-name roomenglishstorage \
  --static-website \
  --404-document 404.html \
  --index-document index.html
```

**Primary endpoint:** `https://roomenglishstorage.z13.web.core.windows.net`

#### **Bước 2: Build và Upload**

```bash
cd src/Web/ClientApp

# Set API URL
$env:VITE_API_URL="https://your-api-name.azurewebsites.net/api"

# Build
npm run build

# Upload to Azure Storage
az storage blob upload-batch \
  --account-name roomenglishstorage \
  --source ./dist \
  --destination '$web' \
  --overwrite
```

#### **Bước 3: Setup CDN (Optional but Recommended)**

```bash
# Create CDN profile
az cdn profile create \
  --name RoomEnglishCDN \
  --resource-group YourResourceGroup \
  --sku Standard_Microsoft

# Create CDN endpoint
az cdn endpoint create \
  --name roomenglish \
  --profile-name RoomEnglishCDN \
  --resource-group YourResourceGroup \
  --origin roomenglishstorage.z13.web.core.windows.net \
  --origin-host-header roomenglishstorage.z13.web.core.windows.net
```

**CDN URL:** `https://roomenglish.azureedge.net`

#### **Bước 4: GitHub Actions for Auto Deploy**

Create file: `.github/workflows/deploy-storage.yml`

```yaml
name: Deploy to Azure Storage

on:
  push:
    branches: [ main ]
    paths:
      - 'src/Web/ClientApp/**'

env:
  NODE_VERSION: '18.x'
  STORAGE_ACCOUNT: 'roomenglishstorage'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: ${{ env.NODE_VERSION }}
        cache: 'npm'
        cache-dependency-path: src/Web/ClientApp/package-lock.json
    
    - name: Install dependencies
      working-directory: src/Web/ClientApp
      run: npm ci
    
    - name: Build
      working-directory: src/Web/ClientApp
      run: npm run build
      env:
        VITE_API_URL: ${{ secrets.VITE_API_URL }}
    
    - name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}
    
    - name: Upload to Azure Storage
      uses: azure/CLI@v1
      with:
        inlineScript: |
          az storage blob upload-batch \
            --account-name ${{ env.STORAGE_ACCOUNT }} \
            --source ./src/Web/ClientApp/dist \
            --destination '$web' \
            --overwrite \
            --auth-mode login
    
    - name: Purge CDN
      uses: azure/CLI@v1
      with:
        inlineScript: |
          az cdn endpoint purge \
            --profile-name RoomEnglishCDN \
            --name roomenglish \
            --resource-group YourResourceGroup \
            --content-paths '/*'
```

**GitHub Secrets cần thêm:**

1. `VITE_API_URL` = `https://your-api-name.azurewebsites.net/api`

2. `AZURE_CREDENTIALS` (Service Principal):

```bash
az ad sp create-for-rbac \
  --name "github-actions-roomenglish" \
  --role contributor \
  --scopes /subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/YourResourceGroup \
  --sdk-auth
```

Copy output JSON vào GitHub Secret `AZURE_CREDENTIALS`.

---

## 🔧 Code Changes Needed

### **1. Create API Config** ✅

File: `src/Web/ClientApp/src/config/api.config.ts`
```typescript
export const API_CONFIG = {
  baseURL: import.meta.env.VITE_API_URL || '/api',
  timeout: 30000,
} as const

export default API_CONFIG
```

### **2. Create Environment Files**

File: `src/Web/ClientApp/.env.development`
```env
# Local development
VITE_API_URL=/api
```

File: `src/Web/ClientApp/.env.production`
```env
# Production - set in build process or leave empty for same domain
VITE_API_URL=
```

### **3. Update useVocabulary.ts** ✅

Already updated to use `API_CONFIG.baseURL`

### **4. Update useAudioCacheAPI.ts**

Change:
```typescript
const API_BASE = '/api/audio-cache'
```

To:
```typescript
import { API_CONFIG } from '@/config/api.config'
const API_BASE = `${API_CONFIG.baseURL}/audio-cache`
```

### **5. Update any other composables**

Search for hardcoded `/api/` paths:
```bash
grep -r "'/api" src/Web/ClientApp/src/composables
grep -r "'/api" src/Web/ClientApp/src/modules
```

Replace with `API_CONFIG.baseURL`.

---

## 📋 Decision Guide

### **Choose Option 1 (Same App Service) if:**
- ✅ Bạn muốn đơn giản nhất
- ✅ App nhỏ, traffic thấp
- ✅ Không cần tách biệt frontend/backend
- ✅ Đã có App Service rồi

### **Choose Option 2 (Static Web Apps) if:**
- ✅ Bạn muốn **miễn phí**
- ✅ Cần CI/CD tự động
- ✅ Cần performance tốt (CDN global)
- ✅ Muốn tách biệt frontend/backend
- ✅ Production app

### **Choose Option 3 (Storage + CDN) if:**
- ✅ App có traffic lớn
- ✅ Cần control chi tiết về CDN
- ✅ Budget rất thấp
- ✅ Đã có Azure infrastructure sẵn

---

## 🎯 Recommended Flow

### **For Quick Demo:**
1. Use **Option 1** (Same App Service)
2. No extra setup needed
3. Just `git push`

### **For Production:**
1. Start with **Option 2** (Static Web Apps)
2. Free tier, excellent performance
3. Easy to migrate to Option 3 later if needed

---

## ✅ Next Steps

1. **Choose your deployment option** from above
2. **Update code** if using Option 2 or 3:
   ```bash
   # Update useAudioCacheAPI.ts
   # Add .env files
   ```
3. **Configure CORS** on API if using Option 2 or 3
4. **Test locally:**
   ```bash
   cd src/Web/ClientApp
   npm run dev
   ```
5. **Deploy:**
   ```bash
   git add .
   git commit -m "Deploy client app"
   git push origin main
   ```

Good luck! 🚀
