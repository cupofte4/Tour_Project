import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import AdminNavbar, { AdminSidebar } from "../components/AdminNavbar";
import { createLocation } from "../services/locationService";
import "../styles/admin.css";

function AddLocation() {
  const navigate = useNavigate();
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [location, setLocation] = useState({
    name: "",
    description: "",
    image: "",
    images: "[]",
    address: "",
    phone: "",
    reviewsJson: "[]",
    latitude: "",
    longitude: "",
    textVi: "",
    textEn: "",
    textZh: "",
    textDe: "",
  });

  useEffect(() => {
    const userString = localStorage.getItem("user");
    if (!userString) {
      navigate("/login");
      return;
    }

    try {
      const userData = JSON.parse(userString);
      const role = (userData?.role || "").toLowerCase();
      if (role !== "admin") {
        navigate("/");
        return;
      }
      setIsAuthorized(true);
    } catch {
      navigate("/login");
    }
  }, [navigate]);

  const handleSubmit = async () => {
    await createLocation({
      ...location,
      latitude: Number(location.latitude),
      longitude: Number(location.longitude),
      images: location.images || "[]",
      reviewsJson: location.reviewsJson || "[]",
    });

    window.alert("Da them dia diem!");
    navigate("/admin/dashboard");
  };

  if (!isAuthorized) return null;

  return (
    <div className="layout">
      <AdminSidebar />

      <div className="main">
        <AdminNavbar
          title="Add new location"
          subtitle="Create a new destination entry with media, coordinates, and essential travel context."
        />

        <div className="content">
          <div className="card">
            <h3>Add Location</h3>

            <input
              placeholder="Name"
              onChange={(e) =>
                setLocation({ ...location, name: e.target.value })
              }
            />

            <textarea
              placeholder="Description"
              onChange={(e) =>
                setLocation({ ...location, description: e.target.value })
              }
            />

            <input
              placeholder="Image file"
              onChange={(e) =>
                setLocation({ ...location, image: e.target.value })
              }
            />

            <input
              placeholder="Address"
              onChange={(e) =>
                setLocation({ ...location, address: e.target.value })
              }
            />

            <input
              placeholder="Phone"
              onChange={(e) =>
                setLocation({ ...location, phone: e.target.value })
              }
            />

            <input
              placeholder="Latitude"
              onChange={(e) =>
                setLocation({ ...location, latitude: e.target.value })
              }
            />

            <input
              placeholder="Longitude"
              onChange={(e) =>
                setLocation({ ...location, longitude: e.target.value })
              }
            />

            <button onClick={handleSubmit}>Add Location</button>
          </div>
        </div>
      </div>
    </div>
  );
}

export default AddLocation;
