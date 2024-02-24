#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/ApiGateways/Harmony.ApiGateway/Harmony.ApiGateway.csproj", "ApiGateways/Harmony.ApiGateway/"]
RUN dotnet restore "ApiGateways/Harmony.ApiGateway/Harmony.ApiGateway.csproj"
COPY src/ .
COPY ["src/ApiGateways/Harmony.ApiGateway/ocelot.docker.json", "ApiGateways/Harmony.ApiGateway/ocelot.json"] 
WORKDIR "/src/ApiGateways/Harmony.ApiGateway"
RUN dotnet build "Harmony.ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Harmony.ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Harmony.ApiGateway.dll"]