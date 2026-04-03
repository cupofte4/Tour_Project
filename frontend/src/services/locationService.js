import API_URL from "./api";

async function readJsonOrThrow(res, fallbackMessage) {
  if (!res.ok) {
    const message = await res.text();
    throw new Error(message || fallbackMessage);
  }

  return await res.json();
}

export async function getNearLocation(lat, lng) {
  try {
    const res = await fetch(
      `${API_URL}/location/near?lat=${lat}&lng=${lng}&_=${Date.now()}`,
      { cache: "no-store" },
    );
    return await readJsonOrThrow(res, "Khong tai duoc dia diem gan ban.");
  } catch {
    return null;
  }
}

export async function getAllLocations() {
  try {
    const res = await fetch(`${API_URL}/location?_=${Date.now()}`, {
      cache: "no-store",
    });
    return await readJsonOrThrow(res, "Khong tai duoc danh sach dia diem.");
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
  return await readJsonOrThrow(res, "Khong tao duoc dia diem.");
}

export async function updateLocation(id, location) {
  const res = await fetch(`${API_URL}/location/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(location),
  });
  return await readJsonOrThrow(res, "Khong cap nhat duoc dia diem.");
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
  const res = await fetch(`${API_URL}/location/${id}`, { method: "DELETE" });
  if (!res.ok) {
    const message = await res.text();
    throw new Error(message || "Khong xoa duoc dia diem.");
  }
}
