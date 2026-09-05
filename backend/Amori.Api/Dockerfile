FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /build

# Restore dependencies first (layer cache optimisation)
COPY Amori.Api.csproj ./
RUN dotnet restore Amori.Api.csproj

# Copy source and publish
COPY . .
RUN dotnet publish Amori.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Amori.Api.dll"]
