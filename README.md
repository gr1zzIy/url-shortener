# GlassLink — URL Shortener with Analytics

GlassLink is a project that demonstrates URL shortening service with authentication, analytics, and a glass-morphism UI.

The project focuses on **clean architecture**, **patterns**, and
**end-to-end ownership**: from database schema and backend services to UI/UX and CI.

---

## What GlassLink Can Be Used For

- Creating short links for sharing
- Tracking link usage over time
- Analyzing user behavior (countries, devices, browsers, OS)
- Demonstrating authentication flows (login, register, password recovery)
- Showcasing a full-stack architecture

---

## Tech Stack

### Backend
- .NET 9 (ASP.NET Core Web API)
- Entity Framework Core
- PostgreSQL
- ASP.NET Identity + JWT
- Serilog
- Docker / Docker Compose

### Frontend
- React + TypeScript
- Vite
- Tailwind CSS
- shadcn/ui (glass-style components)
- Framer Motion
- Recharts

### Dev & Infrastructure
- GitHub Actions (CI)
- Dockerized local environment
- Monorepo structure

---

## Authentication & User Flow

**Implemented flows:**
- User registration
- Login / Logout
- Password recovery
- JWT-protected routes

### **Screenshots**
- `docs/screenshots/auth-login.png`
- `docs/screenshots/auth-register.png`
- `docs/screenshots/auth-forgot-password.png`

---

## URL Management

**Features:**
- Create short URLs
- Activate / deactivate links
- Delete with confirmation
- Expiration handling
- Click counters

### **Screenshot**
- `docs/screenshots/dashboard-links.png`

---

## Analytics & Statistics

**Each link includes:**
- Total clicks
- Unique visitors
- Clicks over time (chart)
- Country / Device / browser / OS breakdown
- Recent clicks list

### **Screenshots**
- `docs/screenshots/analytics-overview.png`
- `docs/screenshots/analytics-recent-clicks.png`

---

## UI & UX

- Glass-morphism design
- Smooth animations
- Responsive layout
- Theme presets

### **Screenshots**
- `docs/screenshots/theme-light.png`
- `docs/screenshots/theme-balanced.png`
- `docs/screenshots/theme-dark.png`

---

### **Other screenshots**

**Modals:**
- `docs/screenshots/deactivate-modal.png`
- `docs/screenshots/delete-modal.png`
- `docs/screenshots/error-page.png`
- `docs/screenshots/search.png`

---

## Run Locally (Docker)

```bash
docker compose up -d --build
```

API:
- http://localhost:5000/health
- http://localhost:5000/api

---

## Run Frontend

```bash
cd frontend
npm ci
npm run dev
```

Frontend:
- http://localhost:5173

---

## Planned Improvements

- OAuth login
- Public analytics pages
- Rate limiting
- Admin dashboard
- E2E tests

---

### Author: Oleksii Ishchenko

