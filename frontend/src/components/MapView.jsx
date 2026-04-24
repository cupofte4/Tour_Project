import { MapContainer, TileLayer, Marker, Popup, useMap } from "react-leaflet";
import { useEffect } from "react";
import L from "leaflet";
import "leaflet/dist/leaflet.css";

// ===== FIX ICON DEFAULT =====
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: "https://unpkg.com/leaflet@1.9.3/dist/images/marker-icon-2x.png",
  iconUrl: "https://unpkg.com/leaflet@1.9.3/dist/images/marker-icon.png",
  shadowUrl: "https://unpkg.com/leaflet@1.9.3/dist/images/marker-shadow.png",
});

// ===== ICON USER (RED) =====
const RedIcon = new L.Icon({
  iconUrl: "https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-red.png",
  shadowUrl: "https://unpkg.com/leaflet@1.9.3/dist/images/marker-shadow.png",
  iconSize: [25, 41],
  iconAnchor: [12, 41],
});

// ===== AUTO CENTER MAP =====
function ChangeView({ center }) {
  const map = useMap();

  useEffect(() => {
    map.panTo(center, {
      animate: true,
      duration: 0.35,
    });
  }, [center, map]);

  return null;
}

function FitLocationBounds({ locations }) {
  const map = useMap();

  useEffect(() => {
    if (locations.length === 0) return;

    const points = locations.map((loc) => [
      Number(loc.Latitude ?? loc.latitude),
      Number(loc.Longitude ?? loc.longitude),
    ]);

    if (points.length === 1) {
      map.setView(points[0], 17);
      return;
    }

    map.fitBounds(L.latLngBounds(points), {
      padding: [32, 32],
      maxZoom: 17,
    });
  }, [locations, map]);

  return null;
}

// ===== MAIN COMPONENT =====
function MapView({ userLocation, locations = [], onSelectLocation }) {
  const normalizedLocations = locations.filter((loc) => {
    const lat = Number(loc?.Latitude ?? loc?.latitude);
    const lng = Number(loc?.Longitude ?? loc?.longitude);
    return Number.isFinite(lat) && Number.isFinite(lng);
  });
  const userLat = Number(userLocation?.lat);
  const userLng = Number(userLocation?.lng);
  const hasUserLocation = Number.isFinite(userLat) && Number.isFinite(userLng);
  const currentCenter = hasUserLocation
    ? [userLat, userLng]
    : normalizedLocations.length > 0
      ? [
          Number(normalizedLocations[0].Latitude ?? normalizedLocations[0].latitude),
          Number(normalizedLocations[0].Longitude ?? normalizedLocations[0].longitude),
        ]
      : [10.7590, 106.7043];

  return (
    <div className="map-view-shell">
      <MapContainer
        center={currentCenter}
        zoom={17}
        className="map-view-container"
        style={{ height: "100%", width: "100%" }}
      >
        <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />

        {hasUserLocation ? (
          <ChangeView center={currentCenter} />
        ) : normalizedLocations.length > 0 ? (
          <FitLocationBounds locations={normalizedLocations} />
        ) : (
          <ChangeView center={currentCenter} />
        )}

        {hasUserLocation && (
          <Marker position={currentCenter} icon={RedIcon}>
            <Popup>Bạn đang ở đây</Popup>
          </Marker>
        )}

        {/* LOCATION MARKERS */}
        {normalizedLocations.map((loc) => (
          <Marker
            key={`${loc.id}-${Number(loc.Latitude ?? loc.latitude)}-${Number(loc.Longitude ?? loc.longitude)}`}
            position={[
              Number(loc.Latitude ?? loc.latitude),
              Number(loc.Longitude ?? loc.longitude)
            ]}
            eventHandlers={{
              click: () => onSelectLocation?.(loc),
            }}
          >
            <Popup>
              <b>{loc.name}</b>
              <br />
              Click để xem thông tin và review.
            </Popup>
          </Marker>
        ))}
      </MapContainer>
    </div>
  );
}

export default MapView;
