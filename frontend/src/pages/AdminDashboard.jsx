import React, { useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  LuArrowUpRight,
  LuFolderOpen,
  LuLayoutGrid,
  LuLoaderCircle,
  LuLock,
  LuLockOpen,
  LuMapPinned,
  LuMusic4,
  LuPlus,
  LuRefreshCw,
  LuSave,
  LuSparkles,
  LuTrash2,
  LuUpload,
  LuUsers,
} from "react-icons/lu";
import { FaStop, FaVolumeUp } from "react-icons/fa";
import AdminNavbar, { AdminSidebar } from "../components/AdminNavbar";
import {
  createLocation,
  deleteLocation,
  getAllLocations,
  updateLocation,
} from "../services/locationService";
import { uploadAudio, uploadImages } from "../services/uploadService";
import { getAllUsers, updateAdminUser } from "../services/userService";
import "../styles/admin.css";

const DASHBOARD_TABS = [
  { key: "overview", label: "Tổng quan", icon: LuLayoutGrid },
  { key: "users", label: "Người dùng", icon: LuUsers },
  { key: "locations", label: "Địa điểm", icon: LuMapPinned },
];

const createEmptyLocationForm = () => ({
  id: null,
  name: "",
  description: "",
  image: "",
  images: "[]",
  audio: "",
  latitude: "",
  longitude: "",
  textVi: "",
  textEn: "",
  textZh: "",
  textDe: "",
});

const createEmptyUserForm = () => ({
  id: null,
  fullName: "",
  username: "",
  role: "",
  isLocked: false,
  password: "",
});

