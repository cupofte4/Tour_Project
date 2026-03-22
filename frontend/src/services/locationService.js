import API_URL from "./api";

export async function getNearLocation(lat, lng) {
  const res = await fetch(
    `${API_URL}/location/near?lat=${lat}&lng=${lng}`
  );
  return await res.json();
}

export async function getAllLocations() {
  const res = await fetch(`${API_URL}/location`);
  return await res.json();
}

export async function createLocation(location) {
  const res = await fetch(`${API_URL}/location`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(location),
  });
  return await res.json();
}