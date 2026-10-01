FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ApiGateway.API/ApiGateway.API.csproj ApiGateway.API/

RUN dotnet restore ApiGateway.API/ApiGateway.API.csproj

COPY . .

RUN dotnet publish ApiGateway.API/ApiGateway.API.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ApiGateway.API.dll"]