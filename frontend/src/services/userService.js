import { GET, PUT } from "./api";

export async function getAllUsers() {
  return await GET("/users");
}

export async function updateAdminUser(id, payload) {
  return await PUT(`/users/${id}/admin`, payload);
}

export async function createUserAsAdmin(payload) {
  const response = await fetch(`${API_URL}/users/admin/create`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Failed to create user" }));
    throw new Error(error.message || "Failed to create user");
  }

  return await response.json();
}
