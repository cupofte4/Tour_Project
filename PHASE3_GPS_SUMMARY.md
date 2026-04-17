# Phase 3: Map + GPS - Implementation Summary

## ✅ Completed (April 14, 2026)

### GPS Tracking System
**File**: `frontend/src/pages/Home.jsx`

#### 1. State Management Added
```javascript
const [gpsStatus, setGpsStatus] = useState("waiting");        // waiting, loading, active, denied, error
const [gpsError, setGpsError] = useState(null);              // Error message
const watchIdRef = useRef(null);                              // Reference to GPS watch ID
```

#### 2. GPS Initialization useEffect
- Starts `navigator.geolocation.watchPosition()` when component mounts
- High accuracy mode: `enableHighAccuracy: true`
- Timeout: 10 seconds
- Updates real-time: `maximumAge: 0` (no caching)
- Handles 3 error codes:
  - Code 1: Permission denied
  - Code 2: Network/position unavailable
  - Code 3: Timeout

#### 3. GPS Status States
| Status | Display | Color | Meaning |
|--------|---------|-------|---------|
| `waiting` | Initial state | N/A | Not started |
| `loading` | ⏳ Khởi động GPS... | Orange | Requesting permission |
| `active` | 📡 GPS Active + coords | Green | Working, getting updates |
| `denied` | ❌ Permission denied | Red | User rejected permission |
| `error` | ⚠️ GPS Error | Purple | Network/timeout error |

#### 4. Real-Time Updates
- Latitude: `userLocation.lat` (6 decimal places = ~10cm precision)
- Longitude: `userLocation.lng`
- Accuracy: `userLocation.accuracy` (in meters, e.g., ±25m)
- Console logs position updates for debugging

#### 5. Tour Loop Changed
**Before** (hardcoded):
```javascript
lat += 0.00005;  // Manual increment
lng += 0.00005;
setUserLocation({ lat, lng });
const data = await getNearLocation(lat, lng);
```

**After** (real GPS):
```javascript
const { lat, lng } = userLocation;  // Get current GPS coords
const data = await getNearLocation(lat, lng);  // Check actual location
```

#### 6. Map Updates
- Map automatically centers on `userLocation` (Leaflet MapView already handles this)
- MapView receives real-time coordinates via props
- User marker follows GPS position in real-time

#### 7. GPS Status Display
New UI section shows:
```
📡 GPS Active - 10.7595, 106.7047 (±25m)
```
or
```
⏳ Khởi động GPS...
```
or
```
❌ Permission denied
```

### Flow Diagram

```
App Mount
├─ Check Auth
├─ GPS Initialization
│  ├─ Request Permission
│  ├─ Start watchPosition()
│  └─ Status: loading...
├─ GPS First Fix
│  ├─ Set status: active
│  ├─ Update userLocation
│  └─ Update Map
└─ User clicks "Di chuyển"
   ├─ Tour starts
   ├─ Uses real GPS coords
   ├─ Checks nearLocation every 3s
   └─ Plays audio when nearby

Continuous:
├─ GPS watches position
├─ Updates state every time it changes
├─ Map follows user
└─ Tour uses live coords
```

### Key Features

✅ **Real-Time Tracking**: Updates position ~every 1-5 seconds depending on device
✅ **High Accuracy**: enableHighAccuracy mode for better precision (~10-25m)
✅ **Error Handling**: Gracefully handles permission denied, network errors, timeout
✅ **Status Indicator**: Visual feedback for GPS state (loading, active, error)
✅ **Accuracy Display**: Shows GPS accuracy circle (±Xm)
✅ **Auto Cleanup**: Stops GPS watch on component unmount
✅ **No Caching**: Always gets fresh position, never uses old data
✅ **Disabled Button**: "Di chuyển" button disabled until GPS is active
✅ **Backward Compatible**: Falls back to hardcoded coords if GPS unavailable

### Browser Requirements

- Modern browser with Geolocation API support (Chrome, Firefox, Safari, Edge, etc.)
- HTTPS required (or localhost for testing)
- User must grant location permission

### Testing Checklist

- [ ] Open app in Chrome/Firefox
- [ ] Accept GPS permission popup
- [ ] See "GPS Active" status with coordinates
- [ ] See accuracy (±Xm)
- [ ] Click "Di chuyển" to start tour
- [ ] See coordinates update in real-time
- [ ] Check browser console for GPS logs
- [ ] Test deny permission: see "Permission denied" error
- [ ] Test in offline: see "GPS Error" with timeout message
- [ ] Test on mobile: should work with device GPS

### Technical Details

**Geolocation API**:
- watchPosition() = continuous tracking (updates every time location changes)
- getCurrentPosition() = single shot (would need polling)
- clearWatch() = cleanup on unmount

**Accuracy Levels**:
- GPS: ±5-20m typical
- WiFi triangulation: ±50m
- Cell tower: ±100m+
- App shows actual accuracy value from device

**Speed**: 
- Position updates: ~1-5 seconds depending on device/environment
- Poll interval for getNearLocation: 3 seconds (user can be moving)
- Debounce will be added in Phase 4

### Dependencies

- No new npm packages needed
- Uses built-in navigator.geolocation API
- Leaflet MapView already handles map updates

### Known Limitations

These will be fixed in Phase 4:

- ❌ No debounce: Checks nearLocation every 3s (should debounce)
- ❌ No cooldown: Replays audio too frequently (needs 30s between same POI)
- ❌ No geofence logic: Just checks nearest, should check distance threshold
- ❌ No accuracy filter: Uses every GPS update (should filter noisy updates)

### Phase 3 Status

**COMPLETE**: GPS tracking fully implemented and integrated with existing map/tour system.

### Phase 4 (Next)

Will add:
- Geofence check endpoint in backend (/api/geofence/check)
- Debounce geofence checks (5 seconds between)
- Cooldown system (30 seconds per POI)
- Distance-based triggers
- Accuracy-based filtering
