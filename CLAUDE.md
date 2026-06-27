# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Tech Stack

- **.NET 10**, ASP.NET Core, Blazor Server, EF Core 10, PostgreSQL 16
- Clean Architecture: Domain → Application → Infrastructure → Api
- Docker Compose for full-stack deployment

## Commands

### Run with Docker (recommended)

```bash
docker compose up -d           # Start all services (API, Web, DB)
docker compose up -d --build   # Rebuild after code changes
docker compose down            # Stop all services
docker compose up -d db        # Start only the database (for local dev)
```

### Run locally

```bash
# Terminal 1 — API
cd src/BookLibrary.Api && dotnet run

# Terminal 2 — Web frontend
cd src/BookLibrary.Web && dotnet run
```

### Database migrations

```bash
# From src/BookLibrary.Infrastructure/
dotnet ef migrations add MigrationName --startup-project ../BookLibrary.Api
```

Migrations are applied automatically on API startup via `dbContext.Database.Migrate()`.

## Architecture

```
Domain          →  Entities (Book, Place), repository interfaces (no dependencies)
Application     →  DTOs, service interfaces, service implementations (depends on Domain)
Infrastructure  →  EF Core DbContext, repository implementations, migrations (depends on Domain)
Api             →  ASP.NET Core controllers, DI wiring (depends on Application + Infrastructure)
Web             →  Blazor Server pages; calls the API server-side via named HttpClient
Web.Client      →  Blazor WASM stub (scaffolded, not yet used)
```

DI is registered via extension methods: `AddApplication()` and `AddInfrastructure(config)`, both called in `Api/Program.cs`.

The Blazor Server frontend never calls the API from the browser — all HTTP calls are made server-side. `ApiBaseUrl` config key controls the base address (`http://localhost:8080` locally, `http://api:8080` in Docker).

## Local Access

| Service | URL |
|---------|-----|
| Web frontend | http://localhost:8081 |
| API + Swagger UI | http://localhost:8080 (Swagger served at root `/`) |
| API HTTPS | https://localhost:8443 |
| PostgreSQL | localhost:5433 |

## Key Configuration

- DB connection string: `ConnectionStrings__DefaultConnection` (env var or appsettings)
- HTTPS cert: `./certs/aspnetapp.pfx` mounted into the API container at `/https`
- The `api_info` column on `books` is `jsonb` in PostgreSQL — stored as `string` in the entity
