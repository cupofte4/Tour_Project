# Phase 4: Geofence - Implementation Summary

## ✅ Completed (April 14, 2026)

### Backend Implementation

#### 1. GeofenceController.cs Created
**Location**: `backend/Controllers/GeofenceController.cs`

**Endpoints**:

##### POST /api/geofence/check
- **Input**: `lat`, `lng` (query parameters)
- **Output**:
```json
{
  "nearbyPOI": { /* Location object */ },
  "distance": 25.5,
  "inGeofence": true,
  "geofenceRadius": 50
}
```
- **Logic**:
  - Finds nearest POI from all locations
  - Calculates distance using Haversine formula (from LocationService)
  - Returns `inGeofence=true` if distance < 50 meters
  - Returns `null` for nearbyPOI if no nearby locations

##### POST /api/geofence/check-all
- **Input**: `lat`, `lng` (query parameters)
- **Output**: Array of all POIs within 50m radius, sorted by distance
- **Use case**: Batch checking for multiple nearby locations

#### 2. Geofence Radius Constant
```csharp
private const double GEOFENCE_RADIUS = 50; // 50 meters
```
- Defines the trigger distance for geofence events
- Easy to adjust if needed
- Same radius used in both backend and frontend

---

### Frontend Implementation

#### 1. State & Refs Added to Home.jsx

```javascript
// Debounce: Only check geofence every 5 seconds
const lastGeofenceCheckRef = useRef(0);
const GEOFENCE_CHECK_INTERVAL = 5000; // 5 seconds

// Cooldown: Wait 30 seconds before replaying audio for same POI
const poiCooldownMapRef = useRef(new Map()); // Map<poiId, lastPlayTime>
const COOLDOWN_DURATION = 30000; // 30 seconds
const GEOFENCE_RADIUS = 50; // 50 meters
```

#### 2. Helper Functions

##### `checkGeofence(lat, lng)`
- Calls backend `/api/geofence/check` endpoint
- Implements **debounce**: Only checks every 5 seconds
- Returns geofence data or null
- **Benefits**:
  - Reduces API calls from continuous position updates
  - Saves bandwidth and battery
  - Prevents audio triggering too frequently

##### `isInCooldown(poiId)`
- Checks if POI was recently played
- Returns `true` if within 30 second cooldown window
- **Benefits**:
  - Prevents same audio from playing multiple times
  - User moving in/out of boundary won't retrigger immediately
  - Smoother user experience

##### `markPoiAsPlayed(poiId)`
- Records timestamp when POI audio is triggered
- Used by `isInCooldown()` to check against
- Automatically manages cooldown timer

#### 3. Tour Loop Updated

**Before** (simulated movement):
```javascript
lat += 0.00005;
lng += 0.00005;
setUserLocation({ lat, lng });
const data = await getNearLocation(lat, lng);
if (data && !visitedRef.current.has(data.id)) {
  // Play audio
}
```

**After** (real geofence detection):
```javascript
const geofenceData = await checkGeofence(lat, lng);

if (geofenceData && geofenceData.inGeofence && geofenceData.nearbyPOI) {
  const poi = geofenceData.nearbyPOI;
  
  // Only trigger if NOT visited AND NOT in cooldown
  if (!visitedRef.current.has(poi.id) && !isInCooldown(poi.id)) {
    visitedRef.current.add(poi.id);
    markPoiAsPlayed(poi.id);
    setLocation(poi);
    await speakLocationAsync(poi, langRef.current);
  }
} else {
  // Show nearest POI even if outside geofence
  const data = await getNearLocation(lat, lng);
  if (data && !visitedRef.current.has(data.id)) {
    setLocation(data);
  }
}
```

**Features**:
- Debounced API calls (5s interval)
- Cooldown protection (30s per POI)
- Distance-based triggering (50m radius)
- Fallback to show nearest POI info even if outside geofence
- Console logging for debugging

#### 4. UI Enhancements

