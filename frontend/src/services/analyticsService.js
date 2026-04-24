import { POST } from "./api";
import { getOrCreateDeviceId } from "./deviceId";

async function trackEvent(locationId, eventType, count = 1) {
  try {
    const deviceId = getOrCreateDeviceId();
    return await POST("/analytics/event", {
      deviceId,
      locationId,
      eventType,
      count,
      path: getCurrentPath(),
      createdAtUtc: new Date().toISOString(),
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

export async function trackFavoriteClick(locationId, isFavorite) {
  try {
    const deviceId = getOrCreateDeviceId();
    return await POST("/analytics/favorite-clicks", {
      deviceId,
      locationId,
      isFavorite: Boolean(isFavorite),
      occurredAtUtc: new Date().toISOString(),
    });
  } catch (error) {
    console.warn("Favorite analytics failed:", error);
    return null;
  }
}

export async function sendHeartbeat(eventType = "heartbeat") {
  try {
    if (isAdminPath()) return null;

    const deviceId = getOrCreateDeviceId();
    return await POST("/analytics/heartbeat", {
      sessionId: deviceId, // using deviceId as session when client doesn't have a session
      deviceId,
      occurredAtUtc: new Date().toISOString(),
      platform: navigator?.platform ?? "web",
      appVersion: window?.APP_VERSION ?? "",
      path: getCurrentPath(),
      eventType,
    });
  } catch (error) {
    console.warn("Heartbeat failed:", error);
    return null;
  }
}

function getCurrentPath() {
  if (typeof window === "undefined") return "/";
  return `${window.location.pathname}${window.location.search}`;
}

export function isAdminPath(path = getCurrentPath()) {
  return path.toLowerCase().startsWith("/admin");
}
