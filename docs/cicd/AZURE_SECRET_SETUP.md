# ⚠️ CÁCH SETUP AZURE DEPLOYMENT - OIDC (Khuyến nghị)

## Phương pháp: OpenID Connect (OIDC) - An toàn, không cần password

### Bước 1: Setup Federated Credential trên Azure Portal

1. Đăng nhập vào [Azure Portal](https://portal.azure.com)
2. Tìm **WebRoomEnglish** App Service
3. Sidebar bên trái → **Deployment Center**
4. Tab **Settings** → Source: **GitHub**
5. Click **Authorize** và chọn:
   - **Organization**: phongvovan08
   - **Repository**: RoomEnglish
   - **Branch**: main
6. Click **Save**

Azure sẽ tự động:
- Tạo Service Principal
- Setup Federated Credential
- Thêm 3 secrets vào GitHub repository

### Bước 2: Verify GitHub Secrets (Tự động)

Vào https://github.com/phongvovan08/RoomEnglish/settings/secrets/actions

Kiểm tra có 3 secrets sau (Azure tự tạo):
```
AZURE_CLIENT_ID
AZURE_TENANT_ID  
AZURE_SUBSCRIPTION_ID
```

**Nếu chưa có**, làm thủ công:

#### 2.1. Lấy thông tin từ Azure
```powershell
# Trong Azure Cloud Shell hoặc local (cần Azure CLI)
az ad sp list --display-name WebRoomEnglish --query "[0].{clientId:appId, tenantId:appOwnerOrganizationId}" -o json
az account show --query "{subscriptionId:id}" -o json
```

#### 2.2. Thêm vào GitHub
- `AZURE_CLIENT_ID`: Application (client) ID
- `AZURE_TENANT_ID`: Directory (tenant) ID  
- `AZURE_SUBSCRIPTION_ID`: `6d6d5629-96f5-4bb9-8b30-1004108e6a99`

## Bước 3: Test Deployment

### Tự động (Recommended):
```bash
git add .
git commit -m "your message"
git push origin main
```
→ Workflow sẽ tự động chạy khi push lên `main`

### Thủ công:
1. Vào GitHub repository: https://github.com/phongvovan08/RoomEnglish/actions
2. Click workflow **Deploy to Azure Web App**
3. Click **Run workflow** → **Run workflow**

## Bước 4: Kiểm tra kết quả

- **GitHub Actions**: Xem progress tại https://github.com/phongvovan08/RoomEnglish/actions
- **Azure Portal**: App Service → Deployment Center → Logs
- **Website**: https://webroomenglish-b3e5a8ghf9f2geaa.southeastasia-01.azurewebsites.net

## ⚠️ QUAN TRỌNG: Environment Variables

Sau khi deploy, nhớ cấu hình:

1. Azure Portal → **WebRoomEnglish** → **Configuration** → **Application settings**
2. Thêm:
   ```
   OpenAI__ApiKey = [Your OpenAI API Key]
   ```
3. Click **Save** → **Restart**

## Troubleshooting

### ❌ "No credentials found"
→ Chưa add secret `AZURE_WEBAPP_PUBLISH_PROFILE` (xem Bước 2)

### ❌ Deployment thành công nhưng app không chạy
→ Kiểm tra Application Insights / Logs trong Azure Portal

### ❌ 404 Not Found
→ Kiểm tra `wwwroot` có chứa Vue.js files không

---

**Ghi chú:** Workflow tự động build Vue.js app khi publish nhờ MSBuild target trong `Web.csproj`
