FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["OrdersService/OrdersService.csproj", "OrdersService/"]
RUN dotnet restore "OrdersService/OrdersService.csproj"

COPY . .
WORKDIR /src/OrdersService
RUN dotnet build "OrdersService.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR /src/OrdersService
RUN dotnet publish "OrdersService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

RUN useradd -m appuser && \
    chown -R appuser:appuser /app
USER appuser

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrdersService.dll"]