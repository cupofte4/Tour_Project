import "../styles/admin.css";

function Sidebar() {
  const path = window.location.pathname;

  return (
    <div className="sidebar">
      <div className="sidebar-logo">
        🗺️ Travel Admin
      </div>
      <a href="/admin" className={path === "/admin" ? "active" : ""}>📊 Dashboard</a>
      <a href="/admin/add" className={path === "/admin/add" ? "active" : ""}>➕ Thêm địa điểm</a>
      <a href="/admin/tts" className={path === "/admin/tts" ? "active" : ""}>🎙️ Văn bản thuyết minh</a>
      <div className="sidebar-bottom">
        <a href="/">🏠 Trang chủ</a>
        <a href="/login">🚪 Đăng xuất</a>
      </div>
    </div>
  );
}

export default Sidebar;
