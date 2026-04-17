import { GET, POST, DELETE } from "./api";

export async function getAllAssignments() {
  return await GET(`/admin/assignments?_=${Date.now()}`);
}

export async function getManagers() {
  return await GET(`/admin/managers?_=${Date.now()}`);
}

export async function assignLocationToManager(managerId, locationId) {
  return await POST("/admin/assignments", { managerId, locationId });
}

export async function unassignLocationFromManager(managerId, locationId) {
  return await DELETE(
    `/admin/assignments?managerId=${managerId}&locationId=${locationId}`,
  );
}

