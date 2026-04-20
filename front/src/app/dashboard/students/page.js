"use client";

import { useState, useEffect, useCallback } from "react";
import {
  Users, Plus, Search, Trash2, Edit, X, ChevronRight, ChevronLeft,
  User, Phone, MapPin, Calendar, School, Upload, AlertCircle, Check,
  Loader2, RefreshCw, Eye,
} from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";
// الصور تُخزَّن في wwwroot/uploads/... وتُخدَّم من جذر السيرفر
// imagePath يرجع بالشكل: "uploads/students/file.jpg" (بدون / في البداية)
const IMG_BASE = process.env.NEXT_PUBLIC_API_URL?.replace("/api/v1", "") ?? "https://localhost:7045";
function imgUrl(path) {
  if (!path) return null;
  // أضف / في البداية إذا لم تكن موجودة
  return `${IMG_BASE}/${path.replace(/^\//, "")}`;
}

// ─── helpers ───────────────────────────────────────────────────────────────
function getToken() {
  return typeof window !== "undefined" ? sessionStorage.getItem("accessToken") : null;
}
function authHdr(json = true) {
  const h = { Authorization: `Bearer ${getToken()}` };
  if (json) h["Content-Type"] = "application/json";
  return h;
}
function formatDate(d) {
  if (!d) return "—";
  try { return new Date(d).toLocaleDateString("ar-IQ"); } catch { return d; }
}

// ─── Toast ─────────────────────────────────────────────────────────────────
function Toast({ toasts }) {
  return (
    <div className="stu-toast-stack">
      {toasts.map((t) => (
        <div key={t.id} className={`stu-toast stu-toast-${t.type}`}>
          {t.type === "success" ? <Check size={15} /> : <AlertCircle size={15} />}
          {t.msg}
        </div>
      ))}
    </div>
  );
}

// ─── Modal ─────────────────────────────────────────────────────────────────
function Modal({ show, title, onClose, children, wide }) {
  useEffect(() => {
    const fn = (e) => e.key === "Escape" && onClose();
    if (show) window.addEventListener("keydown", fn);
    return () => window.removeEventListener("keydown", fn);
  }, [show, onClose]);

  if (!show) return null;
  return (
    <div className="stu-modal-overlay" onClick={onClose}>
      <div
        className="stu-modal-card"
        style={wide ? { maxWidth: 640 } : {}}
        onClick={(e) => e.stopPropagation()}
      >
        <div className="stu-modal-header">
          <span className="stu-modal-title">{title}</span>
          <button className="stu-modal-close" onClick={onClose}>
            <X size={16} />
          </button>
        </div>
        <div className="stu-modal-body">{children}</div>
      </div>
    </div>
  );
}

