# === build stage ===
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY UniverseLabs.sln ./
COPY Models/Models.csproj Models/
COPY Migrations/Migrations.csproj Migrations/
COPY WebApi/WebApi.csproj WebApi/

RUN dotnet restore WebApi/WebApi.csproj

COPY . .
RUN dotnet publish WebApi/WebApi.csproj -c Release -o /app/publish

# === runtime stage ===
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WebApi.dll"]



