# URL Shortener 

Monorepo starter for a .NET 9 Web API + PostgreSQL (Docker).

## Run locally (Docker)

```bash
docker compose up -d --build
```

API:
- Health: `http://localhost:5000/health`
- Ping: `http://localhost:5000/api/ping`

## Run API locally (without Docker for API)

Start Postgres only:

```bash
docker compose up -d db
```

Then run API:

```bash
dotnet run --project backend/src/UrlShortener.Api/UrlShortener.Api.csproj
```
