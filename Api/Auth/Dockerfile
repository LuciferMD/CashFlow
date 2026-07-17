# STEP 1: Build (context must be ./Api so Shared is available)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Shared/CashFlow.Shared.csproj Shared/
COPY Auth/Auth.csproj Auth/
RUN dotnet restore Auth/Auth.csproj

COPY Shared/ Shared/
COPY Auth/ Auth/
RUN dotnet publish Auth/Auth.csproj -c Release -o /app/out

# STEP 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out ./
EXPOSE 80

ENTRYPOINT ["dotnet", "Auth.dll"]
