# GlassLink Backend - URL Shortener API with Analytics

GlassLink is a backend-only URL shortener API with authentication, analytics, and refresh-token sessions.

The project follows clean architecture and covers the full backend flow: domain model, EF Core mappings/migrations, API endpoints, and CI.

---

## Features

- Create, list, deactivate, and soft-delete short links
- Redirect by short code with click tracking
- Analytics: totals, unique visitors, breakdowns, recent clicks
- Auth flows: register, login, refresh, logout, password reset
- JWT-protected endpoints and ProblemDetails-based error responses

---

## Tech Stack

- .NET 9 (ASP.NET Core Web API)
- Entity Framework Core + PostgreSQL
- ASP.NET Identity + JWT
- Serilog
- Docker / Docker Compose
- GitHub Actions CI

---

## Project Structure

```text
backend/
  src/
	UrlShortener.Api/
	UrlShortener.Application/
	UrlShortener.Domain/
	UrlShortener.Infrastructure/
  tests/
    UrlShortener.Tests/
```

---

## Run with Docker

This repository uses a backend-only Compose project name to avoid conflicts with older full-stack stacks.

```bash
cd /Users/oleksii/Repos/url-shortener
docker compose up -d --build
```

Current Docker mapping:

- API: `http://localhost:5001`
- DB: `localhost:5433`

Useful endpoints:

- `http://localhost:5001/health`
- `http://localhost:5001/ready`
- `http://localhost:5001/info`
- `http://localhost:5001/version`
- `http://localhost:5001/swagger/index.html`

Notes:

- In `Development` and `Staging`, `GET /` redirects to Swagger UI.
- In other environments, `GET /` returns a small JSON status object.
- In container mode, EF Core migrations are applied automatically on startup.

---

## Run Locally (without Docker)

```bash
cd /Users/oleksii/Repos/url-shortener/backend
dotnet restore UrlShortener.sln
dotnet run --project src/UrlShortener.Api
```

By default, local `appsettings.json` expects PostgreSQL on `localhost:5432`.

---

## Apply Migrations Manually

```bash
DOTNET_ROOT="$HOME/.dotnet" DOTNET_ROOT_ARM64="$HOME/.dotnet" ~/.dotnet/tools/dotnet-ef database update \
  --project backend/src/UrlShortener.Infrastructure/UrlShortener.Infrastructure.csproj \
  --startup-project backend/src/UrlShortener.Api/UrlShortener.Api.csproj \
  --context AppDbContext
```

Migrations are in:

- `backend/src/UrlShortener.Infrastructure/Persistence/Migrations`

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

