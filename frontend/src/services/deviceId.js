import React from "react";

const DEVICE_ID_KEY = "travel_device_id";
const LEGACY_DEVICE_ID_KEY = "guest_device_id";

export function getOrCreateDeviceId() {
  try {
    const existing = localStorage.getItem(DEVICE_ID_KEY);
    if (existing && existing.trim().length > 0) return existing;

    const legacy = localStorage.getItem(LEGACY_DEVICE_ID_KEY);
    if (legacy && legacy.trim().length > 0) {
      localStorage.setItem(DEVICE_ID_KEY, legacy);
      return legacy;
    }

    const newId = cryptoRandomId();
    localStorage.setItem(DEVICE_ID_KEY, newId);
    return newId;
  } catch (e) {
    // Fallback to UUID-like string if localStorage unavailable
    return cryptoRandomId();
  }
}

function cryptoRandomId() {
  if (typeof crypto !== "undefined" && crypto.randomUUID) {
    return crypto.randomUUID();
  }

  // Use Web Crypto when available for better randomness
  if (typeof crypto !== "undefined" && crypto.getRandomValues) {
    const arr = new Uint8Array(16);
    crypto.getRandomValues(arr);
    return Array.from(arr).map(b => b.toString(16).padStart(2, "0")).join("");
  }

  // Fallback
  return Math.random().toString(36).slice(2) + Date.now().toString(36);
}

export function useDeviceId() {
  // simple hook to expose device id in React
  // lazy initialize to avoid SSR issues
  const [deviceId] = React.useState(() => {
    try {
      return getOrCreateDeviceId();
    } catch {
      return "";
    }
  });

  return deviceId;
}
