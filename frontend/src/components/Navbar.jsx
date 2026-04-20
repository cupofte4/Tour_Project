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
    </div>
  );
}

export default Navbar;
