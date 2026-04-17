# 📋 PROJECT ANALYSIS - MISSING FEATURES & FIXES

Based on plans.md and prompts.md, here's what's **implemented** vs **missing**:

---

# 🔴 CRITICAL ISSUES (Fix Immediately)

## Backend (.NET)

### 1. **DI Container Not Properly Configured**
- **Issue**: LocationService instantiated with `new` instead of DI
- **Location**: LocationController.cs line 24
- **Fix**: Register LocationService in Program.cs + inject into controller
```csharp
// In Program.cs, add:
builder.Services.AddScoped<LocationService>();

// In LocationController:
public LocationController(AppDbContext context, LocationService service)
{
    _context = context;
    _service = service;  // No longer: _service = new LocationService()
}
```

### 2. **Standard Response Format Missing**
- **Issue**: Controllers return bare objects, no consistent error/success wrapper
- **Plans**: Should follow: `{ success: bool, message: string, data: object }`
- **Skip for now**: Will implement on Phase 2+ (complex refactor)

### 3. **Missing Analytic Endpoints**
- **Issue**: No endpoints to track ViewsCount, AudioPlaysCount
- **Plans Requirement**: Section 6 "Analytics"
- **Missing**:
  - `POST /api/locations/{id}/view` → increment ViewsCount
  - `POST /api/locations/{id}/audio-play` → increment AudioPlaysCount
  - `GET /api/manager/locations/{id}/stats` → return stat by date
- **Status**: ❌ TODO - Phase 7

### 4. **Geofence Endpoint Not Standardized**
- **Current**: `/api/location/near` (singular, returns single Location)
- **Should be**: `/api/geofence/check` with response: `{ nearbyPOI, distance, inGeofence }`
- **Status**: 🔄 PARTIALLY DONE (logic exists, needs wrapper)

---

## Frontend (React)

### 1. **API Layer Not Modernized**
- **Issue**: Using plain `fetch` instead of axios + interceptor
- **Location**: frontend/src/services/api.js (just URL constant)
- **Missing**:
  - axios instance with JWT interceptor
  - automatic token refresh
  - error handling
  - baseURL setup
- **Status**: ❌ TODO - CRITICAL for Phase 2

### 2. **No Protected Routes**
- **Issue**: Any page accessible without auth token
- **Location**: App.jsx routes (no auth check)
- **Missing**:
  - ProtectedRoute component
  - Redirect to /login if !authenticated
  - Role-based route guards (admin, manager)
- **Status**: ❌ TODO - CRITICAL for Phase 2

### 3. **No GPS Tracking**
- **Issue**: Home.jsx has hardcoded userLocation, no real GPS
- **Plans Requirement**: Section 3.1 "GPS"
- **Missing**:
  - navigator.geolocation API
  - useEffect to watch position
  - permission handling
  - interval update
- **Status**: ❌ TODO - Phase 3

### 4. **No Geofence Logic**
- **Issue**: Home.jsx calls getNearLocation but no debounce/cooldown
- **Plans Requirement**: Section 3.2 "Geofence"
- **Missing**:
  - debounce (5s between checks)
  - cooldown (30s before replaying audio for same POI)
  - nearest POI detection
- **Status**: ❌ TODO - Phase 4

### 5. **No Audio Queue System**
- **Current**: Using ttsService (text-to-speech)
- **Plans Requirement**: Section 3.3 "Audio Queue" with HTML5 audio
- **Missing**:
  - Queue management (array of tracks)
  - Play/pause/stop UI controls
  - No overlapping audio
  - Language selection UI
- **Status**: 🔄 PARTIALLY DONE (ttsService exists, needs queue)

---

# 🟡 PHASE 2 ISSUES (AUTH + ROLE) - INCOMPLETE

## Backend ✅ 
- ✅ AuthController exists (login, register, change-password)
- ✅ User model with Role field
- ✅ Roles enum
- ✅ Password stored (not hashed - security issue but works)

