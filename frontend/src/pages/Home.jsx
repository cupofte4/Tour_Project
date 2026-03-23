import { useEffect, useRef, useState } from "react";
import { getNearLocation, getAllLocations } from "../services/locationService";
import { speakLocation, speakLocationAsync, stop, LANGUAGES } from "../services/ttsService";
import LocationCard from "../components/LocationCard";
import MapView from "../components/MapView";
import Navbar from "../components/Navbar";
import "../styles/app.css";

const API_URL = "http://localhost:5093";

function Home() {
  const [location, setLocation] = useState(null);
  const [locations, setLocations] = useState([]);
  const [userLocation, setUserLocation] = useState({ lat: 10.7595, lng: 106.7047 });
  const [lang, setLang] = useState("vi-VN");
  const [done, setDone] = useState(false);
  const [paused, setPaused] = useState(false);
  const langRef = useRef(lang);
  const visitedRef = useRef(new Set());
  const runningRef = useRef(true);
  const pausedRef = useRef(false);

  const [lightbox, setLightbox] = useState(null);

  useEffect(() => { langRef.current = lang; }, [lang]);

  useEffect(() => {
    getAllLocations().then((data) => setLocations(data));

    let lat = 10.7595;
    let lng = 106.7047;

    const run = async () => {
      while (runningRef.current && visitedRef.current.size < 4) {
        // Chờ nếu đang pause
        while (pausedRef.current) {
          await new Promise((r) => setTimeout(r, 300));
        }

        lat += 0.00005;
        lng += 0.00005;
        setUserLocation({ lat, lng });

        const data = await getNearLocation(lat, lng);

        if (data && !visitedRef.current.has(data.id)) {
          visitedRef.current.add(data.id);
          setLocation(data);

          // Đợi TTS đọc xong mới tiếp tục di chuyển
          await speakLocationAsync(data, langRef.current);

          if (visitedRef.current.size >= 4) {
            setDone(true);
            break;
          }
        }

        // Chờ 3s trước khi bước tiếp
        await new Promise((r) => setTimeout(r, 3000));
      }
    };

    run();

    return () => { runningRef.current = false; stop(); };
  }, []);

  return (
    <div>
      <Navbar />
      <div className="container">
        <div className="left-col">
          <div className="map">
            <MapView userLocation={userLocation} locations={locations} />
          </div>
          <div className="image-strip">
            {location ? (() => {
              let imgs = [];
              try { imgs = JSON.parse(location.images || "[]"); } catch {}
              if (imgs.length === 0 && location.image) imgs = [location.image];
              return imgs.length > 0 ? imgs.map((src, i) => (
                <div key={i} className="img-thumb" onClick={() => setLightbox(src)}>
                  <img src={src} alt={i} />
                </div>
              )) : null;
            })() : null}
          </div>
        </div>
        <div className="right-panel">
          <div className="panel-title">📍 Địa điểm gần bạn</div>

          <div className="status-bar">
            {done ? (
              <><span style={{ marginRight: 8 }}>✅</span> Đã hoàn thành tour — 4/4 địa điểm</>
            ) : (
              <>
                <span className={paused ? "" : "pulse-dot"} style={paused ? { marginRight: 8 } : {}}>{paused ? "⏸" : ""}</span>
                {paused ? "Đã tạm dừng" : "Đang theo dõi vị trí của bạn..."}
                <button
                  onClick={() => { pausedRef.current = !pausedRef.current; setPaused(p => !p); }}
                  style={{ marginLeft: "auto", padding: "3px 10px", fontSize: "12px", border: "none", borderRadius: "6px", cursor: "pointer", background: paused ? "#43a047" : "#e53935", color: "white" }}
                >
                  {paused ? "▶ Tiếp tục" : "⏸ Tạm dừng"}
                </button>
              </>
            )}
          </div>

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

          <LocationCard location={location} lang={lang} apiUrl={API_URL} onImageClick={setLightbox} />
        </div>
      </div>

      {lightbox && (
        <div className="lightbox" onClick={() => setLightbox(null)}>
          <img src={lightbox} alt="preview" onClick={(e) => e.stopPropagation()} />
          <button className="lightbox-close" onClick={() => setLightbox(null)}>✕</button>
        </div>
      )}
    </div>
  );
}

export default Home;
