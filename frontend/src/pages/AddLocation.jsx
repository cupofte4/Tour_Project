import { useState } from "react";
import Sidebar from "../components/Sidebar";
import AdminNavbar from "../components/AdminNavbar";
import { createLocation } from "../services/locationService";
import "../styles/admin.css";

function AddLocation() {
  const [location, setLocation] = useState({
    name: "",
    description: "",
    image: "",
    audio: "",
    latitude: "",
    longitude: "",
  });

  const handleSubmit = async () => {
    await createLocation(location);
    alert("Đã thêm địa điểm!");
  };

  return (
    <div className="layout">
      <Sidebar />

      <div className="main">
        <AdminNavbar />

        <div className="content">
          <div className="card">
            <h3>Add Location</h3>

            <input placeholder="Name"
              onChange={(e) =>
                setLocation({ ...location, name: e.target.value })
              }
            />

            <textarea placeholder="Description"
              onChange={(e) =>
                setLocation({ ...location, description: e.target.value })
              }
            />

            <input placeholder="Image file"
              onChange={(e) =>
                setLocation({ ...location, image: e.target.value })
              }
            />

            <input placeholder="Audio file"
              onChange={(e) =>
                setLocation({ ...location, audio: e.target.value })
              }
            />

            <input placeholder="Latitude"
              onChange={(e) =>
                setLocation({ ...location, latitude: e.target.value })
              }
            />

            <input placeholder="Longitude"
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