# Base stage for runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first to cache restore
COPY ["NovaSaaSWebAPI/NovaSaaS.WebApi.csproj", "NovaSaaSWebAPI/"]
COPY ["NovaSaaS.Infrastructure/NovaSaaS.Infrastructure.csproj", "NovaSaaS.Infrastructure/"]
COPY ["NovaSaaS.Application/NovaSaaS.Application.csproj", "NovaSaaS.Application/"]
COPY ["NovaSaaS.Domain/NovaSaaS.Domain.csproj", "NovaSaaS.Domain/"]

# Restore dependencies
RUN dotnet restore "NovaSaaSWebAPI/NovaSaaS.WebApi.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/NovaSaaSWebAPI"
RUN dotnet build "NovaSaaS.WebApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "NovaSaaS.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NovaSaaS.WebApi.dll"]
