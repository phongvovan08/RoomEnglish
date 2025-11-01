# ⚠️ CÁCH SETUP AZURE DEPLOYMENT - QUAN TRỌNG

## Bước 1: Lấy Publish Profile từ Azure Portal

1. Đăng nhập vào [Azure Portal](https://portal.azure.com)
2. Tìm **WebRoomEnglish** App Service
3. Click nút **Get publish profile** (trên thanh toolbar)
4. File `.PublishSettings` sẽ được tải về

## Bước 2: Thêm Secret vào GitHub

1. Mở repository: https://github.com/phongvovan08/RoomEnglish
2. Click **Settings** (tab trên cùng)
3. Sidebar bên trái: **Secrets and variables** → **Actions**
4. Click nút **New repository secret**
5. Điền thông tin:
   ```
   Name: AZURE_WEBAPP_PUBLISH_PROFILE
   
   Secret: [Paste toàn bộ nội dung XML từ file .PublishSettings]
   ```
6. Click **Add secret**

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
