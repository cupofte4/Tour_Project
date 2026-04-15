import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getNearLocation, getAllLocations } from "../services/locationService";
import { speakLocationAsync, stop, LANGUAGES } from "../services/ttsService";
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
  const [userLocation, setUserLocation] = useState({
    lat: 10.7595,
    lng: 106.7047,
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
  const mockModeRef = useRef(false); // Track if we've switched to mock mode

  const loadLocations = async () => {
    try {
      const data = await getAllLocations();
      setLocations(Array.isArray(data) ? data : []);
    } catch {
      setLocations([]);
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

  useEffect(() => {
    const userString = localStorage.getItem("user");
    if (!userString) {
      navigate("/login");
      return;
    }

    try {
      const userData = JSON.parse(userString);
      const role = (userData?.role || "").toLowerCase();
      if (role === "admin") {
        navigate("/admin/dashboard", { replace: true });
        return;
      }
      if (role === "manager") {
        navigate("/manager/dashboard", { replace: true });
        return;
      }
    } catch {
      // Ignore parse errors and let Home handle as normal user.
    }

    setIsAuthenticated(true);
  }, [navigate]);

  /**
   * GPS Tracking - Initialize geolocation on component mount
   * Watches user position in real-time
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
      // Don't log errors if we've already switched to mock mode
      if (mockModeRef.current) return;
      
      console.error("GPS Error:", error);
      
      let statusMsg = error.message;
      
      if (error.code === 1) {
        statusMsg = "Bạn đã từ chối quyền truy cập GPS. Vui lòng bật GPS trong cài đặt.";
        setGpsStatus("denied");
      } else if (error.code === 2) {
        // Only set mock mode once
        if (!mockModeRef.current) {
          mockModeRef.current = true;
          statusMsg = "Không thể xác định vị trí GPS. Sử dụng vị trí giả lập cho demo.";
          setGpsStatus("mock");
          
          // Fallback to mock location for development/demo
          setUserLocation({
            lat: 10.7595,
            lng: 106.7047,
            accuracy: 50, // Mock accuracy
          });
          setGpsError(null); // Clear error since we're using mock
          
          // Stop GPS watching since we're using mock location
          if (watchIdRef.current !== null) {
            navigator.geolocation.clearWatch(watchIdRef.current);
            watchIdRef.current = null;
            console.log("🛑 GPS Watch stopped - using mock location");
          }
        }
        return; // Don't process further for mock mode
      } else if (error.code === 3) {
        statusMsg = "Timeout xác định vị trí GPS. Vui lòng chờ...";
        setGpsStatus("error");
      }
      
      setGpsError(statusMsg);
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
  }, [isAuthenticated]);

  useEffect(() => {
    if (!isAuthenticated || !isTourStarted) {
      runningRef.current = false;
      stop();
      return;
    }

    if (gpsStatus !== "active" && gpsStatus !== "mock") {
      console.warn("Tour started but GPS not active yet:", gpsStatus);
      return;
    }

    runningRef.current = true;

    const run = async () => {
  while (runningRef.current && visitedRef.current.size < 4) {
    if (!runningRef.current) break;

    while (pausedRef.current) {
      await new Promise((resolve) => setTimeout(resolve, 300));
    }

    // Simulate movement: increment lat/lng
    setUserLocation((prev) => ({
      lat: prev.lat + 0.00005,
      lng: prev.lng + 0.00005,
      accuracy: prev.accuracy || 50,
    }));

    // Wait for state update
    await new Promise((resolve) => setTimeout(resolve, 500));

    // Get current location from state
    const currentLoc = userLocation;
    const geofenceData = await checkGeofence(currentLoc.lat, currentLoc.lng);

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
    } else if (!geofenceData || !geofenceData.inGeofence) {
      const data = await getNearLocation(currentLoc.lat, currentLoc.lng);
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
      stop();
    };
  }, [isAuthenticated, isTourStarted, gpsStatus, userLocation]);

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
                      style={{
                        marginLeft: "auto",
                        padding: "3px 10px",
                        fontSize: "12px",
                        border: "none",
                        borderRadius: "6px",
                        cursor: "pointer",
                        background: paused ? "#43a047" : "#e53935",
                        color: "white",
                      }}
                    >
                      {paused ? "▶ Tiếp tục" : "⏸ Tạm dừng"}
                    </button>
                  </>
                )}
              </div>

              {/* GPS Status Indicator */}
              <div
                style={{
                  padding: "8px 12px",
                  marginBottom: "12px",
                  borderRadius: "6px",
                  fontSize: "12px",
                  fontWeight: "500",
                  backgroundColor:
                    gpsStatus === "active"
                      ? "#e8f5e9"
                      : gpsStatus === "loading"
                      ? "#fff3e0"
                      : gpsStatus === "denied"
                      ? "#ffebee"
                      : gpsStatus === "mock"
                      ? "#e3f2fd"
                      : "#f3e5f5",
                  color:
                    gpsStatus === "active"
                      ? "#2e7d32"
                      : gpsStatus === "loading"
                      ? "#e65100"
                      : gpsStatus === "denied"
                      ? "#c62828"
                      : gpsStatus === "mock"
                      ? "#1565c0"
                      : "#4a148c",
                  display: "flex",
                  alignItems: "center",
                  gap: "6px",
                }}
              >
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
                    <span>🎭</span>
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
                <div
                  style={{
                    padding: "8px 12px",
                    marginBottom: "12px",
                    borderRadius: "6px",
                    fontSize: "12px",
                    fontWeight: "500",
                    backgroundColor: "#e8f4f8",
                    color: "#00695c",
                    display: "flex",
                    alignItems: "center",
                    gap: "6px",
                  }}
                >
                  <span>🎯</span>
                  <span>
                    Geofence: 50m radius
                    {location && (
                      <span style={{ marginLeft: "8px" }}>
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
        <div
          style={{
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            height: "100vh",
            background: "linear-gradient(135deg, #e0f7fa 0%, #e1f5fe 100%)",
            flexDirection: "column",
          }}
        >
          <div
            style={{
              textAlign: "center",
              background: "white",
              padding: "40px 60px",
              borderRadius: "16px",
              boxShadow: "0 4px 16px rgba(0,0,0,0.1)",
            }}
          >
            <p style={{ fontSize: "24px", marginBottom: "16px" }}>
              🔐 Yêu cầu đăng nhập
            </p>
            <p
              style={{ color: "#666", marginBottom: "24px", fontSize: "16px" }}
            >
              Vui lòng đăng nhập để xem hình ảnh và nghe thuyết minh
            </p>
            <button
              onClick={() => navigate("/login")}
              style={{
                padding: "12px 32px",
                fontSize: "16px",
                background: "linear-gradient(135deg, #1e88e5 0%, #1565c0 100%)",
                color: "white",
                border: "none",
                borderRadius: "8px",
                cursor: "pointer",
                transition: "0.2s ease",
              }}
              onMouseOver={(event) =>
                (event.target.style.transform = "translateY(-2px)")
              }
              onMouseOut={(event) =>
                (event.target.style.transform = "translateY(0)")
              }
            >
              Đăng nhập ngay
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export default Home;
