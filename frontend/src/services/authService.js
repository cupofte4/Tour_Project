import API_URL from "./api";

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

  return await res.json();
}

export async function register(fullName, username, password) {
  let role = arguments.length > 3 ? arguments[3] : undefined;
  if (!role) role = "user";

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

  return await res.json();
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
