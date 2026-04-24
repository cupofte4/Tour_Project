import React, { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { login } from "../services/authService";
import "../styles/login.css";

function Login() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [rememberMe, setRememberMe] = useState(true);
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
        setErrorMsg("Vui long nhap ten dang nhap.");
        setIsLoading(false);
        return;
      }

      if (!password.trim()) {
        setErrorMsg("Vui long nhap mat khau.");
        setIsLoading(false);
        return;
      }

      if (password.length < 6) {
        setErrorMsg("Mat khau phai co it nhat 6 ky tu.");
        setIsLoading(false);
        return;
      }

      const user = await login(username, password);

      if (!user) {
        setErrorMsg("Ten dang nhap hoac mat khau khong chinh xac.");
        return;
      }

      const normalizedRole = (user.role || "").toLowerCase();

      localStorage.setItem("username", username);
      localStorage.setItem("rememberMe", JSON.stringify(rememberMe));

      if (normalizedRole === "admin") {
        navigate("/admin/dashboard", { replace: true });
      } else if (normalizedRole === "manager") {
        navigate("/manager/dashboard", { replace: true });
      } else {
        navigate("/", { replace: true });
      }
    } catch (error) {
      setErrorMsg(error.message || "Co loi xay ra. Vui long thu lai.");
      console.error("Login error:", error);
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
            <p className="visual-eyebrow">NEW JOURNEY!</p>
            <h1 className="visual-title">Kham pha the gioi, tron ven tung khoanh khac.</h1>
            <p className="visual-description">
              Dang nhap de luu lai cac diem den yeu thich, len ke hoach cho chuyen di va nhan
              nhung uu dai du lich danh rieng cho ban.
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
              <span className="login-brand-mark">KHOI DAU CUNG TRAVEL AUDIO GUIDE</span>
            </div>

            <div className="login-header">
              <h2 className="login-title">Ket noi toi Travel Audio Guide</h2>
              <p className="login-subtitle">
                Mo hanh trinh kham pha bang nhung cau chuyen am thanh tai moi diem den.
              </p>
            </div>

            {successMsg && (
              <div className="login-alert login-alert-success">
                <span className="login-alert-icon">✓</span>
                <span>{successMsg}</span>
              </div>
            )}

            {errorMsg && (
              <div className="login-alert login-alert-error">
                <span className="login-alert-icon">!</span>
                <span>{errorMsg}</span>
              </div>
            )}

            <form onSubmit={handleLogin} className="login-form">
              <div className="form-group">
                <label htmlFor="username">Ten tai khoan</label>
                <input
                  type="text"
                  id="username"
                  className="form-input"
                  autoComplete="username"
                  placeholder="Enter your username"
                  value={username}
                  onChange={(event) => setUsername(event.target.value)}
                />
              </div>

              <div className="form-group">
                <label htmlFor="password">Mat khau</label>
                <input
                  type="password"
                  id="password"
                  className="form-input"
                  autoComplete="current-password"
                  placeholder="Enter your password"
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                />
              </div>

              <div className="login-meta">
                <label className="remember-option" htmlFor="rememberMe">
                  <input
                    id="rememberMe"
                    type="checkbox"
                    checked={rememberMe}
                    onChange={(event) => setRememberMe(event.target.checked)}
                  />
                  <span>Nho tai khoan</span>
                </label>

                <a href="#forgot" className="meta-link">
                  Quen mat khau?
                </a>
              </div>

              <button type="submit" className="login-btn" disabled={isLoading}>
                {isLoading ? "Logging in..." : "Log in"}
              </button>
            </form>
          </div>
        </section>
      </div>
    </div>
  );
}

export default Login;
