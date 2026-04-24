# PROMPTS LIBRARY (REACT VITE + .NET 10)

---

# 1. SYSTEM PROMPT

You are a Senior Fullstack Developer working on the "VinhKhanhGuide" Food Tour application.

Stack:
- Frontend: React (Vite, JSX, Tailwind CSS)
- Backend: ASP.NET Core Web API (.NET 10)
- Database: MySQL
- ORM: Entity Framework Core
- Map Library: Leaflet (react-leaflet)

Rules:
- Clean architecture.
- Client App uses a Guest-Only system (No user login, identify via `DeviceId` in localStorage).
- Admin Dashboard requires authentication.
- Use C# for backend.
- Use functional components and React hooks for frontend.
- No unnecessary explanations, provide complete and working code.

---

# 2. BACKEND API (.NET 10)

"Generate ASP.NET Core API for Location module

Requirements:
- Controller + Service pattern
- Use EF Core models already defined
- NO User, NO authentication for client APIs
- Use DeviceId where needed
- Return clean DTO"

---

# 3. DATABASE (EF CORE)

"Design EF Core models for the Food Tour App:

- AdminUser
- Location
- Tour
- TourLocation
- TourSession (use DeviceId)
- SessionVisit

Requirements:
- REMOVE User entity completely
- Use DeviceId instead of UserId
- Match MySQL schema exactly
- Add indexes"

---

# 4. GPS (REACT)

"Implement GPS tracking in React:

- navigator.geolocation.watchPosition
- useEffect for lifecycle management
- Update interval and accuracy optimization
- Handle permission denied/loading states gracefully"

---

# 5. GEOFENCE (REACT)

"Implement geofence logic:

Input:
- Current user location (lat, lng)
- List of POIs (Locations)

Use:
- Haversine formula to calculate distance.

Requirements:
- Debounce/throttle calculations.
- Cooldown period to prevent spamming audio.
- Identify and trigger the nearest POI within a specific radius (e.g., 50 meters)."

---

# 6. AUDIO SYSTEM (REACT)

"Build the audio system in React:

- Audio queue management.
- Prevent audio overlap.
- Play / pause / resume / stop controls.
- Auto-stop previous audio when a new one is triggered by Geofence or QR scan."

---

# 7. MAP UI (REACT LEAFLET)

"Create React Leaflet Map component:

- Show user's current GPS location marker.
- Show POI markers.
- Handle marker click events to open POI details.
- Crucial: Include `map.invalidateSize()` logic inside a `setTimeout` or `useEffect` to prevent the grey area rendering bug when the map container resizes."

---

# 8. POI DETAIL (REACT)

"Create React component for Point of Interest details:

- Image gallery (scrollable or grid).
- Location description.
- Audio player interface.
- Language switch capability.
- 'Favorite' button (saves state using DeviceId)."

---

# 9. QR SCANNER (REACT)

"Implement QR scan logic in React:

- Access device camera.
- Decode QR code.
- Route directly to the specific POI detail page based on QR payload.
- Automatically trigger the audio play if applicable."

---

# 10. ANALYTICS & GUEST SYSTEM (.NET 10 + REACT)

"CONTEXT: TOUR PROJECT
The app does not require user authentication (No Login). Users access the app by scanning a QR code. We identify users using a unique `DeviceId` stored in `localStorage`.

TASK 1: REACT FRONTEND IMPLEMENTATION
1. Create a utility function to generate/retrieve a `guest_device_id` from `localStorage`.
2. Provide code showing how to include this `DeviceId` in API calls (e.g., Favorite button, Audio play).
3. Hit the `heartbeat` API every 5 minutes while the app is active.

TASK 2: .NET 10 BACKEND - DATABASE SCHEMA
Update EF Core models to use `DeviceId` (string) for Analytics and Favorites tracking.

TASK 3: .NET 10 BACKEND - DATA COLLECTION APIs (Client Side - No Auth)
1. `POST /api/analytics/heartbeat` (Upsert "LastActive" for DeviceId).
2. `POST /api/analytics/audio-plays` (Log play event: LocationId, AudioId, DeviceId).
3. `POST /api/analytics/favorite-clicks` (Log favorite action: DeviceId, LocationId).

TASK 4: .NET 10 BACKEND - DATA RETRIEVAL APIs (Admin Dashboard)
1. `GET /api/admin/analytics/summary` (Returns: CurrentActiveDevices, TotalAudioPlays, TotalFavoritesSaved).
2. `GET /api/admin/analytics/chart-data` (Input: StartDate, EndDate. Returns daily grouped metrics array)."

---

# 11. ADMIN DASHBOARD (REACT)

"Build React Admin Dashboard:

- Authentication (Login for Admins).
- CRUD operations for Locations/POIs.
- Upload Audio files and Images.
- Display Analytics Dashboard (Charts and metric cards using Recharts or Chart.js).
- Manage Admin accounts."

---

# 12. PROJECT STRUCTURE

"Generate project structure:

- React Vite frontend folder architecture (components, hooks, services, utils).
- ASP.NET Core backend architecture (Controllers, Services, Repositories, Data).
- Naming conventions."

---

# 13. PRD

"Write PRD (Product Requirements Document) using HTML template.

System Features:
- React Frontend (Vite)
- .NET 10 Backend
- Geofence & GPS Tracking
- Audio Guide System
- QR Code Integration
- Guest-Only Client Access
- Admin Analytics Dashboard"

---

# 14. DEBUGGING

""Fix code:

[PASTE CODE HERE]

Analyze the issue and provide the corrected code using C# (.NET 10) or React (Vite) depending on the context. Ensure layout fixes account for Leaflet map behaviors and Flexbox constraints.

---

# 15. CONSISTENCY RULES

Always use:
- React (Vite)
- Tailwind CSS (or standard CSS with Flexbox)
- ASP.NET Core (.NET 10)
- EF Core
- MySQL
- Leaflet (react-leaflet)

---

# STATUS
Updated for Guest-Only Architecture & .NET 10.