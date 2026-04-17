const API_URL = "http://localhost:5093/api";

/**
 * Fetch wrapper with JWT interceptor
 * Automatically adds Authorization header if token exists
 */
export async function fetchWithAuth(url, options = {}) {
  const token = localStorage.getItem("token");
  
  const headers = {
    "Content-Type": "application/json",
    ...options.headers,
  };

  // Add JWT token if available
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(url, {
    ...options,
    headers,
  });

  // Handle 401 Unauthorized - token expired
  if (response.status === 401) {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    window.location.href = "/login";
    throw new Error("Session expired. Please login again.");
  }

  return response;
}

/**
 * Helper to make authenticated GET request
 */
export async function GET(endpoint, options = {}) {
  const url = `${API_URL}${endpoint}`;
  const response = await fetchWithAuth(url, {
    method: "GET",
    ...options,
  });
  
  if (!response.ok) {
    throw new Error(`GET ${endpoint} failed: ${response.statusText}`);
  }
  
  return response.json();
}

/**
 * Helper to make authenticated POST request
 */
export async function POST(endpoint, body, options = {}) {
  const url = `${API_URL}${endpoint}`;
  const response = await fetchWithAuth(url, {
    method: "POST",
    body: JSON.stringify(body),
    ...options,
  });

  if (!response.ok) {
    throw new Error(`POST ${endpoint} failed: ${response.statusText}`);
  }

  return response.json();
}

/**
 * Helper to make authenticated PUT request
 */
export async function PUT(endpoint, body, options = {}) {
  const url = `${API_URL}${endpoint}`;
  const response = await fetchWithAuth(url, {
    method: "PUT",
    body: JSON.stringify(body),
    ...options,
  });

  if (!response.ok) {
    throw new Error(`PUT ${endpoint} failed: ${response.statusText}`);
  }

  return response.json();
}

/**
 * Helper to make authenticated DELETE request
 */
export async function DELETE(endpoint, options = {}) {
  const url = `${API_URL}${endpoint}`;
  const response = await fetchWithAuth(url, {
    method: "DELETE",
    ...options,
  });

  if (!response.ok) {
    throw new Error(`DELETE ${endpoint} failed: ${response.statusText}`);
  }

  return response.ok ? response.json().catch(() => ({})) : null;
}

/**
 * Helper to make authenticated PATCH request
 */
export async function PATCH(endpoint, body, options = {}) {
  const url = `${API_URL}${endpoint}`;
  const response = await fetchWithAuth(url, {
    method: "PATCH",
    body: JSON.stringify(body),
    ...options,
  });

  if (!response.ok) {
    throw new Error(`PATCH ${endpoint} failed: ${response.statusText}`);
  }

  return response.json();
}

/**
 * Helper to check geofence - POST with query parameters
 * Note: Uses query params because geofence check endpoint expects them
 */
export async function checkGeofence(lat, lng, options = {}) {
  const url = `${API_URL}/geofence/check?lat=${lat}&lng=${lng}`;
  const response = await fetchWithAuth(url, {
    method: "POST",
    ...options,
  });

  if (!response.ok) {
    return null; // Graceful fallback
  }

  return response.json().catch(() => null);
}

export default API_URL;
