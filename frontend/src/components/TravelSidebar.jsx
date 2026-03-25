import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { FaUserCircle, FaUser, FaHeart, FaCalendar, FaCog, FaSignOutAlt } from 'react-icons/fa';

const TravelSidebar = () => {
  const navigate = useNavigate();
  const [userInfo, setUserInfo] = useState(null);
  
  useEffect(() => {
    // Get user info from localStorage
    const user = localStorage.getItem('user');
    const username = localStorage.getItem('username');
    
    if (user) {
      try {
        const userData = JSON.parse(user);
        setUserInfo({
          fullName: userData.fullName || 'User',
          username: username || userData.username || 'user'
        });
      } catch (error) {
        console.error('Error parsing user data:', error);
      }
    }
  }, []);

  const menuItems = [
    { id: 1, label: 'Hồ sơ của tôi', icon: FaUser },
    { id: 2, label: 'Địa điểm yêu thích', icon: FaHeart },
    { id: 3, label: 'Lịch trình', icon: FaCalendar },
    { id: 4, label: 'Cài đặt', icon: FaCog }
  ];

  const handleLogout = () => {
    localStorage.removeItem('user');
    localStorage.removeItem('username');
    navigate('/login');
  };

  return (
    <aside className="travel-sidebar">
      {/* User Info Section */}
      <div className="sidebar-user-info">
        <div className="user-avatar" style={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          width: '80px',
          height: '80px',
          margin: '0 auto 16px',
          backgroundColor: '#e8f1f8',
          borderRadius: '50%',
          boxShadow: '0 2px 8px rgba(30, 136, 229, 0.2)'
        }}>
          <FaUserCircle size={56} color="#1e88e5" />
        </div>
        <h3 className="user-name">{userInfo?.fullName || 'User'}</h3>
        <p className="user-email">@{userInfo?.username || 'user'}</p>
        <div className="user-divider"></div>
      </div>

      {/* Menu Items */}
      <nav className="sidebar-menu">
        {menuItems.map((item, index) => {
          const IconComponent = item.icon;
          return (
            <React.Fragment key={item.id}>
              <button className="menu-btn">
                <IconComponent size={18} style={{ marginRight: '8px' }} />
                {item.label}
              </button>
              {index < menuItems.length - 1 && <hr className="menu-divider" />}
            </React.Fragment>
          );
        })}
      </nav>

      {/* Logout Button Section */}
      <div className="sidebar-logout">
        <button className="logout-btn" onClick={handleLogout}>
          <FaSignOutAlt size={16} style={{ marginRight: '8px' }} />
          Đăng xuất
        </button>
      </div>
    </aside>
  );
};

export default TravelSidebar;
