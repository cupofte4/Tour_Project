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
            <div className="page-hero page-hero-tour">
              <div>
                <p className="page-eyebrow">Danh sách tour</p>
                <h1 className="myprofile-title">Khám phá tour</h1>
                <p className="page-subtitle">
                  Chọn hành trình phù hợp để bắt đầu trải nghiệm khám phá thành phố theo cách trực quan hơn.
                </p>
              </div>
            </div>

            {loading ? (
              <div className="favorite-empty-state">Đang tải danh sách tour...</div>
            ) : tours.length === 0 ? (
              <div className="favorite-empty-state">Chưa có tour nào.</div>
            ) : (
              <div className="tour-list-grid">
                {tours.map((tour) => (
                  <article
                    key={tour.id}
                    className="tour-discovery-card"
                    onClick={() => navigate(`/tours/${tour.id}`)}
                  >
                    <div className="tour-discovery-media">
                      {tour.coverImage ? (
                        <img
                          src={tour.coverImage}
                          alt={tour.title}
                          className="tour-discovery-image"
                        />
                      ) : (
                        <div className="tour-discovery-placeholder">Chưa có ảnh bìa</div>
                      )}
                    </div>

                    <div className="tour-discovery-content">
                      <div className="tour-discovery-header">
                        <span className="tour-discovery-badge">Tour nổi bật</span>
                        <span className="tour-discovery-meta">
                          ⏱ {tour.estimatedDurationMinutes} phút
                        </span>
                      </div>

                      <h3 className="tour-discovery-title">{tour.title}</h3>
                      <p className="tour-discovery-description">{tour.description}</p>

                      <div className="tour-discovery-footer">
                        <span className="tour-discovery-stat">
                          📍 {tour.locationCount} địa điểm
                        </span>
                        <span className="tour-discovery-link">Xem chi tiết</span>
                      </div>
                    </div>
                  </article>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </>
  );
}
