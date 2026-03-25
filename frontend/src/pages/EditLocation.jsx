import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { LuLoaderCircle, LuPlay, LuSave, LuSquare, LuVolume2 } from "react-icons/lu";
import AdminNavbar, { AdminSidebar } from "../components/AdminNavbar";
import { getAllLocations, updateLocation } from "../services/locationService";
import { speak, stop } from "../services/ttsService";
import "../styles/admin.css";

const DEFAULT_TEXTS = {
  textVi: (name) =>
    `Chào mừng bạn đến với ${name}! Đây là một trong những địa điểm ẩm thực nổi tiếng tại Phố Ẩm thực Vĩnh Khánh. Nơi đây nổi bật với món ăn đặc trưng, hương vị đậm đà và không khí gần gũi. Hãy dừng chân và thưởng thức trải nghiệm ẩm thực đặc sắc tại đây nhé!`,
  textEn: (name) =>
    `Welcome to ${name}! This is one of the most famous food spots at Vinh Khanh Food Street. Known for its authentic local dishes, rich flavors, and cozy atmosphere. Stop by and enjoy the delicious food here!`,
  textZh: (name) =>
    `欢迎来到${name}！这是永庆美食街最著名的美食地点之一。这里以地道的地方风味、浓郁的口感和温馨的氛围而闻名。欢迎停下来，在这里享受一段美味体验！`,
  textDe: (name) =>
    `Willkommen bei ${name}! Dies ist einer der bekanntesten kulinarischen Orte in der Vinh Khanh Essensstraße. Bekannt für authentische lokale Gerichte, kräftige Aromen und eine gemütliche Atmosphäre. Schauen Sie vorbei und genießen Sie hier ein besonderes kulinarisches Erlebnis!`,
};

const TEXT_FIELDS = [
  { key: "textVi", lang: "vi-VN", label: "Tiếng Việt" },
  { key: "textEn", lang: "en-US", label: "English" },
  { key: "textZh", lang: "zh-CN", label: "中文" },
  { key: "textDe", lang: "de-DE", label: "Deutsch" },
];

