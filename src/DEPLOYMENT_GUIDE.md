# RoomEnglish - Deployment Guide

## 📋 Prerequisites

- .NET 8 SDK
- Node.js 18+ & npm
- SQL Server hoặc Azure SQL Database
- Azure Account (cho Azure deployment) hoặc IIS (cho Windows Server)

---

## 🎯 Deployment Options

### **Option 1: Azure App Service (Recommended)** ☁️

#### **Bước 1: Chuẩn bị Database**

**A. Azure SQL Database (Recommended for production)**

1. Tạo Azure SQL Database:
```bash
# Azure Portal
- Tạo SQL Database
- Copy Connection String
- Update appsettings.Production.json
```

2. Hoặc dùng SQL Server hiện tại (cần public IP)

**B. Update Connection String**

File: `Web/appsettings.Production.json`
```json
{
  "ConnectionStrings": {
    "RoomEnglishDb": "Server=your-server.database.windows.net;Database=RoomEnglishDb;User Id=your-user;Password=your-password;Encrypt=True;"
  }
}
```

#### **Bước 2: Build Frontend**

```bash
cd src/Web/ClientApp
npm install
npm run build
```

Build output → `dist/` folder

#### **Bước 3: Publish Backend**

**Option A: Visual Studio**
1. Right-click project `Web` → **Publish**
2. Chọn **Azure** → **Azure App Service**
3. Tạo mới hoặc chọn existing App Service
4. Click **Publish**

**Option B: CLI**
```bash
cd src/Web

# Publish
dotnet publish -c Release -o ./publish

# Deploy to Azure
az webapp deployment source config-zip \
  --resource-group YourResourceGroup \
  --name YourAppName \
  --src publish.zip
```

#### **Bước 4: Configure App Service**

**Application Settings:**
```
Authentication__BearerToken__ExpirationDays = 7
OpenAI__ApiKey = your-openai-key
```

**Connection Strings:**
```
RoomEnglishDb = your-connection-string
```

**Startup Command (if needed):**
```
dotnet Web.dll
```

---

### **Option 2: IIS on Windows Server** 🖥️

#### **Bước 1: Install IIS**

```powershell
# PowerShell as Administrator
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ASPNET47
```

#### **Bước 2: Install .NET Hosting Bundle**

Download và install:
```
https://dotnet.microsoft.com/download/dotnet/8.0
→ .NET 8.0 Runtime & Hosting Bundle for Windows
```

#### **Bước 3: Build & Publish**

```bash
# Build Frontend
cd src/Web/ClientApp
npm install
npm run build

# Publish Backend
cd ../
dotnet publish -c Release -o C:\inetpub\RoomEnglish
```

#### **Bước 4: Create IIS Site**

1. Mở **IIS Manager**
2. **Add Website:**
   - Site name: `RoomEnglish`
   - Physical path: `C:\inetpub\RoomEnglish`
   - Port: `80` (hoặc `443` cho HTTPS)
   - Host name: `yourdomain.com` (optional)

3. **Application Pool Settings:**
   - .NET CLR Version: **No Managed Code**
   - Managed Pipeline Mode: **Integrated**

#### **Bước 5: Configure**

**web.config** (auto-generated, verify):
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" 
                arguments=".\Web.dll" 
                stdoutLogEnabled="true" 
                stdoutLogFile=".\logs\stdout" 
                hostingModel="inprocess" />
  </system.webServer>
</configuration>
```

**appsettings.Production.json:**
```json
{
  "ConnectionStrings": {
    "RoomEnglishDb": "Server=localhost\\SQLEXPRESS;Database=RoomEnglishDb;User Id=sa;Password=YourPassword;Encrypt=False;"
  }
}
```

---

### **Option 3: Docker (Cross-platform)** 🐳

#### **Bước 1: Create Dockerfile**

File: `src/Dockerfile`
```dockerfile
# Build Frontend
FROM node:18 AS frontend-build
WORKDIR /app/clientapp
COPY Web/ClientApp/package*.json ./
RUN npm install
COPY Web/ClientApp/ ./
RUN npm run build

# Build Backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src
COPY ["Web/Web.csproj", "Web/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "Web/Web.csproj"
COPY . .
WORKDIR "/src/Web"
RUN dotnet build "Web.csproj" -c Release -o /app/build
RUN dotnet publish "Web.csproj" -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=backend-build /app/publish .
COPY --from=frontend-build /app/clientapp/dist ./wwwroot
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "Web.dll"]
```

#### **Bước 2: Build & Run**

```bash
cd src

# Build image
docker build -t roomenglish:latest .

# Run container
docker run -d -p 5000:80 \
  -e ConnectionStrings__RoomEnglishDb="Server=host.docker.internal;Database=RoomEnglishDb;User Id=sa;Password=YourPassword;" \
  -e OpenAI__ApiKey="your-key" \
  --name roomenglish \
  roomenglish:latest
```

#### **Bước 3: Docker Compose (Optional)**

File: `docker-compose.yml`
```yaml
version: '3.8'
services:
  app:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__RoomEnglishDb=Server=db;Database=RoomEnglishDb;User Id=sa;Password=YourStrongPassword123;
    depends_on:
      - db
  
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrongPassword123
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

