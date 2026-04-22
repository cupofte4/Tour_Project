import { POST } from "./api";
import { getOrCreateDeviceId } from "./deviceId";

async function trackEvent(locationId, eventType, count = 1) {
  if (!locationId) return null;

  try {
    return await POST("/analytics/event", {
      locationId,
      eventType,
      count,
    });
  } catch (error) {
    console.warn("Analytics tracking failed:", error);
    return null;
  }
}

export async function trackLocationView(locationId) {
  return await trackEvent(locationId, "view");
}

export async function trackAudioPlay(locationId) {
  try {
    const deviceId = getOrCreateDeviceId();
    return await POST("/analytics/audio-plays", {
      deviceId,
      locationId,
      audioId: null,
      occurredAtUtc: new Date().toISOString(),
    });
  } catch (error) {
    console.warn("Audio play analytics failed:", error);
    return null;
  }
}

export async function sendHeartbeat() {
  try {
    const deviceId = getOrCreateDeviceId();
    return await POST("/api/analytics/heartbeat", {
      sessionId: deviceId, // using deviceId as session when client doesn't have a session
      deviceId,
      occurredAtUtc: new Date().toISOString(),
      platform: navigator?.platform ?? "web",
      appVersion: window?.APP_VERSION ?? "",
    });
  } catch (error) {
    console.warn("Heartbeat failed:", error);
    return null;
  }
}