function EditLocation() {
  const navigate = useNavigate();
  const [user, setUser] = useState(null);
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [locations, setLocations] = useState([]);
  const [selected, setSelected] = useState(null);
  const [form, setForm] = useState({});
  const [saved, setSaved] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [playingKey, setPlayingKey] = useState("");

  useEffect(() => {
    const userString = localStorage.getItem("user");
    if (!userString) {
      navigate("/login");
      return;
    }

    try {
      const userData = JSON.parse(userString);
      const role = (userData?.role || "").toLowerCase();
      if (role !== "admin") {
        navigate("/");
        return;
      }

      setUser(userData);
      setIsAuthorized(true);
      getAllLocations().then((data) => setLocations(Array.isArray(data) ? data : []));
    } catch {
      navigate("/login");
    }

    return () => stop();
  }, [navigate]);

  const handleSelect = (location) => {
    setSelected(location);
    setSaved(false);
    setPlayingKey("");
    stop();
    setForm({
      textVi: location.textVi || DEFAULT_TEXTS.textVi(location.name),
      textEn: location.textEn || DEFAULT_TEXTS.textEn(location.name),
      textZh: location.textZh || DEFAULT_TEXTS.textZh(location.name),
      textDe: location.textDe || DEFAULT_TEXTS.textDe(location.name),
    });
  };

  const handleSave = async () => {
    if (!selected) return;

    setIsSaving(true);

    try {
      await updateLocation(selected.id, { ...selected, ...form });
      setSaved(true);
      setLocations((current) =>
        current.map((item) => (item.id === selected.id ? { ...item, ...form } : item))
      );
      setSelected((current) => (current ? { ...current, ...form } : current));
    } finally {
      setIsSaving(false);
    }
  };

  const handlePreview = async (fieldKey, lang) => {
    const text = form[fieldKey];
    if (!text) return;

    if (playingKey === fieldKey) {
      stop();
      setPlayingKey("");
      return;
    }

    stop();
    setPlayingKey(fieldKey);

    try {
      await speak(text, lang);
    } finally {
      setPlayingKey((current) => (current === fieldKey ? "" : current));
    }
  };

  if (!isAuthorized) return null;

  return (
    <div className="admin-shell">
      <AdminSidebar user={user} activeKey="tts" />

      <main className="admin-main">
        <AdminNavbar
          title="Chỉnh sửa văn bản thuyết minh"
          subtitle="Refine multilingual audio descriptions and preview narration before publishing."
          user={user}
        />

        <div className="admin-content">
          <div className="user-manager">
            <div className="table-card shell-card" style={{ padding: 0, overflow: "hidden" }}>
              <div className="section-heading" style={{ padding: "24px 24px 0" }}>
                <div>
                  <p className="section-eyebrow">Location list</p>
                  <h2 className="section-title">Chọn địa điểm</h2>
                </div>
              </div>

              {locations.length === 0 ? (
                <div className="admin-empty-state compact-empty-state">
                  <div className="admin-empty-state-icon">
                    <LuLoaderCircle size={24} className="spin" />
                  </div>
                  <h3>Đang tải địa điểm</h3>
                  <p>Dữ liệu địa điểm sẽ xuất hiện tại đây ngay khi backend phản hồi.</p>
                </div>
              ) : (
                <div style={{ padding: "12px 0" }}>
                  {locations.map((location) => (
                    <button
                      key={location.id}
                      type="button"
                      className={`sidebar-link ${selected?.id === location.id ? "active" : ""}`}
                      style={{ margin: "0 14px 8px" }}
                      onClick={() => handleSelect(location)}
                    >
                      <LuVolume2 size={18} />
                      <span>{location.name}</span>
                    </button>
                  ))}
                </div>
              )}
            </div>

            {selected ? (
              <div className="location-editor shell-card">
                <div className="section-heading">
                  <div>
                    <p className="section-eyebrow">Narration editor</p>
                    <h2 className="section-title">{selected.name}</h2>
                  </div>
                </div>

                <div className="location-form">
                  {TEXT_FIELDS.map(({ key, lang, label }) => (
                    <div className="form-group" key={key}>
                      <label
                        htmlFor={key}
                        style={{
                          display: "flex",
                          alignItems: "center",
                          justifyContent: "space-between",
                          gap: 12,
                        }}
                      >
                        <span>{label}</span>
                        <button
                          type="button"
                          className="action-btn"
                          onClick={() => handlePreview(key, lang)}
                        >
                          {playingKey === key ? <LuSquare size={14} /> : <LuPlay size={14} />}
                          <span>{playingKey === key ? "Dừng nghe thử" : "Nghe thử"}</span>
                        </button>
                      </label>

                      <textarea
                        id={key}
                        rows={5}
                        value={form[key] || ""}
                        onChange={(event) =>
                          setForm((current) => ({ ...current, [key]: event.target.value }))
                        }
                      />
                    </div>
                  ))}

                  <div className="location-form-actions">
                    <button className="btn btn-primary" onClick={handleSave} disabled={isSaving}>
                      {isSaving ? <LuLoaderCircle size={16} className="spin" /> : <LuSave size={16} />}
                      <span>Lưu thay đổi</span>
                    </button>
                    <button
                      className="btn action-btn-muted"
                      onClick={() => {
                        stop();
                        setPlayingKey("");
                      }}
                    >
                      <LuSquare size={16} />
                      <span>Dừng</span>
                    </button>
                    {saved && <span style={{ color: "#0f766e", fontSize: "14px" }}>Đã lưu!</span>}
                  </div>
                </div>
              </div>
            ) : (
              <div className="table-card shell-card admin-empty-state">
                <div className="admin-empty-state-icon">
                  <LuVolume2 size={24} />
                </div>
                <h3>Chọn một địa điểm để chỉnh sửa</h3>
                <p>Nội dung thuyết minh đa ngôn ngữ và nút nghe thử sẽ hiển thị tại đây.</p>
              </div>
            )}
          </div>
        </div>
      </main>
    </div>
  );
}

export default EditLocation;
