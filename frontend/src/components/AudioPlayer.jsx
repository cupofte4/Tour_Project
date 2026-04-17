import { useEffect, useState } from "react";
import audioQueue from "../services/audioQueueService";
import { getTextForLang } from "../services/ttsService";

function AudioPlayer() {
  const [state, setState] = useState(audioQueue.getState());

  useEffect(() => {
    const unsubscribe = audioQueue.subscribe((newState) => setState(newState));
    return () => unsubscribe();
  }, []);

  const currentItem = state.currentItem;
  const queue = state.queue || [];
  const queueLength = queue.length;

  const handlePlay = () => state.isPlaying ? audioQueue.pause() : audioQueue.play();
  const handleStop = () => audioQueue.stop();
  const handleNext = () => audioQueue.skipNext();
  const handlePrev = () => audioQueue.skipPrevious();
  const handleRemove = (id) => audioQueue.removeItem(id);

  const statusLabel = state.isPlaying ? "Đang phát" : state.isPaused ? "Tạm dừng" : "Chờ phát";
  const statusColor = state.isPlaying ? "#0f766e" : state.isPaused ? "#d97706" : "#6b7280";

  if (queueLength === 0) {
    return (
      <div className="ap-wrap">
        <div className="ap-empty">
          <span className="ap-empty-icon">🎧</span>
          <p className="ap-empty-text">Chưa có bài thuyết minh</p>
          <p className="ap-empty-hint">Di chuyển đến gần địa điểm để bắt đầu</p>
        </div>
      </div>
    );
  }

  return (
    <div className="ap-wrap">
      {/* Now Playing Card */}
      {currentItem && (
        <div className="ap-now-playing">
          <div className="ap-np-icon">🎧</div>
          <div className="ap-np-info">
            <div className="ap-np-name">{currentItem.location.name}</div>
            <div className="ap-np-meta">
              <span className="ap-np-lang">{currentItem.langCode}</span>
              <span className="ap-np-dot">·</span>
              <span className="ap-np-status" style={{ color: statusColor }}>{statusLabel}</span>
            </div>
          </div>
          <div className="ap-np-badge">{state.currentIndex + 1}/{queueLength}</div>
        </div>
      )}

      {/* Controls */}
      <div className="ap-controls">
        <button
          className="ap-btn ap-btn-sm"
          onClick={handlePrev}
          disabled={state.currentIndex === 0}
          title="Bài trước"
        >⏮</button>
        <button
          className="ap-btn ap-btn-play"
          onClick={handlePlay}
          title={state.isPlaying ? "Tạm dừng" : "Phát"}
        >
          {state.isPlaying ? "⏸" : "▶"}
        </button>
        <button
          className="ap-btn ap-btn-sm"
          onClick={handleStop}
          title="Dừng"
        >⏹</button>
        <button
          className="ap-btn ap-btn-sm"
          onClick={handleNext}
          disabled={state.currentIndex >= queueLength - 1}
          title="Bài tiếp"
        >⏭</button>
      </div>

      {/* Playlist */}
      {queueLength > 0 && (
        <div className="ap-playlist">
          <div className="ap-playlist-header">
            <span className="ap-playlist-title">Danh sách phát</span>
            <span className="ap-playlist-count">{queueLength} bài</span>
          </div>
          <div className="ap-playlist-items">
            {queue.map((item, index) => {
              const isCurrent = index === state.currentIndex;
              const preview = getTextForLang(item.location, item.langCode);
              return (
                <div
                  key={item.id}
                  className={`ap-item ${isCurrent ? "ap-item-active" : ""}`}
                >
                  <div className="ap-item-left">
                    <span className="ap-item-idx">{isCurrent && state.isPlaying ? "▶" : index + 1}</span>
                    <div className="ap-item-info">
                      <div className="ap-item-name">{item.location.name}</div>
                      <div className="ap-item-preview">
                        {preview ? preview.substring(0, 55) + "…" : item.langCode}
                      </div>
                    </div>
                  </div>
                  <button
                    className="ap-item-del"
                    onClick={() => handleRemove(item.id)}
                    title="Xóa"
                  >🗑</button>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}

export default AudioPlayer;
