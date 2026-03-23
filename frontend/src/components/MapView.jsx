import { MapContainer, TileLayer, Marker, Popup, useMap } from "react-leaflet";
import "leaflet/dist/leaflet.css";
import L from "leaflet";

import iconShadow from "leaflet/dist/images/marker-shadow.png";
import icon from "leaflet/dist/images/marker-icon.png";

const DefaultIcon = L.icon({ iconUrl: icon, shadowUrl: iconShadow, iconSize: [25, 41], iconAnchor: [12, 41] });

const RedIcon = new L.Icon({
  iconUrl: "https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-red.png",
  shadowUrl: iconShadow,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
});

L.Marker.prototype.options.icon = DefaultIcon;

// Component để map di chuyển theo user
function ChangeView({ center }) {
  const map = useMap();
  map.setView(center);
  return null;
}

function MapView({ userLocation, locations }) {
  if (!userLocation || !userLocation.lat)
    return <p>Đang xác định vị trí...</p>;

  const currentCenter = [userLocation.lat, userLocation.lng];

  return (
    <div style={{ height: "500px", width: "100%" }}>
      <MapContainer
        center={currentCenter}
        zoom={17}
        style={{ height: "100%", width: "100%" }}
      >
        <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />

        <ChangeView center={currentCenter} />

        {/* Marker user */}
        <Marker position={currentCenter} icon={RedIcon}>
          <Popup>Bạn đang ở đây</Popup>
        </Marker>

        {/* Marker quán ăn */}
        {locations &&
          locations.map((loc) => (
            <Marker
              key={loc.id}
              position={[Number(loc.latitude), Number(loc.longitude)]}
            >
              <Popup>
                <b>{loc.name}</b>
              </Popup>
            </Marker>
          ))}
      </MapContainer>
    </div>
  );
}

export default MapView;