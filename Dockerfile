# 1️⃣ Build bosqichi
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish GitHubToTelegramBot.csproj -c Release -o /app

# 2️⃣ Runtime bosqichi
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "GitHubToTelegramBot.dll"]
