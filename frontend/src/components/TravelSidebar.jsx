import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { FaUserCircle, FaUser, FaHeart, FaCalendar, FaCog, FaSignOutAlt } from 'react-icons/fa';

const TravelSidebar = () => {
  const navigate = useNavigate();
  const [userInfo, setUserInfo] = useState(null);
  
  // Load user data from localStorage
  const loadUserData = () => {
    const user = localStorage.getItem('user');
    const username = localStorage.getItem('username');
    
    if (user) {
      try {
        const userData = JSON.parse(user);
        setUserInfo({
          fullName: userData.fullName || 'User',
          username: username || userData.username || 'user',
          avatar: userData.avatar || null
        });
      } catch (error) {
        console.error('Error parsing user data:', error);
      }
    }
  };

  useEffect(() => {
    loadUserData();

    // Listen for storage changes (from other tabs/windows)
    const handleStorageChange = (e) => {
      if (e.key === 'user') {
        loadUserData();
      }
    };

    // Listen for custom events from same window
    const handleProfileUpdate = () => {
      loadUserData();
    };

    window.addEventListener('storage', handleStorageChange);
    window.addEventListener('profileUpdated', handleProfileUpdate);

    return () => {
      window.removeEventListener('storage', handleStorageChange);
      window.removeEventListener('profileUpdated', handleProfileUpdate);
    };
  }, []);

  const menuItems = [
    { id: 1, label: 'Hồ sơ của tôi', icon: FaUser, path: '/profile' },
    { id: 2, label: 'Địa điểm yêu thích', icon: FaHeart, path: '/favorites' },
    { id: 3, label: 'Lịch trình', icon: FaCalendar, path: '/schedule' },
    { id: 4, label: 'Cài đặt', icon: FaCog, path: '/settings' }
  ];

  const handleMenuClick = (path) => {
    navigate(path);
  };

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
          boxShadow: '0 2px 8px rgba(30, 136, 229, 0.2)',
          overflow: 'hidden'
        }}>
          {userInfo?.avatar ? (
            <img 
              src={userInfo.avatar} 
              alt="Avatar" 
              style={{ width: '100%', height: '100%', objectFit: 'cover' }}
            />
          ) : (
            <FaUserCircle size={56} color="#1e88e5" />
          )}
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
              <button 
                className="menu-btn"
                onClick={() => handleMenuClick(item.path)}
              >
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
