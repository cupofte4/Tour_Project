import { useEffect, useMemo, useState } from "react";
import {
  speakLocationAsync,
  stop,
  getTextForLang,
} from "../services/ttsService";
import { submitLocationReview } from "../services/locationService";
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

const formatReviewDate = (value) => {
  if (!value) return "Vừa xong";

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "Vừa xong";

  return date.toLocaleString("vi-VN", {
    dateStyle: "short",
    timeStyle: "short",
  });
};

const renderStars = (rating) => "★".repeat(rating) + "☆".repeat(5 - rating);

function LocationCard({ location, lang, apiUrl, onLocationUpdated }) {
  const [isPlaying, setIsPlaying] = useState(false);
  const [reviewRating, setReviewRating] = useState(5);
  const [reviewComment, setReviewComment] = useState("");
  const [isSubmittingReview, setIsSubmittingReview] = useState(false);
  const [reviewError, setReviewError] = useState("");
  const reviews = useMemo(
    () => parseReviews(location?.reviewsJson),
    [location?.reviewsJson],
  );
  const images = useMemo(() => {
    if (!location) {
      return [];
    }

    try {
      const parsedImages = JSON.parse(location.images || "[]");
      if (Array.isArray(parsedImages) && parsedImages.length > 0) {
        return parsedImages;
      }
    } catch {
      // Ignore invalid gallery JSON and fall back to the primary image.
    }

    return location.image ? [location.image] : [];
  }, [location]);

  useEffect(() => {
    setReviewRating(5);
    setReviewComment("");
    setReviewError("");
  }, [location?.id]);

  if (!location) {
    return (
      <div className="empty-state">
        <div className="icon">🔍</div>
        <p>Đang tìm địa điểm gần bạn...</p>
      </div>
    );
  }

  const text = getTextForLang(location, lang);
  const averageRating =
    reviews.length > 0
      ? (
          reviews.reduce((sum, item) => sum + (Number(item.rating) || 0), 0) /
          reviews.length
        ).toFixed(1)
      : null;

  const handlePlay = async () => {
    if (isPlaying) {
      stop();
      setIsPlaying(false);
      return;
    }

    setIsPlaying(true);

    try {
      await speakLocationAsync(location, lang);
    } finally {
      setIsPlaying(false);
    }
  };

  const handleReviewSubmit = async (event) => {
    event.preventDefault();

    const comment = reviewComment.trim();
    if (!comment) {
      setReviewError("Vui lòng nhập bình luận trước khi gửi đánh giá.");
      return;
    }

    const storedUser = localStorage.getItem("user");
    let author = "Khách";

    if (storedUser) {
      try {
        const user = JSON.parse(storedUser);
        author = user?.fullName || user?.username || author;
      } catch {
        author = "Khách";
      }
    }

    setIsSubmittingReview(true);
    setReviewError("");

    try {
      const updatedLocation = await submitLocationReview(location.id, {
        author,
        rating: reviewRating,
        comment,
      });

      setReviewComment("");
      setReviewRating(5);
      onLocationUpdated?.(updatedLocation);
    } catch (error) {
      setReviewError(error.message || "Không gửi được đánh giá lúc này.");
    } finally {
      setIsSubmittingReview(false);
    }
  };

  return (
    <div className="location-card">
      <div className="location-name">{location.name}</div>

      {images.length > 0 && null}

      <div className="location-desc">{text}</div>

      <div className="tts-controls">
        <button className="tts-btn play" onClick={handlePlay}>
          {isPlaying ? "⏸ Đang đọc thuyết minh" : "▶ Đọc thuyết minh"}
        </button>
        <button
          className="tts-btn stop"
          onClick={() => {
            stop();
            setIsPlaying(false);
          }}
        >
          ⏹ Dừng
        </button>
      </div>
      <div className="location-meta-card">
        <div className="location-meta-item">
          <span className="location-meta-label">Địa chỉ</span>
          <span className="location-meta-value">
            {location.address || "Đang cập nhật"}
          </span>
        </div>
        <div className="location-meta-item">
          <span className="location-meta-label">Số điện thoại</span>
          <span className="location-meta-value">
            {location.phone || "Đang cập nhật"}
          </span>
        </div>
      </div>

      <div className="reviews-card">
        <div className="reviews-header">
          <div>
            <div className="reviews-title">Đánh giá quán</div>
            <div className="reviews-summary">
              {averageRating
                ? `${averageRating}/5 sao · ${reviews.length} đánh giá`
                : "Chưa có đánh giá nào"}
            </div>
          </div>
          {averageRating ? (
            <div className="reviews-average">
              {renderStars(Math.round(Number(averageRating)))}
            </div>
          ) : null}
        </div>

        <form className="review-form" onSubmit={handleReviewSubmit}>
          <label className="review-label" htmlFor="review-rating">
            Số sao
          </label>
          <select
            id="review-rating"
            className="review-select"
            value={reviewRating}
            onChange={(event) => setReviewRating(Number(event.target.value))}
          >
            {[5, 4, 3, 2, 1].map((value) => (
              <option key={value} value={value}>
                {value} sao
              </option>
            ))}
          </select>

          <label className="review-label" htmlFor="review-comment">
            Bình luận
          </label>
          <textarea
            id="review-comment"
            className="review-textarea"
            value={reviewComment}
            onChange={(event) => setReviewComment(event.target.value)}
            placeholder="Món nào ngon, giá cả ra sao, phục vụ thế nào?"
          />

          {reviewError ? (
            <div className="review-error">{reviewError}</div>
          ) : null}

          <button
            className="review-submit-btn"
            type="submit"
            disabled={isSubmittingReview}
          >
            {isSubmittingReview ? "Đang gửi..." : "Gửi đánh giá"}
          </button>
        </form>

        <div className="reviews-list">
          {reviews.length > 0 ? (
            reviews.map((review, index) => (
              <article
                key={`${review.author}-${review.createdAt}-${index}`}
                className="review-item"
              >
                <div className="review-item-top">
                  <strong>{review.author || "Khách"}</strong>
                  <span>{renderStars(Number(review.rating) || 0)}</span>
                </div>
                <p>{review.comment}</p>
                <small>{formatReviewDate(review.createdAt)}</small>
              </article>
            ))
          ) : (
            <div className="reviews-empty">
              Hãy là người đầu tiên để lại nhận xét cho quán này.
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default LocationCard;
