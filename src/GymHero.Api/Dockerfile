# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/GymHero.Api/GymHero.Api.csproj", "GymHero.Api/"]
COPY ["src/GymHero.Application/GymHero.Application.csproj", "GymHero.Application/"]
COPY ["src/GymHero.Domain/GymHero.Domain.csproj", "GymHero.Domain/"]
COPY ["src/GymHero.Infrastructure/GymHero.Infrastructure.csproj", "GymHero.Infrastructure/"]

RUN dotnet restore "GymHero.Api/GymHero.Api.csproj"

# Copy the rest of the code
COPY src/ .

# Build the application
WORKDIR "/src/GymHero.Api"
RUN dotnet build "GymHero.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "GymHero.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install FFmpeg for video thumbnail generation
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    rm -rf /var/lib/apt/lists/*

# Verify FFmpeg installation
RUN ffmpeg -version

EXPOSE 80
EXPOSE 443

# Copy published application
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "GymHero.Api.dll"]
