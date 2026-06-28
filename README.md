# Book Library

A full-stack .NET 10 application for managing a personal book library, featuring a REST API backend and a Blazor Server frontend, built using Clean Architecture principles.

## Features

- 📚 **Book Management** - Add, view, and delete books with details like ISBN, author, publisher, year, and page count
- 📍 **Location Tracking** - Organize books by physical location (shelves, rooms, etc.)
- 📋 **API Info Viewer** - View additional book metadata as formatted JSON in a modal
- 🔍 **Swagger API Documentation** - Interactive API exploration
- 🐳 **Docker Support** - Easy deployment with Docker Compose
- 🏗️ **Clean Architecture** - Maintainable and testable code structure
- 📱 **Responsive Design** - Mobile-friendly layout with collapsible sidebar navigation

## Architecture

The solution follows Clean Architecture with the following projects:

### Backend (API)
- **BookLibrary.Domain** - Core entities and repository interfaces
- **BookLibrary.Application** - Business logic, DTOs, and service interfaces  
- **BookLibrary.Infrastructure** - EF Core implementation, PostgreSQL, repositories
- **BookLibrary.Api** - ASP.NET Core Web API with controllers

### Frontend (Web)
- **BookLibrary.Web** - Blazor Server application using Interactive Server rendering mode
- **BookLibrary.Web.Client** - Blazor WebAssembly project (scaffolded for future use)

## Frontend

The frontend is a **Blazor Server** application that renders interactively on the server using SignalR. The browser never directly contacts the API — all HTTP calls to the backend are made server-side.

### Pages

| Route | Page | Description |
|-------|------|-------------|
| `/` | Home | Landing page with navigation cards linking to Books and Places |
| `/books` | Books | Full book management — searchable table, collapsible add form with location dropdown, delete functionality, and API info modal |
| `/places` | Places | Location management — table of places, collapsible add form, and delete functionality |

### UI & Styling

- **Bootstrap** — Bundled locally for layout, tables, forms, alerts, and modals
- **Responsive sidebar** — Collapses to a hamburger menu on screens narrower than 641px
- **Feedback messages** — Success and error alerts displayed after operations
- **Loading states** — Shows loading indicators while fetching data

### API Communication

The frontend uses a named `HttpClient` registered via `IHttpClientFactory`:

```csharp
builder.Services.AddHttpClient("BookLibraryApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]);
});
```

- In Docker: the `ApiBaseUrl` is set to `http://api:8080` (internal container DNS)
- Locally: defaults to `http://localhost:8080`
- Pages inject `HttpClient` and call REST endpoints (`/api/books`, `/api/places`) using `GetFromJsonAsync`, `PostAsJsonAsync`, and `DeleteAsync`

## Prerequisites

