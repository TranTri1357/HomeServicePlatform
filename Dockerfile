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

EXPOSE 10000

ENTRYPOINT ["dotnet","HomeServicePlatform.Api.dll"]