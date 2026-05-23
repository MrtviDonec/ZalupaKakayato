# Этап 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj и восстанавливаем зависимости
COPY NetworkWorm.Server.csproj .
RUN dotnet restore

# Копируем остальной код
COPY . .

# Собираем приложение
RUN dotnet publish -c Release -o /app/publish

# Этап 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Устанавливаем дополнительные пакеты (опционально)
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Копируем собранное приложение
COPY --from=build /app/publish .

# Создаем не-root пользователя для безопасности
RUN useradd -m -u 1000 appuser && chown -R appuser /app
USER appuser

# Порт для приложения
EXPOSE 8080
EXPOSE 8081

# Запускаем приложение
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "NetworkWorm.Server.dll"]