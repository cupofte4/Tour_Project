import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import Navbar from "../components/Navbar";
import TravelSidebar from "../components/TravelSidebar";
import { changePassword } from "../services/authService";
import "../styles/myprofile.css";

function Settings() {
  const navigate = useNavigate();
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [formData, setFormData] = useState({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
  });
  const [errorMsg, setErrorMsg] = useState("");
  const [successMsg, setSuccessMsg] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    const storedUser = localStorage.getItem("user");
    if (!storedUser) {
      navigate("/login", { replace: true });
      return;
    }

    setIsAuthenticated(true);
  }, [navigate]);

  const handleInputChange = (event) => {
    const { name, value } = event.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setErrorMsg("");
    setSuccessMsg("");

    const currentPassword = formData.currentPassword.trim();
    const newPassword = formData.newPassword.trim();
    const confirmPassword = formData.confirmPassword.trim();

    if (!currentPassword || !newPassword || !confirmPassword) {
      setErrorMsg("Vui lòng nhập đầy đủ thông tin.");
      return;
    }

    if (newPassword.length < 6) {
      setErrorMsg("Mật khẩu mới phải có ít nhất 6 ký tự.");
      return;
    }

    if (newPassword !== confirmPassword) {
      setErrorMsg("Xác nhận mật khẩu không khớp.");
      return;
    }

    const storedUser = localStorage.getItem("user");
    if (!storedUser) {
      navigate("/login", { replace: true });
      return;
    }

    try {
      const user = JSON.parse(storedUser);
      setIsSubmitting(true);

      const result = await changePassword(
        user.username,
        currentPassword,
        newPassword,
      );

      setSuccessMsg(result.message || "Đổi mật khẩu thành công.");
      setFormData({
        currentPassword: "",
        newPassword: "",
        confirmPassword: "",
      });
    } catch (error) {
      setErrorMsg(error.message || "Không thể đổi mật khẩu lúc này.");
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!isAuthenticated) {
    return null;
  }

  return (
    <>
      <Navbar />
      <div className="container">
        <TravelSidebar />
        <div className="myprofile-main-content">
          <div className="myprofile-content">
            <h1 className="myprofile-title">Cài đặt tài khoản</h1>

            <form className="myprofile-form" onSubmit={handleSubmit}>
              <div style={{ marginBottom: "28px" }}>
                <h2 style={{ fontSize: "20px", marginBottom: "8px", color: "#1b2a41" }}>
                  Đổi mật khẩu
                </h2>
                <p style={{ color: "#607086", lineHeight: 1.6 }}>
                  Nhập mật khẩu hiện tại và thiết lập mật khẩu mới cho tài khoản của bạn.
                </p>
              </div>

              <div className="form-grid" style={{ gridTemplateColumns: "1fr" }}>
                <div className="form-group">
                  <label htmlFor="currentPassword">Mật khẩu hiện tại</label>
                  <input
                    type="password"
                    id="currentPassword"
                    name="currentPassword"
                    value={formData.currentPassword}
                    onChange={handleInputChange}
                    placeholder="Nhập mật khẩu hiện tại"
                    className="editing"
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="newPassword">Mật khẩu mới</label>
                  <input
                    type="password"
                    id="newPassword"
                    name="newPassword"
                    value={formData.newPassword}
                    onChange={handleInputChange}
                    placeholder="Nhập mật khẩu mới"
                    className="editing"
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="confirmPassword">Xác nhận mật khẩu mới</label>
                  <input
                    type="password"
                    id="confirmPassword"
                    name="confirmPassword"
                    value={formData.confirmPassword}
                    onChange={handleInputChange}
                    placeholder="Nhập lại mật khẩu mới"
                    className="editing"
                  />
                </div>
              </div>

              {errorMsg ? (
                <div
                  style={{
                    marginTop: "12px",
                    padding: "12px 14px",
                    borderRadius: "10px",
                    background: "#ffebee",
                    color: "#c62828",
                  }}
                >
                  {errorMsg}
                </div>
              ) : null}

              {successMsg ? (
                <div
                  style={{
                    marginTop: "12px",
                    padding: "12px 14px",
                    borderRadius: "10px",
                    background: "#e8f5e9",
                    color: "#2e7d32",
                  }}
                >
                  {successMsg}
                </div>
              ) : null}

              <div className="button-section">
                <button type="submit" className="btn btn-save" disabled={isSubmitting}>
                  {isSubmitting ? "Đang cập nhật..." : "Đổi mật khẩu"}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </>
  );
}

export default Settings;
