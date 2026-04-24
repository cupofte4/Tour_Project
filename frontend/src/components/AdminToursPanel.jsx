import { useEffect, useMemo, useState } from "react";
import {
  LuCheck,
  LuLoaderCircle,
  LuMapPin,
  LuPencilLine,
  LuPlus,
  LuTrash2,
  LuX,
} from "react-icons/lu";
import { getAllLocations } from "../services/locationService";
import {
  assignLocationToTour,
  createTour,
  deleteTour,
  getAllToursAdmin,
  getTourManageById,
  removeLocationFromTour,
  updateTour,
  updateTourStatus,
} from "../services/tourService";

const emptyForm = {
  title: "",
  description: "",
  coverImage: "",
  estimatedDurationMinutes: 60,
};

export default function AdminToursPanel() {
  const [tours, setTours] = useState([]);
  const [allLocations, setAllLocations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);
  const [isSaving, setIsSaving] = useState(false);
  const [busyTourId, setBusyTourId] = useState(null);

  const [editingTour, setEditingTour] = useState(null);
  const [form, setForm] = useState(emptyForm);

  const [managingTourId, setManagingTourId] = useState(null);
  const [manageDetail, setManageDetail] = useState(null);
  const [manageLoading, setManageLoading] = useState(false);
  const [selectedLocationId, setSelectedLocationId] = useState("");

  const isEditing = Boolean(editingTour?.id);

  const sortedTours = useMemo(() => {
    return [...tours].sort((a, b) => (b?.id ?? 0) - (a?.id ?? 0));
  }, [tours]);

  const availableLocations = useMemo(() => {
    const mappedIds = new Set(
      (manageDetail?.locations ?? []).map((item) => Number(item?.locationId ?? item?.location?.id)),
    );
    return allLocations.filter((location) => !mappedIds.has(Number(location.id)));
  }, [allLocations, manageDetail]);

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

  const loadManageDetail = async (tourId) => {
    setManageLoading(true);
    try {
      const detail = await getTourManageById(tourId);
      setManageDetail(detail);
      const nextAvailable = allLocations.filter(
        (location) =>
          !(detail?.locations ?? []).some(
            (item) => Number(item?.locationId ?? item?.location?.id) === Number(location.id),
          ),
      );
      setSelectedLocationId(nextAvailable[0]?.id ? String(nextAvailable[0].id) : "");
    } catch (e) {
      setManageDetail(null);
      setError(e?.message || "Không thể tải danh sách POI của tour");
    } finally {
      setManageLoading(false);
    }
  };

  useEffect(() => {
    load();
    getAllLocations().then((data) => setAllLocations(Array.isArray(data) ? data : []));
  }, []);

  useEffect(() => {
    if (!manageDetail) return;
    const nextAvailable = availableLocations[0]?.id ? String(availableLocations[0].id) : "";
    if (!selectedLocationId || !availableLocations.some((item) => String(item.id) === selectedLocationId)) {
      setSelectedLocationId(nextAvailable);
    }
  }, [availableLocations, manageDetail, selectedLocationId]);

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
      if (managingTourId && isEditing && Number(managingTourId) === Number(editingTour.id)) {
        await loadManageDetail(editingTour.id);
      }
    } catch (e) {
      setError(e?.message || "Lưu tour thất bại");
    } finally {
      setIsSaving(false);
    }
  };

  const toggleActive = async (tour) => {
    setBusyTourId(tour.id);
    setError(null);
    setSuccess(null);
    try {
      await updateTourStatus(tour.id, !tour.isActive);
      setSuccess(!tour.isActive ? "Kích hoạt tour thành công" : "Ẩn tour thành công");
      await load();
      if (managingTourId === tour.id) {
        await loadManageDetail(tour.id);
      }
    } catch (e) {
      setError(e?.message || "Cập nhật trạng thái thất bại");
    } finally {
      setBusyTourId(null);
    }
  };

  const handleDelete = async (tour) => {
    const confirmed = window.confirm(`Xóa tour "${tour.title}" và toàn bộ liên kết POI khỏi tour này?`);
    if (!confirmed) return;

    setBusyTourId(tour.id);
    setError(null);
    setSuccess(null);
    try {
      await deleteTour(tour.id);
      if (managingTourId === tour.id) {
        setManagingTourId(null);
        setManageDetail(null);
        setSelectedLocationId("");
      }
      setSuccess("Xóa tour thành công");
      await load();
    } catch (e) {
      setError(e?.message || "Xóa tour thất bại");
    } finally {
      setBusyTourId(null);
    }
  };

  const handleOpenManage = async (tour) => {
    if (managingTourId === tour.id) {
      setManagingTourId(null);
      setManageDetail(null);
      setSelectedLocationId("");
      return;
    }

    setManagingTourId(tour.id);
    setError(null);
    setSuccess(null);
    await loadManageDetail(tour.id);
  };

  const handleAddLocation = async () => {
    if (!managingTourId || !selectedLocationId) return;

    setManageLoading(true);
    setError(null);
    setSuccess(null);
    try {
      const nextOrderIndex = manageDetail?.locations?.length ?? 0;
      await assignLocationToTour(managingTourId, Number(selectedLocationId), nextOrderIndex, false);
      setSuccess("Thêm POI vào tour thành công");
      await Promise.all([load(), loadManageDetail(managingTourId)]);
    } catch (e) {
      setError(e?.message || "Không thể thêm POI vào tour");
      setManageLoading(false);
    }
  };

  const handleRemoveLocation = async (locationId) => {
    if (!managingTourId) return;

    setManageLoading(true);
    setError(null);
    setSuccess(null);
    try {
      await removeLocationFromTour(managingTourId, locationId);
      setSuccess("Đã gỡ POI khỏi tour");
      await Promise.all([load(), loadManageDetail(managingTourId)]);
    } catch (e) {
      setError(e?.message || "Không thể gỡ POI khỏi tour");
      setManageLoading(false);
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
            <div key={tour.id} style={{ display: "grid", gap: 10 }}>
              <div
                style={{
                  border: "1px solid #EAECF0",
                  borderRadius: 12,
                  padding: 14,
                  background: "#fff",
                  display: "flex",
                  justifyContent: "space-between",
                  gap: 12,
                  flexWrap: "wrap",
                }}
              >
                <div style={{ minWidth: 0, flex: "1 1 320px" }}>
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
                  <button
                    type="button"
                    className="ghost-action"
                    onClick={() => handleOpenManage(tour)}
                    disabled={manageLoading && managingTourId === tour.id}
                  >
                    {manageLoading && managingTourId === tour.id ? <LuLoaderCircle size={16} className="spin" /> : <LuMapPin size={16} />}
                    <span>{managingTourId === tour.id ? "Đóng POI" : "Quản lý POI"}</span>
                  </button>
                  <button type="button" className="ghost-action" onClick={() => toggleActive(tour)} disabled={busyTourId === tour.id}>
                    {busyTourId === tour.id ? <LuLoaderCircle size={16} className="spin" /> : tour.isActive ? <LuX size={16} /> : <LuCheck size={16} />}
                    <span>{tour.isActive ? "Ẩn" : "Kích hoạt"}</span>
                  </button>
                  <button type="button" className="ghost-action" onClick={() => handleDelete(tour)} disabled={busyTourId === tour.id}>
                    {busyTourId === tour.id ? <LuLoaderCircle size={16} className="spin" /> : <LuTrash2 size={16} />}
                    <span>Xóa</span>
                  </button>
                </div>
              </div>

              {managingTourId === tour.id && (
                <div style={{ border: "1px solid #EAECF0", borderRadius: 12, padding: 14, background: "#FCFCFD" }}>
                  <div style={{ display: "flex", justifyContent: "space-between", gap: 12, flexWrap: "wrap", marginBottom: 12 }}>
                    <div>
                      <div style={{ fontWeight: 700, fontSize: 15 }}>POI trong tour</div>
                      <div style={{ color: "#667085", fontSize: 13, marginTop: 4 }}>
                        Thêm hoặc gỡ POI khỏi tour mà không ảnh hưởng tới bảng Locations.
                      </div>
                    </div>
                    <div style={{ color: "#667085", fontSize: 13 }}>
                      {manageDetail?.locations?.length ?? tour.locationCount} POI đang được gán
                    </div>
                  </div>

                  <div style={{ display: "flex", gap: 10, flexWrap: "wrap", marginBottom: 12 }}>
                    <select
                      value={selectedLocationId}
                      onChange={(e) => setSelectedLocationId(e.target.value)}
                      style={{
                        minWidth: 260,
                        flex: "1 1 260px",
                        border: "1px solid #D0D5DD",
                        borderRadius: 10,
                        padding: "10px 12px",
                        background: "#fff",
                      }}
                      disabled={manageLoading || availableLocations.length === 0}
                    >
                      {availableLocations.length === 0 ? (
                        <option value="">Không còn POI để thêm</option>
                      ) : (
                        availableLocations.map((location) => (
                          <option key={location.id} value={location.id}>
                            {location.name}
                          </option>
                        ))
                      )}
                    </select>
                    <button
                      type="button"
                      className="btn btn-primary"
                      onClick={handleAddLocation}
                      disabled={manageLoading || !selectedLocationId}
                    >
                      {manageLoading ? <LuLoaderCircle size={16} className="spin" /> : <LuPlus size={16} />}
                      <span>Thêm POI</span>
                    </button>
                  </div>

                  {manageLoading && !manageDetail ? (
                    <div style={{ color: "#667085", fontSize: 14 }}>Đang tải danh sách POI...</div>
                  ) : !manageDetail?.locations?.length ? (
                    <div className="admin-empty-state" style={{ margin: 0 }}>
                      <div className="admin-empty-state-icon">
                        <LuMapPin size={24} />
                      </div>
                      <h3>Tour chưa có POI</h3>
                      <p>Chọn một địa điểm từ danh sách phía trên để thêm vào tour.</p>
                    </div>
                  ) : (
                    <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
                      {manageDetail.locations.map((item) => {
                        const poi = item.location ?? {};
                        return (
                          <div
                            key={`${item.id}-${item.locationId}`}
                            style={{
                              border: "1px solid #EAECF0",
                              borderRadius: 10,
                              padding: 12,
                              background: "#fff",
                              display: "flex",
                              justifyContent: "space-between",
                              gap: 12,
                              flexWrap: "wrap",
                            }}
                          >
                            <div style={{ minWidth: 0, flex: "1 1 280px" }}>
                              <div style={{ fontWeight: 650 }}>
                                #{item.orderIndex + 1} · {poi.name}
                              </div>
                              <div style={{ color: "#667085", fontSize: 13, marginTop: 4 }}>
                                {poi.address || "Chưa cập nhật địa chỉ"}
                              </div>
                            </div>
                            <button
                              type="button"
                              className="ghost-action"
                              onClick={() => handleRemoveLocation(item.locationId)}
                              disabled={manageLoading}
                            >
                              {manageLoading ? <LuLoaderCircle size={16} className="spin" /> : <LuX size={16} />}
                              <span>Gỡ POI</span>
                            </button>
                          </div>
                        );
                      })}
                    </div>
                  )}
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
