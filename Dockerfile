# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["PharmacyStock.API/PharmacyStock.API.csproj", "PharmacyStock.API/"]
COPY ["PharmacyStock.Application/PharmacyStock.Application.csproj", "PharmacyStock.Application/"]
COPY ["PharmacyStock.Domain/PharmacyStock.Domain.csproj", "PharmacyStock.Domain/"]
COPY ["PharmacyStock.Infrastructure/PharmacyStock.Infrastructure.csproj", "PharmacyStock.Infrastructure/"]
RUN dotnet restore "PharmacyStock.API/PharmacyStock.API.csproj"

# Copy everything else and publish directly
COPY . .
WORKDIR "/src/PharmacyStock.API"
RUN dotnet publish "PharmacyStock.API.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PharmacyStock.API.dll"]
