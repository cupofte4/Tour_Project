import React, { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { FaCamera, FaUserCircle } from 'react-icons/fa';
import Navbar from '../components/Navbar';
import TravelSidebar from '../components/TravelSidebar';
import '../styles/myprofile.css';

function MyProfile() {
  const navigate = useNavigate();
  const fileInputRef = useRef(null);
  const [isEditing, setIsEditing] = useState(false);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  
  // Initial data
  const [originalData, setOriginalData] = useState({
    fullName: 'Nguyễn Văn A',
    phone: '0912345678',
    gender: 'Nam',
    avatar: null
  });

  // Working data
  const [formData, setFormData] = useState({
    fullName: 'Nguyễn Văn A',
    phone: '0912345678',
    gender: 'Nam',
    avatar: null
  });

  // Check authentication on mount
  useEffect(() => {
    const user = localStorage.getItem('user');
    if (!user) {
      navigate('/login');
    } else {
      setIsAuthenticated(true);
    }
  }, [navigate]);

  // Load user data from localStorage on mount
  useEffect(() => {
    const user = localStorage.getItem('user');
    
    if (user) {
      try {
        const userData = JSON.parse(user);
        const initialData = {
          fullName: userData.fullName || 'User',
          phone: userData.phone || '',
          gender: userData.gender || 'Nam',
          avatar: userData.avatar || null
        };
        setOriginalData(initialData);
        setFormData(initialData);
      } catch (error) {
        console.error('Error parsing user data:', error);
      }
    }
  }, []);

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleAvatarClick = () => {
    if (isEditing) {
      fileInputRef.current?.click();
    }
  };

  const handleAvatarChange = (e) => {
    const file = e.target.files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (event) => {
        setFormData(prev => ({
          ...prev,
          avatar: event.target?.result
        }));
      };
      reader.readAsDataURL(file);
    }
  };

  const handleEditClick = () => {
    setIsEditing(true);
  };

  const handleSaveClick = (e) => {
    e.preventDefault();
    console.log('Saving user data:', formData);
    
    // Update localStorage
    const user = localStorage.getItem('user');
    if (user) {
      const userData = JSON.parse(user);
      const updatedUser = {
        ...userData,
        fullName: formData.fullName,
        phone: formData.phone,
        gender: formData.gender,
        avatar: formData.avatar
      };
      localStorage.setItem('user', JSON.stringify(updatedUser));
      
      // Dispatch custom event to notify other components
      window.dispatchEvent(new Event('profileUpdated'));
    }

    // Update original data
    setOriginalData(formData);
    setIsEditing(false);
  };

  const handleCancelClick = () => {
    setFormData(originalData);
    setIsEditing(false);
  };

  return (
    <div>
      {isAuthenticated ? (
        <>
          <Navbar />
          <div className="container">
            <TravelSidebar />
            <div className="myprofile-main-content">
              <div className="myprofile-content">
                {/* Header Title */}
                <h1 className="myprofile-title">Thông tin người dùng</h1>

                {/* Form */}
                <form className="myprofile-form" onSubmit={handleSaveClick}>
                  {/* Avatar Section */}
                  <div className="avatar-section">
                    <div 
                      className={`avatar-frame ${isEditing ? 'editable' : ''}`}
                      onClick={handleAvatarClick}
                    >
                      {formData.avatar ? (
                        <img src={formData.avatar} alt="Avatar" className="avatar-image" />
                      ) : (
                        <FaUserCircle className="avatar-icon" />
                      )}
                      
                      {isEditing && (
                        <div className="avatar-overlay">
                          <FaCamera className="camera-icon" />
                          <span>Thay đổi ảnh</span>
                        </div>
                      )}
                    </div>
                    <input
                      ref={fileInputRef}
                      type="file"
                      accept="image/*"
                      onChange={handleAvatarChange}
                      style={{ display: 'none' }}
                    />
                  </div>

                  {/* Form Fields Grid */}
                  <div className="form-grid">
                    {/* Full Name */}
                    <div className="form-group">
                      <label htmlFor="fullName">Họ và tên</label>
                      <input
                        type="text"
                        id="fullName"
                        name="fullName"
                        value={formData.fullName}
                        onChange={handleInputChange}
                        disabled={!isEditing}
                        placeholder="Nhập họ và tên"
                        className={isEditing ? 'editing' : 'disabled'}
                      />
                    </div>

                    {/* Phone */}
                    <div className="form-group">
                      <label htmlFor="phone">Số điện thoại</label>
                      <input
                        type="tel"
                        id="phone"
                        name="phone"
                        value={formData.phone}
                        onChange={handleInputChange}
                        disabled={!isEditing}
                        placeholder="Nhập số điện thoại"
                        className={isEditing ? 'editing' : 'disabled'}
                      />
                    </div>

                    {/* Gender */}
                    <div className="form-group">
                      <label htmlFor="gender">Giới tính</label>
                      <select
                        id="gender"
                        name="gender"
                        value={formData.gender}
                        onChange={handleInputChange}
                        disabled={!isEditing}
                        className={isEditing ? 'editing' : 'disabled'}
                      >
                        <option value="Nam">Nam</option>
                        <option value="Nữ">Nữ</option>
                        <option value="Khác">Khác</option>
                      </select>
                    </div>
                  </div>

                  {/* Button Section */}
                  <div className="button-section">
                    {!isEditing ? (
                      <button
                        type="button"
                        className="btn btn-edit"
                        onClick={handleEditClick}
                      >
                        Thay đổi thông tin
                      </button>
                    ) : (
                      <div className="button-group">
                        <button
                          type="button"
                          className="btn btn-cancel"
                          onClick={handleCancelClick}
                        >
                          Hủy
                        </button>
                        <button
                          type="submit"
                          className="btn btn-save"
                        >
                          Lưu thông tin
                        </button>
                      </div>
                    )}
                  </div>
                </form>
              </div>
            </div>
          </div>
        </>
      ) : null}
    </div>
  );
}

export default MyProfile;
