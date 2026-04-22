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
    map.setView(center);
  }, [center, map]);

  return null;
}

// ===== MAIN COMPONENT =====
function MapView({ userLocation, locations = [], onSelectLocation }) {
  if (!userLocation || !userLocation.lat) {
    return <p>Đang xác định vị trí...</p>;
  }

  const currentCenter = [userLocation.lat, userLocation.lng];

  return (
    <div className="map-view-shell">
      <MapContainer
        center={currentCenter}
        zoom={17}
        className="map-view-container"
        style={{ height: "100%", width: "100%" }}
      >
        <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />

        <ChangeView center={currentCenter} />

        {/* USER MARKER (RED) */}
        <Marker position={currentCenter} icon={RedIcon}>
          <Popup>Bạn đang ở đây</Popup>
        </Marker>

        {/* LOCATION MARKERS */}
        {locations.map((loc) => (
          <Marker
            key={loc.id}
            position={[
              Number(loc.Latitude || loc.latitude),
              Number(loc.Longitude || loc.longitude)
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