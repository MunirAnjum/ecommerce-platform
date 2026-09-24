FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY PaymentService.API/PaymentService.API.csproj PaymentService.API/
COPY PaymentService.Application/PaymentService.Application.csproj PaymentService.Application/
COPY PaymentService.Domain/PaymentService.Domain.csproj PaymentService.Domain/
COPY PaymentService.Infrastructure/PaymentService.Infrastructure.csproj PaymentService.Infrastructure/

RUN dotnet restore PaymentService.API/PaymentService.API.csproj

COPY . .

RUN dotnet publish PaymentService.API/PaymentService.API.csproj \
    -c Release \
    -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PaymentService.API.dll"]