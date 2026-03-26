import API_URL from "./api";

export async function getNearLocation(lat, lng) {
  try {
    const res = await fetch(`${API_URL}/location/near?lat=${lat}&lng=${lng}`);
    return await res.json();
  } catch {
    return null;
  }
}

export async function getAllLocations() {
  try {
    const res = await fetch(`${API_URL}/location`);
    return await res.json();
  } catch {
    return [];
  }
}

export async function createLocation(location) {
  const res = await fetch(`${API_URL}/location`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(location),
  });
  return await res.json();
}

export async function updateLocation(id, location) {
  const res = await fetch(`${API_URL}/location/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(location),
  });
  return await res.json();
}

export async function submitLocationReview(id, review) {
  const res = await fetch(`${API_URL}/location/${id}/reviews`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(review),
  });

  if (!res.ok) {
    const message = await res.text();
    throw new Error(message || "Không gửi được đánh giá.");
  }

  return await res.json();
}

export async function deleteLocation(id) {
  await fetch(`${API_URL}/location/${id}`, { method: "DELETE" });
}
