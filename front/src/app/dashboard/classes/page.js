"use client";

import { useState, useEffect, useCallback } from "react";
import {
  Plus, Search, Trash2, Edit, X, ChevronRight, ChevronLeft,
  Loader2, RefreshCw, School, Users
} from "lucide-react";

const API = process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7045/api/v1";

const GRADES = {
  1: "الاول", 2: "الثاني", 3: "الثالث", 4: "الرابع",
  5: "الخامس", 6: "السادس", 7: "السابع", 8: "الثامن",
  9: "التاسع", 10: "العاشر", 11: "الحادي عشر", 12: "البكالوريا"
};

function getToken() {
  return typeof window !== "undefined" ? sessionStorage.getItem("accessToken") : null;
}

function authHdr(json = true) {
  const h = { Authorization: `Bearer ${getToken()}` };
  if (json) h["Content-Type"] = "application/json";
  return h;
}

export default function ClassesPage() {
  const [classes, setClasses] = useState([]);
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
  const [classNumber, setClassNumber] = useState("");
  const [studentStep, setStudentStep] = useState(1);

  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  function toast(msg, type = "success") {
    const id = Date.now();
    setToasts((p) => [...p, { id, msg, type }]);
    setTimeout(() => setToasts((p) => p.filter((t) => t.id !== id)), 3500);
  }

  const loadClasses = useCallback(async (p = page, q = search) => {
    setLoading(true);
    try {
      let url;
      if (q && q.trim()) {
        url = `${API}/classes/by-number/${encodeURIComponent(q.trim())}`;
      } else {
        url = `${API}/classes?pageNumber=${p}&pageSize=${pageSize}`;
      }
      
      const res = await fetch(url, { headers: authHdr() });
      if (res.status === 401) { sessionStorage.clear(); window.location.href = "/login"; return; }
      
      const d = await res.json();
      const items = Array.isArray(d) ? d : d.items ?? d.data ?? (d.id ? [d] : []); // fallback if API returns single obj
      
      setClasses(items);
      setTotal(Array.isArray(d) ? items.length : (d.totalCount ?? d.count ?? items.length));
    } catch {
      toast("فشل تحميل بيانات الفصول", "error");
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search]);

  useEffect(() => { loadClasses(page, search); }, [page, search, loadClasses]);

  function openAddModal() {
    setClassNumber("");
    setStudentStep(1);
    setModal("add");
  }

  function openEditModal(cls) {
    setSelected(cls);
    setClassNumber(cls.classNumber || "");
    setStudentStep(cls.studentStep || 1);
    setModal("edit");
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    
    // Check if step is a valid number, mapping is 1-12
    const body = { 
      ClassNumber: classNumber, 
      StudentStep: parseInt(studentStep, 10) 
    };
    
    try {
      const method = modal === "add" ? "POST" : "PUT";
      const endpoint = modal === "add" ? `${API}/classes` : `${API}/classes/${selected.id}`;
      
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
      
      toast(modal === "add" ? "تم إضافة الفصل بنجاح" : "تم تعديل الفصل بنجاح");
      setModal(null);
      setSelected(null);
      loadClasses(page, search);
    } catch {
      toast("خطأ في الاتصال", "error");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (!confirm("هل أنت متأكد من حذف هذا الفصل؟")) return;
    try {
      const res = await fetch(`${API}/classes/${id}`, { method: "DELETE", headers: authHdr() });
      if (!res.ok) { toast("حدث خطأ، الفصل مستخدم", "error"); return; }
      toast("تم حذف الفصل بنجاح");
      loadClasses();
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
          <h1 className="stu-title"><School size={22} /> إدارة الفصول</h1>
          <p className="stu-subtitle">إجمالي: <strong>{total}</strong> فصل</p>
        </div>
        <div className="stu-header-actions">
          <button className="stu-refresh-btn" onClick={() => loadClasses()} title="تحديث">
            <RefreshCw size={16} className={loading ? "spin" : ""} />
          </button>
          <button className="btn-gold" onClick={openAddModal}><Plus size={16} /> إضافة فصل</button>
        </div>
      </div>

      <div className="stu-search-bar">
        <Search size={16} className="stu-search-icon" />
        <input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="ابحث برقم/رقم القاعة ..." className="stu-search-input" />
        {search && <button className="stu-search-clear" onClick={() => setSearch("")}><X size={14} /></button>}
      </div>

      <div className="stu-table-wrap">
        {loading ? (
          <div className="stu-loading"><Loader2 size={32} className="spin" color="var(--gold)" /></div>
        ) : classes.length === 0 ? (
          <div className="stu-empty"><School size={48} color="#333" /><p>لا يوجد فصول</p></div>
        ) : (
          <table className="stu-table">
            <thead>
              <tr>
                <th>#</th>
                <th>رقم القاعة </th>
                <th>المرحلة الدراسية</th>
                <th>الطلاب</th>
                <th>الأساتذة</th>
                <th>إجراءات</th>
              </tr>
            </thead>
            <tbody>
              {classes.map((cls, i) => (
                <tr key={cls.id}>
                  <td className="stu-td-num">{(page - 1) * pageSize + i + 1}</td>
                  <td style={{fontWeight: 'bold'}}>{cls.classNumber}</td>
                  <td><span className="stu-badge">{GRADES[cls.studentStep] || `مرحلة ${cls.studentStep}`}</span></td>
                  <td>
                     <div style={{display:'flex', alignItems: 'center', gap: '0.4rem'}}>
                         <Users size={14} color="var(--gray)" />
                         <span>{cls.studentsCount ?? 0}</span>
                     </div>
                  </td>
                  <td>{cls.teachersCount ?? 0}</td>
                  <td>
                    <div className="stu-actions">
                      <button className="stu-btn-edit" onClick={() => openEditModal(cls)}><Edit size={14} /></button>
                      <button className="stu-btn-del" onClick={() => handleDelete(cls.id)}><Trash2 size={14} /></button>
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
              <span className="stu-modal-title">{modal === "add" ? "إضافة فصل جديد" : "تعديل الفصل"}</span>
              <button className="stu-modal-close" onClick={() => setModal(null)}><X size={16} /></button>
            </div>
            <div className="stu-modal-body">
              <form className="stu-form" onSubmit={handleSubmit}>
                <div className="stu-form-grid" style={{marginBottom: "1rem"}}>
                  <div className="stu-fg">
                    <label>رقم القاعة  *</label>
                    <input value={classNumber} onChange={e => setClassNumber(e.target.value)} required />
                  </div>
                  <div className="stu-fg">
                    <label>المرحلة الدراسية *</label>
                    <select value={studentStep} onChange={e => setStudentStep(e.target.value)} required>
                      {Object.entries(GRADES).map(([val, name]) => (
                        <option key={val} value={val}>{name}</option>
                      ))}
                    </select>
                  </div>
                </div>
                <button type="submit" className="btn-gold stu-submit-btn" disabled={saving}>
                  {saving ? <Loader2 size={16} className="spin" /> : (modal === "add" ? <Plus size={16} /> : <Edit size={16} />)}
                  {saving ? "جارٍ الحفظ..." : "حفظ الفصل"}
                </button>
              </form>
            </div>
          </div>
        </div>
      )}

    </div>
  );
}
