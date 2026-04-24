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
  LuUserCog,
  LuLink2,
  LuMap,
  LuMapPin,
  LuPhone,
  LuRoute,
} from "react-icons/lu";
import { FaStop, FaVolumeUp } from "react-icons/fa";
import MapView from "../components/MapView";
import AdminNavbar, { AdminSidebar } from "../components/AdminNavbar";
import AdminToursPanel from "../components/AdminToursPanel";
import {
  createLocation,
  deleteLocation,
  getAllLocations,
  updateLocation,
} from "../services/locationService";
import { speak, stop } from "../services/ttsService";
import { uploadImages } from "../services/uploadService";
import { getAllUsers, updateAdminUser } from "../services/userService";
import {
  assignLocationToManager,
  getAllAssignments,
  unassignLocationFromManager,
} from "../services/assignmentService";
import { GET } from "../services/api";
import "../styles/admin.css";

const DASHBOARD_TABS = [
  { key: "overview", label: "Tổng quan", icon: LuLayoutGrid },
  { key: "map", label: "Bản đồ", icon: LuMap },
  { key: "tours", label: "Tours", icon: LuRoute },
  { key: "users", label: "Người dùng", icon: LuUsers },
  { key: "managers", label: "Managers", icon: LuUserCog },
  { key: "assignments", label: "Phân công", icon: LuLink2 },
  { key: "locations", label: "Địa điểm", icon: LuMapPinned },
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

  const [user, setUser] = useState(null);
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [activeMenu, setActiveMenu] = useState("overview");

  const [userSearch, setUserSearch] = useState("");
  const [managerSearch, setManagerSearch] = useState("");
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
  const [playingLang, setPlayingLang] = useState("");

  const [mapSelectedLocation, setMapSelectedLocation] = useState(null);

  const [analytics, setAnalytics] = useState(null);
  const [isLoadingAnalytics, setIsLoadingAnalytics] = useState(false);
  const [analyticsError, setAnalyticsError] = useState("");

  const [assignments, setAssignments] = useState([]);
  const [isLoadingAssignments, setIsLoadingAssignments] = useState(true);
  const [assignmentError, setAssignmentError] = useState("");
  const [assignmentNotice, setAssignmentNotice] = useState("");
  const [selectedManagerId, setSelectedManagerId] = useState("");

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
    loadAssignments();
    loadAnalytics();
  }, [isAuthorized]);

  useEffect(() => {
    if (!isAuthorized) return;
    const id = setInterval(() => loadAnalytics(), 20000); // refresh every 20s
    return () => clearInterval(id);
  }, [isAuthorized]);

  const loadAnalytics = async () => {
    setIsLoadingAnalytics(true);
    setAnalyticsError("");
    try {
      const data = await GET("/admin/analytics/summary");
      setAnalytics(data || {
        currentActiveDevices: 0,
        totalAudioPlays: 0,
        totalFavoritesSaved: 0,
        totalVisitors: 0,
      });
    } catch (error) {
      console.error("Failed to load analytics summary:", error);
      setAnalyticsError("Không tải được số liệu analytics. Dashboard sẽ thử lại tự động.");
      setAnalytics({
        currentActiveDevices: 0,
        totalAudioPlays: 0,
        totalFavoritesSaved: 0,
        totalVisitors: 0,
      });
    } finally {
      setIsLoadingAnalytics(false);
    }
  };

  useEffect(() => {
    return () => {
      stop();
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

  const loadAssignments = async () => {
    setIsLoadingAssignments(true);
    setAssignmentError("");

    try {
      const data = await getAllAssignments();
      setAssignments(Array.isArray(data) ? data : []);
    } catch (error) {
      console.error("Failed to load assignments:", error);
      setAssignmentError("Không tải được danh sách phân công.");
      setAssignments([]);
    } finally {
      setIsLoadingAssignments(false);
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
        role: userForm.role || null,
        isLocked: userForm.isLocked,
      });

      setUsers((current) =>
        current.map((item) =>
          item.id === updatedUser.id ? updatedUser : item,
        ),
      );
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
        role: null,
        isLocked: !targetUser.isLocked,
      });

      setUsers((current) =>
        current.map((item) =>
          item.id === updatedUser.id ? updatedUser : item,
        ),
      );
      setUserNotice(
        updatedUser.isLocked
          ? "Tài khoản đã được khóa."
          : "Tài khoản đã được mở khóa.",
      );

      if (userForm.id === updatedUser.id) {
        setUserForm((current) => ({
          ...current,
          isLocked: updatedUser.isLocked,
        }));
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
    setActiveMenu("locations");
  };

  const setGalleryImages = (images) => {
    setLocationForm((current) => ({
      ...current,
      images: stringifyImageList(images),
    }));
  };

  const handleGalleryUpload = async (files) => {
    const selectedFiles = Array.from(files || []).filter((file) =>
      file.type.startsWith("image/"),
    );
    if (selectedFiles.length === 0) return;

    setIsUploadingGallery(true);
    setLocationError("");

    try {
      const uploadedUrls = await uploadImages(selectedFiles);
      const mergedImages = Array.from(
        new Set([...galleryImages, ...uploadedUrls].filter(Boolean)),
      );
      setGalleryImages(mergedImages);
      setLocationForm((current) => ({
        ...current,
        image: mergedImages[0] || "",
      }));
      setLocationNotice(
        uploadedUrls.length > 1
          ? `Đã tải ${uploadedUrls.length} ảnh vào bộ sưu tập.`
          : "Đã tải ảnh vào bộ sưu tập.",
      );
    } catch (error) {
      setLocationError(error.message || "Không tải được danh sách ảnh.");
    } finally {
      setIsUploadingGallery(false);
    }
  };

  const removeGalleryImage = (imageUrl) => {
    const nextImages = galleryImages.filter((item) => item !== imageUrl);
    setGalleryImages(nextImages);
    setLocationForm((current) => ({ ...current, image: nextImages[0] || "" }));
  };

  const handlePreviewAudio = async (text, langCode) => {
    const normalizedText = (text || "").trim();

    if (!window?.speechSynthesis) {
      setLocationError("Trình duyệt hiện tại không hỗ trợ text-to-speech.");
      return;
    }

    if (playingLang === langCode) {
      stop();
      setPlayingLang("");
      return;
    }

    if (!normalizedText) {
      setLocationError("Vui lòng nhập nội dung trước khi nghe thử.");
      return;
    }

    setLocationError("");
    setPlayingLang(langCode);

    try {
      await speak(normalizedText, langCode);
    } catch (error) {
      console.error("Preview audio failed:", error);
      setLocationError("Không thể phát text-to-speech cho nội dung này.");
    } finally {
      setPlayingLang((current) => (current === langCode ? "" : current));
    }
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
      address: locationForm.address.trim(),
      phone: locationForm.phone.trim(),
      reviewsJson: locationForm.reviewsJson || "[]",
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
        setLocations((current) =>
          current.map((item) => (item.id === updated.id ? updated : item)),
        );
        setLocationNotice(
          "Đã cập nhật địa điểm và frontend sẽ nhận ngay ở lần tải dữ liệu tiếp theo.",
        );
      } else {
        const created = await createLocation(payload);
        setLocations((current) => [created, ...current]);
        setLocationNotice("Đã thêm địa điểm mới thành công.");
      }

      resetLocationForm();
      await loadLocations();
    } catch (error) {
      setLocationError(
        "Không lưu được địa điểm. Kiểm tra backend rồi thử lại.",
      );
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
      setLocations((current) =>
        current.filter((item) => item.id !== locationId),
      );
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
    return users.filter((item) =>
      (item.fullName || "").toLowerCase().includes(keyword),
    );
  }, [userSearch, users]);

  const filteredManagers = useMemo(() => {
    const keyword = managerSearch.trim().toLowerCase();
    const managers = users.filter(
      (item) => (item.role || "").toLowerCase() === "manager",
    );

    if (!keyword) return managers;
    return managers.filter((item) =>
      `${item.fullName || ""} ${item.username || ""}`
        .toLowerCase()
        .includes(keyword),
    );
  }, [managerSearch, users]);

  const filteredLocations = useMemo(() => {
    const keyword = locationSearch.trim().toLowerCase();
    if (!keyword) return locations;
    return locations.filter((item) =>
      (item.name || "").toLowerCase().includes(keyword),
    );
  }, [locationSearch, locations]);

  if (!isAuthorized || !user) {
    return null;
  }

  const stats = [
    {
      title: "Thiết bị đang hoạt động",
      value: isLoadingAnalytics ? null : (analytics?.currentActiveDevices ?? 0),
      note: "Số thiết bị có hoạt động trong 5 phút gần nhất",
      icon: LuUsers,
    },
    {
      title: "Lượt phát audio",
      value: isLoadingAnalytics ? null : (analytics?.totalAudioPlays ?? 0),
      note: "Tổng số lần phát audio",
      icon: LuMusic4,
    },
    {
      title: "Lượt yêu thích",
      value: isLoadingAnalytics ? null : (analytics?.totalFavoritesSaved ?? 0),
      note: "Tổng số lượt yêu thích hiện tại",
      icon: LuSparkles,
    },
    {
      title: "Khách truy cập",
      value: isLoadingAnalytics ? null : (analytics?.totalVisitors ?? analytics?.totalVisitorsToday ?? 0),
      note: "Tổng số thiết bị đã truy cập website",
      icon: LuArrowUpRight,
    },
  ];

  const renderToursTab = () => <AdminToursPanel />;

  const renderMapTab = () => (
    <div className="map-tab-layout">
      <div className="map-panel">
        <MapView
          userLocation={
            mapSelectedLocation
              ? {
                  lat: Number(mapSelectedLocation.latitude ?? mapSelectedLocation.Latitude),
                  lng: Number(mapSelectedLocation.longitude ?? mapSelectedLocation.Longitude),
                }
              : null
          }
          locations={locations}
          onSelectLocation={(loc) => setMapSelectedLocation(loc)}
        />
      </div>
      <div className="map-detail-panel">
        {mapSelectedLocation ? (
          <>
            {mapSelectedLocation.image ? (
              <img
                className="map-detail-image"
                src={mapSelectedLocation.image}
                alt={mapSelectedLocation.name}
              />
            ) : (
              <div className="map-detail-image-placeholder">📷 Chưa có ảnh</div>
            )}
            <div className="map-detail-name">{mapSelectedLocation.name}</div>
            <div className="map-detail-meta">
              {mapSelectedLocation.address && (
                <div className="map-detail-row">
                  <LuMapPin size={14} className="map-detail-row-icon" />
                  <span>{mapSelectedLocation.address}</span>
                </div>
              )}
              {mapSelectedLocation.phone && (
                <div className="map-detail-row">
                  <LuPhone size={14} className="map-detail-row-icon" />
                  <span>{mapSelectedLocation.phone}</span>
                </div>
              )}
            </div>
            {mapSelectedLocation.description && (
              <div className="map-detail-desc">{mapSelectedLocation.description}</div>
            )}
            <div className="map-detail-coords">
              📍 {mapSelectedLocation.latitude}, {mapSelectedLocation.longitude}
            </div>
            <button
              className="btn btn-primary map-detail-edit-btn"
              onClick={() => startEditingLocation(mapSelectedLocation)}
            >
              <LuMapPinned size={15} />
              <span>Chỉnh sửa địa điểm này</span>
            </button>
          </>
        ) : (
          <div className="map-detail-empty">
            <div className="map-detail-empty-icon">🗺️</div>
            <p className="section-title">Chọn một địa điểm</p>
            <p>Click vào marker trên bản đồ để xem chi tiết.</p>
          </div>
        )}
      </div>
    </div>
  );

  const dashboardTitle =
    activeMenu === "overview" ? "Dashboard overview"
    : activeMenu === "map" ? "Bản đồ địa điểm"
    : activeMenu === "tours" ? "Tour management"
    : activeMenu === "users" ? "User management"
    : activeMenu === "managers" ? "Manager management"
    : activeMenu === "assignments" ? "Location assignment"
    : "Location management";

  const dashboardSubtitle =
    activeMenu === "overview" ? "Follow usage metrics and destination coverage across the guide system."
    : activeMenu === "map" ? "Xem toàn bộ địa điểm trên bản đồ, click marker để xem chi tiết và chỉnh sửa."
    : activeMenu === "tours" ? "Tạo và quản lý các tour tham quan, gán POI và sắp xếp thứ tự."
    : activeMenu === "users" ? "Review accounts, password resets, and locking status from one place."
    : activeMenu === "managers" ? "Manage business accounts and grant manager role to selected users."
    : activeMenu === "assignments" ? "Assign locations to managers for content ownership and operations."
    : "Create, edit, and publish destination data that the frontend can consume immediately.";

  const showSearch =
    activeMenu === "users" ||
    activeMenu === "managers" ||
    activeMenu === "locations";
  const searchValue =
    activeMenu === "users" ? userSearch
    : activeMenu === "managers" ? managerSearch
    : locationSearch;
  const searchPlaceholder =
    activeMenu === "users" ? "Tìm theo tên người dùng"
    : activeMenu === "managers" ? "Tìm theo manager"
    : "Tìm theo tên địa điểm";

  const handleNavbarSearchChange = (value) => {
    if (activeMenu === "users") setUserSearch(value);
    else if (activeMenu === "managers") setManagerSearch(value);
    else if (activeMenu === "locations") setLocationSearch(value);
  };

  const renderUsersOverviewTable = () => (
    <div className="table-card shell-card">
      <div className="section-heading">
        <div>
          <p className="section-eyebrow">Latest activity</p>
          <h2 className="section-title">Tất cả người dùng</h2>
        </div>
        <button
          type="button"
          className="ghost-action"
          onClick={() => setActiveMenu("users")}
        >
          <span>Xem chi tiết</span>
          <LuArrowUpRight size={16} />
        </button>
      </div>

      {isLoadingUsers ? (
        <div className="admin-empty-state">
          <div className="admin-empty-state-icon">
            <LuLoaderCircle size={26} className="spin" />
          </div>
          <h3>Đang tải người dùng</h3>
          <p>Danh sách tài khoản đang được đồng bộ từ backend.</p>
        </div>
      ) : users.length === 0 ? (
        <div className="admin-empty-state">
          <div className="admin-empty-state-icon">
            <LuFolderOpen size={26} />
          </div>
          <h3>Chưa có người dùng nào</h3>
          <p>
            Dữ liệu người dùng đang trống. Tài khoản sẽ xuất hiện tại đây sau
            khi đăng ký.
          </p>
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
                      <div className="table-user-avatar">
                        {member.fullName?.charAt(0) || "U"}
                      </div>
                      <div className="table-user-copy">
                        <strong>{member.fullName}</strong>
                        <span>{member.username}</span>
                      </div>
                    </div>
                  </td>
                  <td>{member.username}</td>
                  <td>{(member.role || "manager").toUpperCase()}</td>
                  <td>
                    <span className={`status-pill status-${status.key}`}>
                      {status.label}
                    </span>
                  </td>
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
            <h2 className="section-title">
              {userForm.id
                ? "Chỉnh sửa người dùng"
                : "Chọn người dùng để chỉnh sửa"}
            </h2>
          </div>
          <button
            type="button"
            className="ghost-action"
            onClick={resetUserForm}
          >
            <LuRefreshCw size={16} />
            <span>Đặt lại form</span>
          </button>
        </div>

        {userError && (
          <div className="admin-feedback admin-feedback-error">{userError}</div>
        )}
        {userNotice && (
          <div className="admin-feedback admin-feedback-success">
            {userNotice}
          </div>
        )}

        {userForm.id ? (
          <form className="user-form" onSubmit={handleUserSubmit}>
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="fullName">Họ và tên</label>
                <input id="fullName" value={userForm.fullName} disabled />
              </div>
              <div className="form-group">
                <label htmlFor="username">Username</label>
                <input id="username" value={userForm.username} disabled />
              </div>
            </div>
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="role">Role</label>
                {(userForm.role || "").toLowerCase() === "admin" ? (
                  <input
                    id="role"
                    value={(userForm.role || "admin").toUpperCase()}
                    disabled
                  />
                ) : (
                  <select
                    id="role"
                    name="role"
                    value={(userForm.role || "user").toLowerCase()}
                    onChange={handleUserFormChange}
                  >
                    <option value="user">USER</option>
                    <option value="manager">MANAGER</option>
                  </select>
                )}
              </div>
              <div className="form-group">
                <label htmlFor="password">Mật khẩu mới</label>
                <input
                  id="password"
                  name="password"
                  type="password"
                  value={userForm.password}
                  placeholder="Để trống nếu không đổi mật khẩu"
                  onChange={handleUserFormChange}
                />
              </div>
            </div>
            <label className="admin-toggle">
              <input
                type="checkbox"
                name="isLocked"
                checked={userForm.isLocked}
                onChange={handleUserFormChange}
              />
              <span className="admin-toggle-switch" />
              <span className="admin-toggle-label">
                {userForm.isLocked
                  ? "Tài khoản đang bị khóa"
                  : "Tài khoản đang hoạt động"}
              </span>
            </label>
            <div className="location-form-actions">
              <button
                type="submit"
                className="btn btn-primary"
                disabled={isSavingUser}
              >
                {isSavingUser ? (
                  <LuLoaderCircle size={16} className="spin" />
                ) : (
                  <LuSave size={16} />
                )}
                <span>Lưu tài khoản</span>
              </button>
              <button
                type="button"
                className="btn action-btn-muted"
                onClick={resetUserForm}
              >
                Hủy chỉnh sửa
              </button>
            </div>
          </form>
        ) : (
          <div className="admin-empty-state compact-empty-state">
            <div className="admin-empty-state-icon">
              <LuUsers size={24} />
            </div>
            <h3>Chưa chọn người dùng</h3>
            <p>
              Chọn một người dùng ở bảng bên phải để đổi mật khẩu hoặc khóa, mở
              khóa tài khoản.
            </p>
          </div>
        )}
      </div>
      <div className="table-card shell-card">
        <div className="section-heading">
          <div>
            <p className="section-eyebrow">User management</p>
            <h2 className="section-title">Danh sách người dùng</h2>
          </div>
          <button type="button" className="ghost-action" onClick={loadUsers}>
            <LuRefreshCw size={16} />
            <span>Tải lại</span>
          </button>
        </div>

        {isLoadingUsers ? (
          <div className="admin-empty-state">
            <div className="admin-empty-state-icon">
              <LuLoaderCircle size={26} className="spin" />
            </div>
            <h3>Đang tải người dùng</h3>
            <p>Danh sách tài khoản đang được đồng bộ từ backend.</p>
          </div>
        ) : filteredUsers.length === 0 ? (
          <div className="admin-empty-state">
            <div className="admin-empty-state-icon">
              <LuFolderOpen size={26} />
            </div>
            <h3>Không tìm thấy người dùng phù hợp</h3>
            <p>Thử lại với tên ngắn hơn hoặc xóa từ khóa tìm kiếm hiện tại.</p>
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
                <th>Hành động</th>
              </tr>
            </thead>
            <tbody>
              {filteredUsers.map((member, index) => {
                const status = getUserStatus(member);
                return (
                  <tr key={member.id}>
                    <td>{index + 1}</td>
                    <td>
                      <div className="table-user">
                        <div className="table-user-avatar">
                          {member.fullName?.charAt(0) || "U"}
                        </div>
                        <div className="table-user-copy">
                          <strong>{member.fullName}</strong>
                          <span>{member.username}</span>
                        </div>
                      </div>
                    </td>
                    <td>{member.username}</td>
                    <td>{(member.role || "manager").toUpperCase()}</td>
                    <td>
                      <span className={`status-pill status-${status.key}`}>
                        {status.label}
                      </span>
                    </td>
                    <td>
                      <div className="table-actions">
                        <button
                          type="button"
                          className="action-btn"
                          onClick={() => startEditingUser(member)}
                        >
                          Sửa
                        </button>
                        <button
                          type="button"
                          className="action-btn action-btn-muted"
                          onClick={() => handleQuickToggleLock(member)}
                        >
                          {member.isLocked ? (
                            <LuLockOpen size={14} />
                          ) : (
                            <LuLock size={14} />
                          )}
                          <span>{member.isLocked ? "Mở khóa" : "Khóa"}</span>
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>
    </section>
  );

  const handlePromoteToManager = async (targetUser) => {
    setUserError("");
    setUserNotice("");

    try {
      const updatedUser = await updateAdminUser(targetUser.id, {
        password: null,
        role: "manager",
        isLocked: !!targetUser.isLocked,
      });

      setUsers((current) =>
        current.map((item) =>
          item.id === updatedUser.id ? updatedUser : item,
        ),
      );
      setUserNotice("Đã cấp quyền manager.");
    } catch (error) {
      console.error("Failed to promote manager:", error);
      setUserError(error.message || "Không cấp quyền manager được.");
    }
  };

  const handleDemoteToUser = async (targetUser) => {
    setUserError("");
    setUserNotice("");

    try {
      const updatedUser = await updateAdminUser(targetUser.id, {
        password: null,
        role: "user",
        isLocked: !!targetUser.isLocked,
      });

      setUsers((current) =>
        current.map((item) =>
          item.id === updatedUser.id ? updatedUser : item,
        ),
      );
      setUserNotice("Đã hạ role về user.");
    } catch (error) {
      console.error("Failed to demote manager:", error);
      setUserError(error.message || "Không hạ role được.");
    }
  };

  const renderManagersTab = () => {
    const eligibleUsers = filteredManagers;


    return (
      <section className="admin-grid">
        <div className="table-card shell-card">
          <div className="section-heading">
            <div>
              <p className="section-eyebrow">Managers</p>
              <h2 className="section-title">Danh sách managers</h2>
            </div>
            <button type="button" className="ghost-action" onClick={loadUsers}>
              <LuRefreshCw size={16} />
              <span>Tải lại</span>
            </button>
          </div>

          {userError && (
            <div className="admin-feedback admin-feedback-error">{userError}</div>
          )}
          {userNotice && (
            <div className="admin-feedback admin-feedback-success">
              {userNotice}
            </div>
          )}

          {isLoadingUsers ? (
            <div className="admin-empty-state">
              <div className="admin-empty-state-icon">
                <LuLoaderCircle size={26} className="spin" />
              </div>
              <h3>Đang tải managers</h3>
              <p>Danh sách managers đang được đồng bộ từ backend.</p>
            </div>
          ) : filteredManagers.length === 0 ? (
            <div className="admin-empty-state">
              <div className="admin-empty-state-icon">
                <LuFolderOpen size={26} />
              </div>
              <h3>Chưa có manager</h3>
              <p>Cấp quyền manager cho một user để quản lý theo doanh nghiệp.</p>
            </div>
          ) : (
            <table className="users-table">
              <thead>
                <tr>
                  <th>STT</th>
                  <th>Họ và tên</th>
                  <th>Username</th>
                  <th>Trạng thái</th>
                  <th>Hành động</th>
                </tr>
              </thead>
              <tbody>
                {filteredManagers.map((member, index) => {
                  const status = getUserStatus(member);
                  return (
                    <tr key={member.id}>
                      <td>{index + 1}</td>
                      <td>
                        <div className="table-user">
                          <div className="table-user-avatar">
                            {member.fullName?.charAt(0) || "M"}
                          </div>
                          <div className="table-user-copy">
                            <strong>{member.fullName}</strong>
                            <span>{member.username}</span>
                          </div>
                        </div>
                      </td>
                      <td>{member.username}</td>
                      <td>
                        <span className={`status-pill status-${status.key}`}>
                          {status.label}
                        </span>
                      </td>
                      <td>
                        <div className="table-actions">
                          <button
                            type="button"
                            className="action-btn"
                            onClick={() => startEditingUser(member)}
                          >
                            Sửa
                          </button>
                          <button
                            type="button"
                            className="action-btn action-btn-muted"
                            onClick={() => handleDemoteToUser(member)}
                          >
                            Hạ role
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          )}
        </div>

        <div className="table-card shell-card">
          <div className="section-heading">
            <div>
              <p className="section-eyebrow">Promote</p>
              <h2 className="section-title">Cấp quyền manager</h2>
            </div>
          </div>

          {isLoadingUsers ? (
            <div className="admin-empty-state">
              <div className="admin-empty-state-icon">
                <LuLoaderCircle size={26} className="spin" />
              </div>
              <h3>Đang tải users</h3>
              <p>Danh sách users đang được đồng bộ từ backend.</p>
            </div>
          ) : eligibleUsers.length === 0 ? (
            <div className="admin-empty-state">
              <div className="admin-empty-state-icon">
                <LuFolderOpen size={26} />
              </div>
              <h3>Không có user khả dụng</h3>
              <p>Tạo user mới hoặc xóa bộ lọc tìm kiếm.</p>
            </div>
          ) : (
            <table className="users-table">
              <thead>
                <tr>
                  <th>STT</th>
                  <th>Họ và tên</th>
                  <th>Username</th>
                  <th>Hành động</th>
                </tr>
              </thead>
              <tbody>
                {eligibleUsers.map((member, index) => (
                  <tr key={member.id}>
                    <td>{index + 1}</td>
                    <td>{member.fullName}</td>
                    <td>{member.username}</td>
                    <td>
                      <div className="table-actions">
                        <button
                          type="button"
                          className="action-btn"
                          onClick={() => handlePromoteToManager(member)}
                        >
                          Cấp quyền
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </section>
    );
  };

  const renderAssignmentsTab = () => {
    const managerId = selectedManagerId ? Number(selectedManagerId) : null;
    const managerList = users.filter(
      (item) => (item.role || "").toLowerCase() === "manager",
    );
    const assignedLocationIds = new Set(
      assignments
        .filter((item) => item.managerId === managerId)
        .map((item) => item.locationId),
    );

    const availableLocations = locations.filter(
      (item) => managerId && !assignedLocationIds.has(item.id),
    );

    const assignedLocations = locations.filter((item) =>
      managerId ? assignedLocationIds.has(item.id) : false,
    );

    const handleAssign = async (locationId) => {
      if (!managerId) return;
      setAssignmentError("");
      setAssignmentNotice("");

      try {
        await assignLocationToManager(managerId, locationId);
        await loadAssignments();
        setAssignmentNotice("Đã phân công địa điểm.");
      } catch (error) {
        console.error("Failed to assign location:", error);
        setAssignmentError(error.message || "Không phân công được địa điểm.");
      }
    };

    const handleUnassign = async (locationId) => {
      if (!managerId) return;
      setAssignmentError("");
      setAssignmentNotice("");

      try {
        await unassignLocationFromManager(managerId, locationId);
        await loadAssignments();
        setAssignmentNotice("Đã gỡ phân công.");
      } catch (error) {
        console.error("Failed to unassign location:", error);
        setAssignmentError(error.message || "Không gỡ phân công được.");
      }
    };

    return (
      <section className="admin-grid">
        <div className="table-card shell-card">
          <div className="section-heading">
            <div>
              <p className="section-eyebrow">Assignment</p>
              <h2 className="section-title">Chọn manager</h2>
            </div>
            <button
              type="button"
              className="ghost-action"
              onClick={loadAssignments}
            >
              <LuRefreshCw size={16} />
              <span>Tải lại</span>
            </button>
          </div>

          <div className="form-group">
            <label htmlFor="managerId">Manager</label>
            <select
              id="managerId"
              value={selectedManagerId}
              onChange={(event) => setSelectedManagerId(event.target.value)}
            >
              <option value="">-- Chọn manager --</option>
              {managerList.map((member) => (
                <option key={member.id} value={String(member.id)}>
                  {member.fullName} ({member.username})
                </option>
              ))}
            </select>
          </div>

          {assignmentError && (
            <div className="admin-feedback admin-feedback-error">
              {assignmentError}
            </div>
          )}
          {assignmentNotice && (
            <div className="admin-feedback admin-feedback-success">
              {assignmentNotice}
            </div>
          )}

          {!selectedManagerId ? (
            <div className="admin-empty-state compact-empty-state">
              <div className="admin-empty-state-icon">
                <LuLink2 size={24} />
              </div>
              <h3>Chưa chọn manager</h3>
              <p>Chọn manager để phân công địa điểm quản lý.</p>
            </div>
          ) : isLoadingAssignments || isLoadingLocations ? (
            <div className="admin-empty-state">
              <div className="admin-empty-state-icon">
                <LuLoaderCircle size={26} className="spin" />
              </div>
              <h3>Đang tải dữ liệu phân công</h3>
              <p>Đang đồng bộ danh sách địa điểm và phân công hiện tại.</p>
            </div>
          ) : assignedLocations.length === 0 ? (
            <div className="admin-empty-state compact-empty-state">
              <div className="admin-empty-state-icon">
                <LuFolderOpen size={24} />
              </div>
              <h3>Chưa có địa điểm nào</h3>
              <p>Chọn một địa điểm bên phải để phân công.</p>
            </div>
          ) : (
            <table className="users-table">
              <thead>
                <tr>
                  <th>STT</th>
                  <th>Tên địa điểm</th>
                  <th>Hành động</th>
                </tr>
              </thead>
              <tbody>
                {assignedLocations.map((item, index) => (
                  <tr key={item.id}>
                    <td>{index + 1}</td>
                    <td>{item.name}</td>
                    <td>
                      <div className="table-actions">
                        <button
                          type="button"
                          className="action-btn action-btn-muted"
                          onClick={() => handleUnassign(item.id)}
                        >
                          Gỡ
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
              <p className="section-eyebrow">Available</p>
              <h2 className="section-title">Địa điểm khả dụng</h2>
            </div>
          </div>

          {!selectedManagerId ? (
            <div className="admin-empty-state">
              <div className="admin-empty-state-icon">
                <LuFolderOpen size={26} />
              </div>
              <h3>Chọn manager trước</h3>
              <p>Danh sách địa điểm sẽ hiển thị sau khi chọn manager.</p>
            </div>
          ) : isLoadingLocations ? (
            <div className="admin-empty-state">
              <div className="admin-empty-state-icon">
                <LuLoaderCircle size={26} className="spin" />
              </div>
              <h3>Đang tải địa điểm</h3>
              <p>Danh sách địa điểm đang được đồng bộ từ backend.</p>
            </div>
          ) : availableLocations.length === 0 ? (
            <div className="admin-empty-state">
              <div className="admin-empty-state-icon">
                <LuFolderOpen size={26} />
              </div>
              <h3>Không còn địa điểm để phân công</h3>
              <p>Tất cả địa điểm đã được gán cho manager này.</p>
            </div>
          ) : (
            <table className="users-table">
              <thead>
                <tr>
                  <th>STT</th>
                  <th>Tên địa điểm</th>
                  <th>Hành động</th>
                </tr>
              </thead>
              <tbody>
                {availableLocations.map((item, index) => (
                  <tr key={item.id}>
                    <td>{index + 1}</td>
                    <td>{item.name}</td>
                    <td>
                      <div className="table-actions">
                        <button
                          type="button"
                          className="action-btn"
                          onClick={() => handleAssign(item.id)}
                        >
                          Gán
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </section>
    );
  };

  const renderPreviewButton = (text, langCode) => (
    <button
      type="button"
      className={`preview-audio-btn ${playingLang === langCode ? "is-playing" : ""}`}
      onClick={() => handlePreviewAudio(text, langCode)}
    >
      {playingLang === langCode ? (
        <FaStop size={11} />
      ) : (
        <FaVolumeUp size={11} />
      )}
      <span>{playingLang === langCode ? "Dừng" : "Nghe thử"}</span>
    </button>
  );

  const renderLocationsTab = () => (
    <section className="location-manager location-manager-stacked">
      <div className="table-card shell-card">
        <div className="section-heading">
          <div>
            <p className="section-eyebrow">Published data</p>
            <h2 className="section-title">Danh sách địa điểm</h2>
          </div>
          <button
            type="button"
            className="ghost-action"
            onClick={loadLocations}
          >
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
            <p>Hệ thống đang đồng bộ dữ liệu từ backend.</p>
          </div>
        ) : filteredLocations.length === 0 ? (
          <div className="admin-empty-state">
            <div className="admin-empty-state-icon">
              <LuMapPinned size={26} />
            </div>
            <h3>Không tìm thấy địa điểm phù hợp</h3>
            <p>
              Thử lại với tên địa điểm ngắn hơn hoặc xóa từ khóa tìm kiếm hiện
              tại.
            </p>
          </div>
        ) : (
          <table className="users-table">
            <thead>
              <tr>
                <th>#</th>
                <th>Tên địa điểm</th>
                <th>Mô tả</th>
                <th>Tọa độ</th>
                <th>Media</th>
                <th>Hành động</th>
              </tr>
            </thead>
            <tbody>
              {filteredLocations.map((location, index) => (
                <tr key={location.id}>
                  <td>{index + 1}</td>
                  <td>
                    <div className="table-user">
                      <div className="table-user-avatar">
                        {location.name?.charAt(0) || "L"}
                      </div>
                      <div className="table-user-copy">
                        <strong>{location.name}</strong>
                        <span>ID {location.id}</span>
                      </div>
                    </div>
                  </td>
                  <td className="location-description-cell">
                    {location.description || "Không có mô tả"}
                  </td>
                  <td className="location-coordinate-cell">
                    {location.latitude}, {location.longitude}
                  </td>
                  <td>
                    <div className="location-media-meta">
                      <span>
                        {parseImageList(location.images).length > 0
                          ? "Images"
                          : "No image"}
                      </span>
                    </div>
                  </td>
                  <td>
                    <div className="table-actions">
                      <button
                        type="button"
                        className="action-btn"
                        onClick={() => startEditingLocation(location)}
                      >
                        Sửa
                      </button>
                      <button
                        type="button"
                        className="action-btn action-btn-muted"
                        onClick={() => handleDeleteLocation(location.id)}
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

      <div className="location-editor shell-card">
        <div className="section-heading">
          <div>
            <p className="section-eyebrow">Location editor</p>
            <h2 className="section-title">
              {locationForm.id ? "Cập nhật địa điểm" : "Thêm địa điểm mới"}
            </h2>
          </div>
          <button
            type="button"
            className="ghost-action"
            onClick={resetLocationForm}
          >
            <LuRefreshCw size={16} />
            <span>Đặt lại form</span>
          </button>
        </div>

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
              <label htmlFor="address">Địa chỉ quán</label>
              <input
                id="address"
                name="address"
                value={locationForm.address}
                onChange={handleLocationInputChange}
                placeholder="Ví dụ: 123 Nguyễn Huệ, Quận 1, TP.HCM"
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
                placeholder="Ví dụ: 0901234567"
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
                      onClick={() => removeGalleryImage(url)}
                      aria-label="Remove"
                    >
                      <LuTrash2 size={14} />
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>
          <br />
          <h2 className="section-title">Nội dung thuyết minh</h2>
          <div className="form-row">
            {/* Tiếng Việt */}
            <div className="form-group">
              <div className="preview-label-row">
                <label htmlFor="textVi">Tiếng Việt</label>
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

            {/* English */}
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

            {/* Chinese */}
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

            {/* German */}
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
            <div className="admin-feedback admin-feedback-error">
              {locationError}
            </div>
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
              ) : locationForm.id ? (
                <LuSave size={16} />
              ) : (
                <LuPlus size={16} />
              )}
              <span>{locationForm.id ? "Lưu thay đổi" : "Thêm địa điểm"}</span>
            </button>
            {locationForm.id && (
              <button
                type="button"
                className="btn action-btn-muted"
                onClick={resetLocationForm}
              >
                Hủy chỉnh sửa
              </button>
            )}
          </div>
        </form>
      </div>
    </section>
  );

  return (
    <div className="admin-shell">
      <AdminSidebar
        user={user}
        activeKey={activeMenu}
        items={DASHBOARD_TABS}
        onItemClick={(item) => setActiveMenu(item.key)}
        onLogout={handleLogout}
      />
      <main className="admin-main">
        <AdminNavbar
          title={dashboardTitle}
          subtitle={dashboardSubtitle}
          user={user}
          showSearch={showSearch}
          searchValue={searchValue}
          searchPlaceholder={searchPlaceholder}
          onSearchChange={handleNavbarSearchChange}
        />
        <div className="admin-content">
          {activeMenu === "overview" && (
            <>
              {analyticsError && (
                <div className="admin-feedback admin-feedback-error">{analyticsError}</div>
              )}
              <section className="stats-grid">
                {stats.map((stat) => {
                  const Icon = stat.icon;
                  return (
                    <article key={stat.title} className="stat-card shell-card">
                      <div className="stat-card-top">
                        <div className="stat-icon">
                          <Icon size={20} />
                        </div>
                        <span className="stat-chip">Live</span>
                      </div>
                      <div className="stat-copy">
                        <p className="stat-title">{stat.title}</p>
                        <h3 className="stat-value">
                          {stat.value === null ? "..." : stat.value}
                        </h3>
                        <p className="stat-note">{stat.note}</p>
                      </div>
                    </article>
                  );
                })}
              </section>
              {renderUsersOverviewTable()}
            </>
          )}
          {activeMenu === "map" && renderMapTab()}
          {activeMenu === "tours" && renderToursTab()}
          {activeMenu === "users" && renderUsersTab()}
          {activeMenu === "managers" && renderManagersTab()}
          {activeMenu === "assignments" && renderAssignmentsTab()}
          {activeMenu === "locations" && renderLocationsTab()}
        </div>
      </main>
    </div>
  );
}

export default AdminDashboard;
