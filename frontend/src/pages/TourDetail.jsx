import { useEffect, useRef, useState, useCallback } from "react";
import { useParams } from "react-router-dom";
import { getTourById, getTourLocations, startTourSession, recordVisit } from "../services/tourService";
import { trackAudioPlay } from "../services/analyticsService";
import audioQueue from "../services/audioQueueService";
import Navbar from "../components/Navbar";

const COOLDOWN_MS = 60000; // 60s per POI

function haversine(lat1, lon1, lat2, lon2) {
  const R = 6371000;
  const toRad = (d) => (d * Math.PI) / 180;
  const dLat = toRad(lat2 - lat1);
  const dLon = toRad(lon2 - lon1);
  const a =
    Math.sin(dLat / 2) ** 2 +
    Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) * Math.sin(dLon / 2) ** 2;
  return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
}

export default function TourDetail() {
  const { id } = useParams();
  const [tour, setTour] = useState(null);
  const [tourLocations, setTourLocations] = useState([]);
  const [userPos, setUserPos] = useState(null);
  const [visitedIds, setVisitedIds] = useState(() => {
    const saved = localStorage.getItem(`visited_tour_${id}`);
    return new Set(saved ? JSON.parse(saved) : []);
  });
  const [sessionId, setSessionId] = useState(null);
  const [nearestPOI, setNearestPOI] = useState(null);
  const [lang, setLang] = useState("vi");
  const [started, setStarted] = useState(false);

  const cooldowns = useRef({});
  const watchRef = useRef(null);

  useEffect(() => {
    getTourById(id).then(setTour);
    getTourLocations(id).then(setTourLocations);
  }, [id]);

  const markVisited = useCallback(
    (locationId) => {
      setVisitedIds((prev) => {
        const next = new Set(prev).add(locationId);
        localStorage.setItem(`visited_tour_${id}`, JSON.stringify([...next]));
        return next;
      });
      if (sessionId) recordVisit(sessionId, locationId, true);
      trackAudioPlay(locationId);
    },
    [id, sessionId]
  );

  const checkGeofence = useCallback(
    (pos) => {
      const pois = tourLocations.map((tl) => tl.location);
      const now = Date.now();

      const candidates = pois
        .filter((poi) => !visitedIds.has(poi.id))
        .filter((poi) => !cooldowns.current[poi.id] || now > cooldowns.current[poi.id])
        .map((poi) => ({
          ...poi,
          distance: haversine(pos.lat, pos.lng, poi.latitude, poi.longitude),
        }))
        .filter((poi) => poi.distance <= 50)
        .sort((a, b) => a.distance - b.distance);

      setNearestPOI(candidates[0] ?? null);

      if (candidates.length === 0) return;

      const nearest = candidates[0];
      cooldowns.current[nearest.id] = now + COOLDOWN_MS;
      markVisited(nearest.id);

      const langMap = { vi: nearest.textVi, en: nearest.textEn, zh: nearest.textZh, de: nearest.textDe };
      const locationWithLang = { ...nearest, text: langMap[lang] || nearest.textVi };
      audioQueue.addToQueue(locationWithLang, lang === "vi" ? "vi-VN" : lang === "en" ? "en-US" : lang === "zh" ? "zh-CN" : "de-DE");
    },
    [tourLocations, visitedIds, lang, markVisited]
  );

  const startTour = async () => {
    const user = JSON.parse(localStorage.getItem("user") || "{}");
    const session = await startTourSession(user.id ?? 0, Number(id), lang);
    if (session) setSessionId(session.id);
    setStarted(true);

    watchRef.current = navigator.geolocation.watchPosition(
      (pos) => {
        const coords = { lat: pos.coords.latitude, lng: pos.coords.longitude };
        setUserPos(coords);
        checkGeofence(coords);
      },
      (err) => console.error("GPS error:", err),
      { enableHighAccuracy: true, maximumAge: 3000 }
    );
  };

  const stopTour = () => {
    if (watchRef.current) navigator.geolocation.clearWatch(watchRef.current);
    audioQueue.stop();
    setStarted(false);
  };

  useEffect(() => () => stopTour(), []);

  if (!tour) return <div style={{ padding: 20 }}>Đang tải...</div>;

  const progress = tourLocations.length
    ? Math.round((visitedIds.size / tourLocations.length) * 100)
    : 0;

  return (
    <>
      <Navbar />
      <div style={{ padding: 20, maxWidth: 700, margin: "0 auto" }}>
        <h2>🗺️ {tour.title}</h2>
        <p style={{ color: "#666" }}>{tour.description}</p>

        {/* Language selector */}
        <div style={{ marginBottom: 12 }}>
          <label>🌐 Ngôn ngữ thuyết minh: </label>
          <select value={lang} onChange={(e) => setLang(e.target.value)} disabled={started}>
            <option value="vi">Tiếng Việt</option>
            <option value="en">English</option>
            <option value="zh">中文</option>
            <option value="de">Deutsch</option>
          </select>
        </div>

        {/* Progress */}
        <div style={{ marginBottom: 16 }}>
          <div style={{ display: "flex", justifyContent: "space-between", fontSize: 14 }}>
            <span>Tiến độ: {visitedIds.size} / {tourLocations.length} địa điểm</span>
            <span>{progress}%</span>
          </div>
          <div style={{ background: "#eee", borderRadius: 8, height: 8, marginTop: 4 }}>
            <div style={{ width: `${progress}%`, background: "#4CAF50", height: "100%", borderRadius: 8, transition: "width 0.4s" }} />
          </div>
        </div>

        {/* Start / Stop */}
        {!started ? (
          <button onClick={startTour} style={{ padding: "10px 24px", background: "#2196F3", color: "#fff", border: "none", borderRadius: 8, cursor: "pointer", fontSize: 15 }}>
            ▶ Bắt đầu Tour
          </button>
        ) : (
          <button onClick={stopTour} style={{ padding: "10px 24px", background: "#f44336", color: "#fff", border: "none", borderRadius: 8, cursor: "pointer", fontSize: 15 }}>
            ⏹ Dừng Tour
          </button>
        )}

        {/* Nearest POI indicator */}
        {started && nearestPOI && (
          <div style={{ marginTop: 12, padding: 10, background: "#fff3cd", borderRadius: 8, fontSize: 14 }}>
            📍 Gần nhất: <strong>{nearestPOI.name}</strong> ({Math.round(nearestPOI.distance)}m)
          </div>
        )}

        {/* POI list */}
        <div style={{ marginTop: 20 }}>
          <h3>Danh sách địa điểm</h3>
          {tourLocations.map((tl) => {
            const poi = tl.location;
            const visited = visitedIds.has(poi.id);
            const isNearest = nearestPOI?.id === poi.id;
            return (
              <div
                key={tl.id}
                style={{
                  display: "flex", alignItems: "center", gap: 12,
                  padding: "10px 14px", marginBottom: 8, borderRadius: 10,
                  background: visited ? "#e8f5e9" : isNearest ? "#fff9c4" : "#fafafa",
                  border: `1px solid ${visited ? "#a5d6a7" : isNearest ? "#ffe082" : "#e0e0e0"}`,
                }}
              >
                <span style={{ fontWeight: "bold", color: "#888", minWidth: 24 }}>{tl.orderIndex}</span>
                {poi.image && <img src={poi.image} alt={poi.name} style={{ width: 48, height: 36, objectFit: "cover", borderRadius: 6 }} />}
                <div style={{ flex: 1 }}>
                  <div style={{ fontWeight: 600 }}>{poi.name}</div>
                  <div style={{ fontSize: 12, color: "#888" }}>{poi.address}</div>
                </div>
                <span>{visited ? "✅" : isNearest ? "📍" : "⬜"}</span>
              </div>
            );
          })}
        </div>
      </div>
    </>
  );
}
