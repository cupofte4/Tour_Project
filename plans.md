# plans.md

# Tourism Guide App - Updated Implementation Plan

Last updated: 2026-04-24

This file tracks the current active web/API work. The old .NET MAUI mobile plan is kept out of scope for now because the active repo focus is:

- `backend/`: ASP.NET Core Web API
- `frontend/`: React + Vite web app
- `database/`: MySQL bootstrap/database assets
- root Docker/deployment files

Ignore `src/mobile/` unless a future task explicitly asks for mobile work.

---

## 1. Current Stack

- Backend: ASP.NET Core Web API, EF Core
- Database: MySQL via Pomelo EF Core provider
- Frontend: React 18 + Vite, React Router, Leaflet map
- Auth: JWT bearer token for admin/manager flows
- Dev infra: Docker Compose for MySQL, backend/frontend Dockerfiles for deployment
- Analytics: backend event/heartbeat tables plus frontend tracking helpers

---

## 2. Current Product Direction

The project is now a web-based tourism guide/admin system, not only a mobile food-street MVP.

Main user-facing goals:

- visitors can browse guided tours and tour locations
- visitors can use a map, GPS/demo location, audio narration, favorites, and route links
- admin can manage locations, tours, users, manager assignments, uploads, and analytics
- manager can manage assigned locations and view statistics

---

## 3. Implemented Tasks

### 3.1 Backend Foundation - Done

- ASP.NET Core API project exists under `backend/`.
- EF Core is wired through `AppDbContext`.
- MySQL provider is configured through Pomelo.
- `docker-compose.yml` provides a local MySQL 8 container.
- Swagger/OpenAPI is enabled.
- Static files are enabled for uploaded assets.
- App startup runs migrations with retry logic.
- Initial admin seed exists.

Main files:

- `backend/Program.cs`
- `backend/Data/AppDbContext.cs`
- `backend/Migrations/`
- `docker-compose.yml`

### 3.2 Auth And Role Routing - Done

- Backend login/change-password flow exists.
- JWT service exists.
- Frontend auth service stores and reads auth state.
- API fetch wrapper attaches JWT token automatically.
- Protected route guard exists in `frontend/src/App.jsx`.
- Admin and manager dashboards are role-gated.
- Logged-in admin/manager users are redirected away from login/register pages.

Main files:

- `backend/Controllers/AuthController.cs`
- `backend/Services/JwtService.cs`
- `frontend/src/App.jsx`
- `frontend/src/services/api.js`
- `frontend/src/services/authService.js`

### 3.3 Location Management - Done

- Location CRUD endpoints exist.
- Admin can manage locations.
- Manager can manage assigned locations.
- Locations support text in multiple languages, contact fields, coordinates, image/gallery fields, and review JSON.
- Upload endpoint exists for location images.

Main files:

- `backend/Controllers/LocationController.cs`
- `backend/Controllers/LocationsController.cs`
- `backend/Controllers/ManagerLocationsController.cs`
- `backend/Controllers/UploadController.cs`
- `frontend/src/pages/AdminDashboard.jsx`
- `frontend/src/pages/ManagerDashboard.jsx`
- `frontend/src/services/locationService.js`
- `frontend/src/services/managerService.js`
- `frontend/src/services/uploadService.js`

### 3.4 Map, GPS, And Geofence - Mostly Done

- Leaflet-based map component exists.
- Home page supports map selection and tour-location display.
- Browser geolocation support exists with HTTPS/localhost checks.
- Demo GPS mode exists for presentation and local testing.
- Haversine distance logic exists on frontend.
- Geofence debounce exists.
- Per-POI audio cooldown exists.
- Backend geofence controller exists with check endpoints.

Main files:

- `frontend/src/components/MapView.jsx`
- `frontend/src/pages/Home.jsx`
- `frontend/src/hooks/useGeolocation.js`
- `backend/Controllers/GeofenceController.cs`
- `backend/Services/LocationService.cs`
- `backend/Services/ILocationService.cs`

Remaining:

- tighten behavior between frontend geofence logic and backend geofence response shape
- add automated tests for distance/geofence logic

### 3.5 Audio Narration - Mostly Done

