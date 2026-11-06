# Multi-stage build for IgloReservation.Api (.NET 10).
# Build context = this folder (src/IgloReservation.Api), e.g.:
#   docker build -t iglo-api .
# Cloud Run continuous deploy: Dockerfile path src/IgloReservation.Api/Dockerfile

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY IgloReservation.Api.csproj .
RUN dotnet restore IgloReservation.Api.csproj

COPY . .
RUN dotnet publish IgloReservation.Api.csproj \
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
