import { useEffect, useState } from "react";
import Sidebar from "../components/Sidebar";
import AdminNavbar from "../components/AdminNavbar";
import { getAllLocations } from "../services/locationService";
import "../styles/admin.css";

function Admin() {
  const [locations, setLocations] = useState([]);

  useEffect(() => {
    getAllLocations().then((data) => setLocations(data));
  }, []);

  return (
    <div className="layout">
      <Sidebar />
      <div className="main">
        <AdminNavbar title="Dashboard" />
        <div className="content">

          <div className="stats-grid">
            <div className="stat-card">
              <div className="stat-icon blue">📍</div>
              <div className="stat-info">
                <p>Tổng địa điểm</p>
                <h3>{locations.length}</h3>
              </div>
            </div>
            <div className="stat-card">
              <div className="stat-icon green">🎵</div>
              <div className="stat-info">
                <p>Audio files</p>
                <h3>{locations.filter((l) => l.audio).length}</h3>
              </div>
            </div>
            <div className="stat-card">
              <div className="stat-icon orange">🖼️</div>
              <div className="stat-info">
                <p>Có hình ảnh</p>
                <h3>{locations.filter((l) => l.image).length}</h3>
              </div>
            </div>
          </div>

          <div className="card">
            <div className="card-title">📋 Danh sách địa điểm</div>
            <table>
              <thead>
                <tr>
                  <th>#</th>
                  <th>Tên địa điểm</th>
                  <th>Mô tả</th>
                  <th>Tọa độ</th>
                </tr>
              </thead>
              <tbody>
                {locations.length === 0 ? (
                  <tr>
                    <td colSpan="4" style={{ textAlign: "center", color: "#aaa", padding: "30px" }}>
                      Chưa có địa điểm nào
                    </td>
                  </tr>
                ) : (
                  locations.map((loc, i) => (
                    <tr key={loc.id}>
                      <td>{i + 1}</td>
                      <td><strong>{loc.name}</strong></td>
                      <td style={{ color: "#666", maxWidth: "300px" }}>{loc.description}</td>
                      <td style={{ fontFamily: "monospace", fontSize: "12px" }}>
                        {loc.latitude}, {loc.longitude}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>

        </div>
      </div>
    </div>
  );
}

export default Admin;
