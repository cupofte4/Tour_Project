import { GET, POST, PUT, DELETE } from "./api";

export async function getMyLocations(managerId) {
  return await GET(`/manager/locations?managerId=${managerId}&_=${Date.now()}`);
}

export async function updateMyLocation(managerId, id, location) {
  return await PUT(`/manager/locations/${id}?managerId=${managerId}`, location);
}

export async function deleteMyLocation(managerId, id) {
  return await DELETE(`/manager/locations/${id}?managerId=${managerId}`);
}

export async function getLocationTotals(managerId) {
  return await GET(`/manager/stats/locations?managerId=${managerId}&_=${Date.now()}`);
}

export async function getTimeSeries(managerId, days = 30) {
  return await GET(`/manager/stats/timeseries?managerId=${managerId}&days=${days}&_=${Date.now()}`);
}

