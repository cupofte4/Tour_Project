import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getAllTours, getTourLocations } from "../services/tourService";
import { LANGUAGES } from "../services/ttsService";
import { trackLocationView } from "../services/analyticsService";
import API_URL from "../services/api";
import audioQueue from "../services/audioQueueService";
import LocationCard from "../components/LocationCard";
import MapView from "../components/MapView";
import AudioPlayer from "../components/AudioPlayer";
import Navbar from "../components/Navbar";
import TravelSidebar from "../components/TravelSidebar";
import "../styles/app.css";

const GPS_MODES = {
  REAL: "real",
  DEMO: "demo",
};
const DEMO_ANIMATION_STEPS = 24;
const DEMO_ANIMATION_INTERVAL_MS = 45;

function haversineDistance(lat1, lng1, lat2, lng2) {
  const toRad = (value) => (value * Math.PI) / 180;
  const earthRadius = 6371000;
  const dLat = toRad(lat2 - lat1);
  const dLng = toRad(lng2 - lng1);
  const a =
    Math.sin(dLat / 2) ** 2 +
    Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) * Math.sin(dLng / 2) ** 2;

  return earthRadius * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
}

function getCoordinates(locationItem) {
  return {
    lat: Number(locationItem?.Latitude ?? locationItem?.latitude),
    lng: Number(locationItem?.Longitude ?? locationItem?.longitude),
  };
}

function isLocalhostHost(hostname) {
  return hostname === "localhost" || hostname === "127.0.0.1" || hostname === "::1";
}

function canUseBrowserGeolocation() {
  if (typeof window === "undefined" || typeof navigator === "undefined") return false;
  return Boolean(navigator.geolocation) && (window.isSecureContext || isLocalhostHost(window.location.hostname));
}

function getGeolocationUnavailableMessage() {
  if (typeof navigator !== "undefined" && !navigator.geolocation) {
    return "Trình duyệt của bạn không hỗ trợ định vị. App đang dùng vị trí demo gần POI để bạn trình bày tour.";
  }

  return "Theo dõi vị trí trực tiếp chỉ hoạt động trên HTTPS hoặc localhost. App đang dùng vị trí demo gần POI để bạn trình bày tour.";
}

function createDirectionsUrl(destination) {
  const { lat, lng } = getCoordinates(destination);
  if (!Number.isFinite(lat) || !Number.isFinite(lng)) return null;

  return `https://www.google.com/maps/dir/?api=1&destination=${lat},${lng}`;
}

function createLocationAtPoi(poi) {
  const { lat, lng } = getCoordinates(poi);
  if (!Number.isFinite(lat) || !Number.isFinite(lng)) return null;

  return {
    lat,
    lng,
    accuracy: 8,
    isDemo: true,
  };
}

function normalizeTourLocationRecords(records) {
  return (Array.isArray(records) ? records : [])
    .map((record) => {
      const source = record?.location ?? record?.Location ?? record;
      if (!source) return null;

      const normalized = {
        ...source,
        id: source.id ?? source.Id ?? record?.locationId ?? record?.LocationId,
        name: source.name ?? source.Name ?? "",
        description: source.description ?? source.Description ?? "",
        image: source.image ?? source.Image ?? "",
        images: source.images ?? source.Images ?? "[]",
        address: source.address ?? source.Address ?? "",
        phone: source.phone ?? source.Phone ?? "",
        reviewsJson: source.reviewsJson ?? source.ReviewsJson ?? "[]",
        latitude: Number(source.latitude ?? source.Latitude),
        longitude: Number(source.longitude ?? source.Longitude),
        textVi: source.textVi ?? source.TextVi ?? "",
        textEn: source.textEn ?? source.TextEn ?? "",
        textZh: source.textZh ?? source.TextZh ?? "",
        textDe: source.textDe ?? source.TextDe ?? "",
        orderIndex: Number(record?.orderIndex ?? record?.OrderIndex ?? 0),
      };

      if (!normalized.id) return null;
      if (!Number.isFinite(normalized.latitude) || !Number.isFinite(normalized.longitude)) return null;

      return normalized;
    })
    .filter(Boolean)
    .sort((a, b) => (a.orderIndex ?? 0) - (b.orderIndex ?? 0));
}

