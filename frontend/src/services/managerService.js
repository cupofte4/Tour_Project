import API_URL from "./api";

async function readJsonOrThrow(res, fallbackMessage) {
  if (!res.ok) {
    const message = await res.text();
    throw new Error(message || fallbackMessage);
  }

  return await res.json();
}

export async function getMyLocations(managerId) {
  const res = await fetch(
    `${API_URL}/manager/locations?managerId=${managerId}&_=${Date.now()}`,
    { cache: "no-store" },
  );
  return await readJsonOrThrow(res, "Khong tai duoc danh sach dia diem.");
}

export async function createMyLocation(managerId, location) {
  const res = await fetch(`${API_URL}/manager/locations?managerId=${managerId}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(location),
  });
  return await readJsonOrThrow(res, "Khong tao duoc dia diem.");
}

export async function updateMyLocation(managerId, id, location) {
  const res = await fetch(
    `${API_URL}/manager/locations/${id}?managerId=${managerId}`,
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(location),
    },
  );
  return await readJsonOrThrow(res, "Khong cap nhat duoc dia diem.");
}

export async function deleteMyLocation(managerId, id) {
  const res = await fetch(
    `${API_URL}/manager/locations/${id}?managerId=${managerId}`,
    { method: "DELETE" },
  );
  if (!res.ok) {
    const message = await res.text();
    throw new Error(message || "Khong xoa duoc dia diem.");
  }
}

export async function getLocationTotals(managerId) {
  const res = await fetch(
    `${API_URL}/manager/stats/locations?managerId=${managerId}&_=${Date.now()}`,
    { cache: "no-store" },
  );
  return await readJsonOrThrow(res, "Khong tai duoc thong ke dia diem.");
}

export async function getTimeSeries(managerId, days = 30) {
  const res = await fetch(
    `${API_URL}/manager/stats/timeseries?managerId=${managerId}&days=${days}&_=${Date.now()}`,
    { cache: "no-store" },
  );
  return await readJsonOrThrow(res, "Khong tai duoc thong ke theo thoi gian.");
}

