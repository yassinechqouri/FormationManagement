# syntax=docker/dockerfile:1

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first and restore, so Docker can cache the restore layer
# as long as the .csproj files themselves don't change.
COPY FormationManagement.Domain/FormationManagement.Domain.csproj FormationManagement.Domain/
COPY FormationManagement.Application/FormationManagement.Application.csproj FormationManagement.Application/
COPY FormationManagement.Infrastructure/FormationManagement.Infrastructure.csproj FormationManagement.Infrastructure/
COPY FormationManagement.Web/FormationManagement.Web.csproj FormationManagement.Web/
RUN dotnet restore FormationManagement.Web/FormationManagement.Web.csproj

# Cache-busting: Railway automatically populates RAILWAY_GIT_COMMIT_SHA as a
# build arg. Referencing it in a RUN step forces Docker to invalidate every
# layer below on every new commit, guaranteeing the build never silently
# reuses stale compiled output from a previous deployment.
ARG RAILWAY_GIT_COMMIT_SHA=local
RUN echo "Building commit: ${RAILWAY_GIT_COMMIT_SHA}"

# Copy everything else and publish.
COPY . .
RUN dotnet publish FormationManagement.Web/FormationManagement.Web.csproj -c Release -o /app/publish --no-restore

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

# Railway/Render inject PORT at container runtime. Using shell-form CMD so
# $PORT is expanded when the container actually starts (falls back to 8080
# locally). ASPNETCORE_ENVIRONMENT=Production ensures appsettings.Production.json
# (Postgres connection) is loaded regardless of what the host platform sets.
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
CMD ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet FormationManagement.Web.dll