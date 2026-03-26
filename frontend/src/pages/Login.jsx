import React, { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { login } from "../services/authService";
import "../styles/login.css";

function Login() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [errorMsg, setErrorMsg] = useState("");
  const [successMsg, setSuccessMsg] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    if (location.state?.message) {
      setSuccessMsg(location.state.message);
      const timer = setTimeout(() => setSuccessMsg(""), 5000);
      return () => clearTimeout(timer);
    }
  }, [location.state?.message]);

  const handleLogin = async (event) => {
    event.preventDefault();
    setErrorMsg("");
    setSuccessMsg("");
    setIsLoading(true);

    try {
      if (!username.trim()) {
        setErrorMsg("Vui lòng nhập tên đăng nhập");
        setIsLoading(false);
        return;
      }

      if (!password.trim()) {
        setErrorMsg("Vui lòng nhập mật khẩu");
        setIsLoading(false);
        return;
      }

      if (password.length < 6) {
        setErrorMsg("Mật khẩu phải có ít nhất 6 ký tự");
        setIsLoading(false);
        return;
      }

      const user = await login(username, password);

      if (!user) {
        setErrorMsg("Tên đăng nhập hoặc mật khẩu không chính xác");
        return;
      }

      const normalizedUser = {
        ...user,
        role: (user.role || "").toLowerCase(),
      };

      localStorage.setItem("user", JSON.stringify(normalizedUser));
      localStorage.setItem("username", username);

      if (normalizedUser.role === "admin") {
        navigate("/admin/dashboard", { replace: true });
      } else {
        navigate("/", { replace: true });
      }
    } catch (error) {
      setErrorMsg(error.message || "Có lỗi xảy ra. Vui lòng thử lại!");
      console.error("Login error:", error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-box">
        <div className="login-header">
          <div className="login-logo">
            <span className="logo-icon">🎧</span>
          </div>
          <h1 className="login-title">Travel Audio Guide</h1>
          <p className="login-subtitle">Đăng nhập để tiếp tục</p>
        </div>

        {successMsg && (
          <div
            style={{
              backgroundColor: "#e8f5e9",
              color: "#2e7d32",
              padding: "12px 16px",
              borderRadius: "8px",
              marginBottom: "20px",
              fontSize: "14px",
              display: "flex",
              alignItems: "center",
              gap: "8px",
              borderLeft: "4px solid #2e7d32",
            }}
          >
            <span>✅</span> {successMsg}
          </div>
        )}

        {errorMsg && (
          <div className="error-msg">
            <span>⚠️</span> {errorMsg}
          </div>
        )}

        <form onSubmit={handleLogin} className="login-form">
          <div className="form-group">
            <label htmlFor="username">Tên đăng nhập</label>
            <input
              type="text"
              id="username"
              className="form-input"
              autoComplete="username"
              placeholder="Nhập tên đăng nhập"
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
              autoComplete="current-password"
              placeholder="Nhập mật khẩu"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
            />
          </div>

          <button type="submit" className="login-btn" disabled={isLoading}>
            {isLoading ? "Đang đăng nhập..." : "Đăng nhập"}
          </button>
        </form>

        <div className="login-footer">
          <a href="#forgot" className="footer-link">
            Quên mật khẩu?
          </a>
          <span className="footer-divider">•</span>
          <a href="/register" className="footer-link">
            Chưa có tài khoản? <strong>Đăng ký ngay</strong>
          </a>
        </div>
      </div>
    </div>
  );
}

export default Login;
