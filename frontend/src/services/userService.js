import { GET, PUT } from "./api";

export async function getAllUsers() {
  return await GET("/users");
}

export async function updateAdminUser(id, payload) {
  return await PUT(`/users/${id}/admin`, payload);
}
