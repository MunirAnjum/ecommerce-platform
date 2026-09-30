FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY InventoryService.API/InventoryService.API.csproj InventoryService.API/
COPY InventoryService.Application/InventoryService.Application.csproj InventoryService.Application/
COPY InventoryService.Domain/InventoryService.Domain.csproj InventoryService.Domain/
COPY InventoryService.Infrastructure/InventoryService.Infrastructure.csproj InventoryService.Infrastructure/

RUN dotnet restore InventoryService.API/InventoryService.API.csproj

COPY . .

RUN dotnet publish InventoryService.API/InventoryService.API.csproj \
    -c Release \
    -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "InventoryService.API.dll"]