- TTS service exists.
- Audio queue service exists to prevent overlapping playback.
- Audio player UI exists.
- Home page uses narration for selected tour locations.
- Admin/manager pages can preview narration content.
- Audio play analytics tracking exists.

Main files:

- `frontend/src/services/ttsService.js`
- `frontend/src/services/audioQueueService.js`
- `frontend/src/components/AudioPlayer.jsx`
- `frontend/src/pages/Home.jsx`
- `frontend/src/services/analyticsService.js`

Remaining:

- polish play/pause/stop states across all flows
- confirm language fallback behavior for unsupported browser voices

### 3.6 Tours And Tour Sessions - Done

- Tour CRUD endpoints exist.
- Tour-location assignment/reorder endpoints exist.
- Tour sessions can be started, inspected, visited, and completed.
- Public tour list/detail pages exist.
- Admin tour management panel exists.
- Home page can load tour locations.

Main files:

- `backend/Controllers/ToursController.cs`
- `backend/Controllers/TourSessionsController.cs`
- `frontend/src/pages/TourList.jsx`
- `frontend/src/pages/TourDetail.jsx`
- `frontend/src/components/AdminToursPanel.jsx`
- `frontend/src/services/tourService.js`

### 3.7 Admin Dashboard - Mostly Done

- Admin dashboard includes overview, map, tours, users, managers, assignments, and locations.
- User management supports admin updates/locking.
- Manager assignment flow exists.
- Analytics summary loads and refreshes.
- Image upload/gallery support exists.
- Map tab exists for admin location visibility.

Main files:

- `frontend/src/pages/AdminDashboard.jsx`
- `frontend/src/components/AdminNavbar.jsx`
- `frontend/src/components/AdminToursPanel.jsx`
- `backend/Controllers/UsersController.cs`
- `backend/Controllers/AdminAssignmentsController.cs`
- `backend/Controllers/AdminAnalyticsController.cs`

Remaining:

- improve validation and empty states
- review role/security enforcement on backend endpoints

### 3.8 Manager Dashboard - Mostly Done

- Manager dashboard is role-gated.
- Manager can view assigned locations.
- Manager can edit/upload gallery/narration text for managed locations.
- Manager map tab exists.
- Manager statistics totals and time series exist.

Main files:

- `frontend/src/pages/ManagerDashboard.jsx`
- `backend/Controllers/ManagerLocationsController.cs`
- `backend/Controllers/ManagerStatsController.cs`
- `frontend/src/services/managerService.js`

Remaining:

- improve authorization hardening on backend manager endpoints
- add chart polish and clearer loading/error states

### 3.9 Analytics - Done / In Progress

- App heartbeat tracking exists.
- Generic analytics event endpoint exists.
- Audio play tracking exists.
- Favorite click tracking exists.
- Visitor device tracking exists.
- Admin analytics summary/chart endpoints exist.
- Frontend heartbeat hook exists and is used for public routes.

Main files:

- `backend/Controllers/AnalyticsController.cs`
- `backend/Controllers/AdminAnalyticsController.cs`
- `backend/Services/AnalyticsService.cs`
- `backend/Application/Analytics/`
- `backend/Models/AppUsageHeartbeat.cs`
- `backend/Models/AnalyticsEvent.cs`
- `backend/Models/VisitorDevice.cs`
- `backend/Models/LocationFavoriteState.cs`
- `frontend/src/hooks/useHeartbeat.js`
- `frontend/src/services/analyticsService.js`
- `frontend/src/services/deviceId.js`

Remaining:

- verify all tracking calls are firing where expected
- add tests for analytics aggregation

### 3.10 Favorites - Done

- Frontend favorites service exists.
- Favorites page exists.
- Favorite click/state analytics models exist.
- Device ID support exists for guest identification.

Main files:

- `frontend/src/pages/Favorites.jsx`
- `frontend/src/services/favoritesService.js`
- `frontend/src/services/deviceId.js`
- `backend/Models/LocationFavoriteState.cs`

Remaining:

- confirm backend persistence contract for favorites is complete
- align favorites behavior across guest sessions/devices

### 3.11 Deployment Artifacts - Done / In Progress

- Backend Dockerfile exists.
- Frontend Dockerfile exists.
- Frontend nginx config exists.
- Deployment compose file exists.
- Deployment scripts exist.

Main files:

