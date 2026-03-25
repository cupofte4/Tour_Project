import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { register } from '../services/authService';
import '../styles/login.css';

function Register() {
  const [fullName, setFullName] = useState('');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [errorMsg, setErrorMsg] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();

  const handleRegister = async (e) => {
    e.preventDefault();
    setErrorMsg('');
    setIsLoading(true);

    try {
      // Validation
      if (!fullName.trim()) {
        setErrorMsg('Vui lòng nhập tên đầy đủ');
        setIsLoading(false);
        return;
      }

      if (!username.trim()) {
        setErrorMsg('Vui lòng nhập tên đăng nhập');
        setIsLoading(false);
        return;
      }

      // Username validation (minimum 3 characters)
      if (username.length < 3) {
        setErrorMsg('Tên đăng nhập phải có ít nhất 3 ký tự');
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

      if (!confirmPassword.trim()) {
        setErrorMsg('Vui lòng xác nhận mật khẩu');
        setIsLoading(false);
        return;
      }

      // Password match validation
      if (password !== confirmPassword) {
        setErrorMsg('Mật khẩu xác nhận không khớp');
        setIsLoading(false);
        return;
      }

      // Call API register
      const result = await register(fullName, username, password);

      if (result) {
        // Navigate to login page with success message
        navigate('/login', { 
          state: { message: 'Đăng ký thành công! Vui lòng đăng nhập.' } 
        });
      } else {
        setErrorMsg('Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác!');
      }
    } catch (error) {
      setErrorMsg('Có lỗi xảy ra. Vui lòng thử lại!');
      console.error('Register error:', error);
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
          <p className="login-subtitle">Đăng ký tài khoản mới</p>
        </div>

        {/* Error Message */}
        {errorMsg && (
          <div className="error-msg">
            <span>⚠️</span> {errorMsg}
          </div>
        )}

        {/* Form */}
        <form onSubmit={handleRegister} className="login-form">
          {/* Full Name Input */}
          <div className="form-group">
            <label htmlFor="fullName">Tên đầy đủ</label>
            <input
              type="text"
              id="fullName"
              className="form-input"
              placeholder="Nhập tên của bạn"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
            />
          </div>

          {/* Username Input */}
          <div className="form-group">
            <label htmlFor="username">Tên đăng nhập</label>
            <input
              type="text"
              id="username"
              className="form-input"
              placeholder="Nhập tên đăng nhập (tối thiểu 3 ký tự)"
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
              placeholder="Nhập mật khẩu (tối thiểu 6 ký tự)"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>

          {/* Confirm Password Input */}
          <div className="form-group">
            <label htmlFor="confirmPassword">Xác nhận mật khẩu</label>
            <input
              type="password"
              id="confirmPassword"
              className="form-input"
              placeholder="Nhập lại mật khẩu"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
            />
          </div>

          {/* Register Button */}
          <button type="submit" className="login-btn" disabled={isLoading}>
            {isLoading ? 'Đang đăng ký...' : 'Đăng ký'}
          </button>
        </form>

        {/* Footer Links */}
        <div className="login-footer">
          <span style={{ fontSize: '13px', color: '#666' }}>Đã có tài khoản?</span>
          <a href="/login" className="footer-link"><strong>Đăng nhập ngay</strong></a>
        </div>
      </div>
    </div>
  );
}

export default Register;
