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
<img width="1919" height="899" alt="image" src="https://github.com/user-attachments/assets/33167ab3-d98c-488c-a66d-a8b9ea1b72f7" />

- `docs/screenshots/auth-register.png`
<img width="1919" height="900" alt="image" src="https://github.com/user-attachments/assets/2ea30028-70e5-4d0d-93ac-147de2bfbe3b" />

- `docs/screenshots/auth-forgot-password.png`
<img width="1919" height="897" alt="image" src="https://github.com/user-attachments/assets/ac7d20e0-41c9-47b3-a300-a5deb371c387" />

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
<img width="1919" height="897" alt="image" src="https://github.com/user-attachments/assets/4a3028fc-803d-4970-bf1a-65b4b434a81d" />

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
<img width="1919" height="899" alt="image" src="https://github.com/user-attachments/assets/086c8189-d5f0-4c7e-a7f2-6353b06a1d14" />

- `docs/screenshots/analytics-recent-clicks.png`
<img width="1919" height="904" alt="image" src="https://github.com/user-attachments/assets/0439b109-ae35-4dc4-a698-7ca17ec7d5dd" />

---

## UI & UX

- Glass-morphism design
- Smooth animations
- Responsive layout
- Theme presets

### **Screenshots**
- `docs/screenshots/theme-light.png`
<img width="1919" height="901" alt="image" src="https://github.com/user-attachments/assets/bde2f3ff-3acc-4b6b-b1db-79b43d0c8755" />

- `docs/screenshots/theme-balanced.png`
<img width="1919" height="899" alt="image" src="https://github.com/user-attachments/assets/8cdd6535-927f-45de-bef2-b9b4900b0dbc" />

- `docs/screenshots/theme-dark.png`
<img width="1919" height="902" alt="image" src="https://github.com/user-attachments/assets/8f6ce302-7043-4788-a617-a56835d74420" />

---

### **Other screenshots**

**Modals:**
- `docs/screenshots/deactivate-modal.png`
<img width="1918" height="880" alt="image" src="https://github.com/user-attachments/assets/657a6656-f810-4517-82b1-110d0b7ad6cf" />

- `docs/screenshots/delete-modal.png`
<img width="1919" height="890" alt="image" src="https://github.com/user-attachments/assets/61f89fc2-4c95-49fd-b70a-ecd408353818" />

- `docs/screenshots/error-page.png`
<img width="1919" height="894" alt="image" src="https://github.com/user-attachments/assets/bfbbfd9d-2dee-47dc-b67c-8fcf4da2870a" />

- `docs/screenshots/search.png`
<img width="1919" height="893" alt="image" src="https://github.com/user-attachments/assets/50aa4646-7133-43ca-9e07-6a2a1ff541f0" />

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

