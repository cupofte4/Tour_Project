import { useEffect, useRef } from "react";
import { sendHeartbeat } from "../services/analyticsService";

// Sends public-site page view and heartbeat events while the tab is active.
export default function useHeartbeat({ intervalMs = 60 * 1000, enabled = true, pageKey = "" } = {}) {
  const intervalIdRef = useRef(null);

  useEffect(() => {
    if (!enabled) return undefined;

    async function doHeartbeat(eventType = "heartbeat") {
      try {
        await sendHeartbeat(eventType);
      } catch {
        // swallow
      }
    }

    doHeartbeat("page_view");

    function startInterval() {
      if (intervalIdRef.current) return;
      intervalIdRef.current = setInterval(() => {
        if (document.hidden) return; // pause when not visible
        doHeartbeat();
      }, intervalMs);
    }

    function stopInterval() {
      if (!intervalIdRef.current) return;
      clearInterval(intervalIdRef.current);
      intervalIdRef.current = null;
    }

    function handleVisibility() {
      if (document.hidden) {
        stopInterval();
      } else {
        doHeartbeat();
        startInterval();
      }
    }

    // start when mounted and visible
    if (!document.hidden) startInterval();
    document.addEventListener("visibilitychange", handleVisibility);

    // cleanup
    return () => {
      stopInterval();
      document.removeEventListener("visibilitychange", handleVisibility);
    };
  }, [enabled, intervalMs, pageKey]);
}
