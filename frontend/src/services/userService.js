import { GET, PATCH, POST, PUT } from "./api";

export async function getAllUsers() {
  return await GET("/admin-users");
}

export async function createAdminUser(payload) {
  return await POST("/admin-users", {
    ...payload,
    role: (payload?.role || "manager").toLowerCase(),
  });
}

export async function updateAdminUser(id, payload) {
  return await PUT(`/admin-users/${id}/admin`, {
    ...payload,
    role: payload?.role ? payload.role.toLowerCase() : payload?.role,
  });
}

export async function updateAdminUserRole(id, role) {
  return await PATCH(`/admin-users/${id}/role`, { role: (role || "manager").toLowerCase() });
}
