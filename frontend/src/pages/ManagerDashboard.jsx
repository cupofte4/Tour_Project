import { useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  LuArrowUpRight,
  LuFolderOpen,
  LuLayoutGrid,
  LuLoaderCircle,
  LuMapPinned,
  LuRefreshCw,
  LuSave,
  LuTrash2,
  LuUpload,
} from "react-icons/lu";
import { FaStop, FaVolumeUp } from "react-icons/fa";
import AdminNavbar, { AdminSidebar } from "../components/AdminNavbar";
import { speak, stop } from "../services/ttsService";
import { uploadImages } from "../services/uploadService";
import {
  deleteMyLocation,
  getLocationTotals,
  getMyLocations,
  getTimeSeries,
  updateMyLocation,
} from "../services/managerService";
import "../styles/admin.css";

const MANAGER_TABS = [
  { key: "my-locations", label: "My Locations", icon: LuMapPinned },
  { key: "statistics", label: "Statistics", icon: LuLayoutGrid },
];

const createEmptyLocationForm = () => ({
  id: null,
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

const parseImageList = (rawValue) => {
  if (!rawValue) return [];
  if (Array.isArray(rawValue)) return rawValue.filter(Boolean);

  try {
    const parsed = JSON.parse(rawValue);
    return Array.isArray(parsed) ? parsed.filter(Boolean) : [];
  } catch {
    return rawValue
      .split(",")
      .map((item) => item.trim())
      .filter(Boolean);
  }
};

const stringifyImageList = (images) => JSON.stringify(images.filter(Boolean));

function ManagerDashboard() {
  const navigate = useNavigate();
  const galleryInputRef = useRef(null);

  const [user, setUser] = useState(null);
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [activeMenu, setActiveMenu] = useState("my-locations");

  const [locationSearch, setLocationSearch] = useState("");
  const [locations, setLocations] = useState([]);
  const [isLoadingLocations, setIsLoadingLocations] = useState(true);
  const [isSavingLocation, setIsSavingLocation] = useState(false);
  const [locationError, setLocationError] = useState("");
  const [locationNotice, setLocationNotice] = useState("");
  const [locationForm, setLocationForm] = useState(createEmptyLocationForm());
  const [isUploadingGallery, setIsUploadingGallery] = useState(false);
  const [playingLang, setPlayingLang] = useState("");

  const [statsTotals, setStatsTotals] = useState([]);
  const [timeSeries, setTimeSeries] = useState([]);
  const [isLoadingStats, setIsLoadingStats] = useState(true);
  const [statsError, setStatsError] = useState("");

  const galleryImages = useMemo(
    () => parseImageList(locationForm.images),
    [locationForm.images],
  );

  useEffect(() => {
    try {
      const userString = localStorage.getItem("user");
      if (!userString) {
        navigate("/login");
        return;
      }

      const userData = JSON.parse(userString);
      const role = (userData?.role || "").toLowerCase();

      if (!userData || role !== "manager") {
        navigate("/");
        return;
      }

      setUser(userData);
      setIsAuthorized(true);
    } catch {
      navigate("/login");
    }
  }, [navigate]);

  useEffect(() => {
    if (!isAuthorized || !user?.id) return;
    loadLocations();
    loadStats();
  }, [isAuthorized, user?.id]);

  useEffect(() => {
    return () => stop();
  }, []);

  const loadLocations = async () => {
    if (!user?.id) return;
    setIsLoadingLocations(true);
    setLocationError("");

    try {
      const data = await getMyLocations(user.id);
      setLocations(Array.isArray(data) ? data : []);
    } catch (error) {
      console.error("Failed to load manager locations:", error);
      setLocationError("Không tải được danh sách địa điểm.");
      setLocations([]);
    } finally {
      setIsLoadingLocations(false);
    }
  };

  const loadStats = async () => {
    if (!user?.id) return;
    setIsLoadingStats(true);
    setStatsError("");

    try {
      const [totals, series] = await Promise.all([
        getLocationTotals(user.id),
        getTimeSeries(user.id, 30),
      ]);
      setStatsTotals(Array.isArray(totals) ? totals : []);
      setTimeSeries(Array.isArray(series) ? series : []);
    } catch (error) {
      console.error("Failed to load stats:", error);
      setStatsError("Không tải được thống kê.");
      setStatsTotals([]);
      setTimeSeries([]);
    } finally {
      setIsLoadingStats(false);
    }
  };

  const filteredLocations = useMemo(() => {
    const keyword = locationSearch.trim().toLowerCase();
    if (!keyword) return locations;
    return locations.filter((item) =>
      (item.name || "").toLowerCase().includes(keyword),
    );
  }, [locationSearch, locations]);

  if (!isAuthorized || !user) return null;

  const dashboardTitle =
    activeMenu === "statistics" ? "Statistics" : "My locations";
  const dashboardSubtitle =
    activeMenu === "statistics"
      ? "Track views and audio plays for the locations you own."
      : "Create and manage your own stalls/locations and narration content.";

  const showSearch = activeMenu === "my-locations";

  const handleLogout = () => {
    localStorage.removeItem("user");
    localStorage.removeItem("username");
    navigate("/login");
  };

  const resetLocationForm = () => {
    stop();
    setLocationForm(createEmptyLocationForm());
    setLocationError("");
    setLocationNotice("");
    setIsUploadingGallery(false);
    setPlayingLang("");
  };

  const startEditingLocation = (location) => {
    stop();
    setLocationError("");
    setLocationNotice("");
    setPlayingLang("");
    setLocationForm({
      id: location.id,
      name: location.name || "",
      description: location.description || "",
      image: location.image || "",
      images: location.images || "[]",
      address: location.address || "",
      phone: location.phone || "",
      reviewsJson: location.reviewsJson || "[]",
      latitude: location.latitude ?? "",
      longitude: location.longitude ?? "",
      textVi: location.textVi || "",
      textEn: location.textEn || "",
      textZh: location.textZh || "",
      textDe: location.textDe || "",
    });
    setActiveMenu("my-locations");
  };

  const handleLocationInputChange = (event) => {
    const { name, value } = event.target;
    setLocationForm((current) => ({ ...current, [name]: value }));
  };

  const handlePreviewAudio = async (text, langCode) => {
    if (!text?.trim()) return;
    if (playingLang === langCode) {
      stop();
      setPlayingLang("");
      return;
    }

    setPlayingLang(langCode);
    try {
      await speak(text, langCode);
    } finally {
      setPlayingLang("");
    }
  };

  const renderPreviewButton = (text, langCode) => (
    <button
      type="button"
      className={`preview-audio-btn ${playingLang === langCode ? "is-playing" : ""}`}
      onClick={() => handlePreviewAudio(text, langCode)}
    >
      {playingLang === langCode ? <FaStop size={11} /> : <FaVolumeUp size={11} />}
      <span>{playingLang === langCode ? "Dừng" : "Nghe thử"}</span>
    </button>
  );

  const handleGalleryUpload = async (files) => {
    if (!files || files.length === 0) return;
    setIsUploadingGallery(true);
    setLocationError("");
    setLocationNotice("");

    try {
      const uploadedUrls = await uploadImages(files);
      const next = [...galleryImages, ...(uploadedUrls || [])];
      setLocationForm((current) => ({
        ...current,
        images: stringifyImageList(next),
        image: current.image || next[0] || "",
      }));
      setLocationNotice("Đã upload hình ảnh.");
    } catch (error) {
      console.error("Upload gallery failed:", error);
      setLocationError("Không upload được hình ảnh.");
    } finally {
      setIsUploadingGallery(false);
      if (galleryInputRef.current) galleryInputRef.current.value = "";
    }
  };

  const handleRemoveGalleryImage = (url) => {
    const next = galleryImages.filter((item) => item !== url);
    setLocationForm((current) => ({
      ...current,
      images: stringifyImageList(next),
      image: current.image === url ? next[0] || "" : current.image,
    }));
  };

  const handleLocationSubmit = async (event) => {
    event.preventDefault();
    if (!user?.id) return;
    if (!locationForm.id) {
      setLocationError("Vui lòng chọn một địa điểm để chỉnh sửa.");
      return;
    }

    setIsSavingLocation(true);
    setLocationError("");
    setLocationNotice("");

    try {
      const payload = {
        ...locationForm,
        latitude: Number(locationForm.latitude || 0),
        longitude: Number(locationForm.longitude || 0),
        images: locationForm.images || "[]",
        reviewsJson: locationForm.reviewsJson || "[]",
      };

      const updated = await updateMyLocation(user.id, locationForm.id, payload);

      setLocations((current) => {
        const list = Array.isArray(current) ? current : [];
        return list.map((item) => (item.id === updated.id ? updated : item));
      });
      setLocationNotice("Đã lưu thay đổi.");
      loadStats();
    } catch (error) {
      console.error("Failed to save location:", error);
      setLocationError(error.message || "Không lưu được địa điểm.");
    } finally {
      setIsSavingLocation(false);
    }
  };

  const handleDeleteLocation = async (locationId) => {
    if (!user?.id) return;
    if (!window.confirm("Xóa địa điểm này?")) return;

    setLocationError("");
    setLocationNotice("");

    try {
      await deleteMyLocation(user.id, locationId);
      setLocations((current) => current.filter((item) => item.id !== locationId));
      if (locationForm.id === locationId) resetLocationForm();
      setLocationNotice("Đã xóa địa điểm.");
      loadStats();
    } catch (error) {
      console.error("Failed to delete location:", error);
      setLocationError(error.message || "Không xóa được địa điểm.");
    }
  };

  const totalViews = statsTotals.reduce((sum, item) => sum + (item.viewsCount || 0), 0);
  const totalPlays = statsTotals.reduce(
    (sum, item) => sum + (item.audioPlaysCount || 0),
    0,
  );

  const renderMyLocations = () => (
    <section className="location-manager location-manager-stacked">
      <div className="table-card shell-card">
        <div className="section-heading">
          <div>
            <p className="section-eyebrow">Owned locations</p>
            <h2 className="section-title">Danh sách địa điểm</h2>
          </div>
          <button type="button" className="ghost-action" onClick={loadLocations}>
            <LuRefreshCw size={16} />
            <span>Tải lại</span>
          </button>
        </div>

        {isLoadingLocations ? (
          <div className="admin-empty-state">
            <div className="admin-empty-state-icon">
              <LuLoaderCircle size={26} className="spin" />
            </div>
            <h3>Đang tải địa điểm</h3>
            <p>Dữ liệu đang được đồng bộ từ backend.</p>
          </div>
        ) : filteredLocations.length === 0 ? (
          <div className="admin-empty-state">
            <div className="admin-empty-state-icon">
              <LuFolderOpen size={26} />
            </div>
            <h3>Chưa có địa điểm</h3>
            <p>Tạo một địa điểm mới để bắt đầu quản lý nội dung.</p>
          </div>
        ) : (
          <table className="users-table">
            <thead>
              <tr>
                <th>STT</th>
                <th>Tên</th>
                <th>Địa chỉ</th>
                <th>Hành động</th>
              </tr>
            </thead>
            <tbody>
              {filteredLocations.map((item, index) => (
                <tr key={item.id}>
                  <td>{index + 1}</td>
                  <td>
                    <strong>{item.name}</strong>
                  </td>
                  <td>{item.address || "-"}</td>
                  <td>
                    <div className="table-actions">
                      <button
                        type="button"
                        className="action-btn"
                        onClick={() => startEditingLocation(item)}
                      >
                        Sửa
                      </button>
                      <button
                        type="button"
                        className="action-btn action-btn-danger"
                        onClick={() => handleDeleteLocation(item.id)}
                      >
                        <LuTrash2 size={14} />
                        <span>Xóa</span>
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <div className="table-card shell-card">
          <div className="section-heading">
            <div>
              <p className="section-eyebrow">Edit Location</p>
              <h2 className="section-title">
              {locationForm.id ? "Chỉnh sửa địa điểm" : "Chọn địa điểm để chỉnh sửa"}
              </h2>
            </div>
            <button type="button" className="ghost-action" onClick={resetLocationForm}>
              <span>Đặt lại form</span>
              <LuArrowUpRight size={16} />
            </button>
          </div>

        {!locationForm.id ? (
          <div className="admin-empty-state compact-empty-state">
            <div className="admin-empty-state-icon">
              <LuFolderOpen size={24} />
            </div>
            <h3>Chọn địa điểm để chỉnh sửa</h3>
            <p>Manager chỉ có quyền xem và sửa các địa điểm đã được phân công.</p>
          </div>
        ) : (
          <form className="location-form" onSubmit={handleLocationSubmit}>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="name">Tên địa điểm</label>
              <input
                id="name"
                name="name"
                value={locationForm.name}
                onChange={handleLocationInputChange}
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="address">Địa chỉ</label>
              <input
                id="address"
                name="address"
                value={locationForm.address}
                onChange={handleLocationInputChange}
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="phone">Số điện thoại</label>
              <input
                id="phone"
                name="phone"
                value={locationForm.phone}
                onChange={handleLocationInputChange}
              />
            </div>
            <div className="form-group">
              <label htmlFor="image">Ảnh đại diện (URL)</label>
              <input
                id="image"
                name="image"
                value={locationForm.image}
                onChange={handleLocationInputChange}
                placeholder="https://..."
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="latitude">Latitude</label>
              <input
                id="latitude"
                name="latitude"
                value={locationForm.latitude}
                onChange={handleLocationInputChange}
              />
            </div>
            <div className="form-group">
              <label htmlFor="longitude">Longitude</label>
              <input
                id="longitude"
                name="longitude"
                value={locationForm.longitude}
                onChange={handleLocationInputChange}
              />
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="description">Mô tả ngắn</label>
            <textarea
              id="description"
              name="description"
              rows={3}
              value={locationForm.description}
              onChange={handleLocationInputChange}
              required
            />
          </div>

          <div className="form-group">
            <div className="preview-label-row">
              <label>Gallery</label>
              <button
                type="button"
                className="btn action-btn-muted"
                onClick={() => galleryInputRef.current?.click()}
                disabled={isUploadingGallery}
              >
                <LuUpload size={16} />
                <span>Upload</span>
              </button>
              <input
                ref={galleryInputRef}
                type="file"
                accept="image/*"
                multiple
                hidden
                onChange={(event) => handleGalleryUpload(event.target.files)}
              />
            </div>

            {galleryImages.length > 0 && (
              <div className="gallery-grid">
                {galleryImages.map((url) => (
                  <div key={url} className="gallery-item">
                    <img src={url} alt="gallery" />
                    <button
                      type="button"
                      className="gallery-remove"
                      onClick={() => handleRemoveGalleryImage(url)}
                      aria-label="Remove"
                    >
                      <LuTrash2 size={14} />
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>

          <div className="location-language-grid">
            <div className="form-group">
              <div className="preview-label-row">
                <label htmlFor="textVi">Vietnamese</label>
                {renderPreviewButton(locationForm.textVi, "vi-VN")}
              </div>
              <textarea
                id="textVi"
                name="textVi"
                rows={5}
                value={locationForm.textVi}
                onChange={handleLocationInputChange}
              />
            </div>
            <div className="form-group">
              <div className="preview-label-row">
                <label htmlFor="textEn">English</label>
                {renderPreviewButton(locationForm.textEn, "en-US")}
              </div>
              <textarea
                id="textEn"
                name="textEn"
                rows={5}
                value={locationForm.textEn}
                onChange={handleLocationInputChange}
              />
            </div>
            <div className="form-group">
              <div className="preview-label-row">
                <label htmlFor="textZh">Chinese</label>
                {renderPreviewButton(locationForm.textZh, "zh-CN")}
              </div>
              <textarea
                id="textZh"
                name="textZh"
                rows={5}
                value={locationForm.textZh}
                onChange={handleLocationInputChange}
              />
            </div>
            <div className="form-group">
              <div className="preview-label-row">
                <label htmlFor="textDe">German</label>
                {renderPreviewButton(locationForm.textDe, "de-DE")}
              </div>
              <textarea
                id="textDe"
                name="textDe"
                rows={5}
                value={locationForm.textDe}
                onChange={handleLocationInputChange}
              />
            </div>
          </div>

          {locationError && (
            <div className="admin-feedback admin-feedback-error">{locationError}</div>
          )}
          {locationNotice && (
            <div className="admin-feedback admin-feedback-success">
              {locationNotice}
            </div>
          )}

          <div className="location-form-actions">
            <button
              type="submit"
              className="btn btn-primary"
              disabled={isSavingLocation || isUploadingGallery}
            >
              {isSavingLocation ? (
                <LuLoaderCircle size={16} className="spin" />
              ) : (
                <LuSave size={16} />
              )}
              <span> Lưu thay đổi</span>
            </button>
            <button
              type="button"
              className="btn action-btn-muted"
              onClick={resetLocationForm}
            >
              Hủy chỉnh sửa
            </button>
          </div>
        </form>
        )}
      </div>
    </section>
  );

  const renderStatistics = () => {
    const latest = timeSeries.slice(-7);
    const maxValue = Math.max(
      1,
      ...latest.map((item) => Math.max(item.viewsCount || 0, item.audioPlaysCount || 0)),
    );

    return (
      <section className="admin-grid">
        <div className="table-card shell-card">
          <div className="section-heading">
            <div>
              <p className="section-eyebrow">Summary</p>
              <h2 className="section-title">Tổng quan</h2>
            </div>
            <button type="button" className="ghost-action" onClick={loadStats}>
              <LuRefreshCw size={16} />
              <span>Tải lại</span>
            </button>
          </div>

          {statsError && (
            <div className="admin-feedback admin-feedback-error">{statsError}</div>
          )}

          {isLoadingStats ? (
            <div className="admin-empty-state">
              <div className="admin-empty-state-icon">
                <LuLoaderCircle size={26} className="spin" />
              </div>
              <h3>Đang tải thống kê</h3>
              <p>Dữ liệu đang được đồng bộ từ backend.</p>
            </div>
          ) : (
            <>
              <section className="stats-grid">
                <article className="stat-card shell-card">
                  <div className="stat-copy">
                    <p className="stat-title">Tổng lượt xem</p>
                    <h3 className="stat-value">{totalViews.toLocaleString()}</h3>
                    <p className="stat-note">Tính theo location_stats</p>
                  </div>
                </article>
                <article className="stat-card shell-card">
                  <div className="stat-copy">
                    <p className="stat-title">Tổng lượt phát audio</p>
                    <h3 className="stat-value">{totalPlays.toLocaleString()}</h3>
                    <p className="stat-note">Tính theo location_stats</p>
                  </div>
                </article>
              </section>

              <div className="table-card shell-card">
                <div className="section-heading">
                  <div>
                    <p className="section-eyebrow">Last 7 days</p>
                    <h2 className="section-title">Xu hướng gần đây</h2>
                  </div>
                </div>

                {latest.length === 0 ? (
                  <div className="admin-empty-state compact-empty-state">
                    <div className="admin-empty-state-icon">
                      <LuFolderOpen size={24} />
                    </div>
                    <h3>Chưa có dữ liệu thống kê</h3>
                    <p>Thêm dữ liệu vào bảng location_stats để xem biểu đồ.</p>
                  </div>
                ) : (
                  <table className="users-table">
                    <thead>
                      <tr>
                        <th>Ngày</th>
                        <th>Views</th>
                        <th>Audio plays</th>
                      </tr>
                    </thead>
                    <tbody>
                      {latest.map((item) => (
                        <tr key={item.date}>
                          <td>{item.date}</td>
                          <td>
                            {item.viewsCount}
                            <div
                              style={{
                                height: 6,
                                background: "rgba(59,130,246,0.15)",
                                borderRadius: 999,
                                marginTop: 6,
                              }}
                            >
                              <div
                                style={{
                                  height: 6,
                                  width: `${Math.round(((item.viewsCount || 0) / maxValue) * 100)}%`,
                                  background: "rgba(59,130,246,0.8)",
                                  borderRadius: 999,
                                }}
                              />
                            </div>
                          </td>
                          <td>
                            {item.audioPlaysCount}
                            <div
                              style={{
                                height: 6,
                                background: "rgba(16,185,129,0.15)",
                                borderRadius: 999,
                                marginTop: 6,
                              }}
                            >
                              <div
                                style={{
                                  height: 6,
                                  width: `${Math.round(((item.audioPlaysCount || 0) / maxValue) * 100)}%`,
                                  background: "rgba(16,185,129,0.8)",
                                  borderRadius: 999,
                                }}
                              />
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}
              </div>
            </>
          )}
        </div>

        <div className="table-card shell-card">
          <div className="section-heading">
            <div>
              <p className="section-eyebrow">Per location</p>
              <h2 className="section-title">Thống kê theo địa điểm</h2>
            </div>
          </div>

          {isLoadingStats ? (
            <div className="admin-empty-state">
              <div className="admin-empty-state-icon">
                <LuLoaderCircle size={26} className="spin" />
              </div>
              <h3>Đang tải dữ liệu</h3>
              <p>Vui lòng đợi.</p>
            </div>
          ) : statsTotals.length === 0 ? (
            <div className="admin-empty-state">
              <div className="admin-empty-state-icon">
                <LuFolderOpen size={26} />
              </div>
              <h3>Chưa có dữ liệu</h3>
              <p>Bảng location_stats hiện đang trống.</p>
            </div>
          ) : (
            <table className="users-table">
              <thead>
                <tr>
                  <th>Địa điểm</th>
                  <th>Views</th>
                  <th>Audio plays</th>
                </tr>
              </thead>
              <tbody>
                {statsTotals.map((item) => (
                  <tr key={item.locationId}>
                    <td>{item.locationName}</td>
                    <td>{item.viewsCount}</td>
                    <td>{item.audioPlaysCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </section>
    );
  };

  return (
    <div className="admin-shell">
      <AdminSidebar
        brandTitle="Manager Portal"
        brandSubtitle="Locations & stats"
        activeKey={activeMenu}
        items={MANAGER_TABS}
        onItemClick={(item) => setActiveMenu(item.key)}
        onLogout={handleLogout}
      />
      <main className="admin-main">
        <AdminNavbar
          title={dashboardTitle}
          subtitle={dashboardSubtitle}
          showSearch={showSearch}
          searchValue={locationSearch}
          searchPlaceholder="Tìm theo tên địa điểm"
          onSearchChange={(value) => setLocationSearch(value)}
        />
        <div className="admin-content">
          {activeMenu === "my-locations" && renderMyLocations()}
          {activeMenu === "statistics" && renderStatistics()}
        </div>
      </main>
    </div>
  );
}

export default ManagerDashboard;
