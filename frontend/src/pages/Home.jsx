import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getNearLocation, getAllLocations } from "../services/locationService";
import { getAllTours, getTourLocations } from "../services/tourService";
import { LANGUAGES } from "../services/ttsService";
import { trackLocationView } from "../services/analyticsService";
import { checkGeofence } from "../services/api";
import API_URL from "../services/api";
import audioQueue from "../services/audioQueueService";
import LocationCard from "../components/LocationCard";
import MapView from "../components/MapView";
import AudioPlayer from "../components/AudioPlayer";
import Navbar from "../components/Navbar";
import TravelSidebar from "../components/TravelSidebar";
import "../styles/app.css";

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
  const [gpsStatus, setGpsStatus] = useState("waiting"); // waiting, loading, active, denied, error
  const [gpsError, setGpsError] = useState(null);
  const langRef = useRef(lang);
  const visitedRef = useRef(new Set());
  const runningRef = useRef(false);
  const pausedRef = useRef(false);
  const watchIdRef = useRef(null);
  
  // Geofence debounce: Only check geofence every 5 seconds
  const lastGeofenceCheckRef = useRef(0);
  const GEOFENCE_CHECK_INTERVAL = 5000; // 5 seconds
  
  // Cooldown: Wait 30 seconds before playing audio for same POI again
  const poiCooldownMapRef = useRef(new Map()); // Map<poiId, lastPlayTime>
  const COOLDOWN_DURATION = 30000; // 30 seconds
  const GEOFENCE_RADIUS = 50; // 50 meters

  const [lightbox, setLightbox] = useState(null);
  const lastTrackedLocationRef = useRef(null);
  const mockModeRef = useRef(false);
  const currentLocationRef = useRef({ lat: 10.7590, lng: 106.7043 });

  // Guest mode: Home is public — no login required
  const user = (() => { try { return JSON.parse(localStorage.getItem("user")); } catch { return null; } })();
  const userRole = (user?.role || "").toLowerCase();

  const loadLocations = async () => {
    try {
      if (selectedTourId) {
        setTourLoading(true);
        const tourLocs = await getTourLocations(selectedTourId);
        const ordered = (Array.isArray(tourLocs) ? tourLocs : [])
          .sort((a, b) => (a?.orderIndex ?? 0) - (b?.orderIndex ?? 0))
          .map((tl) => tl?.location)
          .filter(Boolean);
        setLocations(ordered);
      } else {
        const data = await getAllLocations();
        setLocations(Array.isArray(data) ? data : []);
      }
    } catch {
      setLocations([]);
    } finally {
      setTourLoading(false);
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
  };

  

  useEffect(() => {
    if (!location?.id) return;
    if (lastTrackedLocationRef.current === location.id) return;

    lastTrackedLocationRef.current = location.id;
    trackLocationView(location.id).catch(() => {});
  }, [location]);

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

    // Reset mock mode when GPS setup starts
    mockModeRef.current = false;

    if (!navigator.geolocation) {
      setGpsStatus("error");
      setGpsError("Trình duyệt của bạn không hỗ trợ GPS");
      return;
    }

    setGpsStatus("loading");

    const successCallback = (position) => {
      const { latitude, longitude, accuracy } = position.coords;
      
      setUserLocation({
        lat: latitude,
        lng: longitude,
        accuracy: Math.round(accuracy),
      });
      
      setGpsStatus("active");
      setGpsError(null);
      
      console.log(`📍 GPS Updated: ${latitude.toFixed(6)}, ${longitude.toFixed(6)} (±${Math.round(accuracy)}m)`);
    };

    const errorCallback = (error) => {
      if (mockModeRef.current) return;
      console.error("GPS Error:", error);

      if (error.code === 1) {
        setGpsStatus("denied");
        setGpsError("Bạn đã từ chối quyền truy cập GPS. Vui lòng bật GPS trong cài đặt.");
      } else if (error.code === 2 || error.code === 3) {
        mockModeRef.current = true;
        setGpsStatus("mock");
        setUserLocation({ lat: 10.7590, lng: 106.7043, accuracy: 50 });
        setGpsError(null);
        if (watchIdRef.current !== null) {
          navigator.geolocation.clearWatch(watchIdRef.current);
          watchIdRef.current = null;
        }
        console.log("🎭 Switched to mock location mode");
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
  }, [isAuthenticated]);

  useEffect(() => {
    langRef.current = lang;
  }, [lang]);

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
      runningRef.current = false;
      audioQueue.stop();
      return;
    }

    if (gpsStatus !== "active" && gpsStatus !== "mock") {
      console.warn("Tour started but GPS not active yet:", gpsStatus);
      return;
    }

    // Khởi tạo vị trí giả lập từ userLocation hiện tại
    currentLocationRef.current = { lat: userLocation.lat, lng: userLocation.lng };
    runningRef.current = true;

    const run = async () => {
      while (runningRef.current && visitedRef.current.size < 4) {
        if (!runningRef.current) break;

        while (pausedRef.current) {
          await new Promise((resolve) => setTimeout(resolve, 300));
        }

        // Tăng dần lat/lng để giả lập di chuyển
        currentLocationRef.current.lat += 0.00006;
        currentLocationRef.current.lng += 0.00006;
        setUserLocation({ ...currentLocationRef.current, accuracy: 50 });

        const { lat, lng } = currentLocationRef.current;
        const geofenceData = await checkGeofence(lat, lng);

        if (geofenceData && geofenceData.inGeofence && geofenceData.nearbyPOI) {
          const poi = geofenceData.nearbyPOI;

          if (!visitedRef.current.has(poi.id) && !isInCooldown(poi.id)) {
            visitedRef.current.add(poi.id);
            markPoiAsPlayed(poi.id);
            setLocation(poi);
            console.log(`📍 Geofence triggered! POI: ${poi.name}, Distance: ${geofenceData.distance}m`);
            audioQueue.addToQueue(poi, langRef.current);

            if (visitedRef.current.size >= 4) {
              setDone(true);
              break;
            }
          }
        } else {
          const data = await getNearLocation(lat, lng);
          if (data && !visitedRef.current.has(data.id)) {
            setLocation(data);
          }
        }

        await new Promise((resolve) => setTimeout(resolve, 3000));
      }
    };

    run();

    return () => {
      runningRef.current = false;
      audioQueue.stop();
    };
  }, [isAuthenticated, isTourStarted, gpsStatus]);

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
                      setLocation(null);
                      setSelectedTourId(e.target.value);
                    }}
                    className="tour-select-input"
                  >
                    <option value="">Tất cả địa điểm (không chọn tour)</option>
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
                    Click vào point trên bản đồ để xem thông tin và đánh giá, hoặc bắt đầu di chuyển để nghe audio tự động.
                    <button
                      className="btn-start-tour"
                      onClick={() => setIsTourStarted(true)}
                      disabled={gpsStatus !== "active" && gpsStatus !== "mock"}
                    >
                      ▶ Di chuyển
                    </button>
                  </>
                ) : done ? (
                  <>
                    <span style={{ marginRight: 8 }}>✅</span> Đã hoàn thành tour - 4/4 địa điểm
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

              {/* GPS Status Indicator */}
              <div className={`gps-status-card gps-status-${gpsStatus}`}>
                {gpsStatus === "active" && (
                  <>
                    <span>📡</span>
                    <span>
                      GPS Active - {userLocation.lat?.toFixed(4)}, {userLocation.lng?.toFixed(4)}
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
                {gpsStatus === "mock" && (
                  <>
                    <span>ᯓ➤</span>
                    <span>
                      GPS Tracking - {userLocation.lat?.toFixed(4)}, {userLocation.lng?.toFixed(4)}
                      {userLocation.accuracy && <span> (±{userLocation.accuracy}m)</span>}
                    </span>
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