// ─── StudentForm ────────────────────────────────────────────────────────────
function StudentForm({ initial, classes, onSubmit, loading }) {
  const [form, setForm] = useState(
    initial ?? {
      firstName: "", lastName: "", city: "", phone: "",
      birthDay: "", classId: "", email: "", password: "",
    }
  );
  const [img, setImg] = useState(null);
  const [preview, setPreview] = useState(initial?.imagePath ? imgUrl(initial.imagePath) : null);
  const isEdit = !!initial;

  function set(k, v) { setForm((p) => ({ ...p, [k]: v })); }

  function pickImg(e) {
    const f = e.target.files[0];
    if (!f) return;
    setImg(f);
    setPreview(URL.createObjectURL(f));
  }

  function handleSubmit(e) {
    e.preventDefault();
    const fd = new FormData();
    fd.append("FirstName", form.firstName);
    fd.append("LastName", form.lastName);
    fd.append("City", form.city);
    fd.append("Phone", form.phone);
    fd.append("BirthDay", form.birthDay);
    fd.append("ClassId", form.classId);
    if (!isEdit) {
      fd.append("Email", form.email);
      fd.append("Password", form.password);
    }
    if (img) fd.append("Image", img);
    onSubmit(fd);
  }

  return (
    <form className="stu-form" onSubmit={handleSubmit}>
      {/* Avatar picker */}
      <div className="stu-avatar-picker">
        <div className="stu-avatar-preview">
          {preview
            ? <img src={preview} alt="صورة الطالب" />
            : <User size={36} color="var(--gold-dark)" />}
        </div>
        <label className="stu-upload-btn">
          <Upload size={14} /> اختر صورة
          <input type="file" accept="image/*" hidden onChange={pickImg} />
        </label>
      </div>

      <div className="stu-form-grid">
        <div className="stu-fg">
          <label>الاسم الأول *</label>
          <input value={form.firstName} onChange={(e) => set("firstName", e.target.value)} required placeholder="محمد" />
        </div>
        <div className="stu-fg">
          <label>الاسم الأخير *</label>
          <input value={form.lastName} onChange={(e) => set("lastName", e.target.value)} required placeholder="أحمد" />
        </div>
        <div className="stu-fg">
          <label>المدينة *</label>
          <input value={form.city} onChange={(e) => set("city", e.target.value)} required placeholder="بغداد" />
        </div>
        <div className="stu-fg">
          <label>رقم الهاتف *</label>
          <input value={form.phone} onChange={(e) => set("phone", e.target.value)} required placeholder="07X XXXX XXXX" dir="ltr" />
        </div>
        <div className="stu-fg">
          <label>تاريخ الميلاد *</label>
          <input type="date" value={form.birthDay} onChange={(e) => set("birthDay", e.target.value)} required />
        </div>
        <div className="stu-fg">
          <label>الفصل *</label>
          <select value={form.classId} onChange={(e) => set("classId", e.target.value)} required>
            <option value="">— اختر الفصل —</option>
            {classes.map((c) => (
              <option key={c.id} value={c.id}>{c.classNumber}</option>
            ))}
          </select>
        </div>
        {!isEdit && (
          <>
            <div className="stu-fg">
              <label>البريد الإلكتروني *</label>
              <input type="email" value={form.email} onChange={(e) => set("email", e.target.value)} required placeholder="student@school.com" dir="ltr" />
            </div>
            <div className="stu-fg">
              <label>كلمة المرور *</label>
              <input type="password" value={form.password} onChange={(e) => set("password", e.target.value)} required placeholder="••••••••" />
            </div>
          </>
        )}
      </div>

      <button type="submit" className="btn-gold stu-submit-btn" disabled={loading}>
        {loading ? <Loader2 size={16} className="spin" /> : isEdit ? <Edit size={16} /> : <Plus size={16} />}
        {loading ? "جارٍ الحفظ..." : isEdit ? "حفظ التعديلات" : "إضافة الطالب"}
      </button>
    </form>
  );
}

