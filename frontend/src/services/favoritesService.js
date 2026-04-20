const FAVORITES_KEY_PREFIX = "favoriteLocations:";
const GUEST_FAVORITES_KEY = `${FAVORITES_KEY_PREFIX}guest`;

const getCurrentUsername = () => {
  const storedUser = localStorage.getItem("user");
  const storedUsername = localStorage.getItem("username");

  if (storedUsername) {
    return storedUsername;
  }

  if (!storedUser) {
    return null;
  }

  try {
    const user = JSON.parse(storedUser);
    return user?.username || null;
  } catch {
    return null;
  }
};

const getStorageKey = () => {
  const username = getCurrentUsername();
  return username ? `${FAVORITES_KEY_PREFIX}${username}` : GUEST_FAVORITES_KEY;
};

export function getFavoriteLocationIds() {
  const key = getStorageKey();

  try {
    const raw = localStorage.getItem(key);
    const parsed = JSON.parse(raw || "[]");
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

export function isFavoriteLocation(locationId) {
  return getFavoriteLocationIds().includes(locationId);
}

export function toggleFavoriteLocation(locationId) {
  const key = getStorageKey();
  const current = getFavoriteLocationIds();
  const next = current.includes(locationId)
    ? current.filter((id) => id !== locationId)
    : [locationId, ...current];

  localStorage.setItem(key, JSON.stringify(next));
  window.dispatchEvent(new Event("favoritesUpdated"));

  return next.includes(locationId);
}
