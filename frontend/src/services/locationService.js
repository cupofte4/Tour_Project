import { GET, POST, PUT, DELETE } from "./api";

export async function getNearLocation(lat, lng) {
  try {
    return await GET(`/locations/near?lat=${lat}&lng=${lng}&_=${Date.now()}`);
  } catch {
    return null;
  }
}

export async function getAllLocations() {
  try {
    return await GET(`/locations?_=${Date.now()}`);
  } catch {
    return [];
  }
}

export async function createLocation(location) {
  return await POST("/locations", location);
}

export async function updateLocation(id, location) {
  return await PUT(`/locations/${id}`, location);
}

export async function submitLocationReview(id, review) {
  return await POST(`/locations/${id}/reviews`, review);
}

export async function deleteLocation(id) {
  return await DELETE(`/locations/${id}`);
}
