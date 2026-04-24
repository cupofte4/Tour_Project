export const DEFAULT_POI_COOLDOWN_MS = 30000;

export function getPoiPriority(poi) {
  const value = Number(poi?.prio ?? poi?.Prio ?? 0);
  return Number.isFinite(value) ? value : 0;
}

export function getPoiLastPlayed(lastPlayedLookup, poiId) {
  if (lastPlayedLookup instanceof Map) {
    return lastPlayedLookup.get(poiId);
  }

  return lastPlayedLookup?.[poiId];
}

export function canTriggerPoi(poiId, lastPlayedLookup, cooldownMs = DEFAULT_POI_COOLDOWN_MS, now = Date.now()) {
  const lastPlayedAt = getPoiLastPlayed(lastPlayedLookup, poiId);
  if (!lastPlayedAt) {
    return true;
  }

  return now - lastPlayedAt >= cooldownMs;
}

export function selectBestPoi(candidates) {
  if (!Array.isArray(candidates) || candidates.length === 0) {
    return null;
  }

  return [...candidates].sort((a, b) => {
    const priorityDiff = getPoiPriority(b) - getPoiPriority(a);
    if (priorityDiff !== 0) {
      return priorityDiff;
    }

    return (a.distance ?? Number.POSITIVE_INFINITY) - (b.distance ?? Number.POSITIVE_INFINITY);
  })[0];
}
