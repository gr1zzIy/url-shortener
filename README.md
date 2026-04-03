# GlassLink Backend - URL Shortener API with Analytics

GlassLink is a backend-only project that provides a URL shortener API with authentication, analytics, and refresh-token based sessions.

The project follows clean architecture and covers the full backend flow: from database schema and EF Core migrations to API endpoints and CI.

---

## What This API Provides

- Short URL creation and management
- Redirect handling and click counting
- Link analytics (countries, devices, browsers, OS, recent clicks)
- Authentication flows (register, login, password reset)
- JWT-protected endpoints

---

## Tech Stack

- .NET 9 (ASP.NET Core Web API)
- Entity Framework Core
- PostgreSQL
- ASP.NET Identity + JWT
- Serilog
- Docker / Docker Compose
- GitHub Actions (CI)

---

## Project Structure

```text
backend/
  src/
	UrlShortener.Api/
	UrlShortener.Application/
	UrlShortener.Domain/
	UrlShortener.Infrastructure/
```

---

## Run with Docker

```bash
docker compose up -d --build
```

API endpoints:
- `http://localhost:5000/health`
- `http://localhost:5000/api`

Note: in container mode the API applies EF Core migrations automatically on startup.

---

## Run Backend Locally (without Docker)

```bash
cd backend
dotnet restore UrlShortener.sln
dotnet run --project src/UrlShortener.Api
```

Default local connection string is configured in `backend/src/UrlShortener.Api/appsettings.json` and expects PostgreSQL on `localhost:5432`.

---

## Apply Migrations Manually

```bash
DOTNET_ROOT="$HOME/.dotnet" DOTNET_ROOT_ARM64="$HOME/.dotnet" ~/.dotnet/tools/dotnet-ef database update \
  --project backend/src/UrlShortener.Infrastructure/UrlShortener.Infrastructure.csproj \
  --startup-project backend/src/UrlShortener.Api/UrlShortener.Api.csproj \
  --context AppDbContext
```

Migrations are located in `backend/src/UrlShortener.Infrastructure/Persistence/Migrations`.

---

## Run Tests

```bash
dotnet test backend/UrlShortener.sln -c Release
```

---

## Planned Improvements

- OAuth login
- Public analytics API endpoints
- Rate limiting
- Admin API surface
- E2E tests

---

### Author: Oleksii Ishchenko

