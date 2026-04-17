import { useNavigate } from "react-router-dom";
import "../styles/app.css";

function Navbar() {
  const navigate = useNavigate();
  return (
    <div className="navbar">
      <div className="navbar-brand">
        🎧 <span>Food Tour Audio Guide</span>
        <span style={{ opacity: 0.6, fontWeight: "normal", fontSize: "14px" }}>
          - Phố Ẩm Thực Vĩnh Khánh
        </span>
      </div>
      <div className="navbar-links">
        <a href="/" className="nav-icon" title="Trang chủ">🏠</a>
        <a href="/tours" className="nav-icon" title="Khám phá Tour">🗺️ Tour</a>
      </div>
    </div>
  );
}

export default Navbar;
