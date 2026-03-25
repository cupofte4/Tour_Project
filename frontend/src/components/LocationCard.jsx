import { useState } from "react";
import { speakLocation, stop, getTextForLang } from "../services/ttsService";
import "../styles/app.css";

function LocationCard({ location, lang, apiUrl, onImageClick }) {
  const [isPlaying, setIsPlaying] = useState(false);
  if (!location)
    return (
      <div className="empty-state">
        <div className="icon">🔍</div>
        <p>Đang tìm địa điểm gần bạn...</p>
      </div>
    );

  const text = getTextForLang(location, lang);
  let images = [];
  try { images = JSON.parse(location.images || "[]"); } catch {}
  if (images.length === 0 && location.image) images = [location.image];

  return (
    <div className="location-card">
      <div className="location-name">{location.name}</div>

      {images.length > 0 && null}

      <div className="location-desc">{text}</div>

      <div className="tts-controls">
        <button 
          className="tts-btn play" 
          onClick={() => {
            if (isPlaying) {
              // Pause
              stop();
              setIsPlaying(false);
            } else {
              // Play
              speakLocation(location, lang);
              setIsPlaying(true);
            }
          }}
        >
          {isPlaying ? "⏸ Đọc thuyết minh" : "▶ Đọc thuyết minh"}
        </button>
        <button 
          className="tts-btn stop" 
          onClick={() => {
            stop();
            setIsPlaying(false);
          }}
        >
          ⏹ Dừng
        </button>
      </div>

      {location.audio && (
        <audio
          key={location.audio}
          controls
          style={{ width: "100%", marginTop: "12px" }}
          src={`${apiUrl}/audio/${location.audio}`}
        />
      )}
    </div>
  );
}

export default LocationCard;