function Home() {
  const navigate = useNavigate();
  const [location, setLocation] = useState(null);
  const [locations, setLocations] = useState([]);
  const [tours, setTours] = useState([]);
  const [selectedTourId, setSelectedTourId] = useState("");
  const [tourLoading, setTourLoading] = useState(false);
  const [userLocation, setUserLocation] = useState({
    lat: 10.7590,
    lng: 106.7043,
  });
  const [lang, setLang] = useState("vi-VN");
  const [done, setDone] = useState(false);
  const [paused, setPaused] = useState(false);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isTourStarted, setIsTourStarted] = useState(false);
  const [gpsStatus, setGpsStatus] = useState("demo"); // demo, loading, active, denied, error
  const [gpsMode, setGpsMode] = useState(GPS_MODES.DEMO);
  const [demoPoiIndex, setDemoPoiIndex] = useState(0);
  const [isMovingDemo, setIsMovingDemo] = useState(false);
  const [gpsError, setGpsError] = useState(null);
  const [movementError, setMovementError] = useState("");
  const langRef = useRef(lang);
  const visitedRef = useRef(new Set());
  const pausedRef = useRef(false);
  const watchIdRef = useRef(null);
  const demoAnimationTimerRef = useRef(null);
  const locationsRef = useRef([]);
  const currentLocationRef = useRef(userLocation);
  const loadLocationsRequestRef = useRef(0);
  
  // Geofence debounce: Only check geofence every 5 seconds
  const lastGeofenceCheckRef = useRef(0);
  const GEOFENCE_CHECK_INTERVAL = 5000; // 5 seconds
  
  // Cooldown: Wait 30 seconds before playing audio for same POI again
  const poiCooldownMapRef = useRef(new Map()); // Map<poiId, lastPlayTime>
  const COOLDOWN_DURATION = 30000; // 30 seconds
  const GEOFENCE_RADIUS = 50; // 50 meters

  const [lightbox, setLightbox] = useState(null);
  const lastTrackedLocationRef = useRef(null);
  const liveGeolocationAvailable = canUseBrowserGeolocation();
  const canUseRealGps = gpsMode === GPS_MODES.REAL && gpsStatus === "active";
  const canUseDemoGps = gpsMode === GPS_MODES.DEMO && locations.length > 0;
  const canUseTourLocation = canUseRealGps || canUseDemoGps;

  // Guest mode: Home is public — no login required
  const user = (() => { try { return JSON.parse(localStorage.getItem("user")); } catch { return null; } })();
  const userRole = (user?.role || "").toLowerCase();

  const loadLocations = async () => {
    const requestId = ++loadLocationsRequestRef.current;
    setTourLoading(Boolean(selectedTourId));

    try {
      if (selectedTourId) {
        const tourLocs = await getTourLocations(selectedTourId);
        if (requestId !== loadLocationsRequestRef.current) return;
        setLocations(normalizeTourLocationRecords(tourLocs));
      } else {
        if (requestId !== loadLocationsRequestRef.current) return;
        setLocations([]);
      }
    } catch {
      if (requestId !== loadLocationsRequestRef.current) return;
      setLocations([]);
    } finally {
      if (requestId === loadLocationsRequestRef.current) {
        setTourLoading(false);
      }
    }
  };

  const loadTours = async () => {
    try {
      const data = await getAllTours();
      setTours(Array.isArray(data) ? data : []);
    } catch {
      setTours([]);
    }
  };

  const handleLocationUpdated = (updatedLocation) => {
    if (!updatedLocation?.id) return;

    setLocation((current) =>
      current?.id === updatedLocation.id ? updatedLocation : current,
    );
    setLocations((current) =>
      current.map((item) => (item.id === updatedLocation.id ? updatedLocation : item)),
    );
  };

  const handleMapLocationSelect = (selectedLocation) => {
    if (!selectedLocation?.id) return;
    setLocation(selectedLocation);
    if (gpsMode === GPS_MODES.DEMO && locationsRef.current.length > 0) {
      const selectedIndex = locationsRef.current.findIndex((item) => item.id === selectedLocation.id);
      const nextDemoLocation = createLocationAtPoi(selectedLocation);

      if (selectedIndex >= 0) {
        setDemoPoiIndex(selectedIndex);
      }
      if (nextDemoLocation) {
        currentLocationRef.current = nextDemoLocation;
        setUserLocation(nextDemoLocation);
      }
    }
  };

  const openSelectedLocationDirections = () => {
    if (!location?.id) {
      setMovementError("HTTP không hỗ trợ theo dõi vị trí trực tiếp. Hãy chọn một POI trên bản đồ để mở chỉ đường.");
      return;
    }

    const directionsUrl = createDirectionsUrl(location);
    if (!directionsUrl) {
      setMovementError("POI đã chọn chưa có tọa độ hợp lệ để mở chỉ đường.");
      return;
    }

    setMovementError("Đang mở Google Maps để chỉ đường tới POI đã chọn. Theo dõi trực tiếp cần HTTPS hoặc localhost.");
    window.open(directionsUrl, "_blank", "noopener,noreferrer");
  };

  const clearDemoAnimation = () => {
    if (demoAnimationTimerRef.current !== null) {
      window.clearTimeout(demoAnimationTimerRef.current);
      demoAnimationTimerRef.current = null;
    }
  };

  const findDemoStartIndex = () => {
    if (location?.id) {
      const selectedIndex = locationsRef.current.findIndex((item) => item.id === location.id);
      if (selectedIndex >= 0) return selectedIndex;
    }

    return 0;
  };

  const setDemoPositionToPoi = (poiIndex, { selectPoi = false } = {}) => {
    const poi = locationsRef.current[poiIndex];
    const nextDemoLocation = createLocationAtPoi(poi);
    if (!nextDemoLocation) return false;

    currentLocationRef.current = nextDemoLocation;
    setDemoPoiIndex(poiIndex);
    setUserLocation(nextDemoLocation);
    if (selectPoi) {
      setLocation(poi);
    }

    return true;
  };

  const enableDemoLocation = (message) => {
    clearDemoAnimation();
    setGpsStatus("demo");
    setGpsMode(GPS_MODES.DEMO);
    setGpsError(message);
    setIsMovingDemo(false);

    if (locationsRef.current.length > 0) {
      setDemoPositionToPoi(findDemoStartIndex(), { selectPoi: Boolean(location?.id) });
    }
  };

  const triggerPoiArrival = (poi) => {
    if (!poi?.id) return;

    setLocation(poi);

    if (visitedRef.current.has(poi.id) || isInCooldown(poi.id)) {
      return;
    }

    visitedRef.current.add(poi.id);
    markPoiAsPlayed(poi.id);
    audioQueue.addToQueue(poi, langRef.current);

    if (visitedRef.current.size >= locationsRef.current.length) {
      setDone(true);
    }
  };

  const animateDemoToPoi = (targetIndex) => {
    const targetPoi = locationsRef.current[targetIndex];
    const targetLocation = createLocationAtPoi(targetPoi);
    const startLocation = currentLocationRef.current;

    if (!targetLocation || !Number.isFinite(startLocation?.lat) || !Number.isFinite(startLocation?.lng)) {
      if (setDemoPositionToPoi(targetIndex, { selectPoi: true })) {
        triggerPoiArrival(targetPoi);
      }
      return;
    }

    clearDemoAnimation();
    setIsMovingDemo(true);
    setMovementError("");

    let step = 0;
    const tick = () => {
      step += 1;
      const progress = Math.min(step / DEMO_ANIMATION_STEPS, 1);
      const nextLocation = {
        lat: startLocation.lat + (targetLocation.lat - startLocation.lat) * progress,
        lng: startLocation.lng + (targetLocation.lng - startLocation.lng) * progress,
        accuracy: targetLocation.accuracy,
        isDemo: true,
      };

      currentLocationRef.current = nextLocation;
      setUserLocation(nextLocation);

      if (progress < 1) {
        demoAnimationTimerRef.current = window.setTimeout(tick, DEMO_ANIMATION_INTERVAL_MS);
        return;
      }

      demoAnimationTimerRef.current = null;
      currentLocationRef.current = targetLocation;
      setUserLocation(targetLocation);
      setDemoPoiIndex(targetIndex);
      setIsMovingDemo(false);
      triggerPoiArrival(targetPoi);
    };

    tick();
  };

  const moveDemoToNextPoi = () => {
    if (isMovingDemo || locationsRef.current.length === 0) return;

    if (!isTourStarted) {
      setIsTourStarted(true);
      const startIndex = Math.min(demoPoiIndex, locationsRef.current.length - 1);
      setDemoPositionToPoi(startIndex, { selectPoi: true });
      triggerPoiArrival(locationsRef.current[startIndex]);
      return;
    }

    const nextIndex = (demoPoiIndex + 1) % locationsRef.current.length;
    setDone(false);
    animateDemoToPoi(nextIndex);
  };

  useEffect(() => {
    if (!location?.id) return;
    if (lastTrackedLocationRef.current === location.id) return;

    lastTrackedLocationRef.current = location.id;
    trackLocationView(location.id).catch(() => {});
  }, [location]);

  useEffect(() => {
    locationsRef.current = locations;
  }, [locations]);

  useEffect(() => {
    currentLocationRef.current = userLocation;
  }, [userLocation]);

  /**
   * Check if POI is in cooldown (recently played)
   * Returns true if cooldown is active, false if OK to play
   */
  const isInCooldown = (poiId) => {
    const lastPlayTime = poiCooldownMapRef.current.get(poiId);
    if (!lastPlayTime) return false;
    
    return Date.now() - lastPlayTime < COOLDOWN_DURATION;
  };

  /**
   * Mark POI as just played, starts cooldown timer
   */
  const markPoiAsPlayed = (poiId) => {
    poiCooldownMapRef.current.set(poiId, Date.now());
  };

  // Redirect admin/manager to their dashboards; guests stay on Home
  useEffect(() => {
    if (userRole === "admin") {
      navigate("/admin/dashboard", { replace: true });
      return;
    }
    if (userRole === "manager") {
      navigate("/manager/dashboard", { replace: true });
      return;
    }
    // Guest or any other case: allow access
    setIsAuthenticated(true);
  }, [navigate, userRole]);

  /**
   * GPS Tracking - works for both guests and authenticated users
   */
  useEffect(() => {
    if (!isAuthenticated) return;

    if (!liveGeolocationAvailable) {
      enableDemoLocation(getGeolocationUnavailableMessage());
      return;
    }

    setGpsStatus("loading");
    setGpsError(null);

    const successCallback = (position) => {
      const { latitude, longitude, accuracy } = position.coords;
      const nextRealLocation = {
        lat: latitude,
        lng: longitude,
        accuracy: Math.round(accuracy),
      };
      
      clearDemoAnimation();
      currentLocationRef.current = nextRealLocation;
      setUserLocation(nextRealLocation);
      
      setGpsStatus("active");
      setGpsMode(GPS_MODES.REAL);
      setIsMovingDemo(false);
      setGpsError(null);
      setMovementError("");
      
      console.log(`📍 GPS Updated: ${latitude.toFixed(6)}, ${longitude.toFixed(6)} (±${Math.round(accuracy)}m)`);
    };

    const errorCallback = (error) => {
      console.error("GPS Error:", error);
      const requiresSecureOrigin =
        typeof window !== "undefined" &&
        !window.isSecureContext &&
        window.location.hostname !== "localhost" &&
        window.location.hostname !== "127.0.0.1";

      if (error.code === 1) {
        enableDemoLocation(
          requiresSecureOrigin
            ? "Trình duyệt chỉ cho phép lấy vị trí trên HTTPS hoặc localhost. App đang dùng vị trí demo gần POI để bạn trình bày tour."
            : "Bạn đã từ chối quyền truy cập vị trí. App đang dùng vị trí demo gần POI để bạn trình bày tour.",
        );
      } else if (error.code === 2) {
        enableDemoLocation(
          requiresSecureOrigin
            ? "Không thể lấy vị trí trên kết nối hiện tại. Hãy dùng HTTPS hoặc localhost rồi thử lại."
            : "Không thể xác định vị trí hiện tại. App đang dùng vị trí demo gần POI để bạn trình bày tour.",
        );
      } else if (error.code === 3) {
        enableDemoLocation("Hết thời gian chờ lấy vị trí. App đang dùng vị trí demo gần POI để bạn trình bày tour.");
      } else {
        enableDemoLocation("Không thể lấy vị trí hiện tại. App đang dùng vị trí demo gần POI để bạn trình bày tour.");
      }
    };

    // Request high accuracy GPS
    const options = {
      enableHighAccuracy: true,
      timeout: 10000,      // 10 seconds
      maximumAge: 0,       // Don't use cached position
    };

    // Start watching position
    watchIdRef.current = navigator.geolocation.watchPosition(
      successCallback,
      errorCallback,
      options
    );

    console.log("🚀 GPS Watch started");

    // Cleanup: Stop watching position on unmount
    return () => {
      if (watchIdRef.current !== null) {
        navigator.geolocation.clearWatch(watchIdRef.current);
        console.log("🛑 GPS Watch stopped");
      }
    };
  }, [isAuthenticated, liveGeolocationAvailable]);

  useEffect(() => {
    if (gpsMode !== GPS_MODES.DEMO || locations.length === 0) return;
    if (isMovingDemo) return;

    const startIndex = Math.min(findDemoStartIndex(), locations.length - 1);
    const startDemoLocation = createLocationAtPoi(locations[startIndex]);
    if (!startDemoLocation) return;

    currentLocationRef.current = startDemoLocation;
    setDemoPoiIndex(startIndex);
    setUserLocation(startDemoLocation);
  }, [gpsMode, isMovingDemo, locations]);

  useEffect(() => {
    langRef.current = lang;
  }, [lang]);

  useEffect(() => {
    return () => {
      clearDemoAnimation();
    };
  }, []);

  useEffect(() => {
    pausedRef.current = false;
    visitedRef.current = new Set();
    poiCooldownMapRef.current = new Map();
    lastGeofenceCheckRef.current = 0;
    clearDemoAnimation();
    lastTrackedLocationRef.current = null;
    setPaused(false);
    setDone(false);
    setIsTourStarted(false);
    setIsMovingDemo(false);
    setDemoPoiIndex(0);
    setMovementError("");
    setLocation(null);
    setLocations([]);
    audioQueue.stop();
  }, [selectedTourId]);

  useEffect(() => {
    if (!isAuthenticated) return;
    loadTours();
    loadLocations();

    const handleRefreshLocations = () => {
      loadLocations();
    };

    window.addEventListener("focus", handleRefreshLocations);
    const intervalId = window.setInterval(handleRefreshLocations, 15000);

    return () => {
      window.removeEventListener("focus", handleRefreshLocations);
      window.clearInterval(intervalId);
    };
  }, [isAuthenticated, selectedTourId]);

  useEffect(() => {
    if (!isAuthenticated || !isTourStarted) {
      audioQueue.stop();
      return;
    }

    if (gpsMode !== GPS_MODES.REAL) {
      return;
    }

    if (!selectedTourId || locations.length === 0) {
      setMovementError("Hãy chọn một tour trước khi bắt đầu di chuyển.");
      return;
    }

    if (!canUseRealGps) {
      setMovementError(gpsError || "Vị trí chưa sẵn sàng. Hãy cấp quyền vị trí rồi thử lại.");
      return;
    }

    if (pausedRef.current) {
      return;
    }

    const now = Date.now();
    if (now - lastGeofenceCheckRef.current < GEOFENCE_CHECK_INTERVAL) {
      return;
    }

    lastGeofenceCheckRef.current = now;
    setMovementError("");

    const candidates = locations
      .map((item) => {
        const { lat, lng } = getCoordinates(item);
        return {
          ...item,
          lat,
          lng,
          distance: haversineDistance(userLocation.lat, userLocation.lng, lat, lng),
        };
      })
      .filter((item) => Number.isFinite(item.lat) && Number.isFinite(item.lng))
      .filter((item) => !visitedRef.current.has(item.id))
      .filter((item) => !isInCooldown(item.id))
      .filter((item) => item.distance <= GEOFENCE_RADIUS)
      .sort((a, b) => a.distance - b.distance);

    if (candidates.length === 0) {
      return;
    }

    const poi = candidates[0];
    visitedRef.current.add(poi.id);
    markPoiAsPlayed(poi.id);
    setLocation(poi);
    audioQueue.addToQueue(poi, langRef.current);

    if (visitedRef.current.size >= locations.length) {
      setDone(true);
    }
  }, [
    gpsError,
    gpsMode,
    isAuthenticated,
    isTourStarted,
    locations,
    selectedTourId,
    userLocation.lat,
    userLocation.lng,
    canUseRealGps,
  ]);

  useEffect(() => {
    if (!location?.id) return;

    const stillExists = locations.some((item) => item.id === location.id);
    if (!stillExists) {
      setLocation(null);
    }
  }, [location, locations]);

  return (
    <div>
      {isAuthenticated ? (
        <>
          <Navbar />
          <div className="container">
            <TravelSidebar />
            <div className="left-col">
              <div className="map">
                <MapView
                  key={selectedTourId || "no-tour"}
                  userLocation={userLocation}
                  locations={locations}
                  onSelectLocation={handleMapLocationSelect}
                />
              </div>
              <div className="image-strip">
                {location
                  ? (() => {
                      let imgs = [];
                      try {
                        imgs = JSON.parse(location.images || "[]");
                      } catch {}
                      if (imgs.length === 0 && location.image) {
                        imgs = [location.image];
                      }
                      return imgs.length > 0
                        ? imgs.map((src, i) => (
                            <div
                              key={i}
                              className="img-thumb"
                              onClick={() => setLightbox(src)}
                            >
                              <img src={src} alt={i} />
                            </div>
                          ))
                        : null;
                    })()
                  : null}
              </div>
            </div>
            <div className="right-panel">
              <div className="panel-title">📍 Địa điểm để khám phá và review</div>

              {/* Tour selector (active tours only) */}

              <div className="tour-select-card">
                <div className="tour-select-title">
                  Chọn tour đang có sẵn
                </div>
                <div className="tour-select-row">
                  <select
                    value={selectedTourId}
                    onChange={(e) => {
                      setSelectedTourId(e.target.value);
                    }}
                    className="tour-select-input"
                  >
                    <option value="">Chưa chọn tour</option>
                    {tours.map((t) => (
                      <option key={t.id} value={String(t.id)}>
                        {t.title} ({t.locationCount} POI)
                      </option>
                    ))}
                  </select>
                  {tourLoading && (
                    <span className="tour-loading-status">
                      ⏳ Đang tải...
                    </span>
                  )}
                </div>
              </div>

              <div className="status-bar">
                {!isTourStarted ? (
                  <>
                    <span style={{ marginRight: 8 }}>🗺️</span>
                    {gpsMode === GPS_MODES.DEMO
                      ? locations.length > 0
                        ? "Demo Mode: vị trí đang đặt tại POI đầu tiên của tour. Bấm Di chuyển để phát điểm hiện tại."
                        : "Demo Mode: hãy chọn tour có POI để bắt đầu di chuyển."
                      : liveGeolocationAvailable
                      ? "Click vào point trên bản đồ để xem thông tin và đánh giá. Để nghe audio tự động, hãy chọn tour rồi bắt đầu di chuyển."
                      : "HTTP không hỗ trợ theo dõi vị trí trực tiếp. Hãy chọn một POI trên bản đồ rồi bấm Di chuyển để mở chỉ đường."}
                    <button
                      className="btn-start-tour"
                      onClick={() => {
                        if (!selectedTourId) {
                          setMovementError("Hãy chọn một tour trước khi bắt đầu di chuyển.");
                          return;
                        }

                        if (gpsMode === GPS_MODES.DEMO) {
                          moveDemoToNextPoi();
                          return;
                        }

                        if (!liveGeolocationAvailable) {
                          openSelectedLocationDirections();
                          return;
                        }

                        if (!canUseTourLocation) {
                          setMovementError(gpsError || "Vị trí chưa sẵn sàng. Hãy cấp quyền vị trí rồi thử lại.");
                          return;
                        }

                        setMovementError("");
                        setIsTourStarted(true);
                      }}
                      disabled={
                        !selectedTourId ||
                        locations.length === 0 ||
                        (gpsMode === GPS_MODES.REAL ? !canUseRealGps : !canUseDemoGps) ||
                        isMovingDemo
                      }
                    >
                      ▶ Di chuyển
                    </button>
                  </>
                ) : done ? (
                  <>
                    <span style={{ marginRight: 8 }}>✅</span> Đã hoàn thành tour - {locations.length}/{locations.length} địa điểm
                  </>
                ) : gpsMode === GPS_MODES.DEMO ? (
                  <>
                    <span className={isMovingDemo ? "pulse-dot" : ""} />
                    {isMovingDemo
                      ? "Demo Mode: đang di chuyển tới POI kế tiếp..."
                      : `Demo Mode: đang ở POI ${Math.min(demoPoiIndex + 1, locations.length)}/${locations.length}`}
                    <button
                      className="btn-start-tour"
                      onClick={moveDemoToNextPoi}
                      disabled={isMovingDemo || locations.length === 0}
                    >
                      ▶ Di chuyển
                    </button>
                  </>
                ) : (
                  <>
                    <span
                      className={paused ? "" : "pulse-dot"}
                      style={paused ? { marginRight: 8 } : {}}
                    >
                      {paused ? "⏸" : ""}
                    </span>
                    {paused ? "Đã tạm dừng" : "Đang theo dõi vị trí của bạn..."}
                    <button
                      onClick={() => {
                        pausedRef.current = !pausedRef.current;
                        setPaused((prev) => !prev);
                      }}
                      className={`tour-toggle-btn ${paused ? "resume" : "pause"}`}
                    >
                      {paused ? "▶ Tiếp tục" : "⏸ Tạm dừng"}
                    </button>
                  </>
                )}
              </div>

              {movementError ? (
                <div className="gps-status-card gps-status-error">
                  <span>⚠️</span>
                  <span>{movementError}</span>
                </div>
              ) : null}

              {/* GPS Status Indicator */}
              <div className={`gps-status-card gps-status-${gpsStatus}`}>
                {gpsMode === GPS_MODES.REAL && gpsStatus === "active" && (
                  <>
                    <span>📡</span>
                    <span>
                      GPS Active - {userLocation.lat?.toFixed(4)}, {userLocation.lng?.toFixed(4)}
                      {userLocation.accuracy && <span> (±{userLocation.accuracy}m)</span>}
                    </span>
                  </>
                )}
                {gpsMode === GPS_MODES.DEMO && (
                  <>
                    <span>📍</span>
                    <span>
                      Demo Mode - {userLocation.lat?.toFixed(4)}, {userLocation.lng?.toFixed(4)}
                      {userLocation.accuracy && <span> (±{userLocation.accuracy}m)</span>}
                    </span>
                  </>
                )}
                {gpsStatus === "loading" && (
                  <>
                    <span>⏳</span>
                    <span>Khởi động GPS...</span>
                  </>
                )}
                {gpsStatus === "denied" && (
                  <>
                    <span>❌</span>
                    <span>{gpsError || "GPS bị từ chối"}</span>
                  </>
                )}
                {gpsStatus === "error" && (
                  <>
                    <span>⚠️</span>
                    <span>{gpsError || "GPS lỗi"}</span>
                  </>
                )}
              </div>

              {/* Geofence Status - Show during tour */}
              {isTourStarted && !done && (
                <div className="geofence-status-card">
                  <span>🎯</span>
                  <span>
                    Geofence: 50m radius
                    {location && (
                      <span className="geofence-location">
                        | POI gần nhất: <strong>{location.name}</strong>
                      </span>
                    )}
                  </span>
                </div>
              )}

              <div className="lang-selector">
                <label>🌐 Ngôn ngữ thuyết minh</label>
                <div className="lang-buttons">
                  {LANGUAGES.map((l) => (
                    <button
                      key={l.code}
                      className={`lang-btn ${lang === l.code ? "active" : ""}`}
                      onClick={() => setLang(l.code)}
                    >
                      {l.label}
                    </button>
                  ))}
                </div>
              </div>

              <AudioPlayer lang={lang} />

              
              

              <LocationCard
                location={location}
                lang={lang}
                apiUrl={API_URL}
                onLocationUpdated={handleLocationUpdated}
                onImageClick={setLightbox}
              />
            </div>
          </div>

          {lightbox && (
            <div className="lightbox" onClick={() => setLightbox(null)}>
              <img
                src={lightbox}
                alt="preview"
                onClick={(event) => event.stopPropagation()}
              />
              <button
                className="lightbox-close"
                onClick={() => setLightbox(null)}
              >
                ✕
              </button>
            </div>
          )}

          
        </>
      ) : (
        // Loading state while checking role redirect
        <div style={{ display: "flex", justifyContent: "center", alignItems: "center", height: "100vh" }}>
          <span>⏳ Đang tải...</span>
        </div>
      )}
    </div>
  );
}

export default Home;
