"use client";

import { useState, useEffect, useCallback } from "react";
import {
  Users, Plus, Search, Trash2, Edit, X, ChevronRight, ChevronLeft,
  User, Phone, MapPin, Loader2, RefreshCw, Eye, BookOpen, GraduationCap,
  Upload, AlertCircle, Check
} from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";
const IMG_BASE = process.env.NEXT_PUBLIC_API_URL?.replace("/api/v1", "") ?? "https://localhost:7045";

function imgUrl(path) {
  if (!path) return null;
  return `${IMG_BASE}/${path.replace(/^\//, "")}`;
}

function getToken() {
  return typeof window !== "undefined" ? sessionStorage.getItem("accessToken") : null;
}

function authHdr(json = true) {
  const h = { Authorization: `Bearer ${getToken()}` };
  if (json) h["Content-Type"] = "application/json";
  return h;
}

export default function TeachersPage() {
  const [teachers, setTeachers] = useState([]);
  const [classesList, setClassesList] = useState([]);
  const [subjectsList, setSubjectsList] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [modal, setModal] = useState(null); // "add" | "edit" | "detail"
  const [selected, setSelected] = useState(null);
  const [toasts, setToasts] = useState([]);

  // Form State
  const [form, setForm] = useState({
    firstName: "", lastName: "", city: "", phone: "", subjectId: "", classId: "", email: "", password: ""
  });
  const [img, setImg] = useState(null);
  const [preview, setPreview] = useState(null);

  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  function setFieldValue(k, v) { setForm((p) => ({ ...p, [k]: v })); }

  function toast(msg, type = "success") {
    const id = Date.now();
    setToasts((p) => [...p, { id, msg, type }]);
    setTimeout(() => setToasts((p) => p.filter((t) => t.id !== id)), 3500);
  }

  const loadClassesAndSubjects = useCallback(async () => {
    try {
      const p1 = fetch(`${API}/classes`, { headers: authHdr() }).then(r => r.json());
      const p2 = fetch(`${API}/subjects`, { headers: authHdr() }).then(r => r.json());
      const [resClasses, resSubjects] = await Promise.all([p1, p2]);
      
      setClassesList(Array.isArray(resClasses) ? resClasses : resClasses.items ?? resClasses.data ?? []);
      setSubjectsList(Array.isArray(resSubjects) ? resSubjects : resSubjects.items ?? resSubjects.data ?? []);
    } catch { }
  }, []);

  const loadTeachers = useCallback(async (p = page, q = search) => {
    setLoading(true);
    try {
      let url;
      if (q && q.trim()) {
        url = `${API}/teachers/by-name/${encodeURIComponent(q.trim())}`;
      } else {
        url = `${API}/teachers?pageNumber=${p}&pageSize=${pageSize}`;
      }
      
      const res = await fetch(url, { headers: authHdr() });
      if (res.status === 401) { sessionStorage.clear(); window.location.href = "/login"; return; }
      
      const d = await res.json();
      const items = Array.isArray(d) ? d : d.items ?? d.data ?? [];
      
      setTeachers(items);
      setTotal(Array.isArray(d) ? items.length : (d.totalCount ?? d.count ?? items.length));
    } catch {
      toast("فشل تحميل بيانات الأساتذة", "error");
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search]);

  useEffect(() => { loadClassesAndSubjects(); }, [loadClassesAndSubjects]);
  useEffect(() => { loadTeachers(page, search); }, [page, search, loadTeachers]); // Need proper debounce in real app, simplified here

  function openAddModal() {
    setForm({ firstName: "", lastName: "", city: "", phone: "", subjectId: "", classId: "", email: "", password: "" });
    setImg(null);
    setPreview(null);
    setModal("add");
  }

  function openEditModal(t) {
    setSelected(t);
    setForm({
      firstName: t.firstName || "",
      lastName: t.lastName || "",
      city: t.city || "",
      phone: t.phone || "",
      subjectId: subjectsList.find(s => s.subjectName === t.subjectName)?.id || "",
      classId: classesList.find(c => c.classNumber === t.className)?.id || ""
    });
    setImg(null);
    setPreview(t.imagePath ? imgUrl(t.imagePath) : null);
    setModal("edit");
  }

  function pickImg(e) {
    const f = e.target.files[0];
    if (!f) return;
    setImg(f);
    setPreview(URL.createObjectURL(f));
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    
    const fd = new FormData();
    fd.append("FirstName", form.firstName);
    fd.append("LastName", form.lastName);
    if(form.city) fd.append("City", form.city);
    if(form.phone) fd.append("Phone", form.phone);
    fd.append("SubjectId", form.subjectId);
    fd.append("ClassId", form.classId);
    
    if (modal === "add") {
      fd.append("Email", form.email);
      fd.append("Password", form.password);
    }
    
    if (img) fd.append("Image", img);
    
    try {
      const method = modal === "add" ? "POST" : "PUT";
      const endpoint = modal === "add" ? `${API}/teachers` : `${API}/teachers/${selected.id}`;
      
      const res = await fetch(endpoint, {
        method,
        headers: { Authorization: `Bearer ${getToken()}` }, // No Content-Type for FormData
        body: fd
      });
      
      if (!res.ok) { 
        let errStr = "حدث خطأ أثناء الحفظ";
        try {
          const err = await res.json();
          // API returns problem details or validation errors
          if (err.errors) {
            const msgs = Object.values(err.errors).flatMap(x => x);
            errStr = msgs.join(" | ");
          } else if (err.detail) {
            errStr = err.detail;
          } else if (err.title) {
            errStr = err.title;
          } else if (err.message) {
            errStr = err.message;
          }
        } catch {}
        toast(errStr, "error"); 
        return; 
      }
      
      toast(modal === "add" ? "تم إضافة الأستاذ بنجاح" : "تم تعديل الأستاذ بنجاح");
      setModal(null);
      setSelected(null);
      loadTeachers(page, search);
    } catch {
      toast("خطأ في الاتصال", "error");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (!confirm("هل أنت متأكد من حذف هذا الأستاذ؟")) return;
    try {
      const res = await fetch(`${API}/teachers/${id}`, { method: "DELETE", headers: authHdr() });
      if (!res.ok) { toast("حدث خطأ", "error"); return; }
      toast("تم حذف الأستاذ بنجاح");
      loadTeachers();
    } catch {
      toast("خطأ في الاتصال", "error");
    }
  }

  return (
    <div className="stu-page">
      <div className="stu-toast-stack">
        {toasts.map((t) => (
          <div key={t.id} className={`stu-toast stu-toast-${t.type}`}>
            {t.type === "success" ? <Check size={15} /> : <AlertCircle size={15} />}
            {t.msg}
          </div>
        ))}
      </div>

      <div className="stu-header">
        <div>
          <h1 className="stu-title"><GraduationCap size={22} /> إدارة الأساتذة</h1>
          <p className="stu-subtitle">إجمالي: <strong>{total}</strong> أستاذ</p>
        </div>
        <div className="stu-header-actions">
          <button className="stu-refresh-btn" onClick={() => loadTeachers()} title="تحديث">
            <RefreshCw size={16} className={loading ? "spin" : ""} />
          </button>
          <button className="btn-gold" onClick={openAddModal}><Plus size={16} /> إضافة أستاذ</button>
        </div>
      </div>

      <div className="stu-search-bar">
        <Search size={16} className="stu-search-icon" />
        <input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="ابحث عن أستاذ..." className="stu-search-input" />
        {search && <button className="stu-search-clear" onClick={() => setSearch("")}><X size={14} /></button>}
      </div>

      <div className="stu-table-wrap">
        {loading ? (
          <div className="stu-loading"><Loader2 size={32} className="spin" color="var(--gold)" /><span>جارٍ التحميل...</span></div>
        ) : teachers.length === 0 ? (
          <div className="stu-empty"><Users size={48} color="#333" /><p>لا يوجد أساتذة</p></div>
        ) : (
          <table className="stu-table">
            <thead>
              <tr>
                <th>#</th>
                <th>الأستاذ</th>
                <th>المادة</th>
                <th>الفصل</th>
                <th>الهاتف</th>
                <th> المدينة</th>
                <th>إجراءات</th>
              </tr>
            </thead>
            <tbody>
              {teachers.map((t, i) => (
                <tr key={t.id}>
                  <td className="stu-td-num">{(page - 1) * pageSize + i + 1}</td>
                  <td>
                    <div className="stu-student-cell">
                      <div className="stu-avatar">
                        {t.imagePath ? <img src={imgUrl(t.imagePath)} alt={t.firstName} /> : <span>{t.firstName?.[0]}{t.lastName?.[0]}</span>}
                      </div>
                      <div>
                        <div className="stu-name">{t.firstName} {t.lastName}</div>
                        <div className="stu-id">ID: {t.id}</div>
                      </div>
                    </div>
                  </td>
                  <td><span className="stu-badge">{t.subjectName || "—"}</span></td>
                  <td><span className="stu-badge">{t.className || "—"}</span></td>
                  <td className="stu-muted" dir="ltr">{t.phone || "—"}</td>
                  <td className="stu-muted">{t.city || "—"}</td>
                  <td>
                    <div className="stu-actions">
                      <button className="stu-btn-view" onClick={() => { setSelected(t); setModal("detail"); }}><Eye size={14} /></button>
                      <button className="stu-btn-edit" onClick={() => openEditModal(t)}><Edit size={14} /></button>
                      <button className="stu-btn-del" onClick={() => handleDelete(t.id)}><Trash2 size={14} /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {totalPages > 1 && (
        <div className="stu-pagination">
          <button className="stu-page-btn" onClick={() => setPage(p => p - 1)} disabled={page === 1}><ChevronRight size={16} /></button>
          <span className="stu-page-info">صفحة <strong>{page}</strong> من <strong>{totalPages}</strong></span>
          <button className="stu-page-btn" onClick={() => setPage(p => p + 1)} disabled={page === totalPages}><ChevronLeft size={16} /></button>
        </div>
      )}

      {/* --- ADD/EDIT MODAL --- */}
      {modal && (modal === "add" || modal === "edit") && (
        <div className="stu-modal-overlay" onClick={() => setModal(null)}>
          <div className="stu-modal-card" style={{ maxWidth: 640 }} onClick={e => e.stopPropagation()}>
            <div className="stu-modal-header">
              <span className="stu-modal-title">{modal === "add" ? "إضافة أستاذ جديد" : "تعديل أستاذ"}</span>
              <button className="stu-modal-close" onClick={() => setModal(null)}><X size={16} /></button>
            </div>
            <div className="stu-modal-body">
              <form className="stu-form" onSubmit={handleSubmit}>
                <div className="stu-avatar-picker">
                  <div className="stu-avatar-preview">
                    {preview ? <img src={preview} alt="preview" /> : <User size={36} color="var(--gold-dark)" />}
                  </div>
                  <label className="stu-upload-btn">
                    <Upload size={14} /> اختر صورة
                    <input type="file" accept="image/*" hidden onChange={pickImg} />
                  </label>
                </div>
                <div className="stu-form-grid">
                  <div className="stu-fg"><label>الاسم الأول *</label><input value={form.firstName} onChange={e => setFieldValue("firstName", e.target.value)} required /></div>
                  <div className="stu-fg"><label>الاسم الأخير *</label><input value={form.lastName} onChange={e => setFieldValue("lastName", e.target.value)} required /></div>
                  <div className="stu-fg"><label>المدينة</label><input value={form.city} onChange={e => setFieldValue("city", e.target.value)} /></div>
                  <div className="stu-fg"><label>رقم الهاتف</label><input value={form.phone} onChange={e => setFieldValue("phone", e.target.value)} dir="ltr" /></div>
                  <div className="stu-fg">
                    <label>المادة *</label>
                    <select value={form.subjectId} onChange={e => setFieldValue("subjectId", e.target.value)} required>
                      <option value="">— اختر المادة —</option>
                      {subjectsList.map(s => <option key={s.id} value={s.id}>{s.subjectName}</option>)}
                    </select>
                  </div>
                  <div className="stu-fg">
                    <label>الفصل *</label>
                    <select value={form.classId} onChange={e => setFieldValue("classId", e.target.value)} required>
                      <option value="">— اختر الفصل —</option>
                      {classesList.map(c => <option key={c.id} value={c.id}>{c.classNumber}</option>)}
                    </select>
                  </div>
                  {modal === "add" && (
                    <>
                      <div className="stu-fg"><label>البريد الإلكتروني *</label><input type="email" value={form.email} onChange={e => setFieldValue("email", e.target.value)} required dir="ltr" /></div>
                      <div className="stu-fg"><label>كلمة المرور *</label><input type="password" value={form.password} onChange={e => setFieldValue("password", e.target.value)} required /></div>
                    </>
                  )}
                </div>
                <button type="submit" className="btn-gold stu-submit-btn" disabled={saving}>
                  {saving ? <Loader2 size={16} className="spin" /> : (modal === "add" ? <Plus size={16} /> : <Edit size={16} />)}
                  {saving ? "جارٍ الحفظ..." : "حفظ الأستاذ"}
                </button>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* --- DETAIL MODAL --- */}
      {modal === "detail" && selected && (
        <div className="stu-modal-overlay" onClick={() => setModal(null)}>
          <div className="stu-modal-card" onClick={e => e.stopPropagation()}>
            <div className="stu-modal-header">
              <span className="stu-modal-title">تفاصيل الأستاذ</span>
              <button className="stu-modal-close" onClick={() => setModal(null)}><X size={16} /></button>
            </div>
            <div className="stu-modal-body">
              <div className="stu-detail">
                <div className="stu-detail-avatar">
                  {selected.imagePath ? <img src={imgUrl(selected.imagePath)} alt="teacher" /> : <User size={48} color="var(--gold-dark)" />}
                </div>
                <h3 className="stu-detail-name">{selected.firstName} {selected.lastName}</h3>
                <div style={{display:'flex',gap:'0.5rem'}}><span className="stu-badge">{selected.subjectName || "—"}</span><span className="stu-badge">{selected.className || "—"}</span></div>
                <div className="stu-detail-grid">
                  <div className="stu-detail-item"><Phone size={15} /> {selected.phone || "—"}</div>
                  <div className="stu-detail-item"><MapPin size={15} /> {selected.city || "—"}</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

    </div>
  );
}