- `backend/Dockerfile`
- `frontend/Dockerfile`
- `frontend/nginx.conf`
- `frontend/nginx.ec2.conf`
- `docker-compose.ec2.yml`
- `deploy-backend.sh`
- `deploy-frontend.sh`

Remaining:

- verify deployment scripts in a clean environment
- document required environment variables

---

## 4. Current Gaps / Next Tasks

### Priority 1 - Stabilize

- Clean generated build artifacts from source control planning, especially `backend/bin`, `backend/obj`, `frontend/dist`, `backend/bin_build`, and `backend/build-check`.
- Review `.gitignore` / `.dockerignore` so build outputs do not keep changing.
- Run full backend build and frontend build after current edits settle.
- Verify database migrations apply cleanly from an empty MySQL database.

### Priority 2 - Security And Auth Hardening

- Hash stored passwords instead of storing plaintext/admin seed passwords.
- Add or verify `[Authorize]` and role checks on admin/manager backend endpoints.
- Avoid trusting only frontend route guards for sensitive operations.
- Replace development JWT secret defaults in real deployment.

### Priority 3 - Testing

- Add backend tests for:
  - auth login failure/success
  - geofence response behavior
  - manager assignment authorization
  - analytics aggregation
  - tour ordering/session visits
- Add frontend smoke tests or manual test checklist for:
  - login redirects
  - admin location CRUD
  - manager assigned-location edits
  - tour start and POI narration
  - GPS demo mode and real GPS fallback

### Priority 4 - UX Polish

- Normalize Vietnamese/English labels across admin and manager dashboards.
- Improve loading, empty, and error states.
- Improve mobile responsiveness for map/home/dashboard pages.
- Make audio controls consistent between visitor, admin preview, and manager preview.

### Priority 5 - Feature Gaps

- QR scanner fallback is not implemented yet.
- Translation service via external provider is not implemented; current flow relies on stored multilingual text and browser TTS.
- Review system is still JSON/location-field based rather than a normalized review table.
- Standard API response wrapper is not consistently applied.

---

## 5. Updated Build Roadmap

### Phase A - Repo Hygiene

Definition of done:

- generated artifacts are ignored or intentionally documented
- clean checkout can build backend/frontend
- `plans.md` matches live repo direction

### Phase B - Auth/Security Hardening

Definition of done:

- passwords are hashed
- admin/manager APIs enforce backend authorization
- seed credentials are documented for local dev only

### Phase C - Visitor Tour Experience

Definition of done:

- tour selection, map, GPS/demo location, geofence, narration, favorites, and directions work together
- visitor flow has clear empty/error states

### Phase D - Admin/Manager Completion

Definition of done:

- admin can manage locations, tours, users, assignments, uploads, and analytics without code-level workarounds
- manager can manage only assigned content and view accurate stats

### Phase E - Analytics And Reporting

Definition of done:

- key visitor events are recorded consistently
- admin dashboard shows reliable summary and chart data
- manager statistics match assigned locations

### Phase F - Optional Enhancements

- QR scanner fallback
- external translation provider behind an interface
- normalized review table and review UI
- automated frontend tests

---

## 6. Run / Test Commands

Use these commands from the repo root unless noted otherwise.

### Start MySQL

```bash
docker compose up -d mysql
```

### Run Backend

```bash
cd backend
dotnet restore
dotnet build
dotnet run
```

Swagger is available at:

```text
http://localhost:5093/swagger
```

The exact backend URL can differ if `dotnet run` chooses another launch profile/port.

### Run Frontend

```bash
cd frontend
npm install
npm run start
```

### Build Frontend

```bash
cd frontend
npm run build
```

### Optional Environment Variables

```bash
JWT_SECRET_KEY=<at-least-32-characters>
VITE_API_ORIGIN=http://localhost:5093
```

---

## 7. Working Rules For Future Codex Tasks

- Trust live code over older notes.
- Keep edits in `backend/`, `frontend/src/`, `frontend/public/`, `database/`, and root infra files unless the task expands scope.
- Ignore `src/mobile/` unless explicitly requested.
- Keep changes small and compilable.
- Explain each modified file in the final response.
- Include exact run/test commands in the final response.
- Prefer stubs/fakes over inventing full external integrations.
- Avoid broad refactors unless they are required for the requested slice.
