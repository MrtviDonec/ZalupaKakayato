# Укажите linux контейнер, а не windows
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["NetworkWorm.Server/NetworkWorm.Server.csproj", "NetworkWorm.Server/"]
RUN dotnet restore "NetworkWorm.Server/NetworkWorm.Server.csproj"

COPY . .
WORKDIR "/src/NetworkWorm.Server"
RUN dotnet build "NetworkWorm.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetworkWorm.Server.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetworkWorm.Server.dll"]
