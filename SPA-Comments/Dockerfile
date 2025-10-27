# =============== Базовый слой для запуска ===============
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Устанавливаем libgdiplus для System.Drawing (если понадобится)
RUN apt-get update && apt-get install -y libgdiplus && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

# =============== Слой сборки ===============
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ------------------- Установка Node.js -------------------
RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y nodejs && \
    npm install -g @angular/cli && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

# ------------------- .NET -------------------
# Копируем все .csproj-файлы для кэширования restore
COPY ["SPA-Comments.Server/SPA-Comments.Server.csproj", "SPA-Comments.Server/"]
COPY ["Service/Service.csproj", "Service/"]
COPY ["Dto/Dto.csproj", "Dto/"]
COPY ["Dal/Dal.csproj", "Dal/"]
COPY ["Resources/Resources.csproj", "Resources/"]
COPY ["Common/Common.csproj", "Common/"]

RUN dotnet restore "SPA-Comments.Server/SPA-Comments.Server.csproj"

# ------------------- Angular -------------------
COPY ["spa-comments.client/", "spa-comments.client/"]
WORKDIR /src/spa-comments.client
RUN npm ci --legacy-peer-deps
RUN npm run build -- --configuration production --output-path=dist

# ------------------- Копируем исходники бэка -------------------
WORKDIR /src
COPY . .

# ------------------- Сборка и публикация .NET приложения -------------------
WORKDIR "/src/SPA-Comments.Server"
RUN dotnet build "SPA-Comments.Server.csproj" -c Release -o /app/build
RUN dotnet publish "SPA-Comments.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =============== Финальный слой ===============
FROM base AS final
WORKDIR /app

# Копируем опубликованные файлы
COPY --from=build /app/publish ./ 

# Копируем Angular фронтенд
COPY --from=build /src/spa-comments.client/dist/browser/ /app/wwwroot/

# Создаем папку для шрифтов и копируем Roboto
RUN mkdir -p wwwroot/fonts
COPY ["SPA-Comments.Server/Fonts/Roboto.ttf", "wwwroot/fonts/Roboto.ttf"]

ENTRYPOINT ["dotnet", "SPA-Comments.Server.dll"]
