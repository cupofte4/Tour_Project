import React, { useEffect, useRef, useState } from "react";
import { FaCamera, FaUserCircle } from "react-icons/fa";
import Navbar from "../components/Navbar";
import TravelSidebar from "../components/TravelSidebar";
import "../styles/myprofile.css";

const LOCAL_PROFILE_KEY = "localProfile";

const getInitialProfileData = () => {
  const storedUser = localStorage.getItem("user");
  const storedLocalProfile = localStorage.getItem(LOCAL_PROFILE_KEY);

  let userData = {};
  let localProfile = {};

  try {
    userData = storedUser ? JSON.parse(storedUser) : {};
  } catch (error) {
    console.error("Error parsing user data:", error);
  }

  try {
    localProfile = storedLocalProfile ? JSON.parse(storedLocalProfile) : {};
  } catch (error) {
    console.error("Error parsing local profile data:", error);
  }

  return {
    fullName: localProfile.fullName || userData.fullName || userData.username || "User",
    username: localProfile.username || userData.username || "guest",
    phone: localProfile.phone || userData.phone || "",
    gender: localProfile.gender || userData.gender || "Nam",
    avatar: localProfile.avatar || userData.avatar || null,
  };
};

function MyProfile() {
  const fileInputRef = useRef(null);
  const [isEditing, setIsEditing] = useState(false);
  const [originalData, setOriginalData] = useState({
    fullName: "User",
    username: "guest",
    phone: "",
    gender: "Nam",
    avatar: null,
  });
  const [formData, setFormData] = useState({
    fullName: "User",
    username: "guest",
    phone: "",
    gender: "Nam",
    avatar: null,
  });

  useEffect(() => {
    const initialData = getInitialProfileData();
    setOriginalData(initialData);
    setFormData(initialData);
  }, []);

  const handleInputChange = (event) => {
    const { name, value } = event.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleAvatarClick = () => {
    if (isEditing) {
      fileInputRef.current?.click();
    }
  };

  const handleAvatarChange = (event) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (loadEvent) => {
      setFormData((prev) => ({
        ...prev,
        avatar: loadEvent.target?.result,
      }));
    };
    reader.readAsDataURL(file);
  };

  const handleSaveClick = (event) => {
    event.preventDefault();

    const updatedProfile = {
      ...formData,
      fullName: formData.fullName.trim() || "User",
    };

    localStorage.setItem(LOCAL_PROFILE_KEY, JSON.stringify(updatedProfile));

    const storedUser = localStorage.getItem("user");
    if (storedUser) {
      try {
        const userData = JSON.parse(storedUser);
        const updatedUser = {
          ...userData,
          fullName: updatedProfile.fullName,
          phone: updatedProfile.phone,
          gender: updatedProfile.gender,
          avatar: updatedProfile.avatar,
        };

        localStorage.setItem("user", JSON.stringify(updatedUser));
      } catch (error) {
        console.error("Error updating stored user:", error);
      }
    }

    window.dispatchEvent(new Event("profileUpdated"));
    setOriginalData(updatedProfile);
    setFormData(updatedProfile);
    setIsEditing(false);
  };

  const handleCancelClick = () => {
    setFormData(originalData);
    setIsEditing(false);
  };

  return (
    <>
      <Navbar />
      <div className="container">
        <TravelSidebar />
        <div className="myprofile-main-content">
          <div className="myprofile-content">
            <h1 className="myprofile-title">Thông tin người dùng</h1>

            <form className="myprofile-form" onSubmit={handleSaveClick}>
              <div className="avatar-section">
                <div
                  className={`avatar-frame ${isEditing ? "editable" : ""}`}
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
                  style={{ display: "none" }}
                />
              </div>

                <div className="form-grid">
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
                    className={isEditing ? "editing" : "disabled"}
                  />
                  </div>

                  <div className="form-group">
                    <label htmlFor="username">Tên hiển thị</label>
                    <input
                      type="text"
                      id="username"
                      name="username"
                      value={formData.username}
                      disabled
                      className="disabled"
                    />
                  </div>

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
                    className={isEditing ? "editing" : "disabled"}
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="gender">Giới tính</label>
                  <select
                    id="gender"
                    name="gender"
                    value={formData.gender}
                    onChange={handleInputChange}
                    disabled={!isEditing}
                    className={isEditing ? "editing" : "disabled"}
                  >
                    <option value="Nam">Nam</option>
                    <option value="Nữ">Nữ</option>
                    <option value="Khác">Khác</option>
                  </select>
                </div>
              </div>

              <div className="button-section">
                {!isEditing ? (
                  <button
                    type="button"
                    className="btn btn-edit"
                    onClick={() => setIsEditing(true)}
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
                    <button type="submit" className="btn btn-save">
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
  );
}

export default MyProfile;
