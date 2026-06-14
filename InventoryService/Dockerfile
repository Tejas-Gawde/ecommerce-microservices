FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["InventoryService/InventoryService.csproj", "InventoryService/"]
RUN dotnet restore "InventoryService/InventoryService.csproj"

COPY . .
WORKDIR /src/InventoryService
RUN dotnet build "InventoryService.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR /src/InventoryService
RUN dotnet publish "InventoryService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create non-root user (let system assign UID)
RUN useradd -m appuser && \
    chown -R appuser:appuser /app
USER appuser

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InventoryService.dll"]