// ─── DetailModal ────────────────────────────────────────────────────────────
function DetailModal({ student, onClose }) {
  return (
    <Modal show={!!student} title="تفاصيل الطالب" onClose={onClose}>
      {student && (
        <div className="stu-detail">
          <div className="stu-detail-avatar">
            {student.imagePath
              ? <img src={imgUrl(student.imagePath)} alt={student.firstName} />
              : <User size={48} color="var(--gold-dark)" />}
          </div>
          <h3 className="stu-detail-name">{student.firstName} {student.lastName}</h3>
          <span className="stu-badge">{student.className || "—"}</span>
          <div className="stu-detail-grid">
            <div className="stu-detail-item"><Phone size={15} /> {student.phone || "—"}</div>
            <div className="stu-detail-item"><MapPin size={15} /> {student.city || "—"}</div>
            <div className="stu-detail-item"><Calendar size={15} /> {formatDate(student.birthDay)}</div>
            <div className="stu-detail-item"><School size={15} /> الفصل: {student.className || "—"}</div>
          </div>
        </div>
      )}
    </Modal>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
export default function StudentsPage() {
  const [students, setStudents]     = useState([]);
  const [classes,  setClasses]      = useState([]);
  const [total,    setTotal]        = useState(0);
  const [page,     setPage]         = useState(1);
  const [pageSize] = useState(10);
  const [search,   setSearch]       = useState("");
  const [loading,  setLoading]      = useState(false);
  const [saving,   setSaving]       = useState(false);
  const [modal,    setModal]        = useState(null); // "add" | "edit" | "detail"
  const [selected, setSelected]     = useState(null);
  const [toasts,   setToasts]       = useState([]);

  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  // ── Toast ──
  function toast(msg, type = "success") {
    const id = Date.now();
    setToasts((p) => [...p, { id, msg, type }]);
    setTimeout(() => setToasts((p) => p.filter((t) => t.id !== id)), 3500);
  }

  // ── Load classes ──
  const loadClasses = useCallback(async () => {
    try {
      const res = await fetch(`${API}/classes`, { headers: authHdr() });
      if (!res.ok) return;
      const d = await res.json();
      // GET /classes يرجع PageResult لذا نأخذ d.items
      setClasses(Array.isArray(d) ? d : d.items ?? d.data ?? []);
    } catch { }
  }, []);

  // ── Load students ──
  const loadStudents = useCallback(async (p = page, q = search) => {
    setLoading(true);
    try {
      let url;
      if (q && q.trim()) {
        // استخدام endpoint البحث بالاسم من الباك اند
        url = `${API}/students/by-name/${encodeURIComponent(q.trim())}`;
      } else {
        url = `${API}/students?pageNumber=${p}&pageSize=${pageSize}`;
      }
      const res = await fetch(url, { headers: authHdr() });
      if (res.status === 401) { sessionStorage.clear(); window.location.href = "/login"; return; }
      const d = await res.json();
      // بدون بحث: d هو PageResult → .items
      // مع بحث: d هو مصفوفة مباشرة
      const items = Array.isArray(d) ? d : d.items ?? d.data ?? [];
      setStudents(items);
      setTotal(Array.isArray(d) ? items.length : (d.totalCount ?? d.count ?? items.length));
    } catch {
      toast("فشل تحميل بيانات الطلاب", "error");
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search]);

  useEffect(() => { loadClasses(); }, [loadClasses]);
  useEffect(() => { loadStudents(page, search); }, [page]);

  // search debounce
  useEffect(() => {
    const t = setTimeout(() => { setPage(1); loadStudents(1, search); }, 400);
    return () => clearTimeout(t);
  }, [search]);

  // ── Create ──
  async function handleCreate(fd) {
    setSaving(true);
    try {
      const res = await fetch(`${API}/students`, {
        method: "POST",
        headers: { Authorization: `Bearer ${getToken()}` },
        body: fd,
      });
      if (!res.ok) { const e = await res.json(); toast(e.detail ?? "فشل إضافة الطالب", "error"); return; }
      toast("تم إضافة الطالب بنجاح ✓");
      setModal(null);
      loadStudents(1, search);
    } catch { toast("خطأ في الاتصال", "error"); }
    finally { setSaving(false); }
  }

  // ── Update ──
  async function handleUpdate(fd) {
    if (!selected) return;
    setSaving(true);
    try {
      const res = await fetch(`${API}/students/${selected.id}`, {
        method: "PUT",
        headers: { Authorization: `Bearer ${getToken()}` },
        body: fd,
      });
      if (!res.ok) { const e = await res.json(); toast(e.detail ?? "فشل تحديث الطالب", "error"); return; }
      toast("تم تحديث بيانات الطالب ✓");
      setModal(null); setSelected(null);
      loadStudents(page, search);
    } catch { toast("خطأ في الاتصال", "error"); }
    finally { setSaving(false); }
  }

  // ── Delete ──
  async function handleDelete(id) {
    if (!confirm("هل أنت متأكد من حذف هذا الطالب؟")) return;
    try {
      const res = await fetch(`${API}/students/${id}`, {
        method: "DELETE",
        headers: authHdr(),
      });
      if (!res.ok) { toast("فشل حذف الطالب", "error"); return; }
      toast("تم حذف الطالب");
      if (students.length === 1 && page > 1) setPage((p) => p - 1);
      else loadStudents(page, search);
    } catch { toast("خطأ في الاتصال", "error"); }
  }

  // ─── Render ────────────────────────────────────────────────────────────
  return (
    <div className="stu-page">
      <Toast toasts={toasts} />

      {/* ── Header ── */}
      <div className="stu-header">
        <div>
          <h1 className="stu-title">
            <Users size={22} /> إدارة الطلاب
          </h1>
          <p className="stu-subtitle">
            إجمالي: <strong>{total}</strong> طالب
          </p>
        </div>
        <div className="stu-header-actions">
          <button className="stu-refresh-btn" onClick={() => loadStudents(page, search)} title="تحديث">
            <RefreshCw size={16} className={loading ? "spin" : ""} />
          </button>
          <button className="btn-gold" onClick={() => { setSelected(null); setModal("add"); }}>
            <Plus size={16} /> إضافة طالب
          </button>
        </div>
      </div>

      {/* ── Search ── */}
      <div className="stu-search-bar">
        <Search size={16} className="stu-search-icon" />
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="ابحث باسم الطالب..."
          className="stu-search-input"
        />
        {search && (
          <button className="stu-search-clear" onClick={() => setSearch("")}>
            <X size={14} />
          </button>
        )}
      </div>

      {/* ── Table ── */}
      <div className="stu-table-wrap">
        {loading ? (
          <div className="stu-loading">
            <Loader2 size={32} className="spin" color="var(--gold)" />
            <span>جارٍ التحميل...</span>
          </div>
        ) : students.length === 0 ? (
          <div className="stu-empty">
            <Users size={48} color="#333" />
            <p>{search ? "لا توجد نتائج للبحث" : "لا يوجد طلاب مسجلون بعد"}</p>
            {!search && (
              <button className="btn-gold" onClick={() => setModal("add")}>
                <Plus size={15} /> أضف أول طالب
              </button>
            )}
          </div>
        ) : (
          <table className="stu-table">
            <thead>
              <tr>
                <th>#</th>
                <th>الطالب</th>
                <th>الفصل</th>
                <th>المدينة</th>
                <th>الهاتف</th>
                <th>تاريخ الميلاد</th>
                <th>إجراءات</th>
              </tr>
            </thead>
            <tbody>
              {students.map((s, i) => (
                <tr key={s.id}>
                  <td className="stu-td-num">{(page - 1) * pageSize + i + 1}</td>
                  <td>
                    <div className="stu-student-cell">
                      <div className="stu-avatar">
                        {s.imagePath
                          ? <img src={imgUrl(s.imagePath)} alt={s.firstName} />
                          : <span>{s.firstName?.[0]}{s.lastName?.[0]}</span>}
                      </div>
                      <div>
                        <div className="stu-name">{s.firstName} {s.lastName}</div>
                        <div className="stu-id">ID: {s.id}</div>
                      </div>
                    </div>
                  </td>
                  <td>
                    <span className="stu-badge">{s.className || "—"}</span>
                  </td>
                  <td className="stu-muted">{s.city || "—"}</td>
                  <td className="stu-muted" dir="ltr">{s.phone || "—"}</td>
                  <td className="stu-muted">{formatDate(s.birthDay)}</td>
                  <td>
                    <div className="stu-actions">
                      <button className="stu-btn-view" title="عرض التفاصيل"
                        onClick={() => { setSelected(s); setModal("detail"); }}>
                        <Eye size={14} />
                      </button>
                      <button className="stu-btn-edit" title="تعديل"
                        onClick={() => { setSelected(s); setModal("edit"); }}>
                        <Edit size={14} />
                      </button>
                      <button className="stu-btn-del" title="حذف"
                        onClick={() => handleDelete(s.id)}>
                        <Trash2 size={14} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* ── Pagination ── */}
      {totalPages > 1 && (
        <div className="stu-pagination">
          <button
            className="stu-page-btn"
            onClick={() => setPage((p) => p - 1)}
            disabled={page === 1}
          >
            <ChevronRight size={16} />
          </button>
          <span className="stu-page-info">
            صفحة <strong>{page}</strong> من <strong>{totalPages}</strong>
          </span>
          <button
            className="stu-page-btn"
            onClick={() => setPage((p) => p + 1)}
            disabled={page === totalPages}
          >
            <ChevronLeft size={16} />
          </button>
        </div>
      )}

      {/* ── Add Modal ── */}
      <Modal show={modal === "add"} title="إضافة طالب جديد" onClose={() => setModal(null)} wide>
        <StudentForm classes={classes} onSubmit={handleCreate} loading={saving} />
      </Modal>

      {/* ── Edit Modal ── */}
      <Modal show={modal === "edit"} title="تعديل بيانات الطالب" onClose={() => { setModal(null); setSelected(null); }} wide>
        {selected && (
          <StudentForm
            initial={{
              firstName: selected.firstName ?? "",
              lastName:  selected.lastName ?? "",
              city:      selected.city ?? "",
              phone:     selected.phone ?? "",
              birthDay:  selected.birthDay ?? "",
              classId:   selected.classId ?? "",
              imagePath: selected.imagePath,
            }}
            classes={classes}
            onSubmit={handleUpdate}
            loading={saving}
          />
        )}
      </Modal>

      {/* ── Detail Modal ── */}
      <DetailModal
        student={modal === "detail" ? selected : null}
        onClose={() => { setModal(null); setSelected(null); }}
      />
    </div>
  );
}
