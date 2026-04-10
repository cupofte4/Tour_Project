import API_URL from "./api";

async function readJsonOrThrow(res, fallbackMessage) {
  if (!res.ok) {
    const message = await res.text();
    throw new Error(message || fallbackMessage);
  }

  return await res.json();
}

export async function getAllAssignments() {
  const res = await fetch(`${API_URL}/admin/assignments?_=${Date.now()}`, {
    cache: "no-store",
  });
  return await readJsonOrThrow(res, "Khong tai duoc danh sach phan cong.");
}

export async function getManagers() {
  const res = await fetch(`${API_URL}/admin/managers?_=${Date.now()}`, {
    cache: "no-store",
  });
  return await readJsonOrThrow(res, "Khong tai duoc danh sach manager.");
}

export async function assignLocationToManager(managerId, locationId) {
  const res = await fetch(`${API_URL}/admin/assignments`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ managerId, locationId }),
  });

  return await readJsonOrThrow(res, "Khong phan cong duoc dia diem.");
}

export async function unassignLocationFromManager(managerId, locationId) {
  const res = await fetch(
    `${API_URL}/admin/assignments?managerId=${managerId}&locationId=${locationId}`,
    { method: "DELETE" },
  );

  if (!res.ok) {
    const message = await res.text();
    throw new Error(message || "Khong go phan cong duoc dia diem.");
  }
}

