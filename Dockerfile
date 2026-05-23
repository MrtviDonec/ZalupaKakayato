# Этап 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем ВСЕ файлы проекта (не только csproj)
COPY . .

# Восстанавливаем зависимости (указываем конкретный csproj)
RUN dotnet restore "NetworkWorm.Server/NetworkWorm.Server.csproj"

# Собираем приложение
RUN dotnet publish "NetworkWorm.Server/NetworkWorm.Server.csproj" -c Release -o /app/publish

# Этап 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Устанавливаем curl для healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Копируем собранное приложение
COPY --from=build /app/publish .

# Создаем не-root пользователя
RUN useradd -m -u 1000 appuser && chown -R appuser /app
USER appuser

EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "NetworkWorm.Server.dll"]