## Frontend ❌
- ✅ authService.js exists
- ✅ Login.jsx exists with form
- ✅ Register.jsx exists with form
- ❌ **No JWT token handling**
- ❌ **No axios interceptor** (can't auto-attach token)
- ❌ **No protected routes**
- ❌ **No role-based navigation redirect**
- ❌ **Auth persistence** (check token on app load)

**To Complete Phase 2**:
1. **Create axios instance** (api.js with JWT interceptor)
2. **Update authService.js** to save token + handle JWT
3. **Create ProtectedRoute component**
4. **Update App.jsx** with route guards + role redirect
5. **Add auth check** on app load (useEffect in App)

---

# 🔵 PHASE 3+ ISSUES (NOT STARTED)

## Phase 3: Map + GPS
- ✅ MapView component exists (using Leaflet)
- ✅ Home page loads map
- ❌ No real GPS tracking (hardcoded lat/lng)
- ❌ No location permission request
- ❌ No watch position interval

## Phase 4: Geofence
- ❌ No geofence check debounce
- ❌ No cooldown system
- ❌ No nearest POI highlight on map

## Phase 5: Audio System
- 🔄 ttsService exists (text-to-speech)
- ❌ No audio queue (multiple tracks)
- ❌ No play/pause/stop UI
- ❌ No language selector UI

## Phase 6: CMS (Manager/Admin)
- ❌ ManagerDashboard.jsx skeleton only
- ❌ AdminDashboard.jsx skeleton only
- ❌ No CRUD UI for locations
- ❌ No stats dashboard
- ❌ No user management

## Phase 7: Analytics
- ❌ No backend endpoints (Phase 2 issue)
- ❌ No frontend tracking calls
- ❌ No stats visualization

## Phase 8: QR
- ❌ No QR scanner component
- ❌ No camera permission
- ❌ No QR decode logic

---

# 📊 SUMMARY TABLE

| Feature | Plans | Backend | Frontend | Status |
|---------|-------|---------|----------|--------|
| **Phase 1: Setup** | Section 1-2 | ✅ | ✅ | DONE |
| **Phase 2: Auth** | Section 9, Prompt 2 | ✅ | 🔴 | 40% (backend done, frontend needs JWT + routes) |
| **GPS** | Section 3.1, Prompt 4 | ✅ | ❌ | 0% |
| **Geofence** | Section 3.2, Prompt 5 | 🔄 | ❌ | 10% (backend distance calc only) |
| **Audio** | Section 3.3, Prompt 6 | ❌ | 🔄 | 20% (ttsService exists, needs queue) |
| **Map UI** | Section 4, Prompt 7 | ✅ | ✅ | DONE (Leaflet working) |
| **POI Detail** | Section 4, Prompt 8 | ❌ | ❌ | 0% |
| **CMS Manager** | Section 5, Prompt 11 | ❌ | 🔴 | 0% |
| **CMS Admin** | Section 5, Prompt 12 | ❌ | 🔴 | 0% |
| **Analytics** | Section 6, Prompt 10 | ❌ | ❌ | 0% |
| **QR** | Section 7, Prompt 9 | ❌ | ❌ | 0% |

---

# 🚀 RECOMMENDED FIX ORDER (Step-by-Step)

## TODAY (Phase 2 - Critical):
1. **[BACKEND]** Register LocationService in DI (Program.cs)
2. **[FRONTEND]** Create axios instance + JWT interceptor (api.js)
3. **[FRONTEND]** Create ProtectedRoute component
4. **[FRONTEND]** Update App.jsx routes with guards
5. **[FRONTEND]** Add auth check on app mount

## This Week (Phase 3 - GPS):
6. **[FRONTEND]** Add GPS tracking to Home.jsx (navigator.geolocation)
7. **[FRONTEND]** Add location permission handling

## Next Week (Phase 4 - Geofence):
8. **[BACKEND]** Create GeofenceController with proper endpoint
9. **[FRONTEND]** Add debounce + cooldown logic

## Longer Term:
10. **[FRONTEND]** Complete audio queue system
11. **[FRONTEND]** Complete dashboards (Manager/Admin)
12. **[BACKEND]** Add analytics endpoints
13. **[FRONTEND]** Add QR scanner

---

# ✅ FILES TO MODIFY/CREATE

## Backend
- `Program.cs` - Add DI registration
- `Controllers/GeofenceController.cs` - NEW (geofence endpoint)
- `Controllers/AnalyticsController.cs` - NEW (views/plays tracking)
- `Services/LocationService.cs` - Already good, just ensure DI

## Frontend
- `src/services/api.js` - **CRITICAL** (add axios + interceptor)
- `src/components/ProtectedRoute.jsx` - NEW
- `src/App.jsx` - Update routes
- `src/pages/Home.jsx` - Add GPS tracking
- `src/pages/ManagerDashboard.jsx` - EMPTY (needs complete rebuild)
- `src/pages/AdminDashboard.jsx` - EMPTY (needs complete rebuild)

---

