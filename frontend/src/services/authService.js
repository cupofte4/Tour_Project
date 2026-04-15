import API_URL, { POST } from "./api";

export async function login(username, password) {
  const res = await fetch(`${API_URL}/auth/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ username, password }),
  });

  if (res.status === 401) {
    const error = await res.json().catch(() => ({ message: "Đăng nhập thất bại" }));
    throw new Error(error.message || "Đăng nhập thất bại");
  }

  const response = await res.json();
  
  // Backend now returns: { token, user }
  if (response.token && response.user) {
    // Save JWT token
    localStorage.setItem("token", response.token);
    // Save user info
    localStorage.setItem("user", JSON.stringify(response.user));
    return response.user;
  }
  
  // Fallback for old response format (just user object)
  if (response.id) {
    localStorage.setItem("user", JSON.stringify(response));
    return response;
  }
  
  return null;
}

export async function register(fullName, username, password, role = "user") {
  const res = await fetch(`${API_URL}/auth/register`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      fullName,
      full_name: fullName,
      username,
      password,
      role,
    }),
  });

  if (res.status === 400) return null;

  const response = await res.json();
  
  // Backend now returns: { token, user }
  if (response.token && response.user) {
    // Save JWT token
    localStorage.setItem("token", response.token);
    // Save user info
    localStorage.setItem("user", JSON.stringify(response.user));
    return response.user;
  }
  
  // Fallback for old response format (just user object)
  if (response.id) {
    localStorage.setItem("user", JSON.stringify(response));
    return response;
  }
  
  return null;
}

export async function changePassword(username, currentPassword, newPassword) {
  const res = await fetch(`${API_URL}/auth/change-password`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ username, currentPassword, newPassword }),
  });

  const data = await res.json().catch(() => ({
    message: "Không thể đổi mật khẩu lúc này",
  }));

  if (!res.ok) {
    throw new Error(data.message || "Không thể đổi mật khẩu lúc này");
  }

  return data;
}

/**
 * Check if user is currently authenticated (has valid token)
 */
export function isAuthenticated() {
  const token = localStorage.getItem("token");
  const user = localStorage.getItem("user");
  return !!(token && user);
}

/**
 * Get current authenticated user
 */
export function getUser() {
  const user = localStorage.getItem("user");
  return user ? JSON.parse(user) : null;
}

/**
 * Get current user role
 */
export function getUserRole() {
  const user = getUser();
  return user ? (user.role || "").toLowerCase() : null;
}

/**
 * Get JWT token
 */
export function getToken() {
  return localStorage.getItem("token");
}

/**
 * Logout user - clear all auth data
 */
export function logout() {
  localStorage.removeItem("token");
  localStorage.removeItem("user");
  localStorage.removeItem("username");
  localStorage.removeItem("rememberMe");
}