- [Docker](https://www.docker.com/get-started) (for containerized deployment)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (for local development)

## Quick Start

### Using Docker Compose (Recommended)

Start all services (API, Web, and PostgreSQL database):

```bash
docker compose up -d
```

Access the applications:
- **Web Frontend**: http://localhost:8081
- **API Swagger UI**: http://localhost:8080/swagger
- **API Base URL (HTTP)**: http://localhost:8080
- **API Base URL (HTTPS)**: https://localhost:8443

To stop the services:
```bash
docker compose down
```

To rebuild after code changes:
```bash
docker compose up -d --build
```

### Running Locally

1. Start PostgreSQL (or use the Docker database):
```bash
docker compose up -d db
```

2. Run the API:
```bash
cd src/BookLibrary.Api
dotnet run
```

3. Run the Web frontend (in a separate terminal):
```bash
cd src/BookLibrary.Web
dotnet run
```

## Project Structure

```
book-library-api/
├── src/
│   ├── BookLibrary.Domain/          # Entities, interfaces
│   ├── BookLibrary.Application/     # DTOs, services
│   ├── BookLibrary.Infrastructure/  # EF Core, repositories
│   ├── BookLibrary.Api/             # REST API controllers
│   └── BookLibrary.Web/             # Blazor Server frontend
├── tests/
│   └── BookLibrary.Tests/           # Unit tests (xUnit + Moq)
├── scripts/
│   ├── backup-postgres.sh           # Database backup script
│   └── restore-postgres.sh          # Database restore script
├── compose.yaml                     # Docker Compose configuration
└── BookLibrary.sln                  # Solution file
```

## Testing

Unit tests live in `tests/BookLibrary.Tests/` and use **xUnit**, **Moq**, and **FluentAssertions**.

Run the full test suite:

```bash
dotnet test
```

### Coverage

| Test class | Subject | Tests |
|---|---|---|
| `BookServiceTests` | `BookService` — all CRUD operations | 20 |
| `PlaceServiceTests` | `PlaceService` — all CRUD operations | 18 |
| `BooksControllerTests` | `BooksController` — HTTP status codes & response bodies | 10 |
| `PlacesControllerTests` | `PlacesController` — HTTP status codes & response bodies | 8 |

Each service test class covers: correct DTO mapping, not-found paths returning `null`, repository methods called with the right arguments, and `Update`/`Delete` never called when the entity doesn't exist.

## Database Backup & Restore

Two scripts in `scripts/` handle PostgreSQL backup and restore. Both work by running commands inside the running `db` container and fall back to a local `pg_dump`/`pg_restore` (via port 5433) if the container is not found.

### Backup

Creates a timestamped folder under `/srv/backups/postgres/` and writes a compressed custom-format dump:

```bash
sudo bash scripts/backup-postgres.sh
```

Output: `/srv/backups/postgres/backup_YYMMDD_HHMMSS/booklibrary.dump`

### Restore

Finds the latest backup folder automatically, then drops and recreates the `booklibrary` database before restoring:

```bash
sudo bash scripts/restore-postgres.sh
```

The script asks for confirmation before touching the database. `sudo` is required to read from `/srv/backups/postgres/` unless the directory is owned by your user.

## API Endpoints

### Books

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/books | Get all books |
| GET | /api/books/{id} | Get a book by ID |
| GET | /api/books/isbn/{isbn} | Get a book by ISBN |
| POST | /api/books | Create a new book |
| PUT | /api/books/{id} | Update a book |
| DELETE | /api/books/{id} | Delete a book |

### Places

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/places | Get all places |
| GET | /api/places/{id} | Get a place by ID |
| POST | /api/places | Create a new place |
| PUT | /api/places/{id} | Update a place |
| DELETE | /api/places/{id} | Delete a place |

## Database Schema

### books
| Column | Type | Description |
|--------|------|-------------|
| id | int | Primary key |
| isbn | varchar(50) | ISBN of the book |
| title | varchar(500) | Book title |
| author | varchar(300) | Author name |
| publisher | varchar(300) | Publisher name |
| published_year | varchar(10) | Year published |
| pagecount | int | Number of pages |
| place_id | int | FK to places table |
| api_info | jsonb | Additional JSON data |

### places
| Column | Type | Description |
|--------|------|-------------|
| id | int | Primary key |
| descr | varchar(500) | Place description |

## Database Migrations

Migrations are applied automatically on startup. To create new migrations:

```bash
cd src/BookLibrary.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../BookLibrary.Api
```

## Docker Services

| Service | Port | Description |
|---------|------|-------------|
| web | 8081 | Blazor Server frontend |
| api | 8080 | REST API (HTTP) with Swagger |
| api | 8443 | REST API (HTTPS) |
| db | 5433 | PostgreSQL database |

## Environment Variables

### API (BookLibrary.Api)
| Variable | Default | Description |
|----------|---------|-------------|
| `ConnectionStrings__DefaultConnection` | (see compose.yaml) | PostgreSQL connection string |
| `ASPNETCORE_ENVIRONMENT` | Production | Environment name |

### Web (BookLibrary.Web)
| Variable | Default | Description |
|----------|---------|-------------|
| `ApiBaseUrl` | http://api:8080 | Base URL for API calls |
| `ASPNETCORE_ENVIRONMENT` | Production | Environment name |

## Technology Stack

- **.NET 10** - Latest .NET runtime
- **ASP.NET Core** - Web framework
- **Blazor Server** - Interactive web UI
- **Entity Framework Core 10** - ORM
- **PostgreSQL 16** - Database
- **Docker** - Containerization
- **Swagger/OpenAPI** - API documentation

## License

MIT
