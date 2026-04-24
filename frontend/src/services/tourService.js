import API_URL, { GET, POST, PUT, DELETE, PATCH } from "./api";
import { getOrCreateDeviceId } from "./deviceId";

// ── Tours ──────────────────────────────────────────────────

export async function getAllTours() {
  try {
    return await GET("/tours");
  } catch {
    return [];
  }
}

export async function getTourById(id) {
  try {
    return await GET(`/tours/${id}`);
  } catch {
    return null;
  }
}

export async function getTourLocations(tourId) {
  try {
    return await GET(`/tours/${tourId}/locations`);
  } catch {
    return [];
  }
}

export async function createTour(data) {
  return await POST("/tours", data);
}

export async function updateTour(id, data) {
  return await PUT(`/tours/${id}`, data);
}

export async function deleteTour(id) {
  return await DELETE(`/tours/${id}`);
}

export async function getAllToursAdmin() {
  try {
    return await GET("/tours/admin");
  } catch {
    return [];
  }
}

export async function updateTourStatus(id, isActive) {
  return await PATCH(`/tours/${id}/status`, { isActive: Boolean(isActive) });
}

export async function assignLocationToTour(tourId, locationId, orderIndex, isOptional = false) {
  return await POST(`/tours/${tourId}/locations`, { locationId, orderIndex, isOptional });
}

export async function removeLocationFromTour(tourId, locationId) {
  return await DELETE(`/tours/${tourId}/locations/${locationId}`);
}

export async function reorderTourLocations(tourId, items) {
  // items: [{ id, orderIndex }]
  return await PUT(`/tours/${tourId}/locations/reorder`, items);
}

// ── Sessions ───────────────────────────────────────────────

export async function startTourSession(_userId, tourId, languageCode = "vi") {
  return await POST("/sessions", {
    deviceId: getOrCreateDeviceId(),
    tourId,
    languageCode,
  });
}

export async function getSession(sessionId) {
  try {
    return await GET(`/sessions/${sessionId}`);
  } catch {
    return null;
  }
}

export async function recordVisit(sessionId, locationId, audioPlayed = false) {
  try {
    return await POST(`/sessions/${sessionId}/visits`, { locationId, audioPlayed });
  } catch {
    return null;
  }
}

export async function completeSession(sessionId) {
  try {
    const res = await fetch(
      `${API_URL}/sessions/${sessionId}/complete`,
      { method: "PATCH", headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
    );
    return res.ok ? res.json() : null;
  } catch {
    return null;
  }
}
