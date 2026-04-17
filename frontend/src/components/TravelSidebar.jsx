import React, { useEffect, useState } from "react";
import { NavLink, useNavigate } from "react-router-dom";
import {
  FaUserCircle,
  FaHome,
  FaUser,
  FaHeart,
  FaCog,
  FaSignOutAlt,
  FaMapMarkedAlt,
} from "react-icons/fa";

const TravelSidebar = () => {
  const navigate = useNavigate();
  const [userInfo, setUserInfo] = useState(null);

  const loadUserData = () => {
    const user = localStorage.getItem("user");
    const username = localStorage.getItem("username");

    if (!user) {
      setUserInfo(null);
      return;
    }

    try {
      const userData = JSON.parse(user);
      setUserInfo({
        fullName: userData.fullName || "User",
        username: username || userData.username || "user",
        avatar: userData.avatar || null,
      });
    } catch (error) {
      console.error("Error parsing user data:", error);
      setUserInfo(null);
    }
  };

  useEffect(() => {
    loadUserData();

    const handleStorageChange = (event) => {
      if (event.key === "user" || event.key === "username") {
        loadUserData();
      }
    };

    const handleProfileUpdate = () => {
      loadUserData();
    };

    window.addEventListener("storage", handleStorageChange);
    window.addEventListener("profileUpdated", handleProfileUpdate);

    return () => {
      window.removeEventListener("storage", handleStorageChange);
      window.removeEventListener("profileUpdated", handleProfileUpdate);
    };
  }, []);

  const menuItems = [
    { id: 1, label: "Trang chủ", icon: FaHome, path: "/" },
    { id: 2, label: "Khám phá Tour", icon: FaMapMarkedAlt, path: "/tours" },
    { id: 3, label: "Hồ sơ của tôi", icon: FaUser, path: "/profile" },
    { id: 4, label: "Địa điểm yêu thích", icon: FaHeart, path: "/favorites" },
    { id: 5, label: "Cài đặt", icon: FaCog, path: "/settings" },
  ];

  const handleLogout = () => {
    localStorage.removeItem("user");
    localStorage.removeItem("username");
    navigate("/login");
  };

  return (
    <aside className="travel-sidebar">
      <div className="sidebar-user-info">
        <div
          className="user-avatar"
          style={{
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            width: "80px",
            height: "80px",
            margin: "0 auto 16px",
            backgroundColor: "#e8f1f8",
            borderRadius: "50%",
            boxShadow: "0 2px 8px rgba(30, 136, 229, 0.2)",
            overflow: "hidden",
          }}
        >
          {userInfo?.avatar ? (
            <img
              src={userInfo.avatar}
              alt="Avatar"
              style={{ width: "100%", height: "100%", objectFit: "cover" }}
            />
          ) : (
            <FaUserCircle size={56} color="rgb(30, 75, 115)" />
          )}
        </div>
        <h3 className="user-name">{userInfo?.fullName || "User"}</h3>
        <p className="user-email">@{userInfo?.username || "user"}</p>
        <div className="user-divider" />
      </div>

      <nav className="sidebar-menu">
        {menuItems.map((item, index) => {
          const IconComponent = item.icon;
          return (
            <React.Fragment key={item.id}>
              <NavLink
                to={item.path}
                className={({ isActive }) =>
                  `menu-btn menu-link ${isActive ? "menu-btn-active" : ""}`
                }
              >
                <IconComponent size={18} style={{ marginRight: "8px" }} />
                {item.label}
              </NavLink>
              {index < menuItems.length - 1 && <hr className="menu-divider" />}
            </React.Fragment>
          );
        })}
      </nav>

      <div className="sidebar-logout">
        <button className="logout-btn" onClick={handleLogout}>
          <FaSignOutAlt size={16} style={{ marginRight: "8px" }} />
          Đăng xuất
        </button>
      </div>
    </aside>
  );
};

export default TravelSidebar;
