# STEP 1: Build (context must be ./Api so Shared is available)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Shared/CashFlow.Shared.csproj Shared/
COPY Gateway/Gateway.csproj Gateway/
RUN dotnet restore Gateway/Gateway.csproj

COPY Shared/ Shared/
COPY Gateway/ Gateway/
RUN dotnet publish Gateway/Gateway.csproj -c Release -o /app/out

# STEP 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out ./
EXPOSE 80

ENTRYPOINT ["dotnet", "Gateway.dll"]
