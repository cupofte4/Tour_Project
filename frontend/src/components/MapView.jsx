import { MapContainer, TileLayer, Marker, Popup, useMap } from "react-leaflet";
import "leaflet/dist/leaflet.css";
import L from "leaflet";

import iconShadow from "leaflet/dist/images/marker-shadow.png";
import icon from "leaflet/dist/images/marker-icon.png";

const DefaultIcon = L.icon({
  iconUrl: icon,
  shadowUrl: iconShadow,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
});

const RedIcon = new L.Icon({
  iconUrl: "https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-red.png",
  shadowUrl: iconShadow,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
});

L.Marker.prototype.options.icon = DefaultIcon;

function ChangeView({ center }) {
  const map = useMap();
  map.setView(center);
  return null;
}

function MapView({ userLocation, locations, onSelectLocation }) {
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
      >
        <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />

        <ChangeView center={currentCenter} />

        <Marker position={currentCenter} icon={RedIcon}>
          <Popup>Bạn đang ở đây</Popup>
        </Marker>

        {locations &&
          locations.map((loc) => (
            <Marker
              key={loc.id}
              position={[Number(loc.latitude), Number(loc.longitude)]}
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
