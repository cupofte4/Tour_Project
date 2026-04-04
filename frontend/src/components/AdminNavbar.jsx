import {
  LuBell,
  LuLayoutDashboard,
  LuLogOut,
  LuMapPinned,
  LuMic,
  LuSearch,
} from "react-icons/lu";
import "../styles/admin.css";

const defaultItems = [
  { key: "dashboard", label: "Dashboard", href: "/admin", icon: LuLayoutDashboard },
  { key: "add-location", label: "Thêm địa điểm", href: "/admin/add", icon: LuMapPinned },
  { key: "tts", label: "Văn bản thuyết minh", href: "/admin/tts", icon: LuMic },
];

export function AdminSidebar({
  brandTitle = "Travel Admin",
  brandSubtitle = "Travel Audio Guide",
  items = defaultItems,
  activeKey,
  onItemClick,
  onLogout,
}) {
  const currentPath = window.location.pathname;

  const handleLogout = () => {
    if (onLogout) {
      onLogout();
      return;
    }

    localStorage.removeItem("user");
    localStorage.removeItem("username");
    window.location.href = "/login";
  };

  const renderItem = (item) => {
    const Icon = item.icon;
    const isActive = activeKey ? activeKey === item.key : item.href === currentPath;

    if (item.onClick || onItemClick) {
      return (
        <button
          key={item.key}
          type="button"
          className={`sidebar-link ${isActive ? "active" : ""}`}
          onClick={() => (item.onClick ? item.onClick(item) : onItemClick?.(item))}
        >
          <Icon size={18} />
          <span>{item.label}</span>
        </button>
      );
    }

    return (
      <a key={item.key} href={item.href} className={`sidebar-link ${isActive ? "active" : ""}`}>
        <Icon size={18} />
        <span>{item.label}</span>
      </a>
    );
  };

  return (
    <aside className="sidebar shell-card">
      <div className="sidebar-brand">
        <div className="sidebar-brand-icon">
          <LuLayoutDashboard size={18} />
        </div>
        <div>
          <div className="sidebar-brand-title">{brandTitle}</div>
          <div className="sidebar-brand-subtitle">{brandSubtitle}</div>
        </div>
      </div>

      <nav className="sidebar-section">
        <div className="sidebar-section-label">Navigation</div>
        <div className="sidebar-links">{items.map(renderItem)}</div>
      </nav>

      <div className="sidebar-footer">
        <button type="button" className="sidebar-link sidebar-link-danger" onClick={handleLogout}>
          <LuLogOut size={18} />
          <span>Đăng xuất</span>
        </button>
      </div>
    </aside>
  );
}

function AdminNavbar({
  title = "Admin Dashboard",
  subtitle = "Track content, users, and operational updates in one place.",
  showSearch = false,
  searchValue = "",
  searchPlaceholder = "Search dashboard",
  onSearchChange,
}) {
  return (
    <header className="admin-navbar shell-card">
      <div className="admin-navbar-copy">
        <p className="admin-eyebrow">Control center</p>
        <h1 className="admin-navbar-title">{title}</h1>
        <p className="admin-navbar-subtitle">{subtitle}</p>
      </div>

      <div className="admin-navbar-actions">
        {showSearch && (
          <label className="admin-search">
            <LuSearch size={16} />
            <input
              type="text"
              value={searchValue}
              placeholder={searchPlaceholder}
              onChange={(event) => onSearchChange?.(event.target.value)}
            />
          </label>
        )}

        <button type="button" className="admin-icon-button" aria-label="Notifications">
          <LuBell size={18} />
        </button>
      </div>
    </header>
  );
}

export default AdminNavbar;
