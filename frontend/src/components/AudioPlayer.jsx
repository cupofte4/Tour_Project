import { useEffect, useState } from "react";
import audioQueue from "../services/audioQueueService";
import { getTextForLang } from "../services/ttsService";

function AudioPlayer({ lang, onAddToQueue }) {
  const [state, setState] = useState(audioQueue.getState());
  const [isExpanded, setIsExpanded] = useState(false);

  useEffect(() => {
    const unsubscribe = audioQueue.subscribe((newState) => {
      setState(newState);
    });

    return () => unsubscribe();
  }, []);

  const handlePlay = () => {
    if (state.isPlaying) {
      audioQueue.pause();
    } else {
      audioQueue.play();
    }
  };

  const handleStop = () => {
    audioQueue.stop();
  };

  const handleSkipNext = () => {
    audioQueue.skipNext();
  };

  const handleSkipPrevious = () => {
    audioQueue.skipPrevious();
  };

  const handleRemoveItem = (id) => {
    audioQueue.removeItem(id);
  };

  const handleClear = () => {
    if (window.confirm("Xóa tất cả trong danh sách phát?")) {
      audioQueue.clearQueue();
    }
  };

  const currentItem = state.currentItem;
  const queueLength = state.queue?.length || 0;
  const upcomingCount =
    state.currentIndex < queueLength ? queueLength - state.currentIndex - 1 : 0;

  if (queueLength === 0) {
    return (
      <div className="audio-player empty">
        <div className="player-header">
          <div className="player-title">🎵 Danh sách phát</div>
          <button
            className="toggle-btn"
            onClick={() => setIsExpanded(!isExpanded)}
          >
            {isExpanded ? "▼" : "▶"}
          </button>
        </div>
        {isExpanded && (
          <div className="player-content">
            <div className="empty-queue">
              <p>Chưa có dữ liệu</p>
              <p className="hint">Nhấn "Thêm vào danh sách" để thêm địa điểm</p>
            </div>
          </div>
        )}
      </div>
    );
  }

  return (
    <div className="audio-player">
      <div className="player-header">
        <div className="player-title">
          🎵 Danh sách phát ({state.currentIndex + 1}/{queueLength})
        </div>
        <button
          className="toggle-btn"
          onClick={() => setIsExpanded(!isExpanded)}
        >
          {isExpanded ? "▼" : "▶"}
        </button>
      </div>

      {isExpanded && (
        <div className="player-content">
          {/* Current Playing */}
          {currentItem && (
            <div className="now-playing">
              <div className="now-playing-title">🎧 Đang phát:</div>
              <div className="now-playing-item">
                <div className="item-name">{currentItem.location.name}</div>
                <div className="item-status">
                  {state.isPlaying && <span className="playing-indicator">▶</span>}
                  {state.isPaused && <span className="playing-indicator">⏸</span>}
                  <span className="item-lang">{currentItem.langCode}</span>
                </div>
              </div>
            </div>
          )}

          {/* Controls */}
          <div className="player-controls">
            <button
              className="control-btn skip-prev"
              onClick={handleSkipPrevious}
              disabled={state.currentIndex === 0}
              title="Bài trước"
            >
              ⏮
            </button>
            <button
              className="control-btn play"
              onClick={handlePlay}
              title={state.isPlaying ? "Tạm dừng" : "Phát"}
            >
              {state.isPlaying ? "⏸" : "▶"}
            </button>
            <button
              className="control-btn stop"
              onClick={handleStop}
              title="Dừng"
            >
              ⏹
            </button>
            <button
              className="control-btn skip-next"
              onClick={handleSkipNext}
              disabled={state.currentIndex >= queueLength - 1}
              title="Bài tiếp"
            >
              ⏭
            </button>
          </div>

          {/* Queue Info */}
          {upcomingCount > 0 && (
            <div className="queue-info">
              <span>{upcomingCount} bài sắp phát</span>
            </div>
          )}

          {/* Queue List */}
          {queueLength > 1 && (
            <div className="queue-list">
              <div className="queue-list-title">Danh sách:</div>
              <div className="queue-items">
                {state.queue.map((item, index) => (
                  <div
                    key={item.id}
                    className={`queue-item ${
                      index === state.currentIndex ? "current" : ""
                    } ${item.status}`}
                  >
                    <div className="item-index">{index + 1}</div>
                    <div className="item-details">
                      <div className="item-name">{item.location.name}</div>
                      <div className="item-preview">
                        {getTextForLang(item.location, item.langCode).substring(
                          0,
                          50,
                        )}
                        ...
                      </div>
                    </div>
                    <button
                      className="item-remove"
                      onClick={() => handleRemoveItem(item.id)}
                      title="Xóa"
                    >
                      ✕
                    </button>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Clear Button */}
          {queueLength > 1 && (
            <div className="player-actions">
              <button className="clear-btn" onClick={handleClear}>
                Xóa danh sách
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default AudioPlayer;
