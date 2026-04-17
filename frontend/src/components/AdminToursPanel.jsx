import { useEffect, useMemo, useState } from "react";
import { LuCheck, LuLoaderCircle, LuPencilLine, LuPlus, LuTrash2, LuX } from "react-icons/lu";
import { createTour, getAllToursAdmin, updateTour, updateTourStatus } from "../services/tourService";

const emptyForm = {
  title: "",
  description: "",
  coverImage: "",
  estimatedDurationMinutes: 60,
};

export default function AdminToursPanel() {
  const [tours, setTours] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);
  const [isSaving, setIsSaving] = useState(false);

  const [editingTour, setEditingTour] = useState(null); // full tour object or null for create
  const [form, setForm] = useState(emptyForm);

  const isEditing = Boolean(editingTour?.id);

  const sortedTours = useMemo(() => {
    return [...tours].sort((a, b) => (b?.id ?? 0) - (a?.id ?? 0));
  }, [tours]);

  const load = async () => {
    setLoading(true);
    setError(null);
    setSuccess(null);
    try {
      const data = await getAllToursAdmin();
      setTours(Array.isArray(data) ? data : []);
    } catch (e) {
      setTours([]);
      setError(e?.message || "Không thể tải danh sách tour");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const startCreate = () => {
    setEditingTour({ id: null });
    setForm(emptyForm);
    setError(null);
    setSuccess(null);
  };

  const startEdit = (tour) => {
    setEditingTour(tour);
    setForm({
      title: tour?.title || "",
      description: tour?.description || "",
      coverImage: tour?.coverImage || "",
      estimatedDurationMinutes: Number(tour?.estimatedDurationMinutes ?? 60),
    });
    setError(null);
    setSuccess(null);
  };

  const cancelEdit = () => {
    setEditingTour(null);
    setForm(emptyForm);
    setError(null);
    setSuccess(null);
  };

  const onChange = (key, value) => setForm((prev) => ({ ...prev, [key]: value }));

  const save = async () => {
    setIsSaving(true);
    setError(null);
    setSuccess(null);
    try {
      if (!form.title?.trim()) {
        setError("Vui lòng nhập tên tour");
        return;
      }

      const payload = {
        title: form.title.trim(),
        description: form.description?.trim() || null,
        coverImage: form.coverImage?.trim() || null,
        estimatedDurationMinutes: Number(form.estimatedDurationMinutes || 0),
      };

      if (isEditing) await updateTour(editingTour.id, payload);
      else await createTour(payload);

      setSuccess(isEditing ? "Cập nhật tour thành công" : "Tạo tour thành công");
      cancelEdit();
      await load();
    } catch (e) {
      setError(e?.message || "Lưu tour thất bại");
    } finally {
      setIsSaving(false);
    }
  };

  const toggleActive = async (tour) => {
    setError(null);
    setSuccess(null);
    try {
      await updateTourStatus(tour.id, !tour.isActive);
      setSuccess(!tour.isActive ? "Kích hoạt tour thành công" : "Ẩn tour thành công");
      await load();
    } catch (e) {
      setError(e?.message || "Cập nhật trạng thái thất bại");
    }
  };

  return (
    <div className="table-card shell-card">
      <div className="section-heading">
        <div>
          <p className="section-eyebrow">Tour management</p>
          <h2 className="section-title">Quản lý Tour</h2>
        </div>
        <button type="button" className="ghost-action" onClick={load} disabled={loading}>
          {loading ? <LuLoaderCircle size={16} className="spin" /> : <LuCheck size={16} />}
          <span>Làm mới</span>
        </button>
      </div>

      <div style={{ display: "flex", justifyContent: "space-between", gap: 12, flexWrap: "wrap", marginBottom: 12 }}>
        <div style={{ color: "#667085", fontSize: 14 }}>
          User chỉ thấy tour đang <strong>Available</strong> (IsActive = true).
        </div>
        {!editingTour && (
          <button type="button" className="btn btn-primary" onClick={startCreate}>
            <LuPlus size={16} />
            <span>Tạo tour</span>
          </button>
        )}
      </div>

      {error && <div className="admin-feedback admin-feedback-error">{error}</div>}
      {success && <div className="admin-feedback admin-feedback-success">{success}</div>}

      {editingTour && (
        <div style={{ border: "1px solid #EAECF0", borderRadius: 12, padding: 14, marginBottom: 14, background: "#fff" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 12 }}>
            <div style={{ fontWeight: 650 }}>
              {isEditing ? `Chỉnh sửa tour #${editingTour.id}` : "Tạo tour mới"}
            </div>
            <button type="button" className="ghost-action" onClick={cancelEdit} disabled={isSaving}>
              <LuX size={16} />
              <span>Đóng</span>
            </button>
          </div>

          <div className="admin-grid" style={{ gridTemplateColumns: "repeat(2, minmax(0, 1fr))", gap: 12 }}>
            <div className="admin-field">
              <label>Tên tour</label>
              <input value={form.title} onChange={(e) => onChange("title", e.target.value)} placeholder="VD: Food Tour Q1" />
            </div>
            <div className="admin-field">
              <label>Thời lượng (phút)</label>
              <input
                type="number"
                min={1}
                value={form.estimatedDurationMinutes}
                onChange={(e) => onChange("estimatedDurationMinutes", e.target.value)}
              />
            </div>
            <div className="admin-field" style={{ gridColumn: "1 / -1" }}>
              <label>Mô tả</label>
              <textarea value={form.description} onChange={(e) => onChange("description", e.target.value)} rows={3} />
            </div>
            <div className="admin-field" style={{ gridColumn: "1 / -1" }}>
              <label>Cover image URL</label>
              <input value={form.coverImage} onChange={(e) => onChange("coverImage", e.target.value)} placeholder="https://..." />
            </div>
          </div>

          <div style={{ display: "flex", justifyContent: "flex-end", gap: 10, marginTop: 12 }}>
            <button type="button" className="ghost-action" onClick={cancelEdit} disabled={isSaving}>
              <span>Hủy</span>
            </button>
            <button type="button" className="btn btn-primary" onClick={save} disabled={isSaving}>
              {isSaving ? <LuLoaderCircle size={16} className="spin" /> : <LuCheck size={16} />}
              <span>{isEditing ? "Lưu thay đổi" : "Tạo tour"}</span>
            </button>
          </div>
        </div>
      )}

      {loading ? (
        <div className="admin-empty-state">
          <div className="admin-empty-state-icon">
            <LuLoaderCircle size={26} className="spin" />
          </div>
          <h3>Đang tải tour</h3>
          <p>Danh sách tour đang được đồng bộ từ backend.</p>
        </div>
      ) : sortedTours.length === 0 ? (
        <div className="admin-empty-state">
          <div className="admin-empty-state-icon">
            <LuTrash2 size={26} />
          </div>
          <h3>Chưa có tour nào</h3>
          <p>Tạo tour mới để user có thể bắt đầu tham quan.</p>
        </div>
      ) : (
        <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
          {sortedTours.map((tour) => (
            <div
              key={tour.id}
              style={{
                border: "1px solid #EAECF0",
                borderRadius: 12,
                padding: 14,
                background: "#fff",
                display: "flex",
                justifyContent: "space-between",
                gap: 12,
              }}
            >
              <div style={{ minWidth: 0 }}>
                <div style={{ display: "flex", alignItems: "center", gap: 10, flexWrap: "wrap" }}>
                  <div style={{ fontWeight: 700, fontSize: 16 }}>
                    #{tour.id} · {tour.title}
                  </div>
                  <span
                    style={{
                      fontSize: 12,
                      fontWeight: 700,
                      padding: "4px 10px",
                      borderRadius: 999,
                      background: tour.isActive ? "#E7F9EF" : "#FEE4E2",
                      color: tour.isActive ? "#067647" : "#B42318",
                      display: "inline-flex",
                      alignItems: "center",
                      gap: 6,
                    }}
                  >
                    {tour.isActive ? <LuCheck size={14} /> : <LuX size={14} />}
                    {tour.isActive ? "Available" : "Hidden"}
                  </span>
                </div>
                <div style={{ color: "#667085", fontSize: 13, marginTop: 6 }}>
                  ⏱ {tour.estimatedDurationMinutes} phút · 📍 {tour.locationCount} địa điểm
                </div>
                {tour.description && (
                  <div style={{ color: "#475467", fontSize: 14, marginTop: 8, whiteSpace: "pre-wrap" }}>
                    {tour.description}
                  </div>
                )}
              </div>

              <div style={{ display: "flex", gap: 8, alignItems: "flex-start", flexWrap: "wrap" }}>
                <button type="button" className="ghost-action" onClick={() => startEdit(tour)}>
                  <LuPencilLine size={16} />
                  <span>Sửa</span>
                </button>
                <button type="button" className="ghost-action" onClick={() => toggleActive(tour)}>
                  {tour.isActive ? <LuX size={16} /> : <LuCheck size={16} />}
                  <span>{tour.isActive ? "Ẩn" : "Kích hoạt"}</span>
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
