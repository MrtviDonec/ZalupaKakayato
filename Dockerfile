# Этап 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем ВСЕ файлы проекта (не только csproj)
COPY . .

# Восстанавливаем зависимости
RUN dotnet restore

# Собираем приложение
RUN dotnet publish -c Release -o /app/publish

# Этап 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Копируем собранное приложение
COPY --from=build /app/publish .

# Создаем не-root пользователя для безопасности
RUN useradd -m -u 1000 appuser && chown -R appuser /app
USER appuser

# Порт для приложения
EXPOSE 8080

# Запускаем приложение
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "NetworkWorm.Server.dll"]
