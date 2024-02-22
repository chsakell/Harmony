#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Harmony.Api/Harmony.Api.csproj", "Services/Harmony.Api/"]
COPY ["src/Core/Harmony.Application/Harmony.Application.csproj", "Core/Harmony.Application/"]
COPY ["src/Shared/Harmony.Shared/Harmony.Shared.csproj", "Shared/Harmony.Shared/"]
COPY ["src/Core/Harmony.Domain/Harmony.Domain.csproj", "Core/Harmony.Domain/"]
COPY ["src/Infrastructure/Harmony.Infrastructure/Harmony.Infrastructure.csproj", "Infrastructure/Harmony.Infrastructure/"]
COPY ["src/Infrastructure/Harmony.Persistence/Harmony.Persistence.csproj", "Infrastructure/Harmony.Persistence/"]
COPY ["src/Infrastructure/Harmony.Messaging/Harmony.Messaging.csproj", "Infrastructure/Harmony.Messaging/"]
RUN dotnet restore "Services/Harmony.Api/Harmony.Api.csproj"
COPY src/ .
WORKDIR "/src/Services/Harmony.Api"
RUN dotnet build "Harmony.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Harmony.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Harmony.Api.dll"]