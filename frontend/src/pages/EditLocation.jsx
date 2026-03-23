import { useEffect, useState } from "react";
import Sidebar from "../components/Sidebar";
import AdminNavbar from "../components/AdminNavbar";
import { getAllLocations, updateLocation } from "../services/locationService";
import { speak, stop, LANGUAGES } from "../services/ttsService";
import "../styles/admin.css";

const DEFAULT_TEXTS = {
  textVi: (name) => `Chào mừng bạn đến với ${name}! Đây là một trong những địa điểm ẩm thực nổi tiếng tại Phố Ẩm Thực Vĩnh Khánh. Nơi đây nổi tiếng với các món ăn đặc sản địa phương, hương vị đậm đà và không gian ấm cúng. Hãy dừng chân và thưởng thức những món ngon tại đây nhé!`,
  textEn: (name) => `Welcome to ${name}! This is one of the most famous food spots at Vinh Khanh Food Street. Known for its authentic local dishes, rich flavors, and cozy atmosphere. Stop by and enjoy the delicious food here!`,
  textZh: (name) => `欢迎来到${name}！这是永庆美食街最著名的美食地点之一。以其正宗的地方菜肴、浓郁的口味和温馨的氛围而闻名。请停下来，在这里享用美食吧！`,
  textDe: (name) => `Willkommen bei ${name}! Dies ist einer der bekanntesten Essensplätze in der Vinh Khanh Essensstraße. Bekannt für seine authentischen lokalen Gerichte, reichen Aromen und gemütliche Atmosphäre. Halten Sie an und genießen Sie das köstliche Essen hier!`,
};

function EditLocation() {
  const [locations, setLocations] = useState([]);
  const [selected, setSelected] = useState(null);
  const [form, setForm] = useState({});
  const [previewLang, setPreviewLang] = useState("vi-VN");
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    getAllLocations().then((data) => setLocations(data));
  }, []);

  const handleSelect = (loc) => {
    setSelected(loc);
    setSaved(false);
    setForm({
      textVi: loc.textVi || DEFAULT_TEXTS.textVi(loc.name),
      textEn: loc.textEn || DEFAULT_TEXTS.textEn(loc.name),
      textZh: loc.textZh || DEFAULT_TEXTS.textZh(loc.name),
      textDe: loc.textDe || DEFAULT_TEXTS.textDe(loc.name),
    });
  };

  const handleSave = async () => {
    await updateLocation(selected.id, { ...selected, ...form });
    setSaved(true);
    setLocations((prev) =>
      prev.map((l) => (l.id === selected.id ? { ...l, ...form } : l))
    );
  };

  const getTextByLang = () => {
    const map = { "vi-VN": form.textVi, "en-US": form.textEn, "zh-CN": form.textZh, "de-DE": form.textDe };
    return map[previewLang] || "";
  };

  const fields = [
    { key: "textVi", lang: "vi-VN", label: "🇻🇳 Tiếng Việt" },
    { key: "textEn", lang: "en-US", label: "🇺🇸 English" },
    { key: "textZh", lang: "zh-CN", label: "🇨🇳 中文" },
    { key: "textDe", lang: "de-DE", label: "🇩🇪 Deutsch" },
  ];

  return (
    <div className="layout">
      <Sidebar />
      <div className="main">
        <AdminNavbar title="Chỉnh sửa văn bản thuyết minh" />
        <div className="content">
          <div style={{ display: "grid", gridTemplateColumns: "280px 1fr", gap: "20px" }}>

            {/* Location list */}
            <div className="card" style={{ padding: 0, overflow: "hidden" }}>
              <div className="card-title" style={{ padding: "16px 20px", marginBottom: 0 }}>
                📍 Chọn địa điểm
              </div>
              {locations.length === 0 ? (
                <p style={{ padding: "20px", color: "#aaa", fontSize: "14px" }}>Chưa có địa điểm nào</p>
              ) : (
                locations.map((loc) => (
                  <div
                    key={loc.id}
                    onClick={() => handleSelect(loc)}
                    style={{
                      padding: "12px 20px",
                      cursor: "pointer",
                      borderBottom: "1px solid #f0f0f0",
                      background: selected?.id === loc.id ? "#e3f2fd" : "white",
                      color: selected?.id === loc.id ? "#1565c0" : "#333",
                      fontWeight: selected?.id === loc.id ? "bold" : "normal",
                      fontSize: "14px",
                    }}
                  >
                    {loc.name}
                  </div>
                ))
              )}
            </div>

            {/* Editor */}
            {selected ? (
              <div>
                <div className="card">
                  <div className="card-title">✏️ {selected.name}</div>

                  {fields.map(({ key, lang, label }) => (
                    <div className="form-group" key={key}>
                      <label>
                        {label}
                        <button
                          className="btn btn-primary"
                          style={{ float: "right", padding: "3px 10px", fontSize: "12px" }}
                          onClick={() => { stop(); speak(form[key], lang); }}
                        >
                          ▶ Nghe thử
                        </button>
                      </label>
                      <textarea
                        rows={4}
                        value={form[key] || ""}
                        onChange={(e) => setForm({ ...form, [key]: e.target.value })}
                      />
                    </div>
                  ))}

                  <div style={{ display: "flex", gap: "10px", alignItems: "center" }}>
                    <button className="btn btn-success" onClick={handleSave}>💾 Lưu thay đổi</button>
                    <button className="btn" style={{ background: "#f5f5f5", color: "#555", border: "1px solid #ddd" }} onClick={stop}>⏹ Dừng</button>
                    {saved && <span style={{ color: "#43a047", fontSize: "14px" }}>✅ Đã lưu!</span>}
                  </div>
                </div>
              </div>
            ) : (
              <div className="card" style={{ display: "flex", alignItems: "center", justifyContent: "center", color: "#aaa", fontSize: "15px" }}>
                👈 Chọn một địa điểm để chỉnh sửa văn bản thuyết minh
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

export default EditLocation;
