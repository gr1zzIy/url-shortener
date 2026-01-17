# URL Shortener (GlassLink)

Pet project: modern URL shortener with analytics and clean UX, built to demonstrate
production-style architecture, authentication, and data tracking.

## Tech stack

### Backend
- .NET 9 (ASP.NET Core Web API)
- Entity Framework Core
- PostgreSQL
- ASP.NET Identity + JWT
- Docker / Docker Compose

### Frontend
- React + TypeScript
- Vite
- Tailwind CSS
- shadcn/ui (glass-style components)
- Framer Motion
- Recharts (analytics)

## What this project is about

- Create and manage short links
- Track clicks and unique visitors
- Analytics by time, device, browser, OS, country
- Authentication (register / login / reset password)
- Focus on clean architecture and production-ready patterns

This is a **portfolio pet project**, not a commercial service.

## Run locally (Docker)

Start everything (API + database):

```bash
docker compose up -d --build
```

API endpoints:
- Health: http://localhost:5000/health
- API base: http://localhost:5000/api
- Swagger: http://localhost:5000/swagger/index.html
- 
## Run API locally (without Docker for API)

Start only PostgreSQL:

```bash
docker compose up -d db
```

Run backend manually:

```bash
dotnet run --project backend/src/UrlShortener.Api/UrlShortener.Api.csproj
```

## Run Frontend locally

From the `frontend` folder:

```bash
npm ci
npm run dev
```

Default Vite dev URL:
- http://localhost:5173
