import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getAllTours } from "../services/tourService";
import Navbar from "../components/Navbar";
import TravelSidebar from "../components/TravelSidebar";
import "../styles/app.css";
import "../styles/app.css";

export default function TourList() {
  const [tours, setTours] = useState([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    getAllTours()
      .then((data) => setTours(Array.isArray(data) ? data : []))
      .finally(() => setLoading(false));
  }, []);

  return (
    <>
      <Navbar />
      <div className="container">
        <TravelSidebar />
        <div className="myprofile-main-content">
          <div className="myprofile-content">
            <h1 className="myprofile-title">Khám phá tour</h1>

            {loading ? (
              <div className="favorite-empty-state">Đang tải danh sách tour...</div>
            ) : tours.length === 0 ? (
              <div className="favorite-empty-state">Chưa có tour nào.</div>
            ) : (
              <div style={{ display: "flex", flexDirection: "column", gap: 16 }}>
                {tours.map((tour) => (
                  <div
                    key={tour.id}
                    onClick={() => navigate(`/tours/${tour.id}`)}
                    style={{
                      border: "1px solid #ddd",
                      borderRadius: 12,
                      padding: 16,
                      cursor: "pointer",
                      display: "flex",
                      gap: 16,
                      alignItems: "center",
                      background: "#fff",
                      boxShadow: "0 2px 6px rgba(0,0,0,0.08)",
                    }}
                  >
                    {tour.coverImage && (
                      <img
                        src={tour.coverImage}
                        alt={tour.title}
                        style={{
                          width: 100,
                          height: 70,
                          objectFit: "cover",
                          borderRadius: 8,
                        }}
                      />
                    )}
                    <div>
                      <h3 style={{ margin: "0 0 4px" }}>{tour.title}</h3>
                      <p style={{ margin: "0 0 6px", color: "#666", fontSize: 14 }}>
                        {tour.description}
                      </p>
                      <span style={{ fontSize: 13, color: "#888" }}>
                        ⏱ {tour.estimatedDurationMinutes} phút | 📍 {tour.locationCount} địa điểm
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </>
  );
}