**Geofence Status Indicator** (shown during tour):
```
🎯 Geofence: 50m radius | POI gần nhất: [POI Name]
```
- Displays during active tour
- Shows current geofence radius
- Displays nearest POI name being tracked
- Color: Teal (#00695c) for geofence info

---

## 🎯 Geofence Flow Diagram

```
User walking with app
├─ GPS updates position (continuous)
├─ Tour loop runs (3s interval)
│  ├─ Check if 5s debounce passed
│  │  └─ No: Skip geofence API call
│  │  └─ Yes: Call /api/geofence/check
│  ├─ Check if within 50m radius
│  │  └─ No: Show nearest POI info only
│  │  └─ Yes: Check if POI visited
│  │     └─ Visited: Skip
│  │     └─ Not visited: Check cooldown
│  │        └─ In cooldown: Skip (recently played)
│  │        └─ Available: Trigger audio
│  │           ├─ Mark as visited
│  │           ├─ Mark as played (30s cooldown)
│  │           ├─ Update location card
│  │           └─ Play audio
│  └─ Wait 3 seconds
└─ Repeat
```

---

## 📊 Geofence Logic Summary

| Aspect | Value | Purpose |
|--------|-------|---------|
| Geofence Radius | 50 meters | Detection distance |
| Check Debounce | 5 seconds | Prevent API spam |
| Audio Cooldown | 30 seconds | Prevent audio repetition |
| Check Interval | 3 seconds | Tour loop polling |
| Max Locations | 4 | Tour completion |

---

## 🔄 Key Improvements from Phase 3

| Issue | Phase 3 Solution | Phase 4 Solution |
|-------|-----------------|-----------------|
| Location checking | Called for every position | Uses geofence with debounce |
| API calls | Every 3 seconds | Every 5 seconds (debounced) |
| Audio triggering | Immediate on detection | Cooldown protection (30s) |
| Distance validation | Simple nearest check | Distance-based threshold (50m) |
| User feedback | None | Shows geofence status in UI |

---

## 💾 Files Modified/Created

| File | Type | Changes |
|------|------|---------|
| `backend/Controllers/GeofenceController.cs` | NEW | Geofence check endpoints |
| `frontend/src/pages/Home.jsx` | MODIFIED | Debounce, cooldown, geofence logic |

---

## 🔧 Technical Details

### API Response Example

```json
{
  "nearbyPOI": {
    "id": 5,
    "name": "Ben Thanh Market",
    "description": "Historic market in HCMC",
    "latitude": 10.7714,
    "longitude": 106.6983,
    "image": "...",
    "textVi": "...",
    "textEn": "..."
  },
  "distance": 35.25,
  "inGeofence": true,
  "geofenceRadius": 50
}
```

### Cooldown Map Structure

```javascript
poiCooldownMapRef.current = new Map([
  [5, 1713099600000],  // POI ID 5 played at timestamp
  [7, 1713099610000],  // POI ID 7 played at timestamp
])
```

---

## ✅ Feature Checklist

- ✅ Geofence radius: 50 meters
- ✅ Debounce: 5 second interval between API calls
- ✅ Cooldown: 30 second cooldown per POI
- ✅ Distance calculation: Using Haversine formula (backend)
- ✅ Smooth UX: No audio spam or jarring retriggers
- ✅ Visual feedback: Geofence status indicator
- ✅ Fallback: Show nearest POI even outside geofence
- ✅ Error handling: Graceful failures with fallback
- ✅ Logging: Console logs for debugging

---

## 🚀 Phase 4 Status

**COMPLETE**: Geofence detection fully implemented with debounce and cooldown protection.

---

## Next: Phase 5 (Audio System)

Phase 5 will enhance the audio system with:
- Audio queue management (multiple tracks)
- Play/pause/stop controls
- No overlapping audio
- Language-specific audio selection
- Audio progress indicator

**Note**: Current implementation uses ttsService (text-to-speech). Phase 5 will add HTML5 audio player for custom audio files.
