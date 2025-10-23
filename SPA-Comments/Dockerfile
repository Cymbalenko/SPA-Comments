# =============== Базовый слой для запуска ===============
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# =============== Слой сборки ===============
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ------------------- Установка Node.js -------------------
RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y nodejs && \
    npm install -g @angular/cli

# ------------------- .NET -------------------
COPY ["SPA-Comments.Server/SPA-Comments.Server.csproj", "SPA-Comments.Server/"]
RUN dotnet restore "SPA-Comments.Server/SPA-Comments.Server.csproj"

# ------------------- Angular -------------------
# 1) Копируем все исходники фронта сразу, включая package.json, package-lock.json, angular.json, src/
COPY ["spa-comments.client/", "spa-comments.client/"]

WORKDIR /src/spa-comments.client

# 2) Устанавливаем зависимости с игнорированием peer-зависимостей
RUN npm ci --legacy-peer-deps

# 3) Сборка Angular
RUN npm run build -- --configuration production --output-path=dist

# ------------------- Копируем исходники бэка -------------------
WORKDIR /src
COPY . .

# Сборка и публикация .NET приложения
WORKDIR "/src/SPA-Comments.Server"
RUN dotnet build "SPA-Comments.Server.csproj" -c Release -o /app/build
RUN dotnet publish "SPA-Comments.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =============== Финальный слой ===============
FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .
COPY --from=build /src/spa-comments.client/dist/browser/ /app/wwwroot/

ENTRYPOINT ["dotnet", "SPA-Comments.Server.dll"]