const parseImageList = (rawValue) => {
  if (!rawValue) return [];

  if (Array.isArray(rawValue)) {
    return rawValue.filter(Boolean);
  }

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

const getUserStatus = (user) =>
  user.isLocked
    ? { key: "locked", label: "Đã khóa" }
    : { key: "active", label: "Đang hoạt động" };

function AdminDashboard() {
  const navigate = useNavigate();
  const galleryInputRef = useRef(null);
  const audioInputRef = useRef(null);

  const [user, setUser] = useState(null);
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [activeMenu, setActiveMenu] = useState("overview");

  const [userSearch, setUserSearch] = useState("");
  const [locationSearch, setLocationSearch] = useState("");

  const [users, setUsers] = useState([]);
  const [isLoadingUsers, setIsLoadingUsers] = useState(true);
  const [isSavingUser, setIsSavingUser] = useState(false);
  const [userError, setUserError] = useState("");
  const [userNotice, setUserNotice] = useState("");
  const [userForm, setUserForm] = useState(createEmptyUserForm());

  const [locations, setLocations] = useState([]);
  const [isLoadingLocations, setIsLoadingLocations] = useState(true);
  const [isSavingLocation, setIsSavingLocation] = useState(false);
  const [locationError, setLocationError] = useState("");
  const [locationNotice, setLocationNotice] = useState("");
  const [locationForm, setLocationForm] = useState(createEmptyLocationForm());
  const [isUploadingGallery, setIsUploadingGallery] = useState(false);
  const [isUploadingAudio, setIsUploadingAudio] = useState(false);
  const [playingLang, setPlayingLang] = useState("");

  const galleryImages = useMemo(() => parseImageList(locationForm.images), [locationForm.images]);

  useEffect(() => {
    try {
      const userString = localStorage.getItem("user");
      if (!userString) {
        navigate("/login");
        return;
      }

      const userData = JSON.parse(userString);
      const role = (userData?.role || "").toLowerCase();

      if (!userData || role !== "admin") {
        navigate("/");
        return;
      }

      setUser(userData);
      setIsAuthorized(true);
    } catch (error) {
      console.error("Error parsing user data:", error);
      navigate("/login");
    }
  }, [navigate]);

  useEffect(() => {
    if (!isAuthorized) return;
    loadUsers();
    loadLocations();
  }, [isAuthorized]);

  useEffect(() => {
    return () => {
      if (window?.speechSynthesis) {
        window.speechSynthesis.cancel();
      }
    };
  }, []);

  const loadUsers = async () => {
    setIsLoadingUsers(true);
    setUserError("");

    try {
      const data = await getAllUsers();
      setUsers(Array.isArray(data) ? data : []);
    } catch (error) {
      console.error("Failed to load users:", error);
      setUserError("Không tải được danh sách người dùng.");
    } finally {
      setIsLoadingUsers(false);
    }
  };

  const loadLocations = async () => {
    setIsLoadingLocations(true);
    setLocationError("");

    try {
      const data = await getAllLocations();
      setLocations(Array.isArray(data) ? data : []);
    } catch (error) {
      console.error("Failed to load locations:", error);
      setLocationError("Không tải được danh sách địa điểm từ backend.");
    } finally {
      setIsLoadingLocations(false);
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("user");
    localStorage.removeItem("username");
    navigate("/login");
  };

  const resetUserForm = () => {
    setUserForm(createEmptyUserForm());
    setUserError("");
  };

  const startEditingUser = (targetUser) => {
    setUserError("");
    setUserNotice("");
    setUserForm({
      id: targetUser.id,
      fullName: targetUser.fullName || "",
      username: targetUser.username || "",
      role: targetUser.role || "",
      isLocked: !!targetUser.isLocked,
      password: "",
    });
    setActiveMenu("users");
  };

  const handleUserFormChange = (event) => {
    const { name, value, type, checked } = event.target;
    setUserForm((current) => ({
      ...current,
      [name]: type === "checkbox" ? checked : value,
    }));
  };

  const handleUserSubmit = async (event) => {
    event.preventDefault();
    if (!userForm.id) return;

    setIsSavingUser(true);
    setUserError("");
    setUserNotice("");

    try {
      const updatedUser = await updateAdminUser(userForm.id, {
        password: userForm.password.trim() || null,
        isLocked: userForm.isLocked,
      });

      setUsers((current) => current.map((item) => (item.id === updatedUser.id ? updatedUser : item)));
      setUserForm((current) => ({ ...current, password: "" }));
      setUserNotice("Đã cập nhật tài khoản người dùng.");
    } catch (error) {
      console.error("Failed to update user:", error);
      setUserError(error.message || "Không cập nhật được người dùng.");
    } finally {
      setIsSavingUser(false);
    }
  };

  const handleQuickToggleLock = async (targetUser) => {
    setUserError("");
    setUserNotice("");

    try {
      const updatedUser = await updateAdminUser(targetUser.id, {
        password: null,
        isLocked: !targetUser.isLocked,
      });

      setUsers((current) => current.map((item) => (item.id === updatedUser.id ? updatedUser : item)));
      setUserNotice(updatedUser.isLocked ? "Tài khoản đã được khóa." : "Tài khoản đã được mở khóa.");

      if (userForm.id === updatedUser.id) {
        setUserForm((current) => ({ ...current, isLocked: updatedUser.isLocked }));
      }
    } catch (error) {
      console.error("Failed to toggle user lock:", error);
      setUserError(error.message || "Không đổi được trạng thái tài khoản.");
    }
  };
  const handleLocationInputChange = (event) => {
    const { name, value } = event.target;
    setLocationForm((current) => ({ ...current, [name]: value }));
  };

  const resetLocationForm = () => {
    if (window?.speechSynthesis) {
      window.speechSynthesis.cancel();
    }

    setLocationForm(createEmptyLocationForm());
    setLocationError("");
    setLocationNotice("");
    setIsUploadingGallery(false);
    setIsUploadingAudio(false);
    setPlayingLang("");
  };

  const startEditingLocation = (location) => {
    if (window?.speechSynthesis) {
      window.speechSynthesis.cancel();
    }

    setLocationError("");
    setLocationNotice("");
    setPlayingLang("");
    setLocationForm({
      id: location.id,
      name: location.name || "",
      description: location.description || "",
      image: location.image || "",
      images: location.images || "[]",
      audio: location.audio || "",
      latitude: location.latitude ?? "",
      longitude: location.longitude ?? "",
      textVi: location.textVi || "",
      textEn: location.textEn || "",
      textZh: location.textZh || "",
      textDe: location.textDe || "",
    });
    setActiveMenu("locations");
  };

  const setGalleryImages = (images) => {
    setLocationForm((current) => ({ ...current, images: stringifyImageList(images) }));
  };

  const handleGalleryUpload = async (files) => {
    const selectedFiles = Array.from(files || []).filter((file) => file.type.startsWith("image/"));
    if (selectedFiles.length === 0) return;

    setIsUploadingGallery(true);
    setLocationError("");

    try {
      const uploadedUrls = await uploadImages(selectedFiles);
      const mergedImages = Array.from(new Set([...galleryImages, ...uploadedUrls].filter(Boolean)));
      setGalleryImages(mergedImages);
      setLocationForm((current) => ({ ...current, image: mergedImages[0] || "" }));
      setLocationNotice(uploadedUrls.length > 1 ? `Đã tải ${uploadedUrls.length} ảnh vào bộ sưu tập.` : "Đã tải ảnh vào bộ sưu tập.");
    } catch (error) {
      setLocationError(error.message || "Không tải được danh sách ảnh.");
    } finally {
      setIsUploadingGallery(false);
    }
  };

  const handleAudioUpload = async (files) => {
    const selectedFiles = Array.from(files || []).filter((file) =>
      file.type.startsWith("audio/") || /\.(mp3|wav|ogg|m4a)$/i.test(file.name)
    );
    if (selectedFiles.length === 0) return;

    setIsUploadingAudio(true);
    setLocationError("");

    try {
      const audioFileName = await uploadAudio(selectedFiles[0]);
      setLocationForm((current) => ({ ...current, audio: audioFileName }));
      setLocationNotice("Đã tải audio lên thành công.");
    } catch (error) {
      setLocationError(error.message || "Không tải được audio.");
    } finally {
      setIsUploadingAudio(false);
    }
  };

  const removeGalleryImage = (imageUrl) => {
    const nextImages = galleryImages.filter((item) => item !== imageUrl);
    setGalleryImages(nextImages);
    setLocationForm((current) => ({ ...current, image: nextImages[0] || "" }));
  };

  const handlePreviewAudio = (text, langCode) => {
    const normalizedText = (text || "").trim();

    if (!window?.speechSynthesis) {
      setLocationError("Trình duyệt hiện tại không hỗ trợ text-to-speech.");
      return;
    }

    window.speechSynthesis.cancel();

    if (playingLang === langCode) {
      setPlayingLang("");
      return;
    }

    if (!normalizedText) {
      setLocationError("Vui lòng nhập nội dung trước khi nghe thử.");
      return;
    }

    setLocationError("");

    const utterance = new SpeechSynthesisUtterance(normalizedText);
    utterance.lang = langCode;
    utterance.onend = () => setPlayingLang("");
    utterance.onerror = () => {
      setPlayingLang("");
      setLocationError("Không thể phát text-to-speech cho nội dung này.");
    };

    setPlayingLang(langCode);
    window.speechSynthesis.speak(utterance);
  };

  const handleLocationSubmit = async (event) => {
    event.preventDefault();
    setLocationError("");
    setLocationNotice("");

    if (!locationForm.name.trim()) {
      setLocationError("Vui lòng nhập tên địa điểm.");
      return;
    }

    const latitude = Number(locationForm.latitude);
    const longitude = Number(locationForm.longitude);

    if (Number.isNaN(latitude) || Number.isNaN(longitude)) {
      setLocationError("Tọa độ không hợp lệ.");
      return;
    }

    const payload = {
      name: locationForm.name.trim(),
      description: locationForm.description.trim(),
      image: galleryImages[0] || "",
      images: stringifyImageList(galleryImages),
      audio: locationForm.audio.trim(),
      latitude,
      longitude,
      textVi: locationForm.textVi.trim(),
      textEn: locationForm.textEn.trim(),
      textZh: locationForm.textZh.trim(),
      textDe: locationForm.textDe.trim(),
    };

    setIsSavingLocation(true);

    try {
      if (locationForm.id) {
        const updated = await updateLocation(locationForm.id, payload);
        setLocations((current) => current.map((item) => (item.id === updated.id ? updated : item)));
        setLocationNotice("Đã cập nhật địa điểm và frontend sẽ nhận ngay ở lần tải dữ liệu tiếp theo.");
      } else {
        const created = await createLocation(payload);
        setLocations((current) => [created, ...current]);
        setLocationNotice("Đã thêm địa điểm mới thành công.");
      }

      resetLocationForm();
      await loadLocations();
    } catch (error) {
      setLocationError("Không lưu được địa điểm. Kiểm tra backend rồi thử lại.");
    } finally {
      setIsSavingLocation(false);
    }
  };
  const handleDeleteLocation = async (locationId) => {
    const isConfirmed = window.confirm("Bạn có chắc muốn xóa địa điểm này?");
    if (!isConfirmed) return;

    setLocationError("");
    setLocationNotice("");

    try {
      await deleteLocation(locationId);
      setLocations((current) => current.filter((item) => item.id !== locationId));
      if (locationForm.id === locationId) {
        resetLocationForm();
      }
      setLocationNotice("Đã xóa địa điểm.");
    } catch (error) {
      setLocationError("Không xóa được địa điểm.");
    }
  };

  const filteredUsers = useMemo(() => {
    const keyword = userSearch.trim().toLowerCase();
    if (!keyword) return users;
    return users.filter((item) => (item.fullName || "").toLowerCase().includes(keyword));
  }, [userSearch, users]);

  const filteredLocations = useMemo(() => {
    const keyword = locationSearch.trim().toLowerCase();
    if (!keyword) return locations;
    return locations.filter((item) => (item.name || "").toLowerCase().includes(keyword));
  }, [locationSearch, locations]);

  if (!isAuthorized || !user) {
    return null;
  }

  const stats = [
    {
      title: "Lượt phát audio hôm nay",
      value: "8,542",
      note: "Tăng 18% so với hôm qua trên các điểm nghe nổi bật",
      icon: LuMusic4,
    },
    {
      title: "Tăng trưởng người dùng app",
      value: "+12.4%",
      note: "Người dùng mới đang tăng đều trong 7 ngày gần nhất",
      icon: LuUsers,
    },
    {
      title: "Tần suất quay lại trải nghiệm",
      value: "3.2x",
      note: "Mỗi người dùng hoạt động quay lại app trung bình 3.2 lần mỗi tuần",
      icon: LuSparkles,
    },
  ];

  const dashboardTitle =
    activeMenu === "overview"
      ? "Dashboard overview"
      : activeMenu === "users"
        ? "User management"
        : "Location management";

  const dashboardSubtitle =
    activeMenu === "overview"
      ? "Follow usage metrics and destination coverage across the guide system."
      : activeMenu === "users"
        ? "Review accounts, password resets, and locking status from one place."
        : "Create, edit, and publish destination data that the frontend can consume immediately.";

  const showSearch = activeMenu !== "overview";
  const searchValue = activeMenu === "users" ? userSearch : locationSearch;
  const searchPlaceholder =
    activeMenu === "users" ? "Tìm theo tên người dùng" : "Tìm theo tên địa điểm";

  const handleNavbarSearchChange = (value) => {
    if (activeMenu === "users") {
      setUserSearch(value);
    } else if (activeMenu === "locations") {
      setLocationSearch(value);
    }
  };

  const renderUsersOverviewTable = () => (
    <div className="table-card shell-card">
      <div className="section-heading">
        <div>
          <p className="section-eyebrow">Latest activity</p>
          <h2 className="section-title">Tất cả người dùng</h2>
        </div>
        <button type="button" className="ghost-action" onClick={() => setActiveMenu("users")}>
          <span>Xem chi tiết</span>
          <LuArrowUpRight size={16} />
        </button>
      </div>

      {isLoadingUsers ? (
        <div className="admin-empty-state">
          <div className="admin-empty-state-icon"><LuLoaderCircle size={26} className="spin" /></div>
          <h3>Đang tải người dùng</h3>
          <p>Danh sách tài khoản đang được đồng bộ từ backend.</p>
        </div>
      ) : users.length === 0 ? (
        <div className="admin-empty-state">
          <div className="admin-empty-state-icon"><LuFolderOpen size={26} /></div>
          <h3>Chưa có người dùng nào</h3>
          <p>Dữ liệu người dùng đang trống. Tài khoản sẽ xuất hiện tại đây sau khi đăng ký.</p>
        </div>
      ) : (
        <table className="users-table">
          <thead>
            <tr>
              <th>STT</th>
              <th>Họ và tên</th>
              <th>Username</th>
              <th>Role</th>
              <th>Trạng thái</th>
            </tr>
          </thead>
          <tbody>
            {users.map((member, index) => {
              const status = getUserStatus(member);
              return (
                <tr key={member.id}>
                  <td>{index + 1}</td>
                  <td>
                    <div className="table-user">
                      <div className="table-user-avatar">{member.fullName?.charAt(0) || "U"}</div>
                      <div className="table-user-copy"><strong>{member.fullName}</strong><span>{member.username}</span></div>
                    </div>
                  </td>
                  <td>{member.username}</td>
                  <td>{(member.role || "user").toUpperCase()}</td>
                  <td><span className={`status-pill status-${status.key}`}>{status.label}</span></td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}
    </div>
  );

  const renderUsersTab = () => (
    <section className="user-manager">
      <div className="user-editor shell-card">
        <div className="section-heading">
          <div>
            <p className="section-eyebrow">Account control</p>
            <h2 className="section-title">{userForm.id ? "Chỉnh sửa người dùng" : "Chọn người dùng để chỉnh sửa"}</h2>
          </div>
          <button type="button" className="ghost-action" onClick={resetUserForm}>
            <LuRefreshCw size={16} />
            <span>Đặt lại form</span>
          </button>
        </div>

        {userError && <div className="admin-feedback admin-feedback-error">{userError}</div>}
        {userNotice && <div className="admin-feedback admin-feedback-success">{userNotice}</div>}

        {userForm.id ? (
          <form className="user-form" onSubmit={handleUserSubmit}>
            <div className="form-row">
              <div className="form-group"><label htmlFor="fullName">Họ và tên</label><input id="fullName" value={userForm.fullName} disabled /></div>
              <div className="form-group"><label htmlFor="username">Username</label><input id="username" value={userForm.username} disabled /></div>
            </div>
            <div className="form-row">
              <div className="form-group"><label htmlFor="role">Role</label><input id="role" value={(userForm.role || "user").toUpperCase()} disabled /></div>
              <div className="form-group"><label htmlFor="password">Mật khẩu mới</label><input id="password" name="password" type="password" value={userForm.password} placeholder="Để trống nếu không đổi mật khẩu" onChange={handleUserFormChange} /></div>
            </div>
            <label className="admin-toggle">
              <input type="checkbox" name="isLocked" checked={userForm.isLocked} onChange={handleUserFormChange} />
              <span className="admin-toggle-switch" />
              <span className="admin-toggle-label">{userForm.isLocked ? "Tài khoản đang bị khóa" : "Tài khoản đang hoạt động"}</span>
            </label>
            <div className="location-form-actions">
              <button type="submit" className="btn btn-primary" disabled={isSavingUser}>{isSavingUser ? <LuLoaderCircle size={16} className="spin" /> : <LuSave size={16} />}<span>Lưu tài khoản</span></button>
              <button type="button" className="btn action-btn-muted" onClick={resetUserForm}>Hủy chỉnh sửa</button>
            </div>
          </form>
        ) : (
          <div className="admin-empty-state compact-empty-state">
            <div className="admin-empty-state-icon"><LuUsers size={24} /></div>
            <h3>Chưa chọn người dùng</h3>
            <p>Chọn một người dùng ở bảng bên phải để đổi mật khẩu hoặc khóa, mở khóa tài khoản.</p>
          </div>
        )}
      </div>
      <div className="table-card shell-card">
        <div className="section-heading">
          <div>
            <p className="section-eyebrow">User management</p>
            <h2 className="section-title">Danh sách người dùng</h2>
          </div>
          <button type="button" className="ghost-action" onClick={loadUsers}><LuRefreshCw size={16} /><span>Tải lại</span></button>
        </div>

        {isLoadingUsers ? (
          <div className="admin-empty-state"><div className="admin-empty-state-icon"><LuLoaderCircle size={26} className="spin" /></div><h3>Đang tải người dùng</h3><p>Danh sách tài khoản đang được đồng bộ từ backend.</p></div>
        ) : filteredUsers.length === 0 ? (
          <div className="admin-empty-state"><div className="admin-empty-state-icon"><LuFolderOpen size={26} /></div><h3>Không tìm thấy người dùng phù hợp</h3><p>Thử lại với tên ngắn hơn hoặc xóa từ khóa tìm kiếm hiện tại.</p></div>
        ) : (
          <table className="users-table">
            <thead><tr><th>STT</th><th>Họ và tên</th><th>Username</th><th>Role</th><th>Trạng thái</th><th>Hành động</th></tr></thead>
            <tbody>
              {filteredUsers.map((member, index) => {
                const status = getUserStatus(member);
                return (
                  <tr key={member.id}>
                    <td>{index + 1}</td>
                    <td><div className="table-user"><div className="table-user-avatar">{member.fullName?.charAt(0) || "U"}</div><div className="table-user-copy"><strong>{member.fullName}</strong><span>{member.username}</span></div></div></td>
                    <td>{member.username}</td>
                    <td>{(member.role || "user").toUpperCase()}</td>
                    <td><span className={`status-pill status-${status.key}`}>{status.label}</span></td>
                    <td><div className="table-actions"><button type="button" className="action-btn" onClick={() => startEditingUser(member)}>Sửa</button><button type="button" className="action-btn action-btn-muted" onClick={() => handleQuickToggleLock(member)}>{member.isLocked ? <LuLockOpen size={14} /> : <LuLock size={14} />}<span>{member.isLocked ? "Mở khóa" : "Khóa"}</span></button></div></td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>
    </section>
  );

  const renderPreviewButton = (text, langCode) => (
    <button type="button" className={`preview-audio-btn ${playingLang === langCode ? "is-playing" : ""}`} onClick={() => handlePreviewAudio(text, langCode)}>
      {playingLang === langCode ? <FaStop size={11} /> : <FaVolumeUp size={11} />}
      <span>{playingLang === langCode ? "Dừng" : "Nghe thử"}</span>
    </button>
  );

  const renderLocationsTab = () => (
    <section className="location-manager location-manager-stacked">
      <div className="table-card shell-card">
        <div className="section-heading">
          <div><p className="section-eyebrow">Published data</p><h2 className="section-title">Danh sách địa điểm</h2></div>
          <button type="button" className="ghost-action" onClick={loadLocations}><LuRefreshCw size={16} /><span>Tải lại</span></button>
        </div>
        {isLoadingLocations ? (
          <div className="admin-empty-state"><div className="admin-empty-state-icon"><LuLoaderCircle size={26} className="spin" /></div><h3>Đang tải địa điểm</h3><p>Hệ thống đang đồng bộ dữ liệu từ backend.</p></div>
        ) : filteredLocations.length === 0 ? (
          <div className="admin-empty-state"><div className="admin-empty-state-icon"><LuMapPinned size={26} /></div><h3>Không tìm thấy địa điểm phù hợp</h3><p>Thử lại với tên địa điểm ngắn hơn hoặc xóa từ khóa tìm kiếm hiện tại.</p></div>
        ) : (
          <table className="users-table">
            <thead><tr><th>#</th><th>Tên địa điểm</th><th>Mô tả</th><th>Tọa độ</th><th>Media</th><th>Hành động</th></tr></thead>
            <tbody>
              {filteredLocations.map((location, index) => (
                <tr key={location.id}>
                  <td>{index + 1}</td>
                  <td><div className="table-user"><div className="table-user-avatar">{location.name?.charAt(0) || "L"}</div><div className="table-user-copy"><strong>{location.name}</strong><span>ID {location.id}</span></div></div></td>
                  <td className="location-description-cell">{location.description || "Không có mô tả"}</td>
                  <td className="location-coordinate-cell">{location.latitude}, {location.longitude}</td>
                  <td><div className="location-media-meta"><span>{parseImageList(location.images).length > 0 ? "Images" : "No image"}</span><span>{location.audio ? "Audio" : "No audio"}</span></div></td>
                  <td><div className="table-actions"><button type="button" className="action-btn" onClick={() => startEditingLocation(location)}>Sửa</button><button type="button" className="action-btn action-btn-muted" onClick={() => handleDeleteLocation(location.id)}><LuTrash2 size={14} /><span>Xóa</span></button></div></td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <div className="location-editor shell-card">
        <div className="section-heading">
          <div><p className="section-eyebrow">Location editor</p><h2 className="section-title">{locationForm.id ? "Cập nhật địa điểm" : "Thêm địa điểm mới"}</h2></div>
          <button type="button" className="ghost-action" onClick={resetLocationForm}><LuRefreshCw size={16} /><span>Đặt lại form</span></button>
        </div>

        <form className="location-form" onSubmit={handleLocationSubmit}>
          <div className="form-row">
            <div className="form-group"><label htmlFor="name">Tên địa điểm</label><input id="name" name="name" value={locationForm.name} onChange={handleLocationInputChange} /></div>
            <div className="form-group">
              <label htmlFor="audio">Audio file</label>
              <input id="audio" name="audio" value={locationForm.audio} readOnly hidden />
              <input ref={audioInputRef} type="file" accept="audio/*" className="hidden-file-input" onChange={(event) => handleAudioUpload(event.target.files)} />
              <button type="button" className={`upload-dropzone ${isUploadingAudio ? "is-uploading" : ""}`} onClick={() => audioInputRef.current?.click()} onDragOver={(event) => event.preventDefault()} onDrop={(event) => { event.preventDefault(); handleAudioUpload(event.dataTransfer.files); }}>
                <span className="upload-dropzone-icon">{isUploadingAudio ? <LuLoaderCircle size={20} className="spin" /> : <LuMusic4 size={20} />}</span>
                <span className="upload-dropzone-copy"><strong>{isUploadingAudio ? "Đang tải audio..." : "Thả file audio vào đây"}</strong><span>Hoặc bấm để chọn file mp3, wav, m4a hoặc ogg</span></span>
              </button>
              <div className="upload-inline-hint">{locationForm.audio ? `Audio hiện tại: ${locationForm.audio}` : "Chưa có audio file cho địa điểm này."}</div>
            </div>
          </div>

          <div className="form-group"><label htmlFor="description">Mô tả ngắn</label><textarea id="description" name="description" value={locationForm.description} onChange={handleLocationInputChange} /></div>
          <div className="form-row location-media-grid">
            <div className="form-group">
              <label>Danh sách ảnh</label>
              <input ref={galleryInputRef} type="file" accept="image/*" multiple className="hidden-file-input" onChange={(event) => handleGalleryUpload(event.target.files)} />
              <button type="button" className={`upload-dropzone ${isUploadingGallery ? "is-uploading" : ""}`} onClick={() => galleryInputRef.current?.click()} onDragOver={(event) => event.preventDefault()} onDrop={(event) => { event.preventDefault(); handleGalleryUpload(event.dataTransfer.files); }}>
                <span className="upload-dropzone-icon">{isUploadingGallery ? <LuLoaderCircle size={20} className="spin" /> : <LuUpload size={20} />}</span>
                <span className="upload-dropzone-copy"><strong>{isUploadingGallery ? "Đang tải danh sách ảnh..." : "Thả nhiều ảnh vào đây"}</strong><span>Hoặc bấm để chọn nhiều ảnh cùng lúc</span></span>
              </button>
              <div className="upload-inline-hint">
                {galleryImages.length > 0
                  ? "Frontend sẽ tự dùng ảnh đầu tiên trong danh sách làm ảnh hiển thị chính."
                  : "Chưa có ảnh trong gallery. Bạn có thể thả nhiều ảnh cùng lúc."}
              </div>
              {galleryImages.length > 0 ? (
                <div className="gallery-preview-grid">
                  {galleryImages.map((imageUrl, index) => (
                    <article key={`${imageUrl}-${index}`} className="gallery-preview-card">
                      <img src={imageUrl} alt={`Ảnh địa điểm ${index + 1}`} />
                      <div className="gallery-preview-actions">
                        {index === 0 ? <span className="status-pill status-active">Ảnh chính</span> : <span className="status-pill">Ảnh phụ</span>}
                        <button type="button" className="icon-action-btn" onClick={() => removeGalleryImage(imageUrl)} aria-label="Xóa ảnh khỏi danh sách"><LuTrash2 size={14} /></button>
                      </div>
                    </article>
                  ))}
                </div>
              ) : null}
            </div>
          </div>

          <div className="form-row">
            <div className="form-group"><label htmlFor="latitude">Latitude</label><input id="latitude" name="latitude" value={locationForm.latitude} onChange={handleLocationInputChange} /></div>
            <div className="form-group"><label htmlFor="longitude">Longitude</label><input id="longitude" name="longitude" value={locationForm.longitude} onChange={handleLocationInputChange} /></div>
          </div>

          <div className="form-group">
            <div className="preview-label-row"><label htmlFor="textVi">Nội dung thuyết minh tiếng Việt</label>{renderPreviewButton(locationForm.textVi, "vi-VN")}</div>
            <textarea id="textVi" name="textVi" value={locationForm.textVi} onChange={handleLocationInputChange} />
          </div>

          <div className="form-row">
            <div className="form-group"><div className="preview-label-row"><label htmlFor="textEn">English</label>{renderPreviewButton(locationForm.textEn, "en-US")}</div><textarea id="textEn" name="textEn" value={locationForm.textEn} onChange={handleLocationInputChange} /></div>
            <div className="form-group"><div className="preview-label-row"><label htmlFor="textZh">Chinese</label>{renderPreviewButton(locationForm.textZh, "zh-CN")}</div><textarea id="textZh" name="textZh" value={locationForm.textZh} onChange={handleLocationInputChange} /></div>
          </div>

          <div className="form-group"><div className="preview-label-row"><label htmlFor="textDe">German</label>{renderPreviewButton(locationForm.textDe, "de-DE")}</div><textarea id="textDe" name="textDe" value={locationForm.textDe} onChange={handleLocationInputChange} /></div>

          {locationError && <div className="admin-feedback admin-feedback-error">{locationError}</div>}
          {locationNotice && <div className="admin-feedback admin-feedback-success">{locationNotice}</div>}

          <div className="location-form-actions">
            <button type="submit" className="btn btn-primary" disabled={isSavingLocation || isUploadingGallery || isUploadingAudio}>{isSavingLocation ? <LuLoaderCircle size={16} className="spin" /> : locationForm.id ? <LuSave size={16} /> : <LuPlus size={16} />}<span>{locationForm.id ? "Lưu thay đổi" : "Thêm địa điểm"}</span></button>
            {locationForm.id && <button type="button" className="btn action-btn-muted" onClick={resetLocationForm}>Hủy chỉnh sửa</button>}
          </div>
        </form>
      </div>
    </section>
  );

  return (
    <div className="admin-shell">
      <AdminSidebar user={user} activeKey={activeMenu} items={DASHBOARD_TABS} onItemClick={(item) => setActiveMenu(item.key)} onLogout={handleLogout} />
      <main className="admin-main">
        <AdminNavbar title={dashboardTitle} subtitle={dashboardSubtitle} user={user} showSearch={showSearch} searchValue={searchValue} searchPlaceholder={searchPlaceholder} onSearchChange={handleNavbarSearchChange} />
        <div className="admin-content">
          {activeMenu === "overview" && <><section className="stats-grid">{stats.map((stat) => { const Icon = stat.icon; return <article key={stat.title} className="stat-card shell-card"><div className="stat-card-top"><div className="stat-icon"><Icon size={20} /></div><span className="stat-chip">Live</span></div><div className="stat-copy"><p className="stat-title">{stat.title}</p><h3 className="stat-value">{stat.value}</h3><p className="stat-note">{stat.note}</p></div></article>; })}</section>{renderUsersOverviewTable()}</>}
          {activeMenu === "users" && renderUsersTab()}
          {activeMenu === "locations" && renderLocationsTab()}
        </div>
      </main>
    </div>
  );
}

export default AdminDashboard;