volumes:
  sqldata:
```

Run:
```bash
docker-compose up -d
```

---

### **Option 4: Linux Server (Ubuntu)** 🐧

#### **Bước 1: Install Dependencies**

```bash
# Install .NET 8
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Install Nginx
sudo apt update
sudo apt install nginx

# Install SQL Server (optional)
# Or use remote SQL Server
```

#### **Bước 2: Build & Deploy**

```bash
# On development machine - build
cd src/Web/ClientApp
npm run build

cd ../
dotnet publish -c Release -o ./publish

# Copy to server
scp -r ./publish user@yourserver:/var/www/roomenglish
```

#### **Bước 3: Configure Systemd Service**

File: `/etc/systemd/system/roomenglish.service`
```ini
[Unit]
Description=RoomEnglish ASP.NET Core App
After=network.target

[Service]
WorkingDirectory=/var/www/roomenglish
ExecStart=/usr/bin/dotnet /var/www/roomenglish/Web.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=roomenglish
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

Start service:
```bash
sudo systemctl enable roomenglish
sudo systemctl start roomenglish
sudo systemctl status roomenglish
```

#### **Bước 4: Configure Nginx**

File: `/etc/nginx/sites-available/roomenglish`
```nginx
server {
    listen 80;
    server_name yourdomain.com;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable:
```bash
sudo ln -s /etc/nginx/sites-available/roomenglish /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

---

## 🔒 SSL/HTTPS Setup

### **Azure App Service**
- Tự động có HTTPS với `*.azurewebsites.net`
- Hoặc add custom domain + SSL certificate

### **IIS**
```powershell
# Install certificate
Import-PfxCertificate -FilePath "certificate.pfx" -CertStoreLocation Cert:\LocalMachine\My

# Bind in IIS
# IIS Manager → Site → Bindings → Add HTTPS → Select certificate
```

### **Nginx (Let's Encrypt)**
```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d yourdomain.com
sudo systemctl restart nginx
```

---

## 🗄️ Database Migration

### **Run migrations on production**

**Option A: Automatic on startup**
```csharp
// Program.cs already has:
await app.InitialiseDatabaseAsync();
```

**Option B: Manual**
```bash
# Connection string in appsettings.Production.json
dotnet ef database update --project Infrastructure --startup-project Web
```

---

## ✅ Pre-Deployment Checklist

- [ ] Update `appsettings.Production.json` with production connection string
- [ ] Set `OpenAI__ApiKey` in production environment variables
- [ ] Configure `Authentication__BearerToken__ExpirationDays`
- [ ] Build frontend: `npm run build`
- [ ] Test publish locally: `dotnet publish -c Release`
- [ ] Backup database before first deployment
- [ ] Configure HTTPS/SSL
- [ ] Set up logging (Application Insights, Serilog, etc.)
- [ ] Configure CORS for production domain
- [ ] Test all endpoints after deployment

---

## 🔍 Troubleshooting

### **Common Issues:**

**1. 500 Internal Server Error**
```bash
# Check logs
# Azure: Log Stream
# IIS: C:\inetpub\RoomEnglish\logs\
# Linux: journalctl -u roomenglish
```

**2. Database connection fails**
```
- Verify connection string
- Check firewall rules
- Ensure SQL Server allows remote connections
```

**3. Frontend not loading**
```
- Verify build output is in wwwroot
- Check CORS settings in Program.cs
- Inspect browser console for errors
```

**4. API calls fail**
```
- Check appsettings for correct API base URL
- Verify CORS configuration
- Check network/firewall rules
```

---

## 📊 Monitoring & Maintenance

### **Azure**
- Application Insights
- Log Stream
- Metrics

### **IIS**
- Event Viewer
- IIS Logs: `C:\inetpub\logs\`

### **Linux**
- `journalctl -u roomenglish -f`
- Nginx logs: `/var/log/nginx/`

---

## 🎯 Quick Start Commands

### **Development → Production (Azure)**
```bash
# 1. Build frontend
cd src/Web/ClientApp && npm run build

# 2. Publish backend
cd .. && dotnet publish -c Release

# 3. Deploy to Azure
az webapp deployment source config-zip --resource-group YourRG --name YourApp --src publish.zip
```

### **Development → Production (IIS)**
```bash
# 1. Build all
cd src/Web/ClientApp && npm run build
cd .. && dotnet publish -c Release -o C:\inetpub\RoomEnglish

# 2. Restart IIS
iisreset
```

### **Development → Production (Docker)**
```bash
# 1. Build & run
cd src
docker build -t roomenglish .
docker run -d -p 5000:80 roomenglish
```

---

## 🚀 Recommended Production Setup

**Best Practice:**
- **Hosting:** Azure App Service (auto-scaling, managed)
- **Database:** Azure SQL Database (managed, backup)
- **CDN:** Azure CDN or Cloudflare (static assets)
- **Monitoring:** Application Insights
- **CI/CD:** GitHub Actions or Azure DevOps

---

## 📞 Support

Nếu gặp vấn đề trong quá trình deploy, check:
1. Logs (quan trọng nhất!)
2. Connection strings
3. Environment variables
4. Firewall/Network settings
5. SSL certificates

Good luck! 🎉
