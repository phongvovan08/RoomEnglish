# Quick Start: Deploy Client App to Azure

## ✅ Code đã được chuẩn bị

Các files sau đã được tạo/update:

1. ✅ `src/Web/ClientApp/src/config/api.config.ts` - API configuration
2. ✅ `src/Web/ClientApp/src/modules/vocabulary/composables/useVocabulary.ts` - Updated to use API_CONFIG
3. ✅ `src/Web/ClientApp/src/composables/useAudioCacheAPI.ts` - Updated to use API_CONFIG
4. ✅ `src/Web/ClientApp/.env.development` - Development environment
5. ✅ `src/Web/ClientApp/.env.production` - Production environment
6. ✅ `DEPLOY_CLIENT_GUIDE.md` - Chi tiết 3 phương án deploy

---

## 🎯 Bạn Có 3 Lựa Chọn:

### **Option 1: Cùng App Service (SIMPLEST)** ⭐ Recommended for Quick Start

**Đã có sẵn! Không cần làm gì thêm.**

API của bạn trên Azure App Service đã serve cả client app từ `wwwroot/`.

✅ **Test ngay:**
```
https://your-api-name.azurewebsites.net
```

Nếu chưa thấy Vue app → Check `Program.cs` có `app.MapFallbackToFile("index.html");`

---

### **Option 2: Azure Static Web Apps (FREE)** ⭐ Recommended for Production

**Miễn phí 100%, auto CI/CD, global CDN**

**Quick Setup:**

1. **Azure Portal** → Search "Static Web Apps" → Create
   - Name: `roomenglish-app`
   - Plan: **Free**
   - GitHub repo: `RoomEnglish`
   - Branch: `main`
   - Build preset: **Vue**
   - App location: `/src/Web/ClientApp`
   - Output: `dist`

2. **Update `.env.production`:**
   ```env
   VITE_API_URL=https://your-api-name.azurewebsites.net/api
   ```

3. **Add CORS trong Program.cs:**
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("AllowAll", policy =>
       {
           policy.AllowAnyOrigin()
                 .AllowAnyMethod()
                 .AllowAnyHeader();
       });
   });
   
   app.UseCors("AllowAll");
   ```

4. **Push code:**
   ```bash
   git add .
   git commit -m "Deploy to Static Web Apps"
   git push origin main
   ```

✅ **App URL:** `https://xxx.azurestaticapps.net`

---

### **Option 3: Azure Storage + CDN**

Xem chi tiết trong `DEPLOY_CLIENT_GUIDE.md`

---

## 🚀 Recommended Action NOW:

### **Test Option 1 (Hiện tại):**

```bash
# Build frontend
cd src/Web/ClientApp
npm install
npm run build

# Check output
ls dist/  # Should have index.html, assets/, etc.

# The GitHub Actions workflow will automatically:
# 1. Build frontend
# 2. Copy dist/ to wwwroot/
# 3. Deploy to Azure
```

### **Push to Deploy:**

```bash
git add .
git commit -m "Update client deployment config"
git push origin main
```

✅ **Sau vài phút, truy cập:**
```
https://your-api-name.azurewebsites.net
```

---

## 📋 Checklist

- [x] API config created
- [x] Environment files configured
- [x] Composables updated to use API_CONFIG
- [ ] Test local: `npm run dev`
- [ ] Build: `npm run build`
- [ ] Deploy to Azure
- [ ] Test production URL

---

## 🔍 Troubleshooting

### **Client app không load:**

1. Check `Program.cs`:
   ```csharp
   app.UseStaticFiles();
   app.MapFallbackToFile("index.html");
   ```

2. Check `wwwroot/` folder có files không

3. Check browser console for errors

### **API calls fail (CORS error):**

1. Add CORS policy trong `Program.cs`
2. Hoặc deploy client cùng App Service (Option 1)

### **404 on page refresh:**

Cần `MapFallbackToFile("index.html")` trong `Program.cs`

---

## 📚 Next Steps

1. **Test current deployment** (Option 1)
2. **If satisfied** → Done! ✅
3. **If need better performance** → Setup Static Web Apps (Option 2)
4. **If need control** → Setup Storage + CDN (Option 3)

---

## 💡 Tips

- **Development:** Use `npm run dev` with proxy (`/api` → `https://localhost:5001`)
- **Production:** Client uses environment variable `VITE_API_URL`
- **Same domain:** Leave `VITE_API_URL` empty
- **Different domain:** Set `VITE_API_URL` to full API URL

Good luck! 🎉
