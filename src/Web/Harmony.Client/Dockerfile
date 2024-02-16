#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Web/Harmony.Client/Harmony.Client.csproj", "Web/Harmony.Client/"]
COPY ["src/Web/Harmony.Client.Infrastructure/Harmony.Client.Infrastructure.csproj", "Web/Harmony.Client.Infrastructure/"]
COPY ["src/Core/Harmony.Application/Harmony.Application.csproj", "Core/Harmony.Application/"]
COPY ["src/Shared/Harmony.Shared/Harmony.Shared.csproj", "Shared/Harmony.Shared/"]
COPY ["src/Core/Harmony.Domain/Harmony.Domain.csproj", "Core/Harmony.Domain/"]

RUN dotnet restore "Web/Harmony.Client/Harmony.Client.csproj"
COPY src/ .
WORKDIR "/src/Web/Harmony.Client"
RUN dotnet build "Harmony.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Harmony.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM nginx:alpine
WORKDIR /app
EXPOSE 80
COPY src/Web/Harmony.Client/nginx.conf /etc/nginx/nginx.conf
COPY --from=publish /app/publish/wwwroot /usr/share/nginx/html