# Frontend (Vanilla)

Minimal static frontend built with plain HTML, CSS and JavaScript.

## Run

```bash
cd frontend
npm run dev
```

Open `http://localhost:5173`.

The API base URL defaults to `http://localhost:5000` and can be changed in browser console:

```js
localStorage.setItem("apiBaseUrl", "http://localhost:5000");
location.reload();
```

## Structure

- `index.html` - main app shell
- `reset-password/index.html` - password reset page from backend link
- `assets/styles.css` - shared style system (light/dark themes)
- `assets/app.js` - app orchestration and event wiring
- `assets/constants.js` - shared view/tab/action keys
- `assets/i18n.js` - active locale helpers and DOM translation
- `assets/layout.js` - app shell markup and element collection
- `assets/http.js` - API requests + refresh token retry logic
- `assets/messages.en.js` / `assets/messages.uk.js` - English and Ukrainian copy
- `assets/storage.js` - access token and theme persistence helpers
- `assets/types.js` - JSDoc typedefs for editor hints
- `assets/ui.js` - rendering and notification helpers
- `assets/config.js` - API base/origin config
- `assets/pages/auth.js` - auth/reset/logout page handlers
- `assets/pages/dashboard.js` - dashboard CRUD/search handlers
- `assets/features/analytics.js` - analytics modal behavior
- `assets/features/events.js` - central event binding for all UI handlers
- `assets/features/session.js` - bootstrap and view/tab session handlers

## Check

```bash
npm run check
```

## Notes

- No Vite, React, TypeScript or Tailwind.
- Theme toggle persists in `localStorage`.
- Language toggle persists in `localStorage` and supports `en` / `uk`.
- Access token is stored in `sessionStorage`, refresh token stays in httpOnly cookie.

