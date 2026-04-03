import "../styles/app.css";

function Navbar() {
  return (
    <div className="navbar">
      <div className="navbar-brand">
        🎧 <span>Travel Audio Guide</span>
        <span style={{ opacity: 0.6, fontWeight: "normal", fontSize: "14px" }}>
          - Phố Ẩm Thực Vĩnh Khánh
        </span>
      </div>
    </div>
  );
}

export default Navbar;
