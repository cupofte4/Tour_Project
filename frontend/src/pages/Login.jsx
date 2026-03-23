import { useState } from "react";
import { login } from "../services/authService";
import "../styles/admin.css";

function Login() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  const handleLogin = async () => {
    setError("");
    const user = await login(username, password);
    if (user) {
      localStorage.setItem("user", JSON.stringify(user));
      window.location.href = "/admin";
    } else {
      setError("Sai tài khoản hoặc mật khẩu!");
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter") handleLogin();
  };

  return (
    <div className="login-page">
      <div className="login-box">
        <div className="login-logo">
          <div className="icon">🗺️</div>
          <h2>Travel Audio Guide</h2>
          <p>Đăng nhập vào trang quản trị</p>
        </div>

        {error && <div className="error-msg">⚠️ {error}</div>}

        <div className="form-group">
          <label>Tài khoản</label>
          <input
            placeholder="Nhập tên đăng nhập"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            onKeyDown={handleKeyDown}
          />
        </div>

        <div className="form-group">
          <label>Mật khẩu</label>
          <input
            type="password"
            placeholder="Nhập mật khẩu"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            onKeyDown={handleKeyDown}
          />
        </div>

        <button className="login-btn" onClick={handleLogin}>
          Đăng nhập
        </button>
      </div>
    </div>
  );
}

export default Login;
