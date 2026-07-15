# ============================
# Build stage
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy toàn bộ source
COPY . .

# Restore
RUN dotnet restore HomeServicePlatform.sln

# Publish API
RUN dotnet publish src/HomeServicePlatform.Api/HomeServicePlatform.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# ============================
# Runtime stage
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

# Khắc phục SIGSEGV (exit 139) của .NET trong một số container/host cloud:
# tắt cơ chế bộ nhớ W^X (nguyên nhân crash phổ biến trên host có kernel siết mmap),
# và tắt tiered compilation để tránh lỗi JIT hiếm gặp lúc khởi động.
ENV DOTNET_EnableWriteXorExecute=0
ENV DOTNET_TieredCompilation=0

# Tắt theo dõi thay đổi file appsettings (reloadOnChange). Production không cần nạp lại
# config nóng, mà FileSystemWatcher lại tạo inotify instance — trên host Render dùng chung
# hạn mức fs.inotify.max_user_instances thấp, dễ cạn → IOException "inotify instances ...
# reached" khi WebApplication.CreateBuilder → crash lúc khởi động.
ENV DOTNET_hostBuilder__reloadConfigOnChange=false

EXPOSE 10000

ENTRYPOINT ["dotnet","HomeServicePlatform.Api.dll"]