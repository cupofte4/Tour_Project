import { useEffect, useState, useRef } from "react";

export default function useGeolocation(options = {}) {
  const [position, setPosition] = useState(null);
  const [error, setError] = useState(null);
  const watchIdRef = useRef(null);

  useEffect(() => {
    if (!('geolocation' in navigator)) {
      setError(new Error('Geolocation not supported'));
      return;
    }

    const success = (pos) => {
      setPosition({
        latitude: pos.coords.latitude,
        longitude: pos.coords.longitude,
        accuracy: pos.coords.accuracy,
        timestamp: pos.timestamp,
      });
    };

    const fail = (err) => setError(err);

    try {
      watchIdRef.current = navigator.geolocation.watchPosition(success, fail, options);
    } catch (e) {
      setError(e);
    }

    return () => {
      if (watchIdRef.current != null) navigator.geolocation.clearWatch(watchIdRef.current);
    };
  }, [JSON.stringify(options)]);

  return { position, error };
}
