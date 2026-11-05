# Multi-stage build for IgloReservation.Api (.NET 10).
# Build from repository root:
#   docker build -f src/IgloReservation.Api/Dockerfile -t iglo-api .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/IgloReservation.Api/IgloReservation.Api.csproj src/IgloReservation.Api/
RUN dotnet restore src/IgloReservation.Api/IgloReservation.Api.csproj

COPY src/IgloReservation.Api/ src/IgloReservation.Api/
RUN dotnet publish src/IgloReservation.Api/IgloReservation.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

# curl is used by the compose healthcheck against GET /health
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "IgloReservation.Api.dll"]
