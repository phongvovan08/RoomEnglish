# GitHub Actions CI/CD Setup

## Workflows

### 1. CI - Build and Test (`ci.yml`)
- **Trigger:** Push hoặc PR vào `main` hoặc `develop`
- **Chức năng:**
  - Build .NET solution
  - Chạy tests
  - Build Vue.js app
  - Kiểm tra artifacts

### 2. Deploy to Azure (`azure-webapps-dotnet.yml`)
- **Trigger:** Push vào `main` hoặc manual workflow dispatch
- **Chức năng:**
  - Build và publish .NET app (bao gồm Vue.js)
  - Deploy lên Azure Web App

## Cách Setup Azure Deployment

### Bước 1: Tải Publish Profile từ Azure Portal

1. Mở [Azure Portal](https://portal.azure.com)
2. Tìm Web App: **WebRoomEnglish**
3. Click **Get publish profile** (trên toolbar)
4. Lưu file `.PublishSettings` về máy

### Bước 2: Thêm Secret vào GitHub

1. Mở repository: https://github.com/phongvovan08/RoomEnglish
2. Vào **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
5. Value: Mở file `.PublishSettings` vừa tải, copy **toàn bộ nội dung XML**
6. Click **Add secret**

### Bước 3: Test Deployment

1. Push code lên `main` branch:
   ```bash
   git push origin main
   ```

2. Hoặc chạy workflow thủ công:
   - Vào **Actions** tab trên GitHub
   - Chọn workflow **Deploy to Azure Web App**
   - Click **Run workflow**

3. Theo dõi progress trong Actions tab

### Bước 4: Verify Deployment

Sau khi deployment thành công, kiểm tra:
- **URL:** https://webroomenglish-b3e5a8ghf9f2geaa.southeastasia-01.azurewebsites.net
- Test API: `/api/Users/login`
- Test Vue app loading

## Environment Variables trên Azure

Nhớ cấu hình các biến môi trường sau trong Azure Portal:

1. **App Service** → **Configuration** → **Application settings**

2. Thêm các settings:
   - `OpenAI__ApiKey`: Your OpenAI API key
   - `ConnectionStrings__DefaultConnection`: Your database connection string

3. Click **Save** và restart app

## Troubleshooting

### Workflow fails với "publish-profile not found"
- Kiểm tra secret `AZURE_WEBAPP_PUBLISH_PROFILE` đã được tạo chưa
- Đảm bảo đã copy **toàn bộ** nội dung XML từ file .PublishSettings

### Deployment thành công nhưng app không chạy
- Kiểm tra **Logs** trong Azure Portal
- Verify environment variables đã được set đúng
- Kiểm tra Connection String đến database

### Vue app không load
- Workflow tự động build Vue app khi publish
- Kiểm tra `wwwroot` folder có chứa `index.html` không
- Xem browser console để debug

## Manual Deployment (Alternative)

Nếu muốn deploy từ Visual Studio:

1. Right-click **Web** project
2. Click **Publish**
3. Select profile **WebRoomEnglish - Web Deploy**
4. Click **Publish**

## Monitoring

- **Azure Portal:** Monitor app performance, logs
- **GitHub Actions:** Xem deployment history
- **Application Insights:** (Optional) Chi tiết monitoring

---

**Note:** Vue.js app được build tự động khi publish .NET project nhờ MSBuild target `BuildClientApp` trong `Web.csproj`.
