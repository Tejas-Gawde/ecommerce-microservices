FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["CatalogService/CatalogService.csproj", "CatalogService/"]
RUN dotnet restore "CatalogService/CatalogService.csproj"

# Copy everything and build
COPY . .
WORKDIR /src/CatalogService
RUN dotnet build "CatalogService.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
WORKDIR /src/CatalogService
RUN dotnet publish "CatalogService.csproj" -c Release -o /app/publish

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create non-root user (let system assign UID)
RUN useradd -m appuser && \
    chown -R appuser:appuser /app
USER appuser

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CatalogService.dll"]
