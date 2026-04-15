import { speakLocationAsync, stop as ttsPause } from "./ttsService";

class AudioQueue {
  constructor() {
    this.queue = [];
    this.currentIndex = 0;
    this.isPlaying = false;
    this.isPaused = false;
    this.currentSessionId = 0;
    this.listeners = new Set();

    // Optional: Auto-advance when current narration finishes
    this.autoAdvanceEnabled = true;
  }

  /**
   * Add a location to the queue
   * @param {Object} location - Location object
   * @param {string} langCode - Language code (vi-VN, en-US, etc.)
   * @param {boolean} insertAtStart - If true, plays immediately; false adds to end
   */
  addToQueue(location, langCode, insertAtStart = false) {
    if (!location) return;

    const queueItem = {
      id: `${location.id}-${Date.now()}`,
      location,
      langCode,
      status: "pending", // pending, playing, completed, skipped
      addedAt: new Date(),
    };

    if (insertAtStart && !this.isPlaying) {
      this.queue.unshift(queueItem);
      this.currentIndex = 0;
      this.play();
    } else {
      this.queue.push(queueItem);
      if (!this.isPlaying && !this.isPaused) {
        this.play();
      }
    }

    this._notify();
  }

  /**
   * Add multiple locations to queue
   */
  addMultipleToQueue(locations, langCode) {
    locations.forEach((loc) => {
      this.queue.push({
        id: `${loc.id}-${Date.now()}`,
        location: loc,
        langCode,
        status: "pending",
        addedAt: new Date(),
      });
    });

    if (!this.isPlaying && !this.isPaused && this.queue.length > 0) {
      this.play();
    }

    this._notify();
  }

  /**
   * Play/Resume audio playback
   */
  async play() {
    if (this.queue.length === 0) {
      return;
    }

    if (this.isPaused) {
      // Resume from pause
      this.isPaused = false;
      this.isPlaying = true;
      this._notify();

      const currentItem = this.queue[this.currentIndex];
      if (currentItem) {
        await this._playItem(currentItem);
      }
      return;
    }

    if (this.isPlaying) {
      return; // Already playing
    }

    this.isPlaying = true;
    this._notify();

    while (
      this.isPlaying &&
      this.currentIndex < this.queue.length &&
      !this.isPaused
    ) {
      const item = this.queue[this.currentIndex];
      item.status = "playing";
      this._notify();

      await this._playItem(item);

      if (this.isPaused || !this.isPlaying) {
        break;
      }

      item.status = "completed";
      ++this.currentIndex;
      this._notify();
    }

    if (this.currentIndex >= this.queue.length) {
      this.isPlaying = false;
      this.currentIndex = 0;
      this._notify();
    }
  }

  /**
   * Play a specific queue item
   */
  async _playItem(item) {
    const sessionId = ++this.currentSessionId;

    try {
      const startTime = Date.now();
      await speakLocationAsync(item.location, item.langCode);

      // If session changed or playback controlled, stop
      if (sessionId !== this.currentSessionId) {
        return;
      }

      // Minimum 500ms between items
      const elapsed = Date.now() - startTime;
      if (elapsed < 500) {
        await new Promise((resolve) =>
          setTimeout(resolve, 500 - elapsed),
        );
      }
    } catch (error) {
      console.error("Error playing audio:", error);
      item.status = "error";
    }
  }

  /**
   * Pause audio playback (can resume later)
   */
  pause() {
    if (!this.isPlaying) {
      return;
    }

    this.isPaused = true;
    this.isPlaying = false;
    ttsPause();
    this._notify();
  }

  /**
   * Stop audio and clear queue
   */
  stop() {
    this.isPlaying = false;
    this.isPaused = false;
    this.currentSessionId += 1;
    ttsPause();
    this.queue = [];
    this.currentIndex = 0;
    this._notify();
  }

  /**
   * Skip to next item
   */
  skipNext() {
    if (this.currentIndex + 1 < this.queue.length) {
      this.queue[this.currentIndex].status = "skipped";
      ++this.currentIndex;
      this.currentSessionId += 1;
      ttsPause();

      if (this.isPlaying || this.isPaused) {
        this.isPaused = false;
        this.play();
      } else {
        this._notify();
      }
    }
  }

  /**
   * Go to previous item
   */
  skipPrevious() {
    if (this.currentIndex > 0) {
      this.queue[this.currentIndex].status = "skipped";
      --this.currentIndex;
      this.currentSessionId += 1;
      ttsPause();

      if (this.isPlaying || this.isPaused) {
        this.isPaused = false;
        this.play();
      } else {
        this._notify();
      }
    }
  }

  /**
   * Remove a specific item from queue
   */
  removeItem(id) {
    const index = this.queue.findIndex((item) => item.id === id);
    if (index === -1) {
      return;
    }

    const wasCurrent = index === this.currentIndex;
    this.queue.splice(index, 1);

    if (wasCurrent) {
      this.currentSessionId += 1;
      ttsPause();

      if (this.currentIndex >= this.queue.length) {
        this.currentIndex = Math.max(0, this.queue.length - 1);
      }

      if (this.isPlaying || this.isPaused) {
        this.isPaused = false;
        this.play();
      } else {
        this._notify();
      }
    }

    this._notify();
  }

  /**
   * Clear queue but keep currently playing
   */
  clearQueue() {
    if (this.currentIndex > 0) {
      const currentItem = this.queue[this.currentIndex];
      this.queue = [currentItem];
      this.currentIndex = 0;
    } else {
      this.queue = [];
      this.currentIndex = 0;
    }
    this._notify();
  }

  /**
   * Get queue state
   */
  getState() {
    return {
      queue: this.queue,
      currentIndex: this.currentIndex,
      currentItem: this.queue[this.currentIndex] || null,
      isPlaying: this.isPlaying,
      isPaused: this.isPaused,
      length: this.queue.length,
    };
  }

  /**
   * Subscribe to queue changes
   */
  subscribe(callback) {
    this.listeners.add(callback);
    return () => this.listeners.delete(callback);
  }

  /**
   * Notify all listeners
   */
  _notify() {
    const state = this.getState();
    this.listeners.forEach((cb) => {
      try {
        cb(state);
      } catch (error) {
        console.error("Listener error:", error);
      }
    });
  }
}

// Singleton instance
const audioQueue = new AudioQueue();

export default audioQueue;
export { audioQueue };
