import { POST } from "./api";

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
  return await trackEvent(locationId, "audio_play");
}
