export const DEFAULT_POI_COOLDOWN_MS = 30000;

export function getPoiPriority(poi) {
  const value = Number(poi?.prio ?? poi?.Prio ?? 0);
  return Number.isFinite(value) ? value : 0;
}

export function getPoiId(poi) {
  const value = Number(poi?.id ?? poi?.Id);
  return Number.isFinite(value) ? value : Number.POSITIVE_INFINITY;
}

export function getPoiDistance(poi) {
  const value = Number(poi?.distance);
  return Number.isFinite(value) ? value : Number.POSITIVE_INFINITY;
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

export function selectBestPoi(candidates, lastSelectedId = null) {
  if (!Array.isArray(candidates) || candidates.length === 0) {
    return null;
  }

  return [...candidates].sort((a, b) => {
    const priorityDiff = getPoiPriority(b) - getPoiPriority(a);
    if (priorityDiff !== 0) {
      return priorityDiff;
    }

    const distanceDiff = getPoiDistance(a) - getPoiDistance(b);
    if (distanceDiff !== 0) {
      return distanceDiff;
    }

    const aId = getPoiId(a);
    const bId = getPoiId(b);

    if (lastSelectedId != null) {
      const aMatchesLastSelected = aId === Number(lastSelectedId);
      const bMatchesLastSelected = bId === Number(lastSelectedId);

      if (aMatchesLastSelected !== bMatchesLastSelected) {
        return aMatchesLastSelected ? -1 : 1;
      }
    }

    return aId - bId;
  })[0];
}
