import { useEffect, useRef } from "react";
import { sendHeartbeat } from "../services/analyticsService";

// Sends heartbeat periodically while app is active/visible.
// intervalMs default: 5 minutes
export default function useHeartbeat({ intervalMs = 5 * 60 * 1000 } = {}) {
  const intervalIdRef = useRef(null);

  useEffect(() => {
    let mounted = true;

    async function doHeartbeat() {
      try {
        await sendHeartbeat();
      } catch {
        // swallow
      }
    }

    // send initial heartbeat
    doHeartbeat();

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
        // immediate heartbeat when tab becomes visible
        doHeartbeat();
        startInterval();
      }
    }

    // start when mounted and visible
    if (!document.hidden) startInterval();
    document.addEventListener("visibilitychange", handleVisibility);

    // cleanup
    return () => {
      mounted = false;
      stopInterval();
      document.removeEventListener("visibilitychange", handleVisibility);
    };
  }, [intervalMs]);
}
