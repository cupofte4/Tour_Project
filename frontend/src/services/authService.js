import API_URL from "./api";

export async function login(username, password) {
  const res = await fetch(`${API_URL}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ Username: username, PasswordHash: password }),
  });

  if (res.status === 401) {
    const error = await res.json().catch(() => ({ message: "Dang nhap that bai" }));
    throw new Error(error.message || "Dang nhap that bai");
  }

  const response = await res.json();

  if (response.token && response.user) {
    localStorage.setItem("token", response.token);
    localStorage.setItem("user", JSON.stringify(response.user));
    return response.user;
  }

  if (response.id) {
    localStorage.setItem("user", JSON.stringify(response));
    return response;
  }

  return null;
}

export async function changePassword(username, currentPassword, newPassword) {
  const res = await fetch(`${API_URL}/auth/change-password`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, currentPassword, newPassword }),
  });

  const data = await res.json().catch(() => ({ message: "Khong the doi mat khau luc nay" }));

  if (!res.ok) throw new Error(data.message || "Khong the doi mat khau luc nay");

  return data;
}

export function isAuthenticated() {
  return !!(localStorage.getItem("token") && localStorage.getItem("user"));
}

export function getUser() {
  const user = localStorage.getItem("user");
  return user ? JSON.parse(user) : null;
}

export function getUserRole() {
  const user = getUser();
  return user ? (user.role || "").toLowerCase() : null;
}

export function isGuest() {
  return !isAuthenticated();
}

export function getToken() {
  return localStorage.getItem("token");
}

export function logout() {
  localStorage.removeItem("token");
  localStorage.removeItem("user");
  localStorage.removeItem("username");
  localStorage.removeItem("rememberMe");
}
