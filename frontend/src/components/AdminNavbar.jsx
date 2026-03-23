import "../styles/admin.css";

function AdminNavbar({ title = "Admin" }) {
  const user = JSON.parse(localStorage.getItem("user") || "{}");

  return (
    <div className="admin-navbar">
      <span className="admin-navbar-title">{title}</span>
      <div className="admin-navbar-user">
        <span>{user.username || "Admin"}</span>
        <div className="avatar">{(user.username || "A")[0].toUpperCase()}</div>
      </div>
    </div>
  );
}

export default AdminNavbar;
