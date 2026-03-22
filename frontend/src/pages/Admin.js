import { useState } from "react";
import { createLocation } from "../services/locationService";

function Admin() {
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
    <div>
      <h2>Thêm địa điểm</h2>

      <input
        placeholder="Tên địa điểm"
        onChange={(e) =>
          setLocation({ ...location, name: e.target.value })
        }
      />
      <br />

      <input
        placeholder="Mô tả"
        onChange={(e) =>
          setLocation({ ...location, description: e.target.value })
        }
      />
      <br />

      <input
        placeholder="Image file name"
        onChange={(e) =>
          setLocation({ ...location, image: e.target.value })
        }
      />
      <br />

      <input
        placeholder="Audio file name"
        onChange={(e) =>
          setLocation({ ...location, audio: e.target.value })
        }
      />
      <br />

      <input
        placeholder="Latitude"
        onChange={(e) =>
          setLocation({ ...location, latitude: e.target.value })
        }
      />
      <br />

      <input
        placeholder="Longitude"
        onChange={(e) =>
          setLocation({ ...location, longitude: e.target.value })
        }
      />
      <br />

      <button onClick={handleSubmit}>Thêm</button>
    </div>
  );
}

export default Admin;