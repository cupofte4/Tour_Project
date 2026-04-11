import API_URL from "./api";

export async function getAllUsers() {
  const response = await fetch(`${API_URL}/users`);

  if (!response.ok) {
    throw new Error("Failed to load users");
  }

  return await response.json();
}

export async function updateAdminUser(id, payload) {
  const response = await fetch(`${API_URL}/users/${id}/admin`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: "Failed to update user" }));
    throw new Error(error.message || "Failed to update user");
  }

  return await response.json();
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
