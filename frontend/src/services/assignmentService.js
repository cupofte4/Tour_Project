import { GET, POST, DELETE } from "./api";

export async function getAllAssignments() {
  return await GET(`/location-manager-assignments?_=${Date.now()}`);
}

export async function getManagers() {
  return await GET(`/admin/managers?_=${Date.now()}`);
}

export async function assignLocationToManager(managerId, locationId) {
  return await POST("/location-manager-assignments", { managerId, locationId });
}

export async function unassignLocationFromManager(managerId, locationId) {
  return await DELETE(
    `/location-manager-assignments?managerId=${managerId}&locationId=${locationId}`,
  );
}

