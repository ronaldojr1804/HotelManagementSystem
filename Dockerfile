# Base image for the runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Install Node.js for Tailwind CSS build
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# Copy csproj and restore
COPY ["HotelManagementSystem.Web/HotelManagementSystem.Web.csproj", "HotelManagementSystem.Web/"]
RUN dotnet restore "./HotelManagementSystem.Web/HotelManagementSystem.Web.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/HotelManagementSystem.Web"
RUN dotnet build "./HotelManagementSystem.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HotelManagementSystem.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
# Ensure globalization is enabled and set to pt-BR
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LC_ALL=pt_BR.UTF-8
ENV LANG=pt_BR.UTF-8
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HotelManagementSystem.Web.dll"]
