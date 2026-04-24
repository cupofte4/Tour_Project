import React, { useEffect, useMemo, useState } from "react";
import Navbar from "../components/Navbar";
import TravelSidebar from "../components/TravelSidebar";
import { getAllLocations } from "../services/locationService";
import {
  getFavoriteLocationIds,
  toggleFavoriteLocation,
} from "../services/favoritesService";
import "../styles/app.css";
import "../styles/app.css";

const parseReviews = (rawReviews) => {
  if (!rawReviews) return [];

  if (Array.isArray(rawReviews)) {
    return rawReviews;
  }

  try {
    const parsed = JSON.parse(rawReviews);
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
};

function Favorites() {
  const [locations, setLocations] = useState([]);
  const [favoriteIds, setFavoriteIds] = useState([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    setFavoriteIds(getFavoriteLocationIds());
  }, []);

  useEffect(() => {
    getAllLocations()
      .then((data) => setLocations(Array.isArray(data) ? data : []))
      .finally(() => setIsLoading(false));
  }, []);

  useEffect(() => {
    const refreshLocations = async () => {
      const data = await getAllLocations();
      setLocations(Array.isArray(data) ? data : []);
    };

    window.addEventListener("focus", refreshLocations);

    return () => {
      window.removeEventListener("focus", refreshLocations);
    };
  }, []);

  useEffect(() => {
    const syncFavorites = () => {
      setFavoriteIds(getFavoriteLocationIds());
    };

    window.addEventListener("favoritesUpdated", syncFavorites);
    return () => {
      window.removeEventListener("favoritesUpdated", syncFavorites);
    };
  }, []);

  const favoriteLocations = useMemo(() => {
    const ids = new Set(favoriteIds);
    return locations.filter((location) => ids.has(location.id));
  }, [locations, favoriteIds]);

  const handleToggleFavorite = (locationId) => {
    toggleFavoriteLocation(locationId);
    setFavoriteIds(getFavoriteLocationIds());
  };

  return (
    <>
      <Navbar />
      <div className="container">
        <TravelSidebar />
        <div className="myprofile-main-content">
          <div className="myprofile-content">
            <div className="page-hero page-hero-favorites">
              <div>
                <p className="page-eyebrow">Danh sách đã lưu</p>
                <h1 className="myprofile-title">Địa điểm yêu thích</h1>
                <p className="page-subtitle">
                  Tổng hợp những nơi bạn muốn quay lại, với thông tin chính được trình bày rõ ràng và dễ quét nhanh.
                </p>
              </div>
            </div>

            <div className="favorites-page">
              {isLoading ? (
                <div className="favorite-empty-state">
                  Đang tải danh sách địa điểm yêu thích...
                </div>
              ) : favoriteLocations.length === 0 ? (
                <div className="favorite-empty-state">
                  Bạn chưa lưu địa điểm nào. Hãy nhấn nút yêu thích ở trang khám phá.
                </div>
              ) : (
                <div className="favorites-list">
                  {favoriteLocations.map((location) => {
                    const reviews = parseReviews(location.reviewsJson);
                    const averageRating =
                      reviews.length > 0
                        ? (
                            reviews.reduce(
                              (sum, item) => sum + (Number(item.rating) || 0),
                              0,
                            ) / reviews.length
                          ).toFixed(1)
                        : null;

                    let imageSrc = location.image || "";
                    try {
                      const images = JSON.parse(location.images || "[]");
                      if (Array.isArray(images) && images.length > 0) {
                        imageSrc = images[0];
                      }
                    } catch {
                      imageSrc = location.image || "";
                    }

                    return (
                      <article key={location.id} className="favorite-card">
                        <div className="favorite-card-media">
                          {imageSrc ? (
                            <img src={imageSrc} alt={location.name} />
                          ) : (
                            <div className="favorite-card-placeholder">
                              Không có ảnh
                            </div>
                          )}
                        </div>

                        <div className="favorite-card-content">
                          <div className="favorite-card-top">
                            <div className="favorite-card-main">
                              <span className="favorite-card-kicker">Địa điểm đã lưu</span>
                              <h2 className="favorite-card-title">{location.name}</h2>
                              <p className="favorite-card-description">
                                {location.description || "Chưa có mô tả cho địa điểm này."}
                              </p>
                            </div>

                            <div className="favorite-card-actions">
                              <p className={`favorite-card-rating ${averageRating ? "has-rating" : ""}`}>
                                {averageRating
                                  ? `★ ${averageRating}/5 · ${reviews.length} đánh giá`
                                  : "Chưa có đánh giá"}
                              </p>
                              <button
                                type="button"
                                className="favorite-btn active"
                                onClick={() => handleToggleFavorite(location.id)}
                              >
                                <span className="favorite-icon">♥</span>
                                <span>Đã lưu</span>
                              </button>
                            </div>
                          </div>

                          <div className="favorite-card-meta">
                            <div className="favorite-card-meta-row">
                              <span className="favorite-card-label">Địa chỉ</span>
                              <p>{location.address || "Đang cập nhật"}</p>
                            </div>
                            <div className="favorite-card-meta-row">
                              <span className="favorite-card-label">Số điện thoại</span>
                              <p>{location.phone || "Đang cập nhật"}</p>
                            </div>
                          </div>
                        </div>
                      </article>
                    );
                  })}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </>
  );
}

export default Favorites;
