"use client";

import { useState, useEffect, useCallback } from "react";
import {
  Plus, Search, Trash2, Edit, X, ChevronRight, ChevronLeft,
  Loader2, RefreshCw, BookOpen
} from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

function getToken() {
  return typeof window !== "undefined" ? sessionStorage.getItem("accessToken") : null;
}

function authHdr(json = true) {
  const h = { Authorization: `Bearer ${getToken()}` };
  if (json) h["Content-Type"] = "application/json";
  return h;
}

export default function SubjectsPage() {
  const [subjects, setSubjects] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [modal, setModal] = useState(null); // "add" | "edit"
  const [selected, setSelected] = useState(null);
  const [toasts, setToasts] = useState([]);

  // Form
  const [subjectName, setSubjectName] = useState("");

  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  function toast(msg, type = "success") {
    const id = Date.now();
    setToasts((p) => [...p, { id, msg, type }]);
    setTimeout(() => setToasts((p) => p.filter((t) => t.id !== id)), 3500);
  }

  const loadSubjects = useCallback(async (p = page, q = search) => {
    setLoading(true);
    try {
      let url;
      if (q && q.trim()) {
        url = `${API}/subjects/by-name/${encodeURIComponent(q.trim())}`;
      } else {
        url = `${API}/subjects?pageNumber=${p}&pageSize=${pageSize}`;
      }
      
      const res = await fetch(url, { headers: authHdr() });
      if (res.status === 401) { sessionStorage.clear(); window.location.href = "/login"; return; }
      
      const d = await res.json();
      const items = Array.isArray(d) ? d : d.items ?? d.data ?? [];
      
      setSubjects(items);
      setTotal(Array.isArray(d) ? items.length : (d.totalCount ?? d.count ?? items.length));
    } catch {
      toast("فشل تحميل بيانات المواد", "error");
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search]);

  useEffect(() => { loadSubjects(page, search); }, [page, search, loadSubjects]);

  function openAddModal() {
    setSubjectName("");
    setModal("add");
  }

  function openEditModal(sub) {
    setSelected(sub);
    setSubjectName(sub.subjectName || "");
    setModal("edit");
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    
    const body = { SubjectName: subjectName };
    
    try {
      const method = modal === "add" ? "POST" : "PUT";
      const endpoint = modal === "add" ? `${API}/subjects` : `${API}/subjects/${selected.id}`;
      
      const res = await fetch(endpoint, {
        method,
        headers: authHdr(),
        body: JSON.stringify(body)
      });
      
      if (!res.ok) { 
        const err = await res.json().catch(() => ({})); 
        toast(err.detail ?? "حدث خطأ أثناء الحفظ", "error"); 
        return; 
      }
      
      toast(modal === "add" ? "تم إضافة المادة بنجاح" : "تم تعديل المادة بنجاح");
      setModal(null);
      setSelected(null);
      loadSubjects(page, search);
    } catch {
      toast("خطأ في الاتصال", "error");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (!confirm("هل أنت متأكد من حذف هذه المادة؟")) return;
    try {
      const res = await fetch(`${API}/subjects/${id}`, { method: "DELETE", headers: authHdr() });
      if (!res.ok) { toast("حدث خطأ المادة مستخدمة من قبل أستاذ ربما", "error"); return; }
      toast("تم حذف المادة بنجاح");
      loadSubjects();
    } catch {
      toast("خطأ في الاتصال", "error");
    }
  }

  return (
    <div className="stu-page">
      <div className="stu-toast-stack">
        {toasts.map((t) => (
          <div key={t.id} className={`stu-toast stu-toast-${t.type}`}>{t.msg}</div>
        ))}
      </div>

      <div className="stu-header">
        <div>
          <h1 className="stu-title"><BookOpen size={22} /> إدارة المواد</h1>
          <p className="stu-subtitle">إجمالي: <strong>{total}</strong> مادة</p>
        </div>
        <div className="stu-header-actions">
          <button className="stu-refresh-btn" onClick={() => loadSubjects()} title="تحديث">
            <RefreshCw size={16} className={loading ? "spin" : ""} />
          </button>
          <button className="btn-gold" onClick={openAddModal}><Plus size={16} /> إضافة مادة</button>
        </div>
      </div>

      <div className="stu-search-bar">
        <Search size={16} className="stu-search-icon" />
        <input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="ابحث عن مادة..." className="stu-search-input" />
        {search && <button className="stu-search-clear" onClick={() => setSearch("")}><X size={14} /></button>}
      </div>

      <div className="stu-table-wrap">
        {loading ? (
          <div className="stu-loading"><Loader2 size={32} className="spin" color="var(--gold)" /></div>
        ) : subjects.length === 0 ? (
          <div className="stu-empty"><BookOpen size={48} color="#333" /><p>لا يوجد مواد</p></div>
        ) : (
          <table className="stu-table">
            <thead>
              <tr>
                <th>#</th>
                <th>اسم المادة</th>
                <th>عدد الأساتذة</th>
                <th>إجراءات</th>
              </tr>
            </thead>
            <tbody>
              {subjects.map((sub, i) => (
                <tr key={sub.id}>
                  <td className="stu-td-num">{(page - 1) * pageSize + i + 1}</td>
                  <td style={{fontWeight: 'bold'}}>{sub.subjectName}</td>
                  <td><span className="stu-badge">{sub.teachersCount ?? 0}</span></td>
                  <td>
                    <div className="stu-actions">
                      <button className="stu-btn-edit" onClick={() => openEditModal(sub)}><Edit size={14} /></button>
                      <button className="stu-btn-del" onClick={() => handleDelete(sub.id)}><Trash2 size={14} /></button>
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
      {modal && (
        <div className="stu-modal-overlay" onClick={() => setModal(null)}>
          <div className="stu-modal-card" onClick={e => e.stopPropagation()}>
            <div className="stu-modal-header">
              <span className="stu-modal-title">{modal === "add" ? "إضافة مادة جديدة" : "تعديل المادة"}</span>
              <button className="stu-modal-close" onClick={() => setModal(null)}><X size={16} /></button>
            </div>
            <div className="stu-modal-body">
              <form className="stu-form" onSubmit={handleSubmit}>
                <div className="stu-fg" style={{marginBottom: "1rem"}}>
                  <label>اسم المادة *</label>
                  <input value={subjectName} onChange={e => setSubjectName(e.target.value)} required />
                </div>
                <button type="submit" className="btn-gold stu-submit-btn" disabled={saving}>
                  {saving ? <Loader2 size={16} className="spin" /> : (modal === "add" ? <Plus size={16} /> : <Edit size={16} />)}
                  {saving ? "جارٍ الحفظ..." : "حفظ المادة"}
                </button>
              </form>
            </div>
          </div>
        </div>
      )}

    </div>
  );
}
