# Stage 1: Build Frontend
FROM node:20-alpine AS frontend-build
WORKDIR /app/clientapp

# Copy package files and install dependencies
COPY Web/ClientApp/package*.json ./
RUN npm ci

# Copy frontend source and build
COPY Web/ClientApp/ ./
RUN npm run build

# Stage 2: Build Backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Web/Web.csproj", "Web/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "Web/Web.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/Web"
RUN dotnet build "Web.csproj" -c Release -o /app/build
RUN dotnet publish "Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published backend
COPY --from=backend-build /app/publish .

# Copy built frontend
COPY --from=frontend-build /app/clientapp/dist ./wwwroot

# Expose ports
EXPOSE 80
EXPOSE 443

# Set environment
ENV ASPNETCORE_URLS=http://+:80

# Run the application
ENTRYPOINT ["dotnet", "Web.dll"]
