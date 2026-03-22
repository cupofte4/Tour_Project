import { useEffect, useState } from "react";
import { getNearLocation } from "../services/locationService";
import LocationCard from "../components/LocationCard";

function Home() {
  const [location, setLocation] = useState(null);

  // tọa độ giả lập trung tâm Vĩnh Khánh
  let lat = 10.7595;
  let lng = 106.7047;

  useEffect(() => {
    const interval = setInterval(async () => {
      // giả lập di chuyển
      lat += 0.00005;
      lng += 0.00005;

      const data = await getNearLocation(lat, lng);

      if (data) {
        setLocation(data);

        const audio = document.getElementById("audio");
        if (audio && data.audio) {
          audio.pause();
          audio.src = "http://localhost:5093/audio/" + data.audio;
          audio.oncanplay = () => audio.play();
        }
      }
    }, 5000); // mỗi 5 giây di chuyển

    return () => clearInterval(interval);
  }, []);

  return (
    <div>
      <h1>Phố Ẩm Thực Vĩnh Khánh</h1>
      <LocationCard location={location} />
    </div>
  );
}

export default Home;