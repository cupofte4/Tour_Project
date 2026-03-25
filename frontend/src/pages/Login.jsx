import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { login } from '../services/authService';
import '../styles/login.css';

function Login() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [errorMsg, setErrorMsg] = useState('');
  const [successMsg, setSuccessMsg] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    // Display success message from register page
    if (location.state?.message) {
      setSuccessMsg(location.state.message);
      // Clear success message after 5 seconds
      const timer = setTimeout(() => setSuccessMsg(''), 5000);
      return () => clearTimeout(timer);
    }
  }, [location.state?.message]);

  const handleLogin = async (e) => {
    e.preventDefault();
    setErrorMsg('');
    setSuccessMsg('');
    setIsLoading(true);

    try {
      // Validation
      if (!username.trim()) {
        setErrorMsg('Vui lòng nhập tên đăng nhập');
        setIsLoading(false);
        return;
      }

      if (!password.trim()) {
        setErrorMsg('Vui lòng nhập mật khẩu');
        setIsLoading(false);
        return;
      }

      // Password validation (minimum 6 characters)
      if (password.length < 6) {
        setErrorMsg('Mật khẩu phải có ít nhất 6 ký tự');
        setIsLoading(false);
        return;
      }

      // Call API login
      const user = await login(username, password);

      if (user) {
        // Save user info to localStorage
        localStorage.setItem('user', JSON.stringify(user));
        localStorage.setItem('username', username);
        
        // Navigate to home
        navigate('/');
      } else {
        setErrorMsg('Tên đăng nhập hoặc mật khẩu không chính xác');
      }
    } catch (error) {
      setErrorMsg('Có lỗi xảy ra. Vui lòng thử lại!');
      console.error('Login error:', error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-box">
        {/* Header */}
        <div className="login-header">
          <div className="login-logo">
            <span className="logo-icon">🎧</span>
          </div>
          <h1 className="login-title">Travel Audio Guide</h1>
          <p className="login-subtitle">Đăng nhập để tiếp tục</p>
        </div>

        {/* Success Message */}
        {successMsg && (
          <div style={{
            backgroundColor: '#e8f5e9',
            color: '#2e7d32',
            padding: '12px 16px',
            borderRadius: '8px',
            marginBottom: '20px',
            fontSize: '14px',
            display: 'flex',
            alignItems: 'center',
            gap: '8px',
            borderLeft: '4px solid #2e7d32'
          }}>
            <span>✅</span> {successMsg}
          </div>
        )}

        {/* Error Message */}
        {errorMsg && (
          <div className="error-msg">
            <span>⚠️</span> {errorMsg}
          </div>
        )}

        {/* Form */}
        <form onSubmit={handleLogin} className="login-form">
          {/* Username Input */}
          <div className="form-group">
            <label htmlFor="username">Tên đăng nhập</label>
            <input
              type="text"
              id="username"
              className="form-input"
              placeholder="Nhập tên đăng nhập"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
            />
          </div>

          {/* Password Input */}
          <div className="form-group">
            <label htmlFor="password">Mật khẩu</label>
            <input
              type="password"
              id="password"
              className="form-input"
              placeholder="Nhập mật khẩu"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>

          {/* Login Button */}
          <button type="submit" className="login-btn" disabled={isLoading}>
            {isLoading ? 'Đang đăng nhập...' : 'Đăng nhập'}
          </button>
        </form>

        {/* Footer Links */}
        <div className="login-footer">
          <a href="#forgot" className="footer-link">Quên mật khẩu?</a>
          <span className="footer-divider">•</span>
          <a href="/register" className="footer-link">Chưa có tài khoản? <strong>Đăng ký ngay</strong></a>
        </div>
      </div>
    </div>
  );
}

export default Login;
