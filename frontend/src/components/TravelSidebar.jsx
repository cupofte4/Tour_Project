import React, { useEffect, useState } from "react";
import { NavLink, useNavigate } from "react-router-dom";
import {
  FaUserCircle,
  FaHome,
  FaHeart,
  FaSignOutAlt,
  FaMapMarkedAlt,
} from "react-icons/fa";

const LOCAL_PROFILE_KEY = "localProfile";

const TravelSidebar = () => {
  const navigate = useNavigate();
  const [userInfo, setUserInfo] = useState({
    fullName: "Khách tham quan",
    username: "Guest_Mode",
    avatar: null,
    isAuthenticated: false,
  });

  const loadUserData = () => {
    const user = localStorage.getItem("user");
    const username = localStorage.getItem("username");
    const localProfile = localStorage.getItem(LOCAL_PROFILE_KEY);

    try {
      const userData = user ? JSON.parse(user) : {};
      const localData = localProfile ? JSON.parse(localProfile) : {};
      setUserInfo({
        fullName: localData.fullName || userData.fullName || "Khách tham quan",
        username: localData.username || username || userData.username || "Guest_Mode",
        avatar: localData.avatar || userData.avatar || null,
        isAuthenticated: !!user,
      });
    } catch (error) {
      console.error("Error parsing user data:", error);
      setUserInfo({
        fullName: "Khách tham quan",
        username: "Guest_Mode",
        avatar: null,
        isAuthenticated: false,
      });
    }
  };

  useEffect(() => {
    loadUserData();

    const handleStorageChange = (event) => {
      if (event.key === "user" || event.key === "username" || event.key === LOCAL_PROFILE_KEY) {
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
    { id: 4, label: "Địa điểm yêu thích", icon: FaHeart, path: "/favorites" },
  ];

  const handleLogout = () => {
    localStorage.removeItem("user");
    localStorage.removeItem("username");
    localStorage.removeItem("token");
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
        <h3 className="user-name">{userInfo?.fullName || "Khách tham quan"}</h3>
        <p className="user-email">@{userInfo?.username || "Guest_Mode"}</p>
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

      {userInfo?.isAuthenticated ? (
        <div className="sidebar-logout">
          <button className="logout-btn" onClick={handleLogout}>
            <FaSignOutAlt size={16} style={{ marginRight: "8px" }} />
            Đăng xuất
          </button>
        </div>
      ) : null}
    </aside>
  );
};

export default TravelSidebar;
