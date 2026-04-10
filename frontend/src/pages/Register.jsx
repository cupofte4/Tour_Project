import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { register } from "../services/authService";
import "../styles/login.css";

function Register() {
  const [fullName, setFullName] = useState("");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [role, setRole] = useState("user");
  const [errorMsg, setErrorMsg] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();

  const handleRegister = async (event) => {
    event.preventDefault();
    setErrorMsg("");
    setIsLoading(true);

    try {
      if (!fullName.trim()) {
        setErrorMsg("Vui lòng nhập tên đầy đủ.");
        setIsLoading(false);
        return;
      }

      if (!username.trim()) {
        setErrorMsg("Vui lòng nhập tên đăng nhập.");
        setIsLoading(false);
        return;
      }

      if (username.length < 3) {
        setErrorMsg("Tên đăng nhập phải có ít nhất 3 ký tự.");
        setIsLoading(false);
        return;
      }

      if (!password.trim()) {
        setErrorMsg("Vui lòng nhập mật khẩu.");
        setIsLoading(false);
        return;
      }

      if (password.length < 6) {
        setErrorMsg("Mật khẩu phải có ít nhất 6 ký tự.");
        setIsLoading(false);
        return;
      }

      if (!confirmPassword.trim()) {
        setErrorMsg("Vui lòng xác nhận mật khẩu.");
        setIsLoading(false);
        return;
      }

      if (password !== confirmPassword) {
        setErrorMsg("Mật khẩu xác nhận không khớp.");
        setIsLoading(false);
        return;
      }

      const result = await register(fullName, username, password, role);

      if (result) {
        navigate("/login", {
          state: { message: "Đăng ký thành công! Vui lòng đăng nhập." },
        });
      } else {
        setErrorMsg("Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.");
      }
    } catch (error) {
      setErrorMsg("Có lỗi xảy ra. Vui lòng thử lại.");
      console.error("Register error:", error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-shell">
        <section className="login-visual-panel" aria-hidden="true">
          <div className="visual-backdrop" />
          <div className="visual-copy">
            <p className="visual-eyebrow">NEW TRAVELLER!</p>
            <h1 className="visual-title">Bắt đầu hành trình khám phá của riêng bạn.</h1>
            <p className="visual-description">
              Chỉ mất vài giây để gia nhập cộng đồng xê dịch Travel Audio Guide!!. Điền thông tin của bạn và mở khóa vô vàn trải nghiệm mới lạ.
            </p>
          </div>

          <div className="map-illustration">
            <div className="map-board">
              <div className="map-fold map-fold-one" />
              <div className="map-fold map-fold-two" />
              <div className="map-route" />
              <span className="map-pin map-pin-one" />
              <span className="map-pin map-pin-two" />
              <span className="map-pin map-pin-three" />
              <span className="map-shadow" />
            </div>
          </div>
        </section>

        <section className="login-form-panel">
          <div className="login-card">
            <div className="login-brand">
              <span className="login-brand-mark">KHỞI ĐẦU CÙNG TRAVEL AUDIO GUIDE</span>
            </div>

            <div className="login-header">
              <h2 className="login-title">Tham gia cùng Travel Audio Guide</h2>
              <p className="login-subtitle">
                Bắt đầu lưu trữ các điểm đến yêu thích, lên lịch trình cá nhân và khám phá những trải nghiệm du lịch tuyệt vời cùng Travel Audio Guide!!.
              </p>
            </div>

            {errorMsg && (
              <div className="login-alert login-alert-error">
                <span className="login-alert-icon">!</span>
                <span>{errorMsg}</span>
              </div>
            )}

            <form onSubmit={handleRegister} className="login-form">
              <div className="form-group">
                <label>Loại tài khoản</label>
                <div className="role-options" role="radiogroup" aria-label="Role selection">
                  <label className="role-option">
                    <input
                      type="radio"
                      name="role"
                      value="user"
                      checked={role === "user"}
                      onChange={() => setRole("user")}
                    />
                    <span>Tôi muốn trải nghiệm app</span>
                  </label>
                  <label className="role-option">
                    <input
                      type="radio"
                      name="role"
                      value="manager"
                      checked={role === "manager"}
                      onChange={() => setRole("manager")}
                    />
                    <span>Tôi muốn kinh doanh</span>
                  </label>
                </div>
              </div>
              <div className="form-group">
                <label htmlFor="fullName">Họ và Tên</label>
                <input
                  type="text"
                  id="fullName"
                  className="form-input"
                  placeholder="Ngo Tran Bao Tin"
                  value={fullName}
                  onChange={(event) => setFullName(event.target.value)}
                />
              </div>

              <div className="form-group">
                <label htmlFor="username">Tên tài khoản</label>
                <input
                  type="text"
                  id="username"
                  className="form-input"
                  placeholder="Choose a username"
                  value={username}
                  onChange={(event) => setUsername(event.target.value)}
                />
              </div>

              <div className="form-group">
                <label htmlFor="password">Mật khẩu</label>
                <input
                  type="password"
                  id="password"
                  className="form-input"
                  placeholder="Create a password"
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                />
              </div>

              <div className="form-group">
                <label htmlFor="confirmPassword">Xác nhận mật khẩu</label>
                <input
                  type="password"
                  id="confirmPassword"
                  className="form-input"
                  placeholder="Confirm your password"
                  value={confirmPassword}
                  onChange={(event) => setConfirmPassword(event.target.value)}
                />
              </div>

              <button type="submit" className="login-btn" disabled={isLoading}>
                {isLoading ? "Creating account..." : "Create account"}
              </button>
            </form>

            <p className="login-footer">
              Đã có tài khoản?
              <a href="/login" className="signup-link">
                Đăng nhập ngay
              </a>
            </p>
          </div>
        </section>
      </div>
    </div>
  );
}

export default Register;
