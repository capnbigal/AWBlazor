# syntax=docker/dockerfile:1
# Multi-stage build for AWBlazorApp (.NET 10 Blazor Web App).
# Stage 1: restore + publish with the SDK image.
# Stage 2: copy published output into a minimal ASP.NET Core runtime image.

ARG DOTNET_VERSION=10.0

# ---- build ---------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Restore as a distinct layer — cache-friendly. Only the main .csproj is needed.
# The Tests project and .slnx are excluded from the build context by .dockerignore
# and aren't required to build the shipped image.
COPY src/AWBlazorApp/AWBlazorApp.csproj AWBlazorApp/
RUN dotnet restore AWBlazorApp/AWBlazorApp.csproj

# Copy the rest of the source and publish.
COPY src/AWBlazorApp/ AWBlazorApp/
WORKDIR /src/AWBlazorApp
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime -------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
WORKDIR /app

# Install culture + icu for MudBlazor formatters, and tzdata for Serilog timestamps.
RUN apt-get update \
 && apt-get install -y --no-install-recommends tzdata libicu-dev \
 && rm -rf /var/lib/apt/lists/*

# Non-root user for the app process.
RUN groupadd -r awblazor && useradd -r -g awblazor -m -d /home/awblazor awblazor

COPY --from=build /app/publish .

# App_Data holds DataProtection keys — must be writable + persistent (mount a volume).
RUN mkdir -p /app/App_Data && chown -R awblazor:awblazor /app

USER awblazor

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080
ENTRYPOINT ["dotnet", "AWBlazorApp.dll"]
