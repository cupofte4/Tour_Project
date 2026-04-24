import API_URL from "./api";

export async function login(username, password) {
  const res = await fetch(`${API_URL}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    // Backend expects AdminUser with Username and PasswordHash fields
    body: JSON.stringify({ Username: username, PasswordHash: password }),
  });

  if (res.status === 401) {
    const error = await res.json().catch(() => ({ message: "Đăng nhập thất bại" }));
    throw new Error(error.message || "Đăng nhập thất bại");
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

// Role is NOT sent — backend always assigns manager
export async function register(fullName, username, password) {
  const res = await fetch(`${API_URL}/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    // If register endpoint exists, backend expects RegisterRequest shape
    body: JSON.stringify({ FullName: fullName, Username: username, Password: password }),
  });

  if (res.status === 400) return null;

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

  const data = await res.json().catch(() => ({ message: "Không thể đổi mật khẩu lúc này" }));

  if (!res.ok) throw new Error(data.message || "Không thể đổi mật khẩu lúc này");

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

// Returns true if visitor has NO token (guest / public mode)
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
