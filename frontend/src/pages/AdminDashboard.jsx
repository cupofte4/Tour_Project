import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { FaUserShield, FaUsers, FaMapMarkerAlt, FaEye, FaSignOutAlt, FaHome, FaList, FaChartBar } from 'react-icons/fa';
import '../styles/admin.css';

function AdminDashboard() {
  const navigate = useNavigate();
  const [user, setUser] = useState(null);
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [activeMenu, setActiveMenu] = useState('overview');

  // Authorization check on mount
  useEffect(() => {
    try {
      const userString = localStorage.getItem('user');

      // Check if user exists
      if (!userString) {
        alert('Bạn không có quyền truy cập trang này!');
        navigate('/login');
        return;
      }

      // Parse user data
      const userData = JSON.parse(userString);

      // Check if user is Admin
      if (!userData || userData.role !== 'Admin') {
        alert('Bạn không có quyền truy cập trang này!');
        navigate('/');
        return;
      }

      // User is authorized
      setUser(userData);
      setIsAuthorized(true);
    } catch (error) {
      console.error('Error parsing user data:', error);
      alert('Bạn không có quyền truy cập trang này!');
      navigate('/login');
    }
  }, [navigate]);

  const handleLogout = () => {
    localStorage.removeItem('user');
    localStorage.removeItem('username');
    navigate('/login');
  };

  // If not authorized, don't render anything
  if (!isAuthorized || !user) {
    return null;
  }

  // Mock data for stats
  const stats = [
    {
      title: 'Tổng số Users',
      value: '242',
      icon: FaUsers,
      color: 'blue'
    },
    {
      title: 'Tổng số Địa điểm',
      value: '156',
      icon: FaMapMarkerAlt,
      color: 'green'
    },
    {
      title: 'Lượt truy cập',
      value: '8,542',
      icon: FaEye,
      color: 'orange'
    }
  ];

  // Mock data for recent users
  const recentUsers = [
    { id: 1, fullName: 'Nguyễn Văn A', username: 'nguyenvana', email: 'nguyenvana@example.com', joinDate: '2026-03-20' },
    { id: 2, fullName: 'Trần Thị B', username: 'tranthib', email: 'tranthib@example.com', joinDate: '2026-03-21' },
    { id: 3, fullName: 'Lê Văn C', username: 'levanc', email: 'levanc@example.com', joinDate: '2026-03-22' },
    { id: 4, fullName: 'Phạm Thị D', username: 'phamthid', email: 'phamthid@example.com', joinDate: '2026-03-23' },
    { id: 5, fullName: 'Hoàng Văn E', username: 'hoangvane', email: 'hoangvane@example.com', joinDate: '2026-03-24' }
  ];

  return (
    <div className="layout">
      {/* Sidebar */}
      <aside className="sidebar">
        <div className="sidebar-logo">
          <FaUserShield style={{ marginRight: '8px', fontSize: '20px' }} />
          ⚙️ Admin Panel
        </div>

        {/* Menu Items */}
        <nav className="sidebar-menu">
          <button
            className={`sidebar-menu-item ${activeMenu === 'overview' ? 'active' : ''}`}
            onClick={() => setActiveMenu('overview')}
          >
            <FaChartBar style={{ marginRight: '8px' }} />
            Tổng quan
          </button>
          <button
            className={`sidebar-menu-item ${activeMenu === 'users' ? 'active' : ''}`}
            onClick={() => setActiveMenu('users')}
          >
            <FaUsers style={{ marginRight: '8px' }} />
            Quản lý Users
          </button>
          <button
            className={`sidebar-menu-item ${activeMenu === 'locations' ? 'active' : ''}`}
            onClick={() => setActiveMenu('locations')}
          >
            <FaMapMarkerAlt style={{ marginRight: '8px' }} />
            Quản lý Địa điểm
          </button>
        </nav>

        {/* Logout Button */}
        <div className="sidebar-bottom">
          <button className="logout-btn" onClick={handleLogout}>
            <FaSignOutAlt style={{ marginRight: '8px' }} />
            Đăng xuất
          </button>
        </div>
      </aside>

      {/* Main Content Area */}
      <main className="main">
        {/* Admin Navbar */}
        <div className="admin-navbar">
          <h1 className="admin-navbar-title">
            {activeMenu === 'overview' && '📊 Tổng quan'}
            {activeMenu === 'users' && '👥 Quản lý Users'}
            {activeMenu === 'locations' && '📍 Quản lý Địa điểm'}
          </h1>

          <div className="admin-navbar-user">
            <div className="avatar">
              {user.avatar ? (
                <img src={user.avatar} alt="User" />
              ) : (
                <div className="avatar-placeholder">
                  {user.fullName?.charAt(0).toUpperCase() || 'A'}
                </div>
              )}
            </div>
            <span className="user-name">{user.fullName || 'Admin'}</span>
          </div>
        </div>

        {/* Main Content */}
        <div className="content">
          {/* Overview Tab */}
          {activeMenu === 'overview' && (
            <>
              {/* Stats Grid */}
              <div className="stats-grid">
                {stats.map((stat, index) => {
                  const Icon = stat.icon;
                  return (
                    <div key={index} className="stat-card">
                      <div className={`stat-icon ${stat.color}`}>
                        <Icon size={32} />
                      </div>
                      <div className="stat-content">
                        <h3 className="stat-title">{stat.title}</h3>
                        <p className="stat-value">{stat.value}</p>
                      </div>
                    </div>
                  );
                })}
              </div>

              {/* Recent Users Card */}
              <div className="card">
                <h2 className="card-title">📝 Danh sách người dùng mới</h2>
                <table className="users-table">
                  <thead>
                    <tr>
                      <th>STT</th>
                      <th>Họ và tên</th>
                      <th>Username</th>
                      <th>Email</th>
                      <th>Ngày tham gia</th>
                    </tr>
                  </thead>
                  <tbody>
                    {recentUsers.map((u, index) => (
                      <tr key={u.id}>
                        <td>{index + 1}</td>
                        <td>{u.fullName}</td>
                        <td>{u.username}</td>
                        <td>{u.email}</td>
                        <td>{u.joinDate}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </>
          )}

          {/* Users Management Tab */}
          {activeMenu === 'users' && (
            <div className="card">
              <h2 className="card-title">👥 Quản lý Users</h2>
              <table className="users-table">
                <thead>
                  <tr>
                    <th>STT</th>
                    <th>Họ và tên</th>
                    <th>Username</th>
                    <th>Email</th>
                    <th>Ngày tham gia</th>
                    <th>Hành động</th>
                  </tr>
                </thead>
                <tbody>
                  {recentUsers.map((u, index) => (
                    <tr key={u.id}>
                      <td>{index + 1}</td>
                      <td>{u.fullName}</td>
                      <td>{u.username}</td>
                      <td>{u.email}</td>
                      <td>{u.joinDate}</td>
                      <td>
                        <button className="action-btn edit-btn">Sửa</button>
                        <button className="action-btn delete-btn">Xóa</button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {/* Locations Management Tab */}
          {activeMenu === 'locations' && (
            <div className="card">
              <h2 className="card-title">📍 Quản lý Địa điểm</h2>
              <div className="coming-soon">
                <p>Chức năng quản lý địa điểm đang được phát triển...</p>
              </div>
            </div>
          )}
        </div>
      </main>
    </div>
  );
}

export default AdminDashboard;